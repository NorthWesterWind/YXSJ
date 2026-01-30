using System.Collections;
using System.Collections.Generic;
using Controller.Pickups;
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
        public float detectRadius = 10f;  // 怪物检测半径
        public LayerMask monsterLayer;    // 只检测怪物层
        public float collectRange = 1.5f; // 采集距离
        public float waitTime = 2f;       // 区域无怪物时的等待时间

        public GameObject weapon;
        public CollectorInventory inventory;
        public LingChuGeController depot;
        public Transform receiveTransform;
        public Transform infoTransform;
        public SkeletonAnimation skeletonAnimation;
        public SpriteRenderer spriteRenderer;
        public SpriteRenderer shadowRenderer;
        public SpriteRenderer weaponRenderer;

        public Collector collectorData;

        public int id;


        private CollectorState currentState;
        private FactoryController currentTarget; // 当前采集物目标

        public float currentHp;
        public CollectorInfo collectorInfo;
        public int currentCarryNum;
        public int maxCarryNum;

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
            currentHp = collectorData.maxHp;
            currentCarryNum = 0;
            maxCarryNum = (int)c.bagCapacity;
        }

        public void SetLayer()
        {
            int newOrder = 30000 - Mathf.FloorToInt(transform.localPosition.y);
            spriteRenderer.sortingOrder = newOrder;
            weaponRenderer.sortingOrder = newOrder;
            shadowRenderer.sortingOrder = newOrder;
        }

        private void Update()
        {
            SetLayer();
            if (agent.hasPath && agent.remainingDistance > 1 && agent.currentSpeed < 0.1f)
            {
                var state = skeletonAnimation.AnimationState;
                var current = state.GetCurrent(0);
                if (weapon.gameObject.activeSelf)
                {
                    if (current == null || current.Animation.Name != "zoulugongji")
                    {
                        state.SetAnimation(0, "zoulugongji", true);
                    }

                }
                else
                {
                    if (current == null || current.Animation.Name != "walk")
                    {
                        state.SetAnimation(0, "walk", true);
                    }
                }
            }
            else
            {
                var state = skeletonAnimation.AnimationState;
                var current = state.GetCurrent(0);

                if (weapon.gameObject.activeSelf)
                {
                    if (current == null || current.Animation.Name != "gongji")
                    {
                        state.SetAnimation(0, "gongji", true);
                    }
                }
                else
                {

                    if (current == null || current.Animation.Name != "idle")
                    {
                        state.SetAnimation(0, "idle", true);
                    }
                }

            }



            if (inventory.IsFull())
            {
                if (currentState != CollectorState.ReturnToDepot)
                {
                    SwitchState(CollectorState.ReturnToDepot);
                    agent.SetDestination(depot.collectorTransform.position);
                }
                return;
            }

            CheckMonster();
            switch (currentState)
            {
                case CollectorState.Idle:
                    SwitchState(CollectorState.FindResource);
                    break;

                case CollectorState.FindResource:
                    currentTarget = GameController.Instance.factoryControllers[monsterType];
                    agent.SetDestination(currentTarget.transform.position);
                    SwitchState(CollectorState.GoToResource);
                    break;

                case CollectorState.GoToResource:
                    if (!agent.hasPath)
                        SwitchState(CollectorState.Fight);
                    break;

                case CollectorState.Fight:
                    DoFight();
                    break;

                case CollectorState.ReturnToDepot:
                    if (!agent.hasPath || agent.remainingDistance < 0.1f)
                    {
                        depot.Store(this, inventory);
                        SwitchState(CollectorState.Idle);
                    }
                    break;

                case CollectorState.Wait:
                    SwitchState(CollectorState.Idle);
                    break;
            }

            if (currentState != CollectorState.Fight &&
                currentState != CollectorState.ReturnToDepot)
            {
                DoCollect();
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
                if (dist <= collectRange && !inventory.IsFull())
                {
                    item.StartAttract(this.transform, receiveTransform);
                }
            }
        }

        public void CheckMonster()
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectRadius, monsterLayer);

            if (hits.Length > 0)
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

            // 前往最近怪物
            agent.SetDestination(nearest.position);
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