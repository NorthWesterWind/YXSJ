using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Controller.Pickups;
using Controller.Structure;
using Module;
using Module.Data;
using Sirenix.OdinInspector;
using Spine.Unity;
using UnityEngine;
using Utils;


namespace Controller.Player
{
    public class PlayerController : SerializedMonoBehaviour
    {
        public int currentCarryNum;
        public float currentHp;
        public float currentMoveSpeed;
        public float currentPinkUpRange;
        public float currenthpRecover;

        public float maxCarryNum;
        public float maxHp;

        public int RemainCapacity => (int)maxCarryNum - currentCarryNum;


        private SkeletonAnimation _skeletonAnimation;
        private Vector2 _dirValue;
        public bool isMoving = false;
        public PlayerDataModule dataModule;
        public SpriteRenderer shadowRenderer;
        public SpriteRenderer weaponRenderer;
        public CinemachineVirtualCamera camera;
      //  public CinemachineVirtualCamera focusCamera;
        private Rigidbody2D _rigidbody;
        public GameObject weapon;
        public SkeletonAnimation weaponEffect;


        private float detectRadius = 6f; // 怪物检测半径
        public LayerMask monsterLayer;   // 只检测怪物层
        public LayerMask productLayer;
        public LayerMask productStationLayer;


        public List<InteractionController> overlappingTrigger = new();
        public Transform receiveTransform;
        public PlayerInfo playerInfo;

        public Dictionary<GoodsType, int> goodsDic = new();
        public Dictionary<DropItemType, int> dropDic = new();

        // 正在飞行中待拾取的物品数量（用于防止超出容量上限）
        private int _pendingPickupCount = 0;

        public bool isDead = false;

        private AssetHandle _assetHandle;
        private Canvas canvas;


        /// <summary>
        /// 角色是否处于交互范围内（解锁）
        // /// </summary>
        // public bool InteractionTriggerInRange = false;
        // public Transform InteractionTriggerTransform;

        public Transform weaponRoot;
        public float speed;
        [SerializeField] private float radiusX = 1.1f;   // 左右摆动距离
        [SerializeField] private float radiusZ = 0.55f;  // 前后景深（决定遮挡感）
        [SerializeField] private float minScale = 0.85f; // 在身后时的最小缩放
        [SerializeField] private float maxScale = 1.15f; // 在身前时的最大缩放s

        public bool InRange;
        private bool isThrowingCoin = false; // 防止重复抛币

        public GameObject finger;
        public Transform fingerRoot;


        public Vector2 guidePosition;

        private void Awake()
        {
            if (dataModule == null)
            {
                dataModule = PlayerDataModule.Instance;
            }
            canvas = GetComponent<Canvas>();
            camera = GameObject.Find("Virtual Camera").GetComponent<CinemachineVirtualCamera>();
            camera.LookAt = transform;
            camera.Follow = transform;
           // focusCamera = GameObject.Find("FocusVirtualCamera").GetComponent<CinemachineVirtualCamera>();
            _rigidbody = GetComponent<Rigidbody2D>();
            _assetHandle = GetComponent<AssetHandle>();
            _skeletonAnimation = transform.Find("Character").GetComponent<SkeletonAnimation>();
            renderer = transform.Find("Character").GetComponent<MeshRenderer>();
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

        }


