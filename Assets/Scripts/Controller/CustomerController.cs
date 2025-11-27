using System.Collections;
using System.Collections.Generic;
using Controller.Pickups;
using Controller.Structure;
using Module.Data;
using PolyNav;
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
       public SpriteRenderer spriteRenderer;
       // public Transform[] positions;
       public int currentIndex = 0;
       public Animator animator;
       public SalesStall salesStall;
       public Transform receiveTransform;
       private List<Production> productionList = new ();
       public List<Production> purchaseList = new();
        void Start()
        {
           
        }
        
        void Update()
        {
            if (agent.hasPath)
            {
                animator.SetBool("move" ,true);
                animator.SetBool("idle",false);
              
            }
            else
            {
                animator.SetBool("move",false);
                animator.SetBool("idle",true);
            }
            SetLayer();
        }

        
        public void SetLayer()
        {
            int newOrder = 3000 - Mathf.FloorToInt(transform.localPosition.y);
            spriteRenderer.sortingOrder = newOrder;
        }

        public void Init(CustomerData outdata  , GoodsType type , StructureBase structureBase )
        {
            goodsType = type;
            data = outdata;
            state = NpcState.QianWangGouMai;
            bornPosition = transform.position;
            salesStall = structureBase as SalesStall;
            SetNextPosition();
            agent.map = GameObject.Find("Map").transform.GetComponent<PolyNavMap>();
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
                // 超时逻辑
                OnPurchaseTimeout();
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
            Debug.Log($"{name} 等待超时，未能购买商品");
           
        }

        public  void SetNextPosition()
        {
            if (state == NpcState.QianWangGouMai)
            {
                nextPosition = salesStall.parchaseTransform.position;
            }else if (state == NpcState.QianWangShouYinTai)
            {
                nextPosition = ((CashierCounter)GameController.Instance.buildings[BuildingType.LingZhangTai]).parchaseTransform.position;
            }else if (state is NpcState.JieZhangChengGong or NpcState.Angry)
            {
                nextPosition = bornPosition;
            }
        }
    }
}
