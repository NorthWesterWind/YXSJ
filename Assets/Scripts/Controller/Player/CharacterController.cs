using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Controller.Pickups;
using Controller.Structure;
using Module;
using Module.Data;
using UnityEngine;
using Utils;
using View._3D;

namespace Controller.Player
{
    public class CharacterController : MonoBehaviour
    {
        private Animator _animator;
        private Vector2 _dirValue;
        public bool isMoving = false;
        private PlayerDataModule dataModule;
        public SpriteRenderer spriteRenderer;
        public SpriteRenderer shadowRenderer;
        public SpriteRenderer weaponRenderer;
        public CinemachineVirtualCamera camera;
        private Rigidbody2D _rigidbody;
        public GameObject weapon;

        public float detectRadius = 10f; // 怪物检测半径
        public LayerMask monsterLayer;   // 只检测怪物层
        public LayerMask productLayer;  
        public LayerMask productStationLayer;  
       

        public List<InteractionController> overlappingTrigger = new();
        public Transform receiveTransform;
        public Transform infoTransform;
        public PlayerInfo playerInfo;
        
        public Dictionary<GoodsType , int> goodsDic = new ();
        public Dictionary<DropItemType, int> dropDic = new ();
        
        public int currentCarryNum = 0;
        public float currentHp;
        public float maxHp;
        public int maxCarryNum;
        public float currentPinkUpRange;
        float velocity = 0f;
        public  bool isDead = false;

        private AssetHandle _assetHandle;
        private void Awake()
        {
            if (_animator == null)
            {
                _animator = transform.Find("Character").GetComponent<Animator>();
            }

            if (dataModule == null)
            {
                dataModule = ModuleMgr.Instance.GetModule<PlayerDataModule>();
            }

            camera = GameObject.Find("Virtual Camera").GetComponent<CinemachineVirtualCamera>();
            _rigidbody = GetComponent<Rigidbody2D>();
            _assetHandle = GetComponent<AssetHandle>();
        }

        void Start()
        {
            AddEvent();
            Init();
        }


        private void AddEvent()
        {
            EventCenter.Instance.AddListener(EventMessages.TriggerDetection, HandleTrigger);
            EventCenter.Instance.AddListener(EventMessages.FocusView, HandleFocusView);
            EventCenter.Instance.AddListener(EventMessages.RestoreFocusView, RestoreFocusView);
            EventCenter.Instance.AddListener(EventMessages.PlayerTakeDamage,HandleTakeDamage);
        }

        public void Init()
        {
            currentCarryNum = 0;
            currentHp = dataModule.data.hp;
            maxCarryNum =  dataModule.data.bagCapacity;
            currentPinkUpRange = dataModule.data.pickUpRange;
            EventCenter.Instance.TriggerEvent(EventMessages.UpdatePlayerMoneyInfo);
            ObjectPoolManager.Instance.WarmPool("DropObj",_assetHandle.Get<GameObject>("DropObj") , 30);
        }

        public void SetLayer()
        {
            int newOrder = 3000 - Mathf.FloorToInt(transform.localPosition.y);
            spriteRenderer.sortingOrder = newOrder;
            weaponRenderer.sortingOrder = newOrder;
            shadowRenderer.sortingOrder = newOrder;
        }

        public bool isShowUI = false;

        void Update()
        {
            if (isShowUI)
                return;
            if (_dirValue != Vector2.zero)
            {
                isMoving = true;

                _animator.SetBool("move", true);
                _animator.SetBool("idle", false);
                if (_dirValue.x < 0)
                {
                    transform.localScale = new Vector3(-1, 1, 1);
                }
                else
                {
                    transform.localScale = new Vector3(1, 1, 1);
                }

                if (!Mathf.Approximately(camera.m_Lens.OrthographicSize, 20))
                {
                    camera.m_Lens.OrthographicSize =
                        Mathf.SmoothDamp(camera.m_Lens.OrthographicSize, 20, ref velocity, 0.15f);
                }

                SetLayer();
            }
            else
            {
                isMoving = false;
                _animator.SetBool("move", false);
                _animator.SetBool("idle", true);
                if (!Mathf.Approximately(camera.m_Lens.OrthographicSize, 18))
                {
                    camera.m_Lens.OrthographicSize =
                        Mathf.SmoothDamp(camera.m_Lens.OrthographicSize, 18, ref velocity, 0.3f);
                }
            }

            CheckMonster();
            CheckDrop();
            CheckProduction();
            CheckProductStation();
        }

        private void FixedUpdate()
        {
            if (isMoving)
            {
                _rigidbody.MovePosition(_rigidbody.position +
                                        new Vector2(_dirValue.x, _dirValue.y) * 10 * Time.fixedDeltaTime);
            }
        }

        private void HandleFocusView(params object[] args)
        {
            isShowUI = true;
            Debug.Log("yj ==>  Focus View");
            if (!Mathf.Approximately(camera.m_Lens.OrthographicSize, 13))
            {
                camera.m_Lens.OrthographicSize =
                    Mathf.SmoothDamp(camera.m_Lens.OrthographicSize, 13, ref velocity, 0.3f);
            }
        }

