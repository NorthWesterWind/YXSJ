using System.Collections;
using System.Collections.Generic;
using Controller.Pickups;
using Controller.Structure;
using Module.Data;
using PolyNav;
using Spine.Unity;
using UnityEngine;
using Utils;
namespace Controller
{
    public enum NpcState
    { None, QianWangGouMai, WaitGouMaiWanCheng, QianWangShouYinTai, JieZhangChengGong, Angry, }
    public class CustomerController : MonoBehaviour
    {
        public PolyNavAgent agent;
        // public NavMeshAgent navAgent;
        public CustomerData data;
        public NpcState state;
        public Vector2 bornPosition;
        public Vector2 nextPosition;
        public Vector2 purchasePosition;
        public GoodsType goodsType;
        public SkeletonAnimation skeletonAnimation;
        public SpriteRenderer shadow;
        public int currentIndex = 0;
        public SalesStall salesStall;
        public Transform receiveTransform;
        public List<Production> purchaseList = new();
        public MeshRenderer _meshRenderer;
        public GameObject fillBg;
        public GameObject fill;
        public bool severing = false;
        private Vector2 spawnOrigin;
        private float spawnRadiusX = 3f;
        private float spawnRadiusY = 1.5f;
        private float returnRepathTimer = 0f;
        private const float ReturnRepathInterval = 0.5f;
        private bool mapWarningShown;
        private Coroutine ensureMoveCoroutine;
        private Coroutine invalidRecoverCoroutine;
        private float purchaseIdleTimer = 0f;
        private int spawnRelocateAttempts = 0;
        private const float PurchaseIdleRelocateDelay = 0.6f;
        private const int MaxSpawnRelocateAttempts = 3;
        private NpcState lastState;
        void Start()
        {

        }
        void Update()
        {
            if (state != lastState)
            {
                lastState = state;
                purchaseIdleTimer = 0f;
                spawnRelocateAttempts = 0;
            }

            SetLayer();
            var currentAnimation = skeletonAnimation.AnimationState.GetCurrent(0);
            if (agent != null && (agent.hasPath || agent.remainingDistance > 1))
            {
                if (currentAnimation == null || currentAnimation.Animation.Name != "walk")
                {
                    skeletonAnimation.AnimationState.SetAnimation(0, "walk", true);
                }
            }
            else
            {
                if (currentAnimation == null || currentAnimation.Animation.Name != "idle")
                {
                    skeletonAnimation.AnimationState.SetAnimation(0, "idle", true);
                }
            }

            Vector2 dir = agent != null ? agent.movingDirection : Vector2.zero;
            // Vector2 dir = navAgent.velocity;
            if (dir != Vector2.zero && Mathf.Abs(dir.x) > 0.01f)
            {
                skeletonAnimation.skeleton.ScaleX = dir.x < 0 ? -1 : 1;
            }

            EnsureMap();
            EnsureReturnToBorn();
            EnsurePurchaseMovement();

            // if (ReachedDestination())
            // {
            //     OnReachDestination();
            // }

        }
        public void SetLayer()
        {
            if (_meshRenderer == null)
                _meshRenderer = skeletonAnimation.GetComponent<MeshRenderer>();

            int order = 30000 - Mathf.RoundToInt(transform.position.y * 100);
            _meshRenderer.sortingOrder = order;
            shadow.sortingOrder = order - 5;
            for (int i = 0; i < purchaseList.Count; i++)
            {
                var obj = purchaseList[i];
                var mesh = obj.GetComponent<Production>().spriteRenderer;
                if (mesh != null)
                {
                    mesh.sortingOrder = order + i + 1;
                }
            }
        }
        public void Init(CustomerData outdata, GoodsType type, StructureBase structureBase, Vector2 origin, float radiusX, float radiusY)
        {
            severing = false;
            goodsType = type; data = outdata;
            state = NpcState.QianWangGouMai;
            lastState = state;
            purchaseIdleTimer = 0f;
            spawnRelocateAttempts = 0;
            bornPosition = transform.position;
            spawnOrigin = origin;
            if (radiusX > 0f) spawnRadiusX = radiusX;
            if (radiusY > 0f) spawnRadiusY = radiusY;
            salesStall = structureBase as SalesStall;
            agent = GetComponent<PolyNavAgent>();
            EnsureMap();
            SnapToValidPosition();

            SetNextPosition();
            TrySetDestination(nextPosition);
            if (ensureMoveCoroutine != null)
            {
                StopCoroutine(ensureMoveCoroutine);
            }
            ensureMoveCoroutine = StartCoroutine(EnsureMoveStarted());
            Vector2 dir = (nextPosition - (Vector2)transform.position).normalized;
            fillBg.gameObject.SetActive(false);
            fill.gameObject.transform.localScale = new Vector3(0, 1, 1);

            Debug.Log($"顾客生成点: {gameObject.transform.position}, 购买点: {nextPosition}, 距离: {(Vector2)transform.position - nextPosition}");
        }
        /// <summary>
        /// 初始化完成后强制顾客开始移动和逻辑
        /// 用于解决生成后原地待机问题
        /// </summary>
        // public void StartBehavior()
        // {
        //     if (agent.map == null)
        //     {
        //         agent.map = GameObject.FindWithTag("Map")?.GetComponent<PolyNavMap>();
        //         if (agent.map == null)
        //         {
        //             Debug.LogError($"{name} 找不到 PolyNavMap");
        //             return;
        //         }
        //     }
        //     // 设置目标点
        //     agent.SetDestination(nextPosition);
        // }

