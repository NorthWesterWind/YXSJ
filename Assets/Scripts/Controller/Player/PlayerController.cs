using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Controller.Pickups;
using Controller.Structure;
using Module;
using Module.Data;
using Sirenix.OdinInspector;
using Spine;
using Spine.Unity;
using UnityEngine;
using Utils;


namespace Controller.Player
{
    public class PlayerController : SerializedMonoBehaviour
    {
        private SkeletonAnimation _skeletonAnimation;
        private Vector2 _dirValue;
        public bool isMoving = false;
        public PlayerDataModule dataModule;
        public SpriteRenderer shadowRenderer;
        public SpriteRenderer weaponRenderer;
        public CinemachineVirtualCamera camera;
        public CinemachineVirtualCamera focusCamera;
        private Rigidbody2D _rigidbody;
        public GameObject weapon;

        public float detectRadius = 6f; // 怪物检测半径
        public LayerMask monsterLayer;   // 只检测怪物层
        public LayerMask productLayer;
        public LayerMask productStationLayer;


        public List<InteractionController> overlappingTrigger = new();
        public Transform receiveTransform;
        public Transform infoTransform;
        public PlayerInfo playerInfo;

        public Dictionary<GoodsType, int> goodsDic = new();
        public Dictionary<DropItemType, int> dropDic = new();

        public int currentCarryNum = 0;
        public float currentHp;
        public float maxHp;
        public float maxCarryNum;
        public float currentPinkUpRange;
        float velocity = 0f;
        public bool isDead = false;

        private AssetHandle _assetHandle;


        public int RemainCapacity => (int)maxCarryNum - currentCarryNum;

        /// <summary>
        /// 角色是否处于交互范围内（解锁）
        /// </summary>
        public bool InteractionTriggerInRange { get; set; } = false;
        public Transform InteractionTriggerTransform;

        public Transform weaponRoot;
        public float speed;
        [SerializeField] private float radiusX = 1.1f;   // 左右摆动距离
        [SerializeField] private float radiusZ = 0.55f;  // 前后景深（决定遮挡感）
        [SerializeField] private float minScale = 0.85f; // 在身后时的最小缩放
        [SerializeField] private float maxScale = 1.15f; // 在身前时的最大缩放s

        private void Awake()
        {
            if (dataModule == null)
            {
                dataModule = ModuleMgr.Instance.GetModule<PlayerDataModule>();
            }

            camera = GameObject.Find("Virtual Camera").GetComponent<CinemachineVirtualCamera>();
            camera.LookAt = transform;
            camera.Follow = transform;
            focusCamera = GameObject.Find("FocusVirtualCamera").GetComponent<CinemachineVirtualCamera>();
            _rigidbody = GetComponent<Rigidbody2D>();
            _assetHandle = GetComponent<AssetHandle>();
            _skeletonAnimation = transform.Find("Character").GetComponent<SkeletonAnimation>();

            var data = _skeletonAnimation.AnimationState.Data;

            data.SetMix("待机", "走路", 0.2f);
            data.SetMix("走路", "待机", 0.2f);

            data.SetMix("走路", "攻击", 0.1f);
            data.SetMix("待机", "攻击", 0.1f);

            data.SetMix("攻击", "待机", 0.15f);
            data.SetMix("攻击", "走路", 0.15f);
        }

        private MeshRenderer renderer;
        void Start()
        {
            AddEvent();
            Init();
            renderer = transform.Find("Character").GetComponent<MeshRenderer>();


        }


        private void AddEvent()
        {
            EventCenter.Instance.AddListener(EventMessages.TriggerDetection, HandleTrigger);
            EventCenter.Instance.AddListener(EventMessages.FocusView, HandleFocusView);
            EventCenter.Instance.AddListener(EventMessages.RestoreFocusView, RestoreFocusView);
            EventCenter.Instance.AddListener(EventMessages.PlayerTakeDamage, HandleTakeDamage);
            EventCenter.Instance.AddListener(EventMessages.FocusNewPosition, HandleFocusNew);
            EventCenter.Instance.AddListener(EventMessages.UpdatePlayerEquimentInfo, UpdatePlayerEquimentInfo);
        }
        private void OnDestroy()
        {
            EventCenter.Instance.RemoveListener(EventMessages.TriggerDetection, HandleTrigger);
            EventCenter.Instance.RemoveListener(EventMessages.FocusView, HandleFocusView);
            EventCenter.Instance.RemoveListener(EventMessages.RestoreFocusView, RestoreFocusView);
            EventCenter.Instance.RemoveListener(EventMessages.PlayerTakeDamage, HandleTakeDamage);
            EventCenter.Instance.RemoveListener(EventMessages.FocusNewPosition, HandleFocusNew);
            EventCenter.Instance.RemoveListener(EventMessages.UpdatePlayerEquimentInfo, UpdatePlayerEquimentInfo);
        }