        private void RestoreFocusView(params object[] args)
        {
            Debug.Log("yj ==> Restore Focus View");
            isShowUI = false;
        }

        
        
        public void SetDir(Vector2 direction)
        {
            _dirValue = direction;
        }

        //玩家生命值回复
        private float lastDamageTime = -999f; // 上次受伤时间
        private bool isRegenerating = false;
        private Coroutine regenCoroutine;
        private float regenDelay = 3f; 
        private IEnumerator RegenerateHealth()
        {
            isRegenerating = true;

            while (currentHp < dataModule.data.hp)
            {
                currentHp += dataModule.data.hpRecover * Time.deltaTime;
                currentHp = Mathf.Min(currentHp,dataModule.data.hp);
                playerInfo.UpdateFill(currentHp / dataModule.data.hp);
                yield return null;

                if (Time.time - lastDamageTime < regenDelay)
                {
                    isRegenerating = false;
                    yield break;
                }
            }

            playerInfo.HideHpInfo();
            isRegenerating = false;
        }


        public void CheckDrop()
        {
            foreach (var item in ScenePickupController.Instance.pickups)
            {
                float dist = Vector2.Distance(transform.position, item.transform.position);
                if (dist <= currentPinkUpRange && currentCarryNum < maxCarryNum )
                {
                    item.StartAttract(this.transform, receiveTransform);
                }
            }
        }
        
        public bool TryPick(Production p)
        {
            if (p.CanPlayerPick)
            {
                p.SetState(ItemState.HeldByPlayer);
                return true;
            }
            return false;
        }
        