        private void AddEvent()
        {
            EventCenter.Instance.AddListener(EventMessages.TriggerDetection, HandleTrigger);
            EventCenter.Instance.AddListener(EventMessages.FocusView, HandleFocusView);
            EventCenter.Instance.AddListener(EventMessages.RestoreFocusView, RestoreFocusView);
            EventCenter.Instance.AddListener(EventMessages.PlayerTakeDamage, HandleTakeDamage);
            EventCenter.Instance.AddListener(EventMessages.FocusNewPosition, HandleFocusNew);
            EventCenter.Instance.AddListener(EventMessages.UpdatePlayerEquimentInfo, UpdatePlayerEquimentInfo);
            EventCenter.Instance.AddListener(EventMessages.UpdatePlayerValueInfo, UpdatePlayerValueInfo);
            EventCenter.Instance.AddListener(EventMessages.MonsterDead, HandleMonsterDead);
            EventCenter.Instance.AddListener(EventMessages.ShowGuideFinger, HandleShowGuideFinger);
            EventCenter.Instance.AddListener(EventMessages.HideGuideFinger, HandleHideGuideFinger);
            EventCenter.Instance.AddListener(EventMessages.CameraBeginShaking, HandleCameraBeginShaking);
        }
        private void OnDestroy()
        {
            EventCenter.Instance.RemoveListener(EventMessages.TriggerDetection, HandleTrigger);
            EventCenter.Instance.RemoveListener(EventMessages.FocusView, HandleFocusView);
            EventCenter.Instance.RemoveListener(EventMessages.RestoreFocusView, RestoreFocusView);
            EventCenter.Instance.RemoveListener(EventMessages.PlayerTakeDamage, HandleTakeDamage);
            EventCenter.Instance.RemoveListener(EventMessages.FocusNewPosition, HandleFocusNew);
            EventCenter.Instance.RemoveListener(EventMessages.UpdatePlayerEquimentInfo, UpdatePlayerEquimentInfo);
            EventCenter.Instance.RemoveListener(EventMessages.UpdatePlayerValueInfo, UpdatePlayerValueInfo);
            EventCenter.Instance.RemoveListener(EventMessages.MonsterDead, HandleMonsterDead);
            EventCenter.Instance.RemoveListener(EventMessages.ShowGuideFinger, HandleShowGuideFinger);
            EventCenter.Instance.RemoveListener(EventMessages.HideGuideFinger, HandleHideGuideFinger);
            EventCenter.Instance.RemoveListener(EventMessages.CameraBeginShaking, HandleCameraBeginShaking);
        }

        public void HandleCameraBeginShaking(params object[] args)
        {
            
        }

        public void HandleShowGuideFinger(params object[] args)
        {
            finger.SetActive(true);
            guidePosition = (Vector2)args[0];
        }
         public void HandleHideGuideFinger(params object[] args)
        {
            finger.SetActive(false);
            guidePosition = Vector2.zero;
        }


        public void HandleMonsterDead(params object[] args)
        {
            currentHp += PlayerDataModule.Instance.data.addhpRecover;
            currentHp = Mathf.Min(currentHp, maxHp);
        }

        public void UpdatePlayerValueInfo(params object[] args)
        {
            currentCarryNum = 0;
            _pendingPickupCount = 0;

            maxHp = dataModule.data.hp + dataModule.data.addHp;
            var cardprogrees = PlayerDataModule.Instance.data.cardUpProgressesList.Find(x => x.developType == CardDevelopType.UpgradeCharacterWithXuanCaiTuHp);
            if (cardprogrees != null)
            {
                maxHp += cardprogrees.level * 30;
            }
            maxCarryNum = dataModule.data.bagCapacity + dataModule.data.addBagCapacity;
            currentPinkUpRange = dataModule.data.pickUpRange + dataModule.data.addPickUpRange;
            currentMoveSpeed = dataModule.data.moveSpeed + dataModule.data.addMoveSpeed;
            playerInfo.UpdateTxt();
        }


        public void Init()
        {
            if (dataModule == null)
            {
                dataModule = PlayerDataModule.Instance;
            }
            finger.SetActive(false);
            UpdatePlayerValueInfo();
            currentHp = maxHp;
            EventCenter.Instance.TriggerEvent(EventMessages.UpdatePlayerMoneyInfo);
            EventCenter.Instance.TriggerEvent(EventMessages.UpdatePlayerValueInfo);
            EventCenter.Instance.TriggerEvent(EventMessages.UpdatePlayerEquimentInfo);
            EventCenter.Instance.TriggerEvent(EventMessages.UpdatePlayerInfo);
        }

        public void SetLayer()
        {
            int newOrder = 30000 - Mathf.RoundToInt(transform.position.y * 100);
            canvas.sortingOrder = newOrder + 1;
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
                    _skeletonAnimation.transform.localScale = new Vector3(-0.6f, 0.6f, 0.6f);
                }
                else
                {
                    _skeletonAnimation.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
                }

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
            }