        public void Init()
        {
            if (dataModule == null)
            {
                dataModule = ModuleMgr.Instance.GetModule<PlayerDataModule>();
            }
            currentCarryNum = 0;
            currentHp = dataModule.data.hp;
            maxCarryNum = dataModule.data.bagCapacity;
            currentPinkUpRange = dataModule.data.pickUpRange;
            EventCenter.Instance.TriggerEvent(EventMessages.UpdatePlayerMoneyInfo);

        }

        public void SetLayer()
        {
            int newOrder = 30000 - Mathf.RoundToInt(transform.position.y * 100);
            renderer.sortingOrder = newOrder;
            //weaponRenderer.sortingOrder = newOrder;
            shadowRenderer.sortingOrder = newOrder;
        }

        public bool isShowUI = false;

        void Update()
        {
            SetLayer();
            if (isShowUI)
            {
                return;
            }
            if (_dirValue != Vector2.zero)
            {
                isMoving = true;
                if (_dirValue.x < 0)
                {
                    _skeletonAnimation.transform.localScale = new Vector3(-0.6f,0.6f, 0.6f);
                }
                else
                {
                    _skeletonAnimation.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
                }

                // if (!Mathf.Approximately(camera.m_Lens.OrthographicSize, 12))
                // {
                //     camera.m_Lens.OrthographicSize =
                //         Mathf.SmoothDamp(camera.m_Lens.OrthographicSize, 12, ref velocity, 0.15f);
                // }


                var state = _skeletonAnimation.AnimationState;
                var current = state.GetCurrent(0);
                if (weapon.gameObject.activeSelf)
                {
                    if (current == null || current.Animation.Name != "攻击")
                    {
                        state.SetAnimation(0, "攻击", true);
                    }

                }
                else
                {
                    if (current == null || current.Animation.Name != "走路")
                    {
                        state.SetAnimation(0, "走路", true);
                    }
                }
            }
            else
            {
                isMoving = false;
                var state = _skeletonAnimation.AnimationState;
                var current = state.GetCurrent(0);

                if (weapon.gameObject.activeSelf)
                {
                    if (current == null || current.Animation.Name != "攻击腿不动")
                    {
                        state.SetAnimation(0, "攻击腿不动", true);
                    }
                }
                else
                {

                    if (current == null || current.Animation.Name != "待机")
                    {
                        state.SetAnimation(0, "待机", true);
                    }
                }

                // if (!Mathf.Approximately(camera.m_Lens.OrthographicSize,10))
                // {
                //     camera.m_Lens.OrthographicSize =
                //         Mathf.SmoothDamp(camera.m_Lens.OrthographicSize, 10, ref velocity, 0.3f);
                // }
            }

            CheckMonster();
            CheckDrop();
            CheckProductStation();
            CheckProduct();
            CheckSaleStall();
            CheckInteractionByDistance();

            if (ModuleMgr.Instance.GetModule<PlayerDataModule>().data.tongbi < 100)
            {
                if (InteractionTriggerInRange && !isMoving)
                {
                    if (coroutine == null)
                    {
                        coroutine = StartCoroutine(ThrowOutTongBi());
                    }
                }
                else
                {
                    if (coroutine != null)
                    {
                        StopCoroutine(coroutine);
                        coroutine = null;
                    }
                }
            }

            if (weapon.gameObject.activeSelf)
            {
                weaponRoot.Rotate(0f, 0f, -speed * Time.deltaTime);
                float z = weaponRoot.localEulerAngles.z;
                if (z > 180f) z -= 360f;
                weaponRenderer.sortingOrder = renderer.sortingOrder + 1;
                float t = Mathf.Abs(Mathf.Cos(z * Mathf.Deg2Rad));
                float scale = Mathf.Lerp(0.85f, 1.1f, t);
                weaponRoot.localScale = Vector3.one * scale;

            }


        }
        private Coroutine coroutine;
        private IEnumerator ThrowOutTongBi()
        {

            GameObject coinObj = _assetHandle.Get<GameObject>("Production");
            coinObj.transform.position = receiveTransform.position;
            var coinCtrl = coinObj.GetComponent<Production>();
            coinCtrl.Init(GoodsType.TongBi);
            ModuleMgr.Instance.GetModule<PlayerDataModule>().data.tongbi -= 100;
            coinCtrl.FlyTo(
               InteractionTriggerTransform.position,
                () =>
                {
                    EventCenter.Instance.TriggerEvent(EventMessages.ThrowOutTongBi, InteractionTriggerTransform);
                    Destroy(coinObj);
                }
            );
            yield return new WaitForSeconds(0.5f);
        }