        //怪物检测函数
        public void CheckMonster()
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectRadius, monsterLayer);
            Debug.Log($" hits.Length => {hits.Length}");
            if (hits.Length > 0)
            {
                weapon.gameObject.SetActive(true);
            }
            else
            {
                weapon.gameObject.SetActive(false);
                // 自动回血检测
                if (currentHp < maxHp && !isRegenerating)
                {
                    if (Time.time - lastDamageTime >= regenDelay)
                    {
                        regenCoroutine = StartCoroutine(RegenerateHealth());
                    }
                }
            }
        }
        
        
        public AnimationCurve scatterCurve;
        private float scatterDuration = 0.1f;
        private Coroutine deliverCoroutine;

        public void CheckProductStation()
        {
            if (isMoving) return;

            Collider2D hit = Physics2D.OverlapCircle(transform.position, 5, productStationLayer);
            if (hit == null) return;

            var station = hit.GetComponent<ProductionStation>();
            if (station == null) return;

            if (!dropDic.ContainsKey(station.dropItemType)) return;
            if (dropDic[station.dropItemType] <= 0) return;

            // 确保协程不会并发重复启动
            if (deliverCoroutine == null)
            {
                deliverCoroutine = StartCoroutine(DeliverMaterial(station));
            }
        }
        private IEnumerator DeliverMaterial(ProductionStation station)
        {
            int count = dropDic[station.dropItemType];

            for (int i = 0; i < count; i++)
            {
                if (isMoving)
                {
                    deliverCoroutine = null;
                    break;
                } 

                GameObject drop = ObjectPoolManager.Instance.GetObject("DropObj");
                var dropCtrl = drop.GetComponent<DropController>();

                dropCtrl.isAttracted = false;
                dropCtrl.Init(station.dropItemType);

                Vector2 start = receiveTransform.position;
                Vector2 target = station.recivePosition.position;
                Vector2 control = Vector2.Lerp(start, target, 0.5f) + Vector2.up * 1.5f;

                float timer = 0f;

                while (timer < scatterDuration)
                {
                    if (isMoving) break;

                    float t = scatterCurve.Evaluate(timer / scatterDuration);
                    Vector2 pos = (1 - t) * (1 - t) * start +
                                  2 * (1 - t) * t * control +
                                  t * t * target;

                    drop.transform.position = pos;
                    timer += Time.deltaTime;
                    yield return null;
                }

                drop.transform.position = target;

                station.AddMaterial(1);
                ObjectPoolManager.Instance.ReturnObject("DropObj", drop);

                // 递减材料计数
                dropDic[station.dropItemType]--;
                currentCarryNum--;
                playerInfo.UpdateTxt();

                yield return new WaitForSeconds(0.05f); // 多个材料间隔
            }

            deliverCoroutine = null;
        }

        
        public void CheckProduction()
        {
           
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 5, productLayer);
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
        


        private void HandleTrigger(params object[] args)
        {
            foreach (var trigger in overlappingTrigger)
            {
                if (trigger != null && trigger.interactionType == InteractionType.OnStop)
                {
                    trigger.Interact();
                }
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var trigger = other.GetComponent<InteractionController>();
            var trigger2 = other.GetComponent<InteractionTrigger>();
            if (trigger != null)
            {
                overlappingTrigger.Add(trigger);
                if (trigger.interactionType == InteractionType.Immediate)
                {
                    trigger.Interact();
                }
            }

            if (trigger2 != null)
            {
                trigger2.TriggerEnter();
            }
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            Debug.Log("持续重叠：" + other.name);
            if (other.GetComponent<InteractionTrigger>())
            {
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            var trigger = other.GetComponent<InteractionController>();
            var trigger2 = other.GetComponent<InteractionTrigger>();
            if (trigger != null)
            {
                overlappingTrigger.Remove(trigger);
            }

            if (trigger2 != null)
            {
                trigger2.TriggerExit();
            }
        }

        public void AddDropItem(DropItemType itemType)
        {
            switch (itemType)
            {
                case DropItemType.ShuangYunZhiFragment:
                    currentCarryNum += 1;
                    if (!dropDic.TryAdd(itemType, 1))
                    {
                        dropDic[itemType]++;
                    }

                    break;
                case DropItemType.YueLuCaoFragment:
                    currentCarryNum += 1;
                    if (!dropDic.TryAdd(itemType, 1))
                    {
                        dropDic[itemType]++;
                    }
                    break;
                case DropItemType.ZiXinHuaFragment:
                    currentCarryNum += 1;
                    if (!dropDic.TryAdd(itemType, 1))
                    {
                        dropDic[itemType]++;
                    }
                    break;
                case DropItemType.YuHuiHeFragment:
                    currentCarryNum += 1;
                    if (!dropDic.TryAdd(itemType, 1))
                    {
                        dropDic[itemType]++;
                    }
                    break;
                case DropItemType.XingWenGuoFragment:
                    currentCarryNum += 1;
                    if (!dropDic.TryAdd(itemType, 1))
                    {
                        dropDic[itemType]++;
                    }
                    break;
                case DropItemType.WuRongJunFragment:
                    currentCarryNum += 1;
                    if (!dropDic.TryAdd(itemType, 1))
                    {
                        dropDic[itemType]++;
                    }
                    break;
                case DropItemType.LingXuShengFragment:
                    currentCarryNum += 1;
                    if (!dropDic.TryAdd(itemType, 1))
                    {
                        dropDic[itemType]++;
                    }
                    break;
                case DropItemType.XueBanHuaFragment:
                    currentCarryNum += 1;
                    if (!dropDic.TryAdd(itemType, 1))
                    {
                        dropDic[itemType]++;
                    }
                    break;
                case DropItemType.MuLingYaFragment:
                    currentCarryNum += 1;
                    if (!dropDic.TryAdd(itemType, 1))
                    {
                        dropDic[itemType]++;
                    }
                    break;
                case DropItemType.JingRuiCaoFragment:
                    currentCarryNum += 1;
                    if (!dropDic.TryAdd(itemType, 1))
                    {
                        dropDic[itemType]++;
                    }
                    break;
                
                case DropItemType.TieKuangShiFragment:
                    currentCarryNum += 1;
                    if (!dropDic.TryAdd(itemType, 1))
                    {
                        dropDic[itemType]++;
                    }
                    break;
                case DropItemType.YinKuangShiFragment:
                    currentCarryNum += 1;
                    if (!dropDic.TryAdd(itemType, 1))
                    {
                        dropDic[itemType]++;
                    }
                    break;
                case DropItemType.TongKuangShiFragment:
                    currentCarryNum += 1;
                    if (!dropDic.TryAdd(itemType, 1))
                    {
                        dropDic[itemType]++;
                    }
                    break;
                case DropItemType.ZiJingShiFragment:
                    currentCarryNum += 1;
                    if (!dropDic.TryAdd(itemType, 1))
                    {
                        dropDic[itemType]++;
                    }
                    break;
                case DropItemType.YueJingShiFragment:
                    currentCarryNum += 1;
                    if (!dropDic.TryAdd(itemType, 1))
                    {
                        dropDic[itemType]++;
                    }
                    break;
                case DropItemType.JingYunBao:
                    dataModule.AddJinYuanBao(10);
                    break;
                case DropItemType.YingQian:
                    dataModule.AddYinQian(100);
                    break;
                
            }
            playerInfo.UpdateTxt();
                 
        }
        
        
        

        public void HandleTakeDamage(params object[] args)
        {
            float value = (float)args[0];
            TakeDamage(value);
        }
        
        private bool invincible = false;
        private float invincibleTime = 0.2f;

        public void TakeDamage(float damage)
        {
            if (isDead) return;
            if (invincible) return;

            StartCoroutine(InvincibleFrame());

            currentHp -= damage;
            if (currentHp <= 0)
            {
                DoDie();
            }
        }

        public void DoDie()
        {
            
        }
        private IEnumerator InvincibleFrame()
        {
            invincible = true;
            yield return new WaitForSeconds(invincibleTime);
            invincible = false;
        }

        private void OnDestroy()
        {
            EventCenter.Instance.RemoveListener(EventMessages.TriggerDetection, HandleTrigger);
            EventCenter.Instance.RemoveListener(EventMessages.FocusView, HandleFocusView);
            EventCenter.Instance.RemoveListener(EventMessages.RestoreFocusView, RestoreFocusView);
            EventCenter.Instance.RemoveListener(EventMessages.PlayerTakeDamage,HandleTakeDamage);
        }
    }
}