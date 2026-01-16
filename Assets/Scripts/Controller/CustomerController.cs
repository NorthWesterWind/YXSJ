using System.Collections;
using System.Collections.Generic;
using Controller.Pickups;
using Controller.Structure;
using Module.Data;
using PolyNav;
using Spine;
using Spine.Unity;
using UnityEngine;
using Utils;
namespace Controller
{
    public enum NpcState
    { None, QianWangGouMai, WaitGouMaiWanCheng, QianWangShouYinTai, JieZhangChengGong, Angry, }
    public class CustomerController : MonoBehaviour
    {
        public PolyNavAgent agent; public CustomerData data;
        public NpcState state;
        public Vector2 bornPosition;
        public Vector2 nextPosition;
        private Rigidbody2D _rigidbody2D;
        public GoodsType goodsType;
        public SkeletonAnimation skeletonAnimation;
        public SpriteRenderer shadow;
        public int currentIndex = 0;
        public SalesStall salesStall;
        public Transform receiveTransform;
        private List<Production> productionList = new();
        public List<Production> purchaseList = new();
        public MeshRenderer _meshRenderer; void Start() { skeletonAnimation.state.Complete += HandleAnimationComplete; }
        private void HandleAnimationComplete(TrackEntry trackEntry)
        {
            if (trackEntry.TrackIndex == 1 && trackEntry.Animation.Name == "angry")
            {
                OnPurchaseTimeout();
            }
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

            if (state == NpcState.QianWangGouMai && Vector2.Distance(transform.position, salesStall.parchaseTransform.position) < 1f && currentAnimation.Animation.Name == "idle")
            {
                WaitPurchase();
            }

            Vector2 dir = agent.movingDirection;
            if (dir == Vector2.zero) return;
            if (Mathf.Abs(dir.x) > 0.01f)
            {
                skeletonAnimation.skeleton.ScaleX = dir.x < 0 ? -1 : 1;
            }

        }
        public void UpdateQueueTarget(Vector2 pos)
        {
            agent.SetDestination(pos);
        }
        public void SetLayer()
        {
            if (_meshRenderer == null)
                _meshRenderer = skeletonAnimation.GetComponent<MeshRenderer>();

            int order = 30000 - Mathf.RoundToInt(transform.position.y * 100);
            _meshRenderer.sortingOrder = order;
            shadow.sortingOrder = order - 5;
        }
        public void Init(CustomerData outdata, GoodsType type, StructureBase structureBase)
        {
            goodsType = type; data = outdata;
            state = NpcState.QianWangGouMai;
            bornPosition = transform.position;
            salesStall = structureBase as SalesStall;
            SetNextPosition();
            agent.map = GameObject.FindWithTag("Map").transform.GetComponent<PolyNavMap>();
            agent.Stop(); ;
            agent.SetDestination(nextPosition);
            Vector2 dir = (nextPosition - (Vector2)transform.position).normalized;
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
            if (Vector2.Distance(new Vector2(transform.position.x, transform.position.y), bornPosition) < 1f && (state == NpcState.Angry || state == NpcState.JieZhangChengGong))
            {
                Destroy(gameObject);
            }
            if (nextPosition == (Vector2)((CashierCounter)GameController.Instance.buildings[BuildingType.LingZhangTai]).parchaseTransform.position
            && state == NpcState.QianWangShouYinTai
            && agent.remainingDistance <= 1f)
            {
                EventCenter.Instance.TriggerEvent(EventMessages.CustomerArrived, this);
            }
        }
        private Coroutine coroutine;
        public void WaitPurchase()
        {
            if (coroutine == null)
            {
                state = NpcState.WaitGouMaiWanCheng;
                coroutine = StartCoroutine(PurchaseRoutine());
            }
        }
        private IEnumerator PurchaseRoutine()
        {
            float timer = 0f;
             bool purchased = false; 
            while (timer < data.waitTime)
            { // 判断摊位商品是否满足顾客需求 
                if (salesStall.TryPurchase(data.carryNum, purchaseList))
                {
                    Purchase(); purchased = true;
                    EventCenter.Instance.TriggerEvent(EventMessages.CustomerLeave, salesStall);
                    break;
                }
                timer += Time.deltaTime;
                Debug.Log($"{name} 正在等待购买商品，已等待时间: {timer:F2}s");
                yield return null;
                if (!purchased)
                {
                    skeletonAnimation.AnimationState.SetAnimation(1, "angry", false);
                }
                coroutine = null;
            }
        }
        private void Purchase()
        {
            for (int i = 0; i < purchaseList.Count; i++) { var obj = purchaseList[i]; purchaseList[i].FlyTo(receiveTransform.position, () => { obj.transform.SetParent(transform, false); obj.transform.position = receiveTransform.position; }); }
            Debug.Log($"{name} 成功购买 {data.carryNum} 件商品");
            state = NpcState.QianWangShouYinTai;
            SetNextPosition();
            agent.Stop();
            agent.SetDestination(nextPosition);
        }
        private void OnPurchaseTimeout()
        {
            state = NpcState.Angry; SetNextPosition();
            agent.Stop();
            agent.SetDestination(nextPosition);
            var _state = skeletonAnimation.AnimationState;
            _state.ClearTrack(1);
            EventCenter.Instance.TriggerEvent(EventMessages.CustomerLeave, salesStall);
        }
        public void SetNextPosition()
        {
            if (state == NpcState.QianWangGouMai)
            {
                var pos = GameController.Instance.AddCustomerToQueue(salesStall, this);
                nextPosition = pos;
            }
            else if (state == NpcState.QianWangShouYinTai)
            {
                GameController.Instance.RemoveCustomerFromQueue(salesStall, this);
                var pos = GameController.Instance.AddCustomerToQueue((CashierCounter)GameController.Instance.buildings[BuildingType.LingZhangTai], this);
                nextPosition = pos;
            }
            else if (state is NpcState.JieZhangChengGong or NpcState.Angry)
            {
                GameController.Instance.RemoveCustomerFromQueue(salesStall, this);
                GameController.Instance.RemoveCustomerFromQueue((CashierCounter)GameController.Instance.buildings[BuildingType.LingZhangTai], this);
                nextPosition = bornPosition;
            }
        }
    }
}