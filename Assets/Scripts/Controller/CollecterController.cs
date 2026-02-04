using System.Collections;
using System.Collections.Generic;
using Controller.Pickups;
using Module;
using Module.Data;
using PolyNav;
using Spine.Unity;
using UnityEngine;
using Utils;

namespace Controller
{
    public enum CollectorState
    {
        Idle,
        FindResource,
        GoToResource,
        Fight,
        ReturnToDepot,
        Wait
    }

    public class CollectorController : MonoBehaviour
    {
        #region Fields

        // 配置参数
        public float detectRadius = 6f;       // 怪物检测半径
        public float collectRadius = 5f;      // 物品吸引半径
        public LayerMask monsterLayer;        // 只检测怪物层
        public float waitTime = 2f;           // 区域无怪物时的等待时间

        // 组件引用
        public PolyNavAgent agent;
        public GameObject weapon;
        public CollectorInventory inventory;
        public LingChuGeController depot;
        public Transform receiveTransform;
        public SkeletonAnimation skeletonAnimation;
        public SpriteRenderer spriteRenderer;
        public SpriteRenderer shadowRenderer;
        public SpriteRenderer weaponRenderer;
        public CollectorInfo collectorInfo;

        // 数据
        public Collector collectorData;
        public MonsterType monsterType;
        public DropItemType targetType;       // 采集目标类型

        // 状态
        private CollectorState currentState;
        private FactoryController currentTarget;
        private bool hasMonsterNearby;

        // 属性
        public float currentHp;
        public float maxHp;
        public int currentCarryNum;
        public int maxCarryNum;

        // 回血相关
        private float lastDamageTime = -999f;
        private bool isRegenerating = false;
        private Coroutine regenCoroutine;
        private const float RegenDelay = 3f;

        // 动画常量
        private const string AnimIdle = "idle";
        private const string AnimWalk = "walk";
        private const string AnimAttack = "gongji";
        private const string AnimWalkAttack = "zoulugongji";

        #endregion

        #region Unity Lifecycle

        private void Start()
        {
            agent = GetComponent<PolyNavAgent>();
            agent.map = GameObject.FindWithTag("Map").transform.GetComponent<PolyNavMap>();
            ChangeState(CollectorState.Idle);
        }

        private void Update()
        {
            SetLayer();
            UpdateAnimation();
            CheckMonster();

            // 背包满，强制回仓
            if (inventory.IsFull() && currentState != CollectorState.ReturnToDepot)
            {
                ChangeState(CollectorState.ReturnToDepot);
                return;
            }
            UpdateState();
            DoCollect();
        }

        #endregion

        #region Initialization

        public void Init(Collector c, LingChuGeController structure)
        {
            collectorData = c;
            (MonsterType, DropItemType) v = Extensions.ExchangeFamilyType(collectorData.monsterType);
            monsterType = v.Item1;
            targetType = v.Item2;
            depot = structure;

            inventory.max = (int)c.bagCapacity;
            agent.maxSpeed = collectorData.moveSpeed;
            maxHp = collectorData.maxHp;
            var cardprogress = PlayerDataModule.Instance.data.cardUpProgressesList.Find(x => x.developType == CardDevelopType.UpgradeCharacterWithXuanCaiTuHp);
            if (cardprogress != null)
            {
                maxHp += cardprogress.level * 30;
            }
            currentHp = maxHp;

            currentCarryNum = 0;
            maxCarryNum = (int)c.bagCapacity;
            collectorInfo.HideHpInfo();
        }

        #endregion

        #region State Machine

        private void ChangeState(CollectorState newState)
        {
            if (currentState == newState) return;

            ExitState(currentState);
            currentState = newState;
            EnterState(newState);
        }

        private void ExitState(CollectorState state)
        {
            switch (state)
            {
                case CollectorState.Wait:
                    CancelInvoke(nameof(BackToIdle));
                    break;

                case CollectorState.Fight:
                    agent.Stop();
                    break;
            }
        }

        private void EnterState(CollectorState state)
        {
            switch (state)
            {
                case CollectorState.Idle:
                    break;

                case CollectorState.FindResource:
                    currentTarget = GameController.Instance.factoryControllers[monsterType];
                    ChangeState(CollectorState.GoToResource);
                    break;

                case CollectorState.GoToResource:
                    agent.SetDestination(currentTarget.transform.position);
                    break;

                case CollectorState.ReturnToDepot:
                    agent.SetDestination(depot.collectorTransform.position);
                    break;

                case CollectorState.Wait:
                    Invoke(nameof(BackToIdle), waitTime);
                    break;
            }
        }

        private void BackToIdle()
        {
            ChangeState(CollectorState.Idle);
        }

        private void UpdateState()
        {
            switch (currentState)
            {
                case CollectorState.Idle:
                    if (!inventory.IsEmpty())
                    {
                        depot.Store(this, inventory);   
                    }
                    else
                    {
                        ChangeState(CollectorState.FindResource);
                    }

                    break;

                case CollectorState.GoToResource:
                    if (!agent.hasPath || agent.remainingDistance < 0.1f)
                        ChangeState(CollectorState.Fight);
                    break;

                case CollectorState.Fight:
                    DoFight();
                    break;

                case CollectorState.ReturnToDepot:
                    if (!agent.hasPath || agent.remainingDistance < 0.1f)
                    {
                        depot.Store(this, inventory);
                        ChangeState(CollectorState.Idle);
                    }
                    break;
            }
        }