        // public void ThrowOutTongBi()
        // {
        //     GameObject coinObj = ObjectPoolManager.Instance.GetObject("TongBi");
        //     var coinCtrl = coinObj.GetComponent<DropController>();
        // }

        private void FixedUpdate()
        {
            if (isMoving)
            {
                _rigidbody.MovePosition(_rigidbody.position +
                                        new Vector2(_dirValue.x, _dirValue.y) * (dataModule.data.moveSpeed * Time.fixedDeltaTime));
            }
        }

        private void UpdatePlayerEquimentInfo(params object[] args)
        {
            if (DataController.Instance.weaponDataDic.ContainsKey(dataModule.data.currentWeapon))
            {
                WeaponData weaponData = DataController.Instance.weaponDataDic[dataModule.data.currentWeapon];
                weaponRenderer.sprite = _assetHandle.Get<Sprite>(weaponData.name);
                _skeletonAnimation.skeleton.SetAttachment(weaponData.slotName, weaponData.attachmentName);
            }
            if (DataController.Instance.storageBagDataDic.ContainsKey(dataModule.data.currentBag))
            {
                StotageBagData stotageBagData = DataController.Instance.storageBagDataDic[dataModule.data.currentBag];
                _skeletonAnimation.skeleton.SetAttachment(stotageBagData.slotName, stotageBagData.attachmentName);
            }


        }

        private void HandleFocusView(params object[] args)
        {
            isShowUI = true;
            // if (!Mathf.Approximately(camera.m_Lens.OrthographicSize, 10))
            // {
            //     camera.m_Lens.OrthographicSize =
            //         Mathf.SmoothDamp(camera.m_Lens.OrthographicSize, 10, ref velocity, 0.3f);
            // }
        }

        private void RestoreFocusView(params object[] args)
        {
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
                currentHp = Mathf.Min(currentHp, dataModule.data.hp);
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
            var list = ScenePickupController.Instance.materials.ToArray();

            foreach (var item in list)
            {
                if (item == null) continue;                       // 回收后 item 可能被销毁
                if (!item.gameObject.activeInHierarchy) continue; // 避免 inactive
                if (item.isTaken) continue;
                float dist = Vector2.Distance(transform.position, item.transform.position);
                if (dist <= currentPinkUpRange && currentCarryNum < maxCarryNum)
                {
                    item.StartAttract(this.transform, receiveTransform);
                }
            }
        }

        public void CheckProduct()
        {
            var list = ScenePickupController.Instance.products.ToArray();
            foreach (var item in list)
            {
                if (item == null) continue;
                if (!item.gameObject.activeInHierarchy) continue;

                float dist = Vector2.Distance(transform.position, item.transform.position);
                if (dist <= currentPinkUpRange && item.GetComponent<Production>().station is CashierCounter)
                {
                    ((CashierCounter)(item.GetComponent<Production>().station)).grid.ReleaseOne();
                    item.StartAttract(this.transform, receiveTransform);
                }
                else if (dist <= currentPinkUpRange && currentCarryNum < maxCarryNum)
                {
                    if (item.GetComponent<Production>().state != ItemState.OnWorkbench)
                        continue;
                    ((ProductionStation)(item.GetComponent<Production>().station)).grid.ReleaseOne();
                    ((ProductionStation)(item.GetComponent<Production>().station)).productionList.Remove(
                        item.GetComponent<Production>());
                    item.StartAttract(this.transform, receiveTransform);
                }

            }
        }

        //怪物检测函数
        public void CheckMonster()
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectRadius, monsterLayer);

