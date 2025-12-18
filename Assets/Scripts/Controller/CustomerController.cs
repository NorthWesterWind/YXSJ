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
    {
        None,
        QianWangGouMai,
        WaitGouMaiWanCheng,
        QianWangShouYinTai,
        JieZhangChengGong,
        Angry,
    }
    public class CustomerController : MonoBehaviour
    {
       public PolyNavAgent  agent;
       public CustomerData data;
       public NpcState state;
       public Vector2 bornPosition;
       public Vector2 nextPosition;
       private Rigidbody2D _rigidbody2D;
       public GoodsType goodsType;
       public SkeletonAnimation  skeletonAnimation;
       public SpriteRenderer shadow;
       public int currentIndex = 0;
       public SalesStall salesStall;
       public Transform receiveTransform;
       private List<Production> productionList = new ();
       public List<Production> purchaseList = new();
       
       private MeshRenderer _meshRenderer;
        void Start()
        {
            skeletonAnimation.state.Complete += HandleAnimationComplete;
        }
        private void HandleAnimationComplete(TrackEntry trackEntry)
        {
            if (trackEntry.TrackIndex == 1 && trackEntry.Animation.Name == "angry")
            {
                // 超时逻辑
                OnPurchaseTimeout();
            }
        }
        
        void Update()
        {
            SetLayer();
            // 检查当前动画槽是否为 null
            var currentAnimation = skeletonAnimation.AnimationState.GetCurrent(0);
            // 根据条件切换动画
            if (agent.hasPath || agent.remainingDistance > 1 )
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
        }

        public void UpdateQueueTarget(Vector2 pos)
        {
            agent.SetDestination(pos);
        }
        public void SetLayer()
        {
            int newOrder = 3000 - Mathf.FloorToInt(transform.localPosition.y);
            if (_meshRenderer == null)
            {
                _meshRenderer =skeletonAnimation.GetComponent<MeshRenderer>();
            }
            
            _meshRenderer.sortingOrder = newOrder;
            shadow.sortingOrder = newOrder;
        }

        public void Init(CustomerData outdata  , GoodsType type , StructureBase structureBase )
        {
            goodsType = type;
            data = outdata;
            state = NpcState.QianWangGouMai;
            bornPosition = transform.position;
            salesStall = structureBase as SalesStall;
            SetNextPosition();
            agent.map = GameObject.FindWithTag("Map").transform.GetComponent<PolyNavMap>();
            agent.SetDestination(nextPosition);
            Vector2 dir = (nextPosition - (Vector2)transform.position).normalized;
            transform.localScale = new Vector3( dir.x < 0 ? -1 : 1, 1, 1);
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
            if (nextPosition == bornPosition)
            {
                Destroy(gameObject);
            }

            if (nextPosition == (Vector2)salesStall.parchaseTransform.position)
            {
                WaitPurchase();
            }

            if (nextPosition == (Vector2)((CashierCounter)GameController.Instance.buildings[BuildingType.LingZhangTai])
                .parchaseTransform.position && state == NpcState.QianWangShouYinTai && agent.remainingDistance <= 0.05f)
            {
                //执行结账逻辑
                EventCenter.Instance.TriggerEvent(EventMessages.CustomerArrived , this);
            }
        }

        
        public void WaitPurchase()
        {
            state = NpcState.WaitGouMaiWanCheng;
            StartCoroutine(PurchaseRoutine());
        }
        
        
        private IEnumerator PurchaseRoutine()
        {
            float timer = 0f;
            bool purchased = false;

            while (timer < data.waitTime)
            {
                // 判断摊位商品是否满足顾客需求
                if (salesStall.TryPurchase(data.carryNum , purchaseList))
                {
                    // 执行购买
                    Purchase();
                    purchased = true;
                    break;
                }

                timer += Time.deltaTime;
                yield return null; // 等待下一帧
            }

            if (!purchased)
            {
                skeletonAnimation.AnimationState.SetAnimation(1, "angry", false);
            }
        }
        private void Purchase()
        {
            // 减少摊位商品数量
            for (int i = 0; i < purchaseList.Count; i++)
            {
                var obj = purchaseList[i];
               purchaseList[i].FlyTo(receiveTransform.position, () =>
               {
                   obj.transform.SetParent(transform,false);
                   obj.transform.position = receiveTransform.position;
               });
            }
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
        }

        public  void SetNextPosition()
        {
            if (state == NpcState.QianWangGouMai)
            {
                var pos =   GameController.Instance.AddCustomerToQueue(salesStall, this);
                nextPosition = pos;
            }else if (state == NpcState.QianWangShouYinTai)
            {
                GameController.Instance.RemoveCustomerFromQueue( salesStall, this);
                var pos =   GameController.Instance.AddCustomerToQueue((CashierCounter)GameController.Instance.buildings[BuildingType.LingZhangTai], this);
                nextPosition = pos;
            }else if (state is NpcState.JieZhangChengGong or NpcState.Angry)
            {
                GameController.Instance.RemoveCustomerFromQueue( salesStall, this);
                GameController.Instance.RemoveCustomerFromQueue( (CashierCounter)GameController.Instance.buildings[BuildingType.LingZhangTai], this);
                nextPosition = bornPosition;
            }
        }
    }
}