        // bool ReachedDestination()
        // {
        //     if (navAgent.pathPending) return false;

        //     return navAgent.remainingDistance <= navAgent.stoppingDistance
        //            && (!navAgent.hasPath || navAgent.velocity.sqrMagnitude == 0f);
        // }

        void OnEnable()
        {
            agent.OnDestinationReached += OnReachDestination;
            agent.OnDestinationInvalid += OnDestinationInvalid;
        }
        void OnDisable()
        {
            agent.OnDestinationReached -= OnReachDestination;
            agent.OnDestinationInvalid -= OnDestinationInvalid;
        }

        void OnReachDestination()
        {
            // 回家后销毁
            if (Vector2.Distance(transform.position, bornPosition) < 1f &&
                (state == NpcState.Angry || state == NpcState.JieZhangChengGong))
            {
                Destroy(gameObject);
                return;
            }

            // 到达购买点
            if (state == NpcState.QianWangGouMai)
            {
                WaitPurchase();
                EventCenter.Instance.TriggerEvent(EventMessages.CustomerArrivedSell, this, salesStall);
                return;
            }

            // 到达收银台
            if (state == NpcState.QianWangShouYinTai)
            {
                EventCenter.Instance.TriggerEvent(EventMessages.CustomerArrived, this, salesStall);
                return;
            }
        }

        private void EnsurePurchaseMovement()
        {
            if (state != NpcState.QianWangGouMai)
            {
                purchaseIdleTimer = 0f;
                return;
            }

            if (agent == null)
            {
                return;
            }

            if (agent.pathPending || agent.hasPath || agent.remainingDistance > 0.1f)
            {
                purchaseIdleTimer = 0f;
                return;
            }

            purchaseIdleTimer += Time.deltaTime;
            if (purchaseIdleTimer < PurchaseIdleRelocateDelay)
            {
                return;
            }
            purchaseIdleTimer = 0f;

            if (spawnRelocateAttempts >= MaxSpawnRelocateAttempts)
            {
                return;
            }
            spawnRelocateAttempts++;

            if (TryRelocateSpawn())
            {
                SetNextPosition();
                TrySetDestination(nextPosition);
                if (ensureMoveCoroutine != null)
                {
                    StopCoroutine(ensureMoveCoroutine);
                }
                ensureMoveCoroutine = StartCoroutine(EnsureMoveStarted());
            }
        }

        private void EnsureReturnToBorn()
        {
            if (state != NpcState.Angry && state != NpcState.JieZhangChengGong)
            {
                returnRepathTimer = 0f;
                return;
            }

            if (agent == null) return;
            if (agent.map == null)
            {
                EnsureMap();
                if (agent.map == null)
                {
                    return;
                }
            }

            float dist = Vector2.Distance(transform.position, bornPosition);
            if (dist <= 0.5f)
            {
                returnRepathTimer = 0f;
                return;
            }

            bool needsRepath = !agent.hasPath || agent.remainingDistance < 0.1f;
            if (!needsRepath && agent.movingDirection == Vector2.zero)
            {
                needsRepath = true;
            }

            if (needsRepath)
            {
                returnRepathTimer += Time.deltaTime;
                if (returnRepathTimer >= ReturnRepathInterval)
                {
                    TrySetDestination(bornPosition);
                    returnRepathTimer = 0f;
                }
            }
            else
            {
                returnRepathTimer = 0f;
            }
        }

