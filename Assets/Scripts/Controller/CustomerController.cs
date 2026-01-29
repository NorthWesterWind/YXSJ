using System.Collections;
using System.Collections.Generic;
using Controller.Pickups;
using Controller.Structure;
using Module.Data;
using PolyNav;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.AI;
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
        void Start()
        {

        }
        void Update()
        {
            SetLayer();
            var currentAnimation = skeletonAnimation.AnimationState.GetCurrent(0);
            if (agent.hasPath || agent.remainingDistance > 1)
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

            Vector2 dir = agent.movingDirection;
            // Vector2 dir = navAgent.velocity;
            if (dir == Vector2.zero) return;
            if (Mathf.Abs(dir.x) > 0.01f)
            {
                skeletonAnimation.skeleton.ScaleX = dir.x < 0 ? -1 : 1;
            }

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
        public void Init(CustomerData outdata, GoodsType type, StructureBase structureBase)
        {
            severing = false;
            goodsType = type; data = outdata;
            state = NpcState.QianWangGouMai;
            bornPosition = transform.position;
            salesStall = structureBase as SalesStall;
            agent = GetComponent<PolyNavAgent>();

            agent.map = GameObject.FindWithTag("Map").transform.GetComponent<PolyNavMap>();

            SetNextPosition();
            agent.SetDestination(nextPosition);
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
        }
        void OnDisable()
        {
            agent.OnDestinationReached -= OnReachDestination;
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


        public void WaitPurchase()
        {
            //            Debug.LogError($"{name} 到达购买点，开始等待购买");
            state = NpcState.WaitGouMaiWanCheng;
            StartCoroutine(PurchaseRoutine());

        }
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
            agent.SetDestination(nextPosition);
        }
        private void OnPurchaseTimeout()
        {
            state = NpcState.Angry;
            SetNextPosition();
            agent.SetDestination(nextPosition);
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
        }
    }
}