            if (hits.Length > 0)
            {
                weapon.gameObject.SetActive(true);
            }
            else

            {

                UpdatePlayerEquimentInfo();
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
        private Dictionary<string, Coroutine> deliverCoroutines = new();

        public void CheckProductStation()
        {
            if (isMoving) return;
            foreach (var data in GameController.Instance.buildings)
            {
                if (data.Key == BuildingType.YuShaHu_1 || data.Key == BuildingType.LianQiLu_1 ||
                    data.Key == BuildingType.YuShaHu_2 || data.Key == BuildingType.YuShaHu_3 ||
                    data.Key == BuildingType.YuShaHu_4 || data.Key == BuildingType.LianQiLu_2 ||
                    data.Key == BuildingType.LianQiLu_3)
                {
                    if ((data.Value.gameObject.transform.position - transform.position).sqrMagnitude < 8)
                    {
                        var station = data.Value as ProductionStation;
                        if (station == null) continue;

                        if (!dropDic.ContainsKey(station.dropItemType)) continue;
                        if (dropDic[station.dropItemType] <= 0) continue;

                        if (!deliverCoroutines.ContainsKey("ProductStation"))
                        {
                            deliverCoroutines["ProductStation"] = StartCoroutine(DeliverMaterial(station));
                        }
                    }
                }
            }
        }

        public void CheckSaleStall()
        {
            if (isMoving) return;
            foreach (var data in GameController.Instance.goodBuild)
            {
                if ((data.Value.gameObject.transform.position - transform.position).sqrMagnitude < 8)
                {
                    var station = data.Value as SalesStall;
                    if (station == null) continue;

                    if (!goodsDic.ContainsKey(station.currentGoodsType)) continue;
                    if (goodsDic[station.currentGoodsType] <= 0) continue;

                    // 确保协程不会并发重复启动
                    if (!deliverCoroutines.ContainsKey("SaleStall"))
                    {
                        deliverCoroutines["SaleStall"] = StartCoroutine(DeliverProduct(station));
                    }
                }
            }
        }

        private IEnumerator DeliverMaterial(ProductionStation station)
        {
            int count = dropDic[station.dropItemType];
            for (int i = 0; i < count; i++)
            {
                if (isMoving)
                {
                    deliverCoroutines["ProductStation"] = null;
                    break;
                }

                GameObject drop = ObjectPoolManager.Instance.GetObject("DropObj");
                var dropCtrl = drop.GetComponent<DropController>();
                dropCtrl.canPickup = false;
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

            deliverCoroutines["ProductStation"] = null;
            deliverCoroutines.Remove("ProductStation");
        }

        private IEnumerator DeliverProduct(SalesStall station)
        {
            int count = goodsDic[station.currentGoodsType];
            for (int i = 0; i < count; i++)
            {
                if (isMoving)
                {
                    deliverCoroutines["SaleStall"] = null;
                    break;
                }

                GameObject drop = ObjectPoolManager.Instance.GetObject("Production");
                var dropCtrl = drop.GetComponent<Production>();
                dropCtrl.canPickup = false;
                dropCtrl.Init(station.currentGoodsType);
                dropCtrl.SetStation(station);
                Vector2 start = receiveTransform.position;
                dropCtrl.spriteRenderer.sortingOrder = station.grid.currentIndex + 4000;
                Vector2 target = station.grid.GetNextPosition();
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

                station.AddGoods(dropCtrl);
                // 递减材料计数
                goodsDic[station.currentGoodsType]--;
                currentCarryNum--;
                playerInfo.UpdateTxt();

                yield return new WaitForSeconds(0.05f);
            }

            deliverCoroutines["SaleStall"] = null;
            deliverCoroutines.Remove("SaleStall");
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

        [SerializeField] private float detectRadiu = 1.5f;
        [SerializeField] private LayerMask interactionLayer;

        private readonly List<InteractionController> overlappTrigger = new();
        private readonly List<InteractionTrigger> overlappingTrigger2 = new();

        private void CheckInteractionByDistance()
        {
            Vector2 center = transform.position;

            // 找范围内的 Collider
            Collider2D[] hits = Physics2D.OverlapCircleAll(
                center,
                detectRadius,
                interactionLayer
            );

            HashSet<InteractionController> currentControllers = new();
            HashSet<InteractionTrigger> currentTriggers = new();

            foreach (var hit in hits)
            {
                // InteractionController
                var controller = hit.GetComponent<InteractionController>();
                if (controller != null)
                {
                    currentControllers.Add(controller);

                    // 进入
                    if (!overlappTrigger.Contains(controller))
                    {
                        overlappTrigger.Add(controller);

                        if (controller.interactionType == InteractionType.Immediate)
                        {
                            controller.Interact();
                        }
                    }
                }

                // InteractionTrigger
                var trigger = hit.GetComponent<InteractionTrigger>();
                if (trigger != null)
                {
                    currentTriggers.Add(trigger);

                    if (!overlappingTrigger2.Contains(trigger))
                    {
                        overlappingTrigger2.Add(trigger);
                        trigger.TriggerEnter();
                    }
                }
            }

            // 处理离开（InteractionController）
            for (int i = overlappTrigger.Count - 1; i >= 0; i--)
            {
                var item = overlappTrigger[i];
                if (!currentControllers.Contains(item))
                {
                    item.CloseInteract();
                    overlappTrigger.RemoveAt(i);
                }
            }

            // 处理离开（InteractionTrigger）
            for (int i = overlappingTrigger2.Count - 1; i >= 0; i--)
            {
                var item = overlappingTrigger2[i];
                if (!currentTriggers.Contains(item))
                {
                    item.TriggerExit();
                    overlappingTrigger2.RemoveAt(i);
                }
            }
        }


        // private void OnTriggerEnter2D(Collider2D other)
        // {
        //     var trigger = other.GetComponent<InteractionController>();
        //     var trigger2 = other.GetComponent<InteractionTrigger>();
        //     if (trigger != null)
        //     {
        //         overlappingTrigger.Add(trigger);
        //         if (trigger.interactionType == InteractionType.Immediate)
        //         {
        //             trigger.Interact();
        //         }
        //     }

        //     if (trigger2 != null)
        //     {
        //         trigger2.TriggerEnter();
        //     }
        // }

        // private void OnTriggerStay2D(Collider2D other)
        // {
        //     Debug.Log("持续重叠：" + other.name);
        //     if (other.GetComponent<InteractionTrigger>())
        //     {
        //     }
        // }

        // private void OnTriggerExit2D(Collider2D other)
        // {
        //     var trigger = other.GetComponent<InteractionController>();
        //     var trigger2 = other.GetComponent<InteractionTrigger>();
        //     if (trigger != null)
        //     {
        //         trigger.CloseInteract();
        //         overlappingTrigger.Remove(trigger);
        //     }

        //     if (trigger2 != null)
        //     {
        //         trigger2.TriggerExit();
        //     }
        // }

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
                    // case DropItemType.JingYunBao:
                    //     dataModule.AddJinYuanBao(10);
                    //     break;
                    // case DropItemType.YingQian:
                    //     dataModule.AddYinQian(100);
                    //     break;
            }

            playerInfo.UpdateTxt();
        }

        public void AddGoods(GoodsType goodsType)
        {
            if (goodsType == GoodsType.TongBi)
            {
                dataModule.AddYinQian(100);
                EventCenter.Instance.TriggerEvent(EventMessages.UpdatePlayerMoneyInfo);
            }
            else if (goodsType == GoodsType.JingYunBao)
            {
                dataModule.AddJinYuanBao(10);
                EventCenter.Instance.TriggerEvent(EventMessages.UpdatePlayerMoneyInfo);
            }
            else
            {
                currentCarryNum++;
                goodsDic.TryAdd(goodsType, 0);
                goodsDic[goodsType]++;
                playerInfo.UpdateTxt();
            }
        }


        public void HandleTakeDamage(params object[] args)
        {
            float value = (float)args[0];
            TakeDamage(value);
        }

        public void HandleFocusNew(params object[] args)
        {
            Transform t = (Transform)args[0];
            EventCenter.Instance.TriggerEvent(EventMessages.FocusView); // 禁止输入
            focusCamera.LookAt = t;
            focusCamera.Priority = 20; // > PlayerCam 的 10
            StartCoroutine(ReturnAfterDelay(3f));
        }
        IEnumerator ReturnAfterDelay(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            focusCamera.LookAt = transform;
            focusCamera.Priority = 5;                                             // 恢复为低
            EventCenter.Instance.TriggerEvent(EventMessages.RestoreFocusView); // 恢复输入
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

    }
}