        private bool EnsureMap()
        {
            if (agent == null)
            {
                return false;
            }

            if (agent.map != null)
            {
                return true;
            }

            PolyNavMap map = null;
            var mapObj = GameObject.FindWithTag("Map");
            if (mapObj != null)
            {
                map = mapObj.GetComponent<PolyNavMap>();
            }
            if (map == null)
            {
                var mapObjByName = GameObject.Find("Map");
                if (mapObjByName != null)
                {
                    map = mapObjByName.GetComponent<PolyNavMap>();
                }
            }
            if (map == null)
            {
                map = FindObjectOfType<PolyNavMap>();
            }

            agent.map = map;
            if (agent.map != null && agent.map.nodesCount == 0)
            {
                agent.map.GenerateMap();
            }

            if ((agent.map == null || agent.map.nodesCount == 0) && !mapWarningShown)
            {
                mapWarningShown = true;
                Debug.LogWarning("[Customer] PolyNavMap not ready, movement disabled.");
            }
            return agent.map != null && agent.map.nodesCount > 0;
        }

        private void SnapToValidPosition()
        {
            if (agent == null || agent.map == null)
            {
                return;
            }
            Vector2 pos = transform.position;
            if (!agent.map.PointIsValid(pos))
            {
                Vector2 fixedPos = agent.map.GetCloserEdgePoint(pos);
                transform.position = new Vector3(fixedPos.x, fixedPos.y, transform.position.z);
            }
        }


        public void WaitPurchase()
        {
            //            Debug.LogError($"{name} 到达购买点，开始等待购买");
            state = NpcState.WaitGouMaiWanCheng;
            StartCoroutine(PurchaseRoutine());

        }

        private bool purchaseConfirmed = false;
        private IEnumerator PurchaseRoutine()
        {
            float timer = 0f;
            while (timer < data.waitTime)
            {
                if (purchaseList.Count >= data.carryNum)
                {
                    Purchase();
                    yield break;
                }
                if (severing)
                {
                    yield return null;
                    continue;
                }
                timer += Time.deltaTime;
                yield return null;
            }
            if (!severing && purchaseList.Count < data.carryNum)
            {
                skeletonAnimation.AnimationState.SetAnimation(0, "angry", false);
                yield return new WaitForSeconds(1f);
                OnPurchaseTimeout();
            }

        }
        private void Purchase()
        {
            for (int i = 0; i < purchaseList.Count; i++)
            {
                var obj = purchaseList[i];
                purchaseList[i].FlyTo(receiveTransform.position, () =>
                 {
                     obj.transform.SetParent(transform, false);
                     obj.transform.position = receiveTransform.position;
                 });
            }
            severing = false;
            Debug.Log($"{name} 成功购买 {data.carryNum} 件商品");
            state = NpcState.QianWangShouYinTai;
            SetNextPosition();
            TrySetDestination(nextPosition);
        }
        private void OnPurchaseTimeout()
        {
            state = NpcState.Angry;
            SetNextPosition();
            TrySetDestination(nextPosition);
            EventCenter.Instance.TriggerEvent(EventMessages.CustomerLeave, salesStall, this);
        }
        public void SetNextPosition()
        {
            if (state == NpcState.QianWangGouMai)
            {
                var pos = salesStall.GetPurchasePosition();
                nextPosition = pos;
            }
            else if (state == NpcState.QianWangShouYinTai)
            {
                // GameController.Instance.RemoveCustomerFromQueue(salesStall, this);
                purchasePosition = (GameController.Instance.buildings[BuildingType.LingZhangTai] as CashierCounter).parchaseTransform.position;
                nextPosition = purchasePosition;
            }
            else if (state is NpcState.JieZhangChengGong or NpcState.Angry)
            {
                //GameController.Instance.RemoveCustomerFromQueue(salesStall, this);
                nextPosition = bornPosition;
            }

            if (EnsureMap())
            {
                var map = agent.map;
                if (map != null)
                {
                    Vector2 pos2 = nextPosition;
                    if (!map.PointIsValid(pos2))
                    {
                        pos2 = map.GetCloserEdgePoint(pos2);
                    }
                    nextPosition = pos2;
                }
            }
        }

