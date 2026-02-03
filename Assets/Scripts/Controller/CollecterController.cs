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
        public PolyNavAgent agent;
        public MonsterType monsterType;
        public DropItemType targetType;   // 采集目标：你说的“只采集某种物品”
        public float detectRadius = 6f;  // 怪物检测半径
        public LayerMask monsterLayer;    // 只检测怪物层

        public float waitTime = 2f;       // 区域无怪物时的等待时间



        public GameObject weapon;
        public CollectorInventory inventory;
        public LingChuGeController depot;
        public Transform receiveTransform;
        public SkeletonAnimation skeletonAnimation;
        public SpriteRenderer spriteRenderer;
        public SpriteRenderer shadowRenderer;
        public SpriteRenderer weaponRenderer;

        public Collector collectorData;


        private CollectorState currentState;
        private FactoryController currentTarget; // 当前采集物目标

        public float currentHp;
        public float maxHp;
        public CollectorInfo collectorInfo;
        public int currentCarryNum;
        public int maxCarryNum;

        // 是否附近存在怪物，用于控制“先打怪，后捡东西”的优先级
        private bool hasMonsterNearby;

        private void Start()
        {
            agent = GetComponent<PolyNavAgent>();
            agent.map = GameObject.Find("Map").transform.GetComponent<PolyNavMap>();
            SwitchState(CollectorState.Idle);
        }


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

        public void SetLayer()
        {
            int newOrder = 30000 - Mathf.FloorToInt(transform.localPosition.y * 100);
            spriteRenderer.sortingOrder = newOrder;
            weaponRenderer.sortingOrder = newOrder;
            shadowRenderer.sortingOrder = newOrder;
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

            // 非战斗 / 非回仓才捡东西
            if (!hasMonsterNearby && currentState != CollectorState.ReturnToDepot)
            {
                DoCollect();
            }
        }

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
                    ChangeState(CollectorState.FindResource);
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
        private void UpdateAnimation()
        {
            var state = skeletonAnimation.AnimationState;
            var current = state.GetCurrent(0);

            bool moving = agent.hasPath && agent.remainingDistance > 1f;
            bool fighting = weapon.gameObject.activeSelf;

            string anim =
                fighting
                    ? (moving ? "zoulugongji" : "gongji")
                    : (moving ? "walk" : "idle");

            if (current == null || current.Animation.Name != anim)
            {
                state.SetAnimation(0, anim, true);
            }
        }



        public void AddDropItem(DropItemType itemType)
        {
            inventory.Add(itemType);
            if (inventory.IsFull())
            {
                SwitchState(CollectorState.ReturnToDepot);
            }
        }

        private void FindResource()
        {
            if (currentState == CollectorState.GoToResource)
                return;
            currentTarget = GameController.Instance.factoryControllers[monsterType];
            agent.SetDestination(currentTarget.transform.position);
            SwitchState(CollectorState.GoToResource);
        }

        private void DoCollect()
        {
            if (inventory.IsFull())
            {
                SwitchState(CollectorState.ReturnToDepot);
                agent.SetDestination(depot.collectorTransform.position);
                return;
            }

            var list = ScenePickupController.Instance.materials.ToArray();
            foreach (var item in list)
            {
                if (item == null) continue;                       // 回收后 item 可能被销毁
                if (!item.gameObject.activeInHierarchy) continue; // 避免 inactive
                if ((item as DropController).itemType != targetType) continue;
                if (item.isTaken) continue;
                float dist = Vector2.Distance(transform.position, item.transform.position);
                if (!inventory.IsFull())
                {
                    item.StartAttract(this.transform, receiveTransform);
                }
            }
        }

        public void CheckDrop()
        {
            var list = ScenePickupController.Instance.materials.ToArray();

            foreach (var item in list)
            {
                if (item == null) continue;                       // 回收后 item 可能被销毁
                if (!item.gameObject.activeInHierarchy) continue; // 避免 inactive
                if (item.isTaken) continue;
                if (inventory.IsFull())
                    break;
                float dist = Vector2.Distance(transform.position, item.transform.position);
                if (dist <= 5 && currentCarryNum < maxCarryNum)
                {
                    item.StartAttract(this.transform, receiveTransform);
                }
            }
        }


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
                if (currentHp < collectorData.maxHp && !isRegenerating)
                {
                    if (Time.time - lastDamageTime >= regenDelay)
                    {
                        regenCoroutine = StartCoroutine(RegenerateHealth());
                    }
                }
            }
        }


        private float lastDamageTime = -999f; // 上次受伤时间
        private bool isRegenerating = false;
        private Coroutine regenCoroutine;
        private float regenDelay = 3f;

        private IEnumerator RegenerateHealth()
        {
            isRegenerating = true;

            while (currentHp < collectorData.maxHp)
            {
                currentHp += 5 * Time.deltaTime;
                currentHp = Mathf.Min(currentHp, collectorData.maxHp);
                collectorInfo.UpdateFill(currentHp / collectorData.maxHp);
                yield return null;

                if (Time.time - lastDamageTime < regenDelay)
                {
                    isRegenerating = false;
                    yield break;
                }
            }

            collectorInfo.HideHpInfo();
            isRegenerating = false;
        }

        private void DoFight()
        {
            var list = GameController.Instance.factoryControllers[monsterType].monsterList;

            if (list.Count == 0)
            {
                // 没怪物了，继续采集
                SwitchState(CollectorState.Wait);
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
                SwitchState(CollectorState.FindResource);
                return;
            }

            // 根据与最近怪物的距离决定“靠近”还是“原地挥武器”
            // 约定：攻击检测范围与玩家一致，当距离小于攻击范围一半时停止移动，让武器去做触发检测
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

        private void SwitchState(CollectorState newState)
        {
            currentState = newState;
        }


        void OnEnable()
        {
            agent.OnDestinationReached += OnReachDestination;
        }

        void OnDisable()
        {
            agent.OnDestinationReached -= OnReachDestination;
        }

        void OnReachDestination()
        {

        }
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
    }
}