            CheckMonster();
            CheckDrop();
            CheckProductStation();
            CheckProduct();
            CheckSaleStall();

            if (weapon.gameObject.activeSelf)
            {
                weaponRoot.Rotate(0f, 0f, -speed * Time.deltaTime);
                float z = weaponRoot.localEulerAngles.z;
                if (z > 180f) z -= 360f;
                weaponRenderer.sortingOrder = renderer.sortingOrder + 1;
                weaponEffect.GetComponent<MeshRenderer>().sortingOrder = renderer.sortingOrder + 1;
                float t = Mathf.Abs(Mathf.Cos(z * Mathf.Deg2Rad));
                float scale = Mathf.Lerp(0.85f, 1.1f, t);
                weaponRoot.localScale = Vector3.one * scale;
                weaponEffect.gameObject.SetActive(true);
                var state = weaponEffect.AnimationState;
                var current = state.GetCurrent(0);
                if (current == null || current.Animation.Name != "animation")
                {
                    state.SetAnimation(0, "animation", true);
                }
            }
            else
            {
                weaponEffect.AnimationState.ClearTrack(0);
                weaponEffect.gameObject.SetActive(false);
            }

            if (finger.activeSelf)
            {
                Vector2 dir = guidePosition - (Vector2)transform.position;
                fingerRoot.transform.right = dir;
                finger.GetComponent<SpriteRenderer>().sortingOrder = renderer.sortingOrder + 100;
                if (Vector2.Distance(guidePosition, transform.position) < 3f)
                {
                    finger.SetActive(false);
                    guidePosition = Vector2.zero;
                }
            }

        }






        public bool CanThrowTongBi()
        {
            return PlayerDataModule.Instance.data.tongbi >= 100 && InRange && !isThrowingCoin && !isMoving;
        }

        public void ThrowOutTongBi(Transform target)
        {
            Debug.Log(" yj = > 判断投掷铜币");

            if (CanThrowTongBi())
            {
                Debug.Log(" yj = > 判断通过");
                StartCoroutine(ThrowOutTongBiCoroutine(target));
            }

        }

        private IEnumerator ThrowOutTongBiCoroutine(Transform target)
        {
            isThrowingCoin = true; // 标记正在抛币
            Debug.Log("yj = >投掷铜币");

            GameObject coinObj = Instantiate(
                _assetHandle.Get<GameObject>("Production"),
                receiveTransform.position,
                Quaternion.identity
            );

            var coinCtrl = coinObj.GetComponent<Production>();
            coinCtrl.Init(GoodsType.TongBi, 100);
            coinCtrl.spriteRenderer.sortingOrder = renderer.sortingOrder + 2;



            coinCtrl.FlyTo(
                target.position,
                () =>
                {
                    EventCenter.Instance.TriggerEvent(
                        EventMessages.ThrowOutTongBi,
                        target
                    );
                    Destroy(coinObj);
                    PlayerDataModule.Instance.data.tongbi -= 100;
                    EventCenter.Instance.TriggerEvent(EventMessages.UpdatePlayerMoneyInfo);
                }
            );

            yield return new WaitForSeconds(0.5f);
            isThrowingCoin = false; // 抛币结束
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
                                        new Vector2(_dirValue.x, _dirValue.y) * (currentMoveSpeed * Time.fixedDeltaTime));
            }
        }
        private void UpdatePlayerEquimentInfo(params object[] args)
        {
            if (DataController.Instance.weaponDataDic.ContainsKey(dataModule.data.currentWeapon))
            {
                WeaponData weaponData = DataController.Instance.weaponDataDic[dataModule.data.currentWeapon];
                weaponRenderer.sprite = _assetHandle.Get<Sprite>(weaponData.name);
                _skeletonAnimation.skeleton.SetAttachment(weaponData.slotName, weaponData.attachmentName);
                SkeletonDataAsset skeletonDataAsset = _assetHandle.Get<SkeletonDataAsset>(weaponData.name + "data");
                weaponEffect.skeletonDataAsset = skeletonDataAsset;
                weaponEffect.Initialize(true);
            }
            if (DataController.Instance.storageBagDataDic.ContainsKey(dataModule.data.currentBag))
            {
                StotageBagData stotageBagData = DataController.Instance.storageBagDataDic[dataModule.data.currentBag];
                _skeletonAnimation.skeleton.SetAttachment(stotageBagData.slotName, stotageBagData.attachmentName);
            }

            playerInfo.UpdateBagInfo();
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
                currentHp += 5 * Time.deltaTime;
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
                if (!item.canPickup) continue;                    // 还在掉落动画中，不可拾取

                float dist = Vector2.Distance(transform.position, item.transform.position);
                if (dist > currentPinkUpRange) continue;

                var drop = item as DropController;
                if (drop == null) continue;

                // 金元宝和银钱不占背包容量，无需检查上限
                if (drop.itemType == DropItemType.JingYuanBao || drop.itemType == DropItemType.YingQian)
                {
                    item.StartAttract(this.transform, receiveTransform);
                }
                else
                {
                    // 检查剩余容量（考虑正在飞行中的物品）
                    // 用 continue 而不是 break，确保后续的银钱/金元宝仍能被拾取
                    if (currentCarryNum + _pendingPickupCount >= maxCarryNum) continue;
                    item.StartAttract(this.transform, receiveTransform,
                        () => _pendingPickupCount = Mathf.Max(0, _pendingPickupCount - 1));
                    _pendingPickupCount++;
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
                if (item.isTaken) continue;
                if (!item.canPickup) continue;

                var production = item.GetComponent<Production>();
                if (production == null) continue;

                float dist = Vector2.Distance(transform.position, item.transform.position);
                if (dist <= currentPinkUpRange && production.station is CashierCounter cashierCounter)
                {
                    bool wasTaken = item.isTaken;
                    item.StartAttract(this.transform, receiveTransform);
                    if (!wasTaken && item.isTaken)
                    {
                        cashierCounter.grid.ReleaseOne();
                    }
                }
                else if (dist <= currentPinkUpRange)
                {
                    // 检查剩余容量（考虑正在飞行中的物品）
                    // 用 continue 而不是 break，确保后续的收银台铜币仍能被拾取
                    if (currentCarryNum + _pendingPickupCount >= maxCarryNum) continue;
                    if (production.state != ItemState.OnWorkbench) continue;
                    if (!(production.station is ProductionStation station)) continue;
                    if (!station.productionList.Contains(production)) continue;
                    if (Controller.FreightClerkController.IsProductReservedByFreight(production)) continue;

                    bool wasTaken = item.isTaken;
                    item.StartAttract(this.transform, receiveTransform,
                        () => _pendingPickupCount = Mathf.Max(0, _pendingPickupCount - 1));
                    if (!wasTaken && item.isTaken)
                    {
                        station.grid.ReleaseOne();
                        station.productionList.Remove(production);
                        _pendingPickupCount++;
                    }
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
        private float scatterDuration = 0.3f;
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
                    if ((data.Value.gameObject.transform.position - transform.position).sqrMagnitude < 10)
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

                GameObject drop = Instantiate(_assetHandle.Get<GameObject>("DropObj"));
                var dropCtrl = drop.GetComponent<DropController>();
                dropCtrl.canPickup = false;
                dropCtrl.Init(station.dropItemType);
                dropCtrl.spriteRenderer.sortingOrder = station.sprite.sortingOrder + 2;
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
                Destroy(drop);

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

                GameObject drop = GameObject.Instantiate(_assetHandle.Get<GameObject>("Production"));
                var dropCtrl = drop.GetComponent<Production>();
                dropCtrl.canPickup = false;
                dropCtrl.Init(station.currentGoodsType);
                dropCtrl.SetStation(station);
                Vector2 start = receiveTransform.position;
                dropCtrl.spriteRenderer.sortingOrder = station.sprite.sortingOrder + 2;
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


        private InteractionController currentInteraction;
        private InteractionTrigger currentInteractionTrigger;
        private Coroutine stayCoroutine;

        private void OnTriggerEnter2D(Collider2D other)
        {
            var interaction = other.GetComponent<InteractionController>();
            var interactionTrigger = other.GetComponent<InteractionTrigger>();
            if (interaction == null && interactionTrigger == null) return;

            currentInteraction = interaction;
            currentInteractionTrigger = interactionTrigger;
            currentInteractionTrigger.TriggerEnter();
            if (stayCoroutine == null)
            {
                stayCoroutine = StartCoroutine(StayCheck());
            }
        }
        private IEnumerator StayCheck()
        {
            float stayTime = 0f;

            while (currentInteraction != null)
            {
                if (!isMoving)   // ⭐ 核心判断
                {
                    stayTime += Time.deltaTime;

                    if (stayTime >= 0.5f) // 停 0.5 秒
                    {
                        currentInteraction.Interact();
                        yield break;
                    }
                }
                else
                {
                    stayTime = 0f; // 一动就清零
                }

                yield return null;
            }

            stayCoroutine = null;
        }


        private void OnTriggerExit2D(Collider2D other)
        {
            var interaction = other.GetComponent<InteractionController>();
            var interactionTrigger = other.GetComponent<InteractionTrigger>();
            if (interaction == null && interactionTrigger == null) return;
            interactionTrigger.TriggerExit();
            if (interaction == currentInteraction)
            {
                currentInteraction.CloseInteract();
                currentInteraction = null;

                if (stayCoroutine != null)
                {
                    StopCoroutine(stayCoroutine);
                    stayCoroutine = null;
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
            EventCenter.Instance.TriggerEvent(EventMessages.HarvestTask, itemType);
            switch (itemType)
            {
                case DropItemType.ShuangYunZhiFragment:
                    currentCarryNum += 1;
                    if (!dropDic.TryAdd(itemType, 1))
                    {
                        dropDic[itemType]++;
                    }
                    _pendingPickupCount = Mathf.Max(0, _pendingPickupCount - 1);
                    break;
                case DropItemType.YueLuCaoFragment:
                    currentCarryNum += 1;
                    if (!dropDic.TryAdd(itemType, 1))
                    {
                        dropDic[itemType]++;
                    }
                    _pendingPickupCount = Mathf.Max(0, _pendingPickupCount - 1);
                    break;
                case DropItemType.ZiXinHuaFragment:
                    currentCarryNum += 1;
                    if (!dropDic.TryAdd(itemType, 1))
                    {
                        dropDic[itemType]++;
                    }
                    _pendingPickupCount = Mathf.Max(0, _pendingPickupCount - 1);
                    break;
                case DropItemType.YuHuiHeFragment:
                    currentCarryNum += 1;
                    if (!dropDic.TryAdd(itemType, 1))
                    {
                        dropDic[itemType]++;
                    }
                    _pendingPickupCount = Mathf.Max(0, _pendingPickupCount - 1);
                    break;
                case DropItemType.XingWenGuoFragment:
                    currentCarryNum += 1;
                    if (!dropDic.TryAdd(itemType, 1))
                    {
                        dropDic[itemType]++;
                    }
                    _pendingPickupCount = Mathf.Max(0, _pendingPickupCount - 1);
                    break;
                case DropItemType.WuRongJunFragment:
                    currentCarryNum += 1;
                    if (!dropDic.TryAdd(itemType, 1))
                    {
                        dropDic[itemType]++;
                    }
                    _pendingPickupCount = Mathf.Max(0, _pendingPickupCount - 1);
                    break;
                case DropItemType.LingXuShengFragment:
                    currentCarryNum += 1;
                    if (!dropDic.TryAdd(itemType, 1))
                    {
                        dropDic[itemType]++;
                    }
                    _pendingPickupCount = Mathf.Max(0, _pendingPickupCount - 1);
                    break;
                case DropItemType.XueBanHuaFragment:
                    currentCarryNum += 1;
                    if (!dropDic.TryAdd(itemType, 1))
                    {
                        dropDic[itemType]++;
                    }
                    _pendingPickupCount = Mathf.Max(0, _pendingPickupCount - 1);
                    break;
                case DropItemType.MuLingYaFragment:
                    currentCarryNum += 1;
                    if (!dropDic.TryAdd(itemType, 1))
                    {
                        dropDic[itemType]++;
                    }
                    _pendingPickupCount = Mathf.Max(0, _pendingPickupCount - 1);
                    break;
                case DropItemType.JingRuiCaoFragment:
                    currentCarryNum += 1;
                    if (!dropDic.TryAdd(itemType, 1))
                    {
                        dropDic[itemType]++;
                    }
                    _pendingPickupCount = Mathf.Max(0, _pendingPickupCount - 1);
                    break;

                case DropItemType.TieKuangShiFragment:
                    currentCarryNum += 1;
                    if (!dropDic.TryAdd(itemType, 1))
                    {
                        dropDic[itemType]++;
                    }
                    _pendingPickupCount = Mathf.Max(0, _pendingPickupCount - 1);
                    break;
                case DropItemType.YinKuangShiFragment:
                    currentCarryNum += 1;
                    if (!dropDic.TryAdd(itemType, 1))
                    {
                        dropDic[itemType]++;
                    }
                    _pendingPickupCount = Mathf.Max(0, _pendingPickupCount - 1);
                    break;
                case DropItemType.TongKuangShiFragment:
                    currentCarryNum += 1;
                    if (!dropDic.TryAdd(itemType, 1))
                    {
                        dropDic[itemType]++;
                    }
                    _pendingPickupCount = Mathf.Max(0, _pendingPickupCount - 1);
                    break;
                case DropItemType.ZiJingShiFragment:
                    currentCarryNum += 1;
                    if (!dropDic.TryAdd(itemType, 1))
                    {
                        dropDic[itemType]++;
                    }
                    _pendingPickupCount = Mathf.Max(0, _pendingPickupCount - 1);
                    break;
                case DropItemType.YueJingShiFragment:
                    currentCarryNum += 1;
                    if (!dropDic.TryAdd(itemType, 1))
                    {
                        dropDic[itemType]++;
                    }
                    _pendingPickupCount = Mathf.Max(0, _pendingPickupCount - 1);
                    break;
                case DropItemType.JingYuanBao:
                    dataModule.AddJinYuanBao(50);
                    break;
                case DropItemType.YingQian:
                    dataModule.AddYinQian(100);
                    break;
            }

            playerInfo.UpdateTxt();
        }
        public void AddGoods(GoodsType goodsType, int value = 0)
        {
            if (goodsType == GoodsType.TongBi)
            {
                dataModule.AddYinQian(value);
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
                _pendingPickupCount = Mathf.Max(0, _pendingPickupCount - 1);
                goodsDic.TryAdd(goodsType, 0);
                goodsDic[goodsType]++;
                playerInfo.UpdateTxt();
            }

            if (goodsType == GoodsType.YunZhiCha && PlayerDataModule.Instance.data.guideStep == GuideStep.TakeTea)
            {
                PlayerDataModule.Instance.data.guideStep = GuideStep.SellTea;
                UIController.Instance.Show<PlayerGuide>();
            }
        }


        public void HandleTakeDamage(params object[] args)
        {
            float value = Convert.ToSingle(args[0]);
            TakeDamage(value);

        }

        public void HandleFocusNew(params object[] args)
        {
            Transform t = (Transform)args[0];
            EventCenter.Instance.TriggerEvent(EventMessages.FocusView); // 禁止输入
            // focusCamera.LookAt = t;
            // focusCamera.Priority = 20; // > PlayerCam 的 10
            StartCoroutine(ReturnAfterDelay(3f));
        }
        IEnumerator ReturnAfterDelay(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            // focusCamera.LookAt = transform;
            // focusCamera.Priority = 5;                                             // 恢复为低
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
            playerInfo.ShowHpInfo();
            playerInfo.UpdateFill(currentHp / dataModule.data.hp);
            if (currentHp <= 0)
            {
                DoDie();
            }


        }

        public void DoDie()
        {
            transform.position = GameController.Instance.RespawnPoint.transform.position;
            currentHp = dataModule.data.hp;
            playerInfo.ShowHpInfo();
            playerInfo.UpdateFill(currentHp / dataModule.data.hp);
            goodsDic.Clear();
            dropDic.Clear();
            _pendingPickupCount = 0;
        }

        private IEnumerator InvincibleFrame()
        {
            invincible = true;
            yield return new WaitForSeconds(invincibleTime);
            invincible = false;
        }

    }
}