        private bool TrySetDestination(Vector2 target)
        {
            if (agent == null)
            {
                return false;
            }
            if (!EnsureMap())
            {
                return false;
            }
            var map = agent.map;
            if (map == null)
            {
                return false;
            }
            if (!map.PointIsValid(target))
            {
                target = map.GetCloserEdgePoint(target);
            }
            nextPosition = target;
            return agent.SetDestination(target);
        }

        private IEnumerator EnsureMoveStarted()
        {
            const int maxAttempts = 3;
            int attempts = 0;
            while (attempts < maxAttempts)
            {
                yield return null;
                if (agent == null)
                {
                    yield break;
                }
                if (agent.pathPending)
                {
                    continue;
                }
                if (agent.hasPath || agent.remainingDistance > 0.1f)
                {
                    yield break;
                }

                attempts++;
                if (EnsureMap())
                {
                    // 修正当前位置到可走区域，再重新寻路
                    SnapToValidPosition();
                    TrySetDestination(nextPosition);
                }
            }

            if (EnsureMap())
            {
                // 仍无法移动时，强制把起点贴近目标所在的可走区域
                if (!TryRelocateSpawn())
                {
                    Vector2 rescue = agent.map.GetCloserEdgePoint(transform.position);
                    transform.position = new Vector3(rescue.x, rescue.y, transform.position.z);
                }
                TrySetDestination(nextPosition);
            }
        }

        private void OnDestinationInvalid()
        {
            if (invalidRecoverCoroutine != null)
            {
                return;
            }
            invalidRecoverCoroutine = StartCoroutine(RecoverFromInvalidPath());
        }

        private IEnumerator RecoverFromInvalidPath()
        {
            // Avoid recursive path requests during PolyNav callbacks
            yield return null;
            const int maxAttempts = 3;
            int attempts = 0;
            while (attempts < maxAttempts)
            {
                attempts++;
                if (!EnsureMap())
                {
                    yield return null;
                    continue;
                }

                SnapToValidPosition();
                TrySetDestination(nextPosition);

                float wait = 0f;
                while (agent != null && agent.pathPending && wait < 0.5f)
                {
                    wait += Time.deltaTime;
                    yield return null;
                }

                if (agent != null && (agent.hasPath || agent.remainingDistance > 0.1f))
                {
                    invalidRecoverCoroutine = null;
                    yield break;
                }

                yield return null;
            }

            if (EnsureMap())
            {
                if (!TryRelocateSpawn())
                {
                    Vector2 rescue = agent.map.GetCloserEdgePoint(transform.position);
                    transform.position = new Vector3(rescue.x, rescue.y, transform.position.z);
                }
                TrySetDestination(nextPosition);
            }
            invalidRecoverCoroutine = null;
        }

        private bool TryRelocateSpawn()
        {
            if (!EnsureMap())
            {
                return false;
            }

            if (state != NpcState.QianWangGouMai)
            {
                return false;
            }

            if (Vector2.Distance(transform.position, bornPosition) > 0.5f)
            {
                return false;
            }

            Vector2 origin = spawnOrigin != Vector2.zero ? spawnOrigin : bornPosition;
            var map = agent.map;

            for (int i = 0; i < 6; i++)
            {
                Vector2 pos = origin;
                pos.x += Random.Range(-spawnRadiusX, spawnRadiusX);
                pos.y += Random.Range(-spawnRadiusY, spawnRadiusY);
                if (map.PointIsValid(pos))
                {
                    bornPosition = pos;
                    transform.position = new Vector3(pos.x, pos.y, transform.position.z);
                    return true;
                }
            }

            Vector2 fallback = map.GetCloserEdgePoint(origin);
            if (map.PointIsValid(fallback))
            {
                bornPosition = fallback;
                transform.position = new Vector3(fallback.x, fallback.y, transform.position.z);
                return true;
            }

            return false;
        }
    }
}