        #endregion

        #region Combat

        public void CheckMonster()
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectRadius, monsterLayer);

            hasMonsterNearby = hits.Length > 0;

            if (hasMonsterNearby)
            {
                weapon.gameObject.SetActive(true);
            }
            else
            {
                weapon.gameObject.SetActive(false);

                // 自动回血检测
                if (currentHp < maxHp && !isRegenerating)
                {
                    if (Time.time - lastDamageTime >= RegenDelay)
                    {
                        regenCoroutine = StartCoroutine(RegenerateHealth());
                    }
                }
            }
        }

        private void DoFight()
        {
            var list = GameController.Instance.factoryControllers[monsterType].monsterList;

            if (list.Count == 0)
            {
                // 没怪物了，进入等待状态
                ChangeState(CollectorState.Wait);
                return;
            }

            // 找出最近的怪物
            float minDist = float.MaxValue;
            Transform nearest = null;

            foreach (var monster in list)
            {
                if (monster == null) continue;

                float dist = Vector2.Distance(transform.position, monster.transform.position);

                if (dist < minDist)
                {
                    minDist = dist;
                    nearest = monster.transform;
                }
            }

            // 万一全部 monster 都被清掉
            if (nearest == null)
            {
                ChangeState(CollectorState.FindResource);
                return;
            }

            // 根据与最近怪物的距离决定"靠近"还是"原地挥武器"
            float stopDistance = detectRadius * 0.5f;

            if (minDist > stopDistance)
            {
                // 还没到攻击距离，继续往怪物位置移动
                agent.SetDestination(nearest.position);
            }
            else
            {
                // 已到攻击距离附近，停下脚步，让武器触发器去做伤害检测
                agent.Stop();
            }
        }

        #endregion

        #region Collection

        public void AddDropItem(DropItemType itemType)
        {
            inventory.Add(itemType);
            if (inventory.IsFull())
            {
                ChangeState(CollectorState.ReturnToDepot);
            }
        }

        private void DoCollect()
        {
            if (inventory.IsFull())
            {
                ChangeState(CollectorState.ReturnToDepot);
                return;
            }

            var materials = ScenePickupController.Instance.materials;
            foreach (var item in materials)
            {
                if (item == null) continue;
                if (!item.gameObject.activeInHierarchy) continue;
                if ((item as DropController).itemType != targetType) continue;
                if (item.isTaken) continue;

                float dist = Vector2.Distance(transform.position, item.transform.position);
                if (dist <= collectRadius && !inventory.IsFull())
                {
                    item.StartAttract(this.transform, receiveTransform);
                }
            }
        }

        #endregion

        #region Health

        private IEnumerator RegenerateHealth()
        {
            isRegenerating = true;

            while (currentHp < maxHp)
            {
                currentHp += 5 * Time.deltaTime;
                currentHp = Mathf.Min(currentHp, maxHp);
                collectorInfo.UpdateFill(currentHp / maxHp);
                yield return null;

                if (Time.time - lastDamageTime < RegenDelay)
                {
                    isRegenerating = false;
                    yield break;
                }
            }

            collectorInfo.HideHpInfo();
            isRegenerating = false;
        }

        #endregion

        #region Utility

        public void SetLayer()
        {
            int newOrder = 30000 - Mathf.FloorToInt(transform.localPosition.y * 100);
            spriteRenderer.sortingOrder = newOrder;
            weaponRenderer.sortingOrder = newOrder;
            shadowRenderer.sortingOrder = newOrder;
        }

        private void UpdateAnimation()
        {
            var state = skeletonAnimation.AnimationState;
            var current = state.GetCurrent(0);

            bool moving = agent.hasPath && agent.remainingDistance > 1f;
            bool fighting = weapon.gameObject.activeSelf;

            string anim = fighting
                ? (moving ? AnimWalkAttack : AnimAttack)
                : (moving ? AnimWalk : AnimIdle);

            if (current == null || current.Animation.Name != anim)
            {
                state.SetAnimation(0, anim, true);
            }
        }

        #endregion
    }

    public class CollectorInventory
    {
        public int max = 20;
        public Dictionary<DropItemType, int> dic = new();

        public bool IsFull()
        {
            int sum = 0;
            foreach (var v in dic.Values) sum += v;
            return sum >= max;
        }

        public void Add(DropItemType t)
        {
            if (!dic.ContainsKey(t)) dic[t] = 0;
            dic[t]++;
        }

        public void Clear()
        {
            dic.Clear();
        }
        public void Remove(DropItemType t, int count)
        {
            if (!dic.ContainsKey(t)) return;

            dic[t] -= count;
            if (dic[t] <= 0)
                dic.Remove(t);
        }
        public bool IsEmpty()
        {
            return dic.Count == 0;
        }


    }
}
