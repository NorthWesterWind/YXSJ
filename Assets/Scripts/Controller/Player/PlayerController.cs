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
        public float maxCarryNum;
        public float maxHp;
        public int RemainCapacity => (int)maxCarryNum - currentCarryNum;
        public int CurrentSortingOrder => renderer != null ? renderer.sortingOrder : 0;
        private const int SortingOrderMin = -32768;
        private const int SortingOrderMax = 32767;
        private const int PlayerSortingOrderBase = 30000;
        private const int PlayerSortingOrderScale = 100;
        private SkeletonAnimation _skeletonAnimation;
        private Vector2 _dirValue;
        public bool isMoving = false;
        public PlayerDataModule dataModule;
        public SpriteRenderer shadowRenderer;
        public SpriteRenderer weaponRenderer;
        public CinemachineVirtualCamera camera;
        private Rigidbody2D _rigidbody;
        public GameObject weapon;
        public SkeletonAnimation weaponEffect;
        private float detectRadius = 6f;
        public LayerMask monsterLayer;
        public List<InteractionController> overlappingTrigger = new();
        public Transform receiveTransform;
        public PlayerInfo playerInfo;
        public Dictionary<GoodsType, int> goodsDic = new();
        public Dictionary<DropItemType, int> dropDic = new();
        private int _pendingPickupCount = 0;
        private const float PickupScanInterval = 0.05f;
        private const float ProductScanInterval = 0.05f;
        private const float InteractionScanInterval = 0.05f;
        private const float MonsterScanInterval = 0.1f;
        private float pickupScanTimer;
        private float productScanTimer;
        private float interactionScanTimer;
        private float monsterScanTimer;
        private readonly HashSet<CashierCounter> handledCashiers = new();
        private readonly HashSet<ProductionStation> handledStations = new();
        public bool isDead = false;
        private AssetHandle _assetHandle;
        private Canvas canvas;
        public Transform weaponRoot;
        public float speed;
        public bool InRange;
        private bool isThrowingCoin = false;
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
            EventCenter.Instance.AddListener(EventMessages.ShowGuideFinger, HandleShowGuideFinger);
            EventCenter.Instance.AddListener(EventMessages.HideGuideFinger, HandleHideGuideFinger);
            EventCenter.Instance.AddListener(EventMessages.CameraBeginShaking, HandleCameraBeginShaking);
            EventCenter.Instance.AddListener(EventMessages.UpdatePlayerClothingInfo, UpdatePlayerClothingInfo);
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
            EventCenter.Instance.RemoveListener(EventMessages.ShowGuideFinger, HandleShowGuideFinger);
            EventCenter.Instance.RemoveListener(EventMessages.HideGuideFinger, HandleHideGuideFinger);
            EventCenter.Instance.RemoveListener(EventMessages.CameraBeginShaking, HandleCameraBeginShaking);
            EventCenter.Instance.RemoveListener(EventMessages.UpdatePlayerClothingInfo, UpdatePlayerClothingInfo);
        }
        public void HandleCameraBeginShaking(params object[] args)
        {

        }
        public void UpdatePlayerClothingInfo(params object[] args)
        {
            int clothingId = PlayerDataModule.Instance.data.currentClothing;
            string skinName = clothingId.ToString();

            _skeletonAnimation.Initialize(false);
            _skeletonAnimation.Skeleton.SetSkin(skinName);
            _skeletonAnimation.Skeleton.SetSlotsToSetupPose();
            _skeletonAnimation.AnimationState.Apply(_skeletonAnimation.Skeleton);

            if (face == -1)
            {
                _skeletonAnimation.skeleton.SetAttachment("衣服", "衣服");
                _skeletonAnimation.transform.localScale = new Vector3(-0.6f, 0.6f, 0.6f);
            }
            else
            {
                _skeletonAnimation.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
                _skeletonAnimation.skeleton.SetAttachment("衣服", "8_2");
            }
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
            maxCarryNum = PlayerDataModule.Instance.GetTotalBagCapacity();
            currentPinkUpRange = dataModule.data.pickUpRange + dataModule.data.addPickUpRange;
            currentMoveSpeed = dataModule.data.moveSpeed + dataModule.data.addMoveSpeed;
            if (playerInfo != null)
            {
                playerInfo.UpdateTxt();
            }

        }
        public void Init()
        {
            if (dataModule == null)
            {
                dataModule = PlayerDataModule.Instance;
            }
            EnsureInventoryDictionariesInitialized();
            UpdatePlayerClothingInfo();
            finger.SetActive(false);
            UpdatePlayerValueInfo();
            currentHp = maxHp;
            TryRestoreInventoryFromData();
            EventCenter.Instance.TriggerEvent(EventMessages.UpdatePlayerMoneyInfo);
            EventCenter.Instance.TriggerEvent(EventMessages.UpdatePlayerValueInfo);
            EventCenter.Instance.TriggerEvent(EventMessages.UpdatePlayerEquimentInfo);
            EventCenter.Instance.TriggerEvent(EventMessages.UpdatePlayerInfo);
        }
        private void TryRestoreInventoryFromData()
        {
            var data = dataModule != null ? dataModule.data : null;
            if (data == null)
            {
                return;
            }

            EnsureInventoryDictionariesInitialized();

            if ((dropDic != null && dropDic.Count > 0) || (goodsDic != null && goodsDic.Count > 0))
            {
                return;
            }

            bool hasSnapshot = (data.runtimePlayerDropList != null && data.runtimePlayerDropList.Count > 0) ||
                               (data.runtimePlayerGoodsList != null && data.runtimePlayerGoodsList.Count > 0);
            if (!hasSnapshot)
            {
                return;
            }
            dropDic.Clear();
            EnsureInventoryDictionariesInitialized();
            if (data.runtimePlayerDropList != null)
            {
                foreach (var entry in data.runtimePlayerDropList)
                {
                    if (entry == null) continue;
                    if (entry.count <= 0) continue;
                    dropDic[entry.itemType] = entry.count;
                }
            }
            goodsDic.Clear();
            EnsureInventoryDictionariesInitialized();
            if (data.runtimePlayerGoodsList != null)
            {
                foreach (var entry in data.runtimePlayerGoodsList)
                {
                    if (entry == null) continue;
                    if (entry.count <= 0) continue;
                    goodsDic[entry.goodsType] = entry.count;
                }
            }
            if (playerInfo != null)
            {
                playerInfo.UpdateTxt();
            }
            else
            {
                EventCenter.Instance.TriggerEvent(EventMessages.UpdatePlayerInfo);
            }
        }

        private void EnsureInventoryDictionariesInitialized()
        {
            dropDic ??= new Dictionary<DropItemType, int>();
            goodsDic ??= new Dictionary<GoodsType, int>();

            foreach (DropItemType type in Enum.GetValues(typeof(DropItemType)))
            {
                if (type == DropItemType.None || type == DropItemType.YingQian || type == DropItemType.JingYuanBao)
                {
                    continue;
                }

                dropDic.TryAdd(type, 0);
            }

            foreach (GoodsType type in Enum.GetValues(typeof(GoodsType)))
            {
                if (type == GoodsType.None || type == GoodsType.JingYunBao || type == GoodsType.TongBi)
                {
                    continue;
                }

                goodsDic.TryAdd(type, 0);
            }
        }
        public void SetLayer()
        {
            int newOrder = GetPlayerSortingOrder();
            canvas.sortingOrder = AddSortingOrderOffset(newOrder, 1);
            renderer.sortingOrder = newOrder;
            shadowRenderer.sortingOrder = newOrder;
        }

        private int GetPlayerSortingOrder()
        {
            int order = PlayerSortingOrderBase - Mathf.RoundToInt(transform.position.y * PlayerSortingOrderScale);
            return Mathf.Clamp(order, SortingOrderMin, SortingOrderMax);
        }

        private int AddSortingOrderOffset(int order, int offset)
        {
            return Mathf.Clamp(order + offset, SortingOrderMin, SortingOrderMax);
        }
        public bool isShowUI = false;
        public int face = 1;
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
                    _skeletonAnimation.skeleton.SetAttachment("衣服", "衣服");
                    _skeletonAnimation.transform.localScale = new Vector3(-0.6f, 0.6f, 0.6f);
                    face = -1;
                }
                else
                {
                    _skeletonAnimation.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
                    _skeletonAnimation.skeleton.SetAttachment("衣服", "8_2");
                    face = 1;
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
            float deltaTime = Time.deltaTime;
            monsterScanTimer -= deltaTime;
            if (monsterScanTimer <= 0f)
            {
                monsterScanTimer = MonsterScanInterval;
                CheckMonster();
            }
            pickupScanTimer -= deltaTime;
            if (pickupScanTimer <= 0f)
            {
                pickupScanTimer = PickupScanInterval;
                CheckDrop();
            }
            productScanTimer -= deltaTime;
            if (productScanTimer <= 0f)
            {
                productScanTimer = ProductScanInterval;
                CheckProduct();
            }
            interactionScanTimer -= deltaTime;
            if (interactionScanTimer <= 0f)
            {
                interactionScanTimer = InteractionScanInterval;
                CheckProductStation();
                CheckSaleStall();
            }
            if (weapon.gameObject.activeSelf)
            {
                weaponRoot.Rotate(0f, 0f, -speed * Time.deltaTime);
                float z = weaponRoot.localEulerAngles.z;
                if (z > 180f) z -= 360f;
                weaponRenderer.sortingOrder = AddSortingOrderOffset(renderer.sortingOrder, 1);
                weaponEffect.GetComponent<MeshRenderer>().sortingOrder = AddSortingOrderOffset(renderer.sortingOrder, 1);
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
                finger.GetComponent<SpriteRenderer>().sortingOrder = AddSortingOrderOffset(renderer.sortingOrder, 100);
                if (Vector2.Distance(guidePosition, transform.position) < 3f)
                {
                    finger.SetActive(false);
                    guidePosition = Vector2.zero;
                }
            }
        }
        public bool CanThrowTongBi(Transform target = null)
        {
            return GetTongBiThrowAmount(target) > 0 && InRange && !isThrowingCoin && !isMoving;
        }
        public void ThrowOutTongBi(Transform target)
        {
            if (CanThrowTongBi(target))
            {
                StartCoroutine(ThrowOutTongBiCoroutine(target));
            }
        }
        private IEnumerator ThrowOutTongBiCoroutine(Transform target)
        {
            int throwAmount = GetTongBiThrowAmount(target);
            if (throwAmount <= 0)
            {
                yield break;
            }

            isThrowingCoin = true;
            GameObject coinObj = Instantiate(
                _assetHandle.Get<GameObject>("Production"),
                receiveTransform.position,
                Quaternion.identity
            );
            var coinCtrl = coinObj.GetComponent<Production>();
            coinCtrl.Init(GoodsType.TongBi, throwAmount);
            coinCtrl.spriteRenderer.sortingOrder = AddSortingOrderOffset(renderer.sortingOrder, 2);
            coinCtrl.FlyTo_1(
                target.position, 0.08f, target,
                () =>
                {
                    EventCenter.Instance.TriggerEvent(
                        EventMessages.ThrowOutTongBi,
                        target,
                        throwAmount
                    );
                    Destroy(coinObj);
                    PlayerDataModule.Instance.data.tongbi -= throwAmount;
                    EventCenter.Instance.TriggerEvent(EventMessages.UpdatePlayerMoneyInfo);
                }
            );
            yield return new WaitForSeconds(0.1f);
            isThrowingCoin = false; // 抛币结束
        }
        private int GetTongBiThrowAmount(Transform target)
        {
            int ownTongBi = Mathf.Max(0, PlayerDataModule.Instance.data.tongbi);
            if (ownTongBi <= 0)
            {
                return 0;
            }

            int unlockNeed = GetUnlockTongBiNeed(target);
            if (unlockNeed > 0)
            {
                return Mathf.Min(ownTongBi, unlockNeed);
            }

            return Mathf.Min(100, ownTongBi);
        }

        private int GetUnlockTongBiNeed(Transform target)
        {
            if (target == null)
            {
                return 0;
            }

            var mapLock = target.GetComponentInParent<MapLock>();
            if (mapLock != null)
            {
                return mapLock.GetRemainingUnlockCost();
            }

            var structureLock = target.GetComponentInParent<StructureLock>();
            if (structureLock != null)
            {
                return structureLock.GetRemainingUnlockCost();
            }

            return 0;
        }
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
            if (playerInfo != null)
            {
                playerInfo.UpdateBagInfo();
            }
        }
        private void HandleFocusView(params object[] args)
        {
            isShowUI = true;
        }
        private void RestoreFocusView(params object[] args)
        {
            isShowUI = false;
        }
        public void SetDir(Vector2 direction)
        {
            _dirValue = direction;
        }
        private float lastDamageTime = -999f;
        private bool isRegenerating = false;
        private Coroutine regenCoroutine;
        private float regenDelay = 4f;
        private const float DefaultHpRecoverRate = 0.05f;
        private IEnumerator RegenerateHealth()
        {
            isRegenerating = true;
            while (currentHp < maxHp)
            {
                float recoverRate = GetHpRecoverRate();
                if (recoverRate <= 0f)
                {
                    isRegenerating = false;
                    regenCoroutine = null;
                    yield break;
                }

                currentHp += maxHp * recoverRate * Time.deltaTime;
                currentHp = Mathf.Min(currentHp, maxHp);
                if (playerInfo != null)
                {
                    playerInfo.ShowHpInfo();
                    playerInfo.UpdateFill(currentHp / maxHp);
                }
                yield return null;

                if (Time.time - lastDamageTime < regenDelay)
                {
                    isRegenerating = false;
                    regenCoroutine = null;
                    yield break;
                }
            }
            if (playerInfo != null && currentHp >= maxHp)
            {
                playerInfo.HideHpInfo();
            }
            isRegenerating = false;
            regenCoroutine = null;
        }

        private float GetHpRecoverRate()
        {
            float baseRecoverRate = dataModule.data.hpRecover > 0f
                ? dataModule.data.hpRecover
                : DefaultHpRecoverRate;
            return Mathf.Max(0f, baseRecoverRate);
        }
        public void CheckDrop()
        {
            var scenePickup = ScenePickupController.Instance;
            if (scenePickup == null)
            {
                return;
            }
            var list = scenePickup.materials;
            for (int i = list.Count - 1; i >= 0; i--)
            {
                var item = list[i];
                if (item == null) continue;
                if (!item.gameObject.activeInHierarchy) continue;
                if (item.isTaken) continue;
                if (!item.canPickup) continue;
                float dist = Vector2.Distance(transform.position, item.transform.position);
                if (dist > currentPinkUpRange) continue;
                var drop = item as DropController;
                if (drop == null) continue;
                if (drop.itemType == DropItemType.JingYuanBao || drop.itemType == DropItemType.YingQian)
                {
                    item.StartAttract(this.transform, receiveTransform);
                }
                else
                {
                    if (currentCarryNum + _pendingPickupCount >= maxCarryNum) continue;
                    item.StartAttract(this.transform, receiveTransform,
                        () => _pendingPickupCount = Mathf.Max(0, _pendingPickupCount - 1));
                    _pendingPickupCount++;
                }
            }
        }
        public void CheckProduct()
        {
            var scenePickup = ScenePickupController.Instance;
            if (scenePickup == null)
            {
                return;
            }
            var list = scenePickup.products;
            handledCashiers.Clear();
            handledStations.Clear();
            for (int i = list.Count - 1; i >= 0; i--)
            {
                var item = list[i];
                if (item == null) continue;
                if (!item.gameObject.activeInHierarchy) continue;
                if (item.isTaken) continue;
                if (!item.canPickup) continue;
                var production = item as Production;
                if (production == null) continue;
                if (production.station is CashierCounter cashierCounter)
                {
                    if (!handledCashiers.Add(cashierCounter)) continue;
                    float rootDist = Vector2.Distance(transform.position, cashierCounter.GetPickupRootPosition());
                    if (rootDist <= currentPinkUpRange)
                    {
                        cashierCounter.TryAttractTopCoin(this.transform, receiveTransform);
                    }
                }
                else if (production.station is ProductionStation station)
                {
                    if (!handledStations.Add(station)) continue;
                    float rootDist = Vector2.Distance(transform.position, station.GetPickupRootPosition());
                    if (rootDist > currentPinkUpRange) continue;
                    if (currentCarryNum + _pendingPickupCount >= maxCarryNum) continue;

                    bool taken = station.TryAttractTopProduct(
                        this.transform,
                        receiveTransform,
                        () => _pendingPickupCount = Mathf.Max(0, _pendingPickupCount - 1));
                    if (taken)
                    {
                        _pendingPickupCount++;
                    }
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
                UpdatePlayerEquimentInfo();
                weapon.gameObject.SetActive(false);
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
            if (GameController.Instance == null || GameController.Instance.buildings == null)
            {
                return;
            }

            foreach (var data in GameController.Instance.buildings)
            {
                if (data.Key == BuildingType.YuShaHu_1 || data.Key == BuildingType.LianQiLu_1 ||
                    data.Key == BuildingType.YuShaHu_2 || data.Key == BuildingType.YuShaHu_3 ||
                    data.Key == BuildingType.YuShaHu_4 || data.Key == BuildingType.LianQiLu_2 ||
                    data.Key == BuildingType.LianQiLu_3)
                {
                    if (data.Value == null)
                    {
                        continue;
                    }

                    if (data.Value is not ProductionStation station)
                    {
                        continue;
                    }

                    if ((station.transform.position - transform.position).sqrMagnitude < 10)
                    {
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
            if (GameController.Instance == null || GameController.Instance.goodBuild == null)
            {
                return;
            }

            foreach (var data in GameController.Instance.goodBuild)
            {
                if (data.Value == null)
                {
                    continue;
                }

                if (data.Value is not SalesStall station)
                {
                    continue;
                }

                if ((station.transform.position - transform.position).sqrMagnitude < 8)
                {
                    if (!IsSalesStallUnlocked(station)) continue;
                    if (!goodsDic.ContainsKey(station.currentGoodsType)) continue;
                    if (goodsDic[station.currentGoodsType] <= 0) continue;
                    if (!deliverCoroutines.ContainsKey("SaleStall"))
                    {
                        deliverCoroutines["SaleStall"] = StartCoroutine(DeliverProduct(station));
                    }
                }
            }
        }
        private IEnumerator DeliverMaterial(ProductionStation station)
        {
            if (station == null)
            {
                deliverCoroutines.Remove("ProductStation");
                yield break;
            }

            if (!dropDic.TryGetValue(station.dropItemType, out int count) || count <= 0)
            {
                deliverCoroutines.Remove("ProductStation");
                yield break;
            }

            if (isMoving)
            {
                deliverCoroutines["ProductStation"] = null;
                deliverCoroutines.Remove("ProductStation");
                yield break;
            }

            GameObject drop = Instantiate(_assetHandle.Get<GameObject>("DropObj"));
            var dropCtrl = drop.GetComponent<DropController>();
            dropCtrl.canPickup = false;
            dropCtrl.Init(station.dropItemType, count);
            dropCtrl.spriteRenderer.sortingOrder = station.sprite.sortingOrder + 2;
            Vector2 start = receiveTransform.position;
            Vector2 target = station.recivePosition.position;
            Vector2 control = Vector2.Lerp(start, target, 0.1f) + Vector2.up * 1.5f;
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
            station.AddMaterial(dropCtrl.count);
            Destroy(drop);
            dropDic[station.dropItemType] = Mathf.Max(0, dropDic[station.dropItemType] - dropCtrl.count);
            currentCarryNum = Mathf.Max(0, currentCarryNum - dropCtrl.count);
            if (playerInfo != null)
            {
                playerInfo.UpdateTxt();
            }

            deliverCoroutines["ProductStation"] = null;
            deliverCoroutines.Remove("ProductStation");
        }
        private IEnumerator DeliverProduct(SalesStall station)
        {
            if (!IsSalesStallUnlocked(station))
            {
                deliverCoroutines.Remove("SaleStall");
                yield break;
            }

            int count = goodsDic[station.currentGoodsType];
            for (int i = 0; i < count; i++)
            {
                if (isMoving)
                {
                    deliverCoroutines["SaleStall"] = null;
                    break;
                }
                if (!IsSalesStallUnlocked(station))
                {
                    deliverCoroutines["SaleStall"] = null;
                    break;
                }
                GameObject drop = GameObject.Instantiate(_assetHandle.Get<GameObject>("Production"));
                var dropCtrl = drop.GetComponent<Production>();
                dropCtrl.canPickup = false;
                dropCtrl.Init(station.currentGoodsType);
                dropCtrl.SetStation(station);
                drop.transform.position = receiveTransform.position;
                station.PlaceProduct(dropCtrl);
                goodsDic[station.currentGoodsType]--;
                currentCarryNum--;
                if (playerInfo != null)
                {
                    playerInfo.UpdateTxt();
                }
                yield return new WaitForSeconds(0.05f);
            }
            deliverCoroutines["SaleStall"] = null;
            deliverCoroutines.Remove("SaleStall");
        }

        private bool IsSalesStallUnlocked(SalesStall station)
        {
            if (station == null)
            {
                return false;
            }

            if (station.buildingType == BuildingType.None)
            {
                return true;
            }

            var gameController = GameController.Instance;
            var playerData = PlayerDataModule.Instance?.data;
            if (gameController == null || playerData == null)
            {
                return false;
            }

            if (gameController.unlockedBuildingTypes.Contains(station.buildingType))
            {
                return true;
            }

            return playerData.structUnLockDataDic != null &&
                   playerData.structUnLockDataDic.TryGetValue(playerData.currentMapID, out var unlockedBuildings) &&
                   unlockedBuildings != null &&
                   unlockedBuildings.Contains(station.buildingType);
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
                if (!isMoving)   // 核心判断
                {
                    stayTime += Time.deltaTime;

                    if (stayTime >= 0.5f) // 0.5 秒
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
            if (playerInfo != null)
            {
                playerInfo.UpdateTxt();
            }
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
                if (playerInfo != null)
                {
                    playerInfo.UpdateTxt();
                }
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
            EventCenter.Instance.TriggerEvent(EventMessages.FocusView);
            StartCoroutine(ReturnAfterDelay(3f));
        }
        IEnumerator ReturnAfterDelay(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            EventCenter.Instance.TriggerEvent(EventMessages.RestoreFocusView);
        }
        private bool invincible = false;
        private float invincibleTime = 0.2f;
        public void TakeDamage(float damage)
        {
            if (isDead) return;
            if (invincible) return;
            StartCoroutine(InvincibleFrame());
            lastDamageTime = Time.time;
            if (regenCoroutine != null)
            {
                StopCoroutine(regenCoroutine);
                regenCoroutine = null;
            }
            isRegenerating = false;
            currentHp -= damage;
            if (playerInfo != null)
            {
                playerInfo.ShowHpInfo();
                playerInfo.UpdateFill(currentHp / maxHp);
            }
            if (currentHp <= 0)
            {
                DoDie();
            }
        }
        public void DoDie()
        {
            if (regenCoroutine != null)
            {
                StopCoroutine(regenCoroutine);
                regenCoroutine = null;
            }
            isRegenerating = false;
            transform.position = GameController.Instance.RespawnPoint.transform.position;
            currentHp = maxHp;
            playerInfo.ShowHpInfo();
            playerInfo.UpdateFill(currentHp / maxHp);
            goodsDic.Clear();
            dropDic.Clear();
            _pendingPickupCount = 0;
            if (playerInfo != null)
            {
                playerInfo.UpdateTxt();
            }
            EventCenter.Instance.TriggerEvent(EventMessages.UpdatePlayerInfo);
            EventCenter.Instance.TriggerEvent(EventMessages.UpdatePlayerCarryInfo);
        }
        private IEnumerator InvincibleFrame()
        {
            invincible = true;
            yield return new WaitForSeconds(invincibleTime);
            invincible = false;
        }

    }
}
