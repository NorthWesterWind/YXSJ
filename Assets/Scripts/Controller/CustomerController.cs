using Module.Data;
using PolyNav;
using UnityEngine;
using View._3D;

namespace Controller
{
    public enum NpcState
    {
        None,
        Move,
        Wait,
        Angry
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
       public Transform[] positions;
       public int currentIndex = 0;
       public Animator animator;
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
            CheckProduction();
        }

        void TryPick(Production p)
        {
            if (p.CanCustomerPick)
            {
                p.SetState(ItemState.HeldByCustomer);
            }
        }
       
        public LayerMask productLayer;
        public void CheckProduction()
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 3,productLayer);
            if (hits.Length > 0)
            {
                foreach (Collider2D item in hits)
                {
                    if (item.GetComponent<Production>() != null)
                    {
                        TryPick(item.GetComponent<Production>());
                    }
                }
            }
        }
        
        
        private void FixedUpdate()
        {
           
        }
        public void SetLayer()
        {
            int newOrder = 3000 - Mathf.FloorToInt(transform.localPosition.y);
            spriteRenderer.sortingOrder = newOrder;
        }

        public void Init(CustomerData outdata ,Transform[] arr )
        {
          
            data = outdata;
            state = NpcState.Move;
            bornPosition = transform.position;
            positions = arr;
            nextPosition = positions[currentIndex].position;
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
            
            currentIndex++;
            if(currentIndex >= positions.Length)
                nextPosition = bornPosition;
            else
            {
                nextPosition = positions[currentIndex].position;
            }
            Vector2 dir = (nextPosition - (Vector2)transform.position).normalized;
            transform.localScale = new Vector3( dir.x < 0 ? -1 : 1, 1, 1);
            agent.SetDestination(nextPosition);
            
        }
    }
}
