using System.Collections;
using System.Collections.Generic;
using Controller.Pickups;
using Module;
using Module.Data;
using Spine.Unity;
using UnityEngine;
using Utils;

namespace Controller.Structure
{
    public class CashierCounter                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                         : StructureBase
    {
        public Transform parchaseTransform1;
        public Transform parchaseTransform2;
        public Transform parchaseTransform;

        public PlacementGrid grid;
        public GameObject content_1;
        public GameObject content_2;
        public Transform exportTransform;
        public Transform exportTransform2;
        public List<CustomerController> customerList = new();
        public List<Production> coinList = new();
        public float speed = 1f;

        public Transform receiveTransform;
        public Transform receiveTransform1;
        public GameObject LingZhangShi1;
        public GameObject LingZhangShi2;
        public GameObject LingZhangShi3;
        public GameObject LingZhangShi1_1;
        public GameObject LingZhangShi2_2;
        public GameObject LingZhangShi3_3;
        public SkeletonAnimation skeletonAnimation1;
        public SkeletonAnimation skeletonAnimation2;
        public SkeletonAnimation skeletonAnimation3;
        public SkeletonAnimation skeletonAnimation4;
        public SkeletonAnimation skeletonAnimation5;
        public SkeletonAnimation skeletonAnimation6;
        public MeshRenderer rend1;
        public MeshRenderer rend2;
        public MeshRenderer rend3;
        public MeshRenderer rend4;
        public MeshRenderer rend5;
        public MeshRenderer rend6;
        public SpriteRenderer shadow_1;
        public SpriteRenderer shadow_2;
        public SpriteRenderer shadow_3;
        public SpriteRenderer shadow_4;
        public SpriteRenderer shadow_5;
        public SpriteRenderer shadow_6;


        public SpriteRenderer speedPoint_1;
        public SpriteRenderer uiPoint_1;
        public MeshRenderer meshRenderer_1;
        public SpriteRenderer speedPoint_2;
        public SpriteRenderer orderPoint;
        public MeshRenderer meshRenderer_2;
        public SpriteRenderer uiPoint_2;
        public MeshRenderer meshRenderer_3;
        private Coroutine initWhenReadyCoroutine;
        private SkeletonAnimation[] cashierAnimations;
        private const float VisualSyncInterval = 0.2f;
        private float visualSyncTimer;
        private string currentCashierLoopAnimation;

        [SerializeField] private int maxWaiters; // 最多服务员
        public int workingWaiters = 0;              // 当前忙的服务员数
        public override void Start()
        {
            base.Start();
            CacheCashierAnimations();
            HideAllCashierNpcs();
        }

        private void OnEnable()
        {
            CacheCashierAnimations();
            HideAllCashierNpcs();
            EventCenter.Instance.AddListener(EventMessages.CustomerArrived, HandleCustomerArrived);
            EventCenter.Instance.AddListener(EventMessages.StructureSpeedUp, HandleStructureSpeedUp);
            EventCenter.Instance.AddListener(EventMessages.StructureSpeedDown, HandleStructureSpeedDown);
            EventCenter.Instance.AddListener(EventMessages.UpdateSturctureLockInfo, Init);
            EventCenter.Instance.AddListener(EventMessages.UpdateLingZhangTai, Init);
            EventCenter.Instance.AddListener(EventMessages.UpdateFunctionState, Init);
            EventCenter.Instance.AddListener(EventMessages.DataPrepared, Init);
            EventCenter.Instance.AddListener(EventMessages.MapDataPrepared, Init);
            if (PlayerDataModule.Instance?.data != null && DataController.Instance != null)
            {
                Init();
            }
            if (initWhenReadyCoroutine != null)
            {
                StopCoroutine(initWhenReadyCoroutine);
            }
            initWhenReadyCoroutine = StartCoroutine(InitWhenReady());
            visualSyncTimer = 0f;
            currentCashierLoopAnimation = null;
            SyncCashierAnimations(true);

        }

        private void OnDisable()
        {
            EventCenter.Instance.RemoveListener(EventMessages.CustomerArrived, HandleCustomerArrived);
            EventCenter.Instance.RemoveListener(EventMessages.StructureSpeedUp, HandleStructureSpeedUp);
            EventCenter.Instance.RemoveListener(EventMessages.StructureSpeedDown, HandleStructureSpeedDown);
            EventCenter.Instance.RemoveListener(EventMessages.UpdateSturctureLockInfo, Init);
            EventCenter.Instance.RemoveListener(EventMessages.UpdateLingZhangTai, Init);
            EventCenter.Instance.RemoveListener(EventMessages.UpdateFunctionState, Init);
            EventCenter.Instance.RemoveListener(EventMessages.DataPrepared, Init);
            EventCenter.Instance.RemoveListener(EventMessages.MapDataPrepared, Init);
            if (initWhenReadyCoroutine != null)
            {
                StopCoroutine(initWhenReadyCoroutine);
                initWhenReadyCoroutine = null;
            }
        }
        void Update()
        {
            visualSyncTimer -= Time.deltaTime;
            if (visualSyncTimer <= 0f)
            {
                visualSyncTimer = VisualSyncInterval;
                TrySyncCashierVisualState();
            }

            SyncCashierAnimations();
            return;

            TrySyncCashierVisualState();

            if (customerList.Count > 0)
            {
                var currentAnimation1 = skeletonAnimation1.AnimationState.GetCurrent(0);

                if (currentAnimation1 == null || currentAnimation1.Animation.Name != "idle穿搭")
                {
                    skeletonAnimation1.AnimationState.SetAnimation(0, "idle穿搭", true);
                }

                var currentAnimation2 = skeletonAnimation2.AnimationState.GetCurrent(0);

                if (currentAnimation2 == null || currentAnimation2.Animation.Name != "idle穿搭")
                {
                    skeletonAnimation2.AnimationState.SetAnimation(0, "idle穿搭", true);
                }
                var currentAnimation3 = skeletonAnimation3.AnimationState.GetCurrent(0);

                if (currentAnimation3 == null || currentAnimation3.Animation.Name != "idle穿搭")
                {
                    skeletonAnimation3.AnimationState.SetAnimation(0, "idle穿搭", true);
                }

                var currentAnimation4 = skeletonAnimation4.AnimationState.GetCurrent(0);

                if (currentAnimation4 == null || currentAnimation4.Animation.Name != "idle穿搭")
                {
                    skeletonAnimation4.AnimationState.SetAnimation(0, "idle穿搭", true);
                }
                var currentAnimation5 = skeletonAnimation5.AnimationState.GetCurrent(0);

                if (currentAnimation5 == null || currentAnimation5.Animation.Name != "idle穿搭")
                {
                    skeletonAnimation5.AnimationState.SetAnimation(0, "idle穿搭", true);
                }
                var currentAnimation6 = skeletonAnimation6.AnimationState.GetCurrent(0);

                if (currentAnimation6 == null || currentAnimation6.Animation.Name != "idle穿搭")
                {
                    skeletonAnimation6.AnimationState.SetAnimation(0, "idle穿搭", true);
                }
            }
            else
            {
                var currentAnimation1 = skeletonAnimation1.AnimationState.GetCurrent(0);
                if (currentAnimation1 == null || currentAnimation1.Animation.Name != "待机")
                {
                    skeletonAnimation1.AnimationState.SetAnimation(0, "待机", true);
                }
                var currentAnimation2 = skeletonAnimation2.AnimationState.GetCurrent(0);
                if (currentAnimation2 == null || currentAnimation2.Animation.Name != "待机")
                {
                    skeletonAnimation2.AnimationState.SetAnimation(0, "待机", true);
                }
                var currentAnimation3 = skeletonAnimation3.AnimationState.GetCurrent(0);
                if (currentAnimation3 == null || currentAnimation3.Animation.Name != "待机")
                {
                    skeletonAnimation3.AnimationState.SetAnimation(0, "待机", true);
                }
                var currentAnimation4 = skeletonAnimation4.AnimationState.GetCurrent(0);
                if (currentAnimation4 == null || currentAnimation4.Animation.Name != "待机")
                {
                    skeletonAnimation4.AnimationState.SetAnimation(0, "待机", true);
                }
                var currentAnimation5 = skeletonAnimation5.AnimationState.GetCurrent(0);
                if (currentAnimation5 == null || currentAnimation5.Animation.Name != "待机")
                {
                    skeletonAnimation5.AnimationState.SetAnimation(0, "待机", true);
                }
                var currentAnimation6 = skeletonAnimation6.AnimationState.GetCurrent(0);
                if (currentAnimation6 == null || currentAnimation6.Animation.Name != "待机")
                {
                    skeletonAnimation6.AnimationState.SetAnimation(0, "待机", true);
                }
            }
        }

        public StructureLockData lockData;
        public StructureState lockstate;

        public void Init(params object[] args)
        {
            try
            {
                //   Debug.LogError("Init begin");

                var playerData = PlayerDataModule.Instance?.data;
                if (playerData == null || DataController.Instance == null)
                {
                    return;
                }

                if (GameController.Instance != null &&
                    GameController.Instance.unlockedBuildingTypes.Contains(structureType))
                {
                    var unlocked = playerData.structUnLockDataDic[playerData.currentMapID];
                    if (!unlocked.Contains(structureType))
                    {
                        unlocked.Add(structureType);
                    }

                    playerData.structLockDataDic[playerData.currentMapID].Remove(structureType);
                    playerData.structCanUnLockDataDic[playerData.currentMapID].Remove(structureType);
                }

                lockData = GetLockData(playerData.currentMapID);
                lockstate = GetStructureState(playerData, lockData);
                RefreshView(lockstate, lockData);
                if (initWhenReadyCoroutine != null && lockstate == StructureState.Unlocked)
                {
                    StopCoroutine(initWhenReadyCoroutine);
                    initWhenReadyCoroutine = null;
                }

                // Debug.LogError("Init end");
            }
            catch (System.Exception e)
            {
                //  Debug.LogError("Init EXCEPTION !!!");
                Debug.LogException(e);
            }
        }

        public StructureLockData GetLockData(int mapId)
        {
            var list = DataController.Instance.GetStructureLockList(mapId);
            return list?.Find(s => s.buildingType == structureType);
        }
        private StructureState GetStructureState(PlayerData playerData, StructureLockData lockData)
        {
            if (lockData == null)
                return StructureState.Unlocked;

            var locked = playerData.structLockDataDic[playerData.currentMapID];
            var unlocked = playerData.structUnLockDataDic[playerData.currentMapID];
            var canUnlock = playerData.structCanUnLockDataDic[playerData.currentMapID];

            if (unlocked.Contains(structureType))
                return StructureState.Unlocked;

            if (locked.Contains(structureType))
                return StructureState.Locked;

            return StructureState.CanUnlock;
        }
        private void RefreshView(StructureState state, StructureLockData lockData)
        {
            isLock = state == StructureState.Locked;
            isCanUnlockState = state == StructureState.CanUnlock;
            switch (state)
            {
                case StructureState.Locked:
                case StructureState.CanUnlock:
                    HideAllCashierNpcs();
                    ShowLock(lockData);
                    break;

                case StructureState.Unlocked:
                    content.SetActive(true);
                    if (PlayerDataModule.Instance.data.cashierData == null)
                    {
                        PlayerDataModule.Instance.data.cashierData = new CashierData();
                    }
                    if (PlayerDataModule.Instance.data.ordenFunction == 1)
                    {
                        ShowContent_2();
                    }
                    else
                    {
                        ShowContent_1();
                    }
                    PlayerData playerData = PlayerDataModule.Instance.data;
                    maxWaiters = GetActiveCashierCount();
                    if (!GameController.Instance.unlockedBuildingTypes.Contains(structureType))
                    {
                        GameController.Instance.unlockedBuildingTypes.Add(structureType);
                    }
                    TryProcessNextCustomer();
                    var unlocked = playerData.structUnLockDataDic[playerData.currentMapID];
                    if (!unlocked.Contains(structureType))
                    {
                        unlocked.Add(structureType);
                    }
                    break;
            }
        }


        public void ShowContent_1()
        {

            content_1.SetActive(true);
            content_2.SetActive(false);
            structureLock.gameObject.SetActive(false);
            parchaseTransform = parchaseTransform1;
            SetCashierNpcActiveState(LingZhangShi1_1, LingZhangShi2_2, LingZhangShi3_3, 0);
            if (PlayerDataModule.Instance.data.cardUpProgressesList.Find(s => s.developType == CardDevelopType.UpgradeLingZhangTai) != null)
            {
                var data = PlayerDataModule.Instance.data.cardUpProgressesList.Find(s => s.developType == CardDevelopType.UpgradeLingZhangTai);
                if (data.level == 1)
                {
                    sprite.sprite = _assetHandle.Get<Sprite>("一级灵账台");
                }
                else
                {
                    sprite.sprite = _assetHandle.Get<Sprite>("二级灵账台");
                }
            }
            SetCashierNpcActiveState(LingZhangShi1, LingZhangShi2, LingZhangShi3, GetActiveCashierCount());
            SyncCashierAnimations(true);

            int newOrder = 30000 - Mathf.RoundToInt(transform.position.y * 100);
            rend1.sortingOrder = 30000 - Mathf.RoundToInt((transform.position.y + LingZhangShi1.transform.localPosition.y) * 100) + 4;
            rend2.sortingOrder = 30000 - Mathf.RoundToInt((transform.position.y + LingZhangShi2.transform.localPosition.y) * 100) + 3;
            rend3.sortingOrder = 30000 - Mathf.RoundToInt((transform.position.y + LingZhangShi3.transform.localPosition.y) * 100) + 2;
            shadow_1.sortingOrder = rend1.sortingOrder - 1;
            shadow_2.sortingOrder = rend2.sortingOrder - 1; ;
            shadow_3.sortingOrder = rend3.sortingOrder - 1;
            speedPoint_1.sortingOrder = newOrder + 2;
            uiPoint_1.sortingOrder = newOrder + 2;
            meshRenderer_1.sortingOrder = newOrder + 2;
            grid.basePosition = exportTransform.position;
            RefreshCoinLayout();

            skeletonAnimation1.skeleton.SetAttachment("衣服", "1_2");
            skeletonAnimation2.skeleton.SetAttachment("衣服", "1_2");
            skeletonAnimation3.skeleton.SetAttachment("衣服", "1_2");
        }

        public void ShowContent_2()
        {
            parchaseTransform = parchaseTransform2;

            content_1.SetActive(false);
            content_2.SetActive(true);
            structureLock.gameObject.SetActive(false);
            grid.basePosition = exportTransform2.position;
            SetCashierNpcActiveState(LingZhangShi1, LingZhangShi2, LingZhangShi3, 0);
            SetCashierNpcActiveState(LingZhangShi1_1, LingZhangShi2_2, LingZhangShi3_3, GetActiveCashierCount());
            SyncCashierAnimations(true);
            int newOrder = 30000 - Mathf.RoundToInt(transform.position.y * 100);
            rend4.sortingOrder = 30000 - Mathf.RoundToInt((transform.position.y + LingZhangShi1_1.transform.localPosition.y) * 100) + 4;
            rend5.sortingOrder = 30000 - Mathf.RoundToInt((transform.position.y + LingZhangShi2_2.transform.localPosition.y) * 100) + +3;
            rend6.sortingOrder = 30000 - Mathf.RoundToInt((transform.position.y + LingZhangShi3_3.transform.localPosition.y) * 100) + +2;
            shadow_4.sortingOrder = rend4.sortingOrder - 1;
            shadow_5.sortingOrder = rend5.sortingOrder - 1;
            shadow_6.sortingOrder = rend6.sortingOrder - 1;
            speedPoint_2.sortingOrder = newOrder + 2;
            uiPoint_2.sortingOrder = newOrder + 2;
            orderPoint.sortingOrder = newOrder + 2;
            meshRenderer_2.sortingOrder = newOrder + 2;
            meshRenderer_3.sortingOrder = newOrder + 2;
            RefreshCoinLayout();
            skeletonAnimation4.skeleton.SetAttachment("衣服", "1_2");
            skeletonAnimation5.skeleton.SetAttachment("衣服", "1_2");
            skeletonAnimation6.skeleton.SetAttachment("衣服", "1_2");
        }

        private void HideAllCashierNpcs()
        {
            SetCashierNpcActiveState(LingZhangShi1, LingZhangShi2, LingZhangShi3, 0);
            SetCashierNpcActiveState(LingZhangShi1_1, LingZhangShi2_2, LingZhangShi3_3, 0);
        }

        private void CacheCashierAnimations()
        {
            if (cashierAnimations != null)
            {
                return;
            }

            cashierAnimations = new[]
            {
                skeletonAnimation1,
                skeletonAnimation2,
                skeletonAnimation3,
                skeletonAnimation4,
                skeletonAnimation5,
                skeletonAnimation6
            };
        }

        private void SetCashierNpcActiveState(GameObject npc1, GameObject npc2, GameObject npc3, int activeCount)
        {
            if (npc1 != null) npc1.SetActive(false);
            if (npc2 != null) npc2.SetActive(false);
            if (npc3 != null) npc3.SetActive(false);

            if (activeCount >= 1 && npc1 != null) npc1.SetActive(true);
            if (activeCount >= 2 && npc2 != null) npc2.SetActive(true);
            if (activeCount >= 3 && npc3 != null) npc3.SetActive(true);
        }

        private void SyncCashierAnimations(bool force = false)
        {
            CacheCashierAnimations();

            string targetAnimation = customerList.Count > 0 ? "idle穿搭" : "待机";
            if (!force && currentCashierLoopAnimation == targetAnimation)
            {
                return;
            }

            currentCashierLoopAnimation = targetAnimation;
            for (int i = 0; i < cashierAnimations.Length; i++)
            {
                var animation = cashierAnimations[i];
                if (animation == null || animation.AnimationState == null)
                {
                    continue;
                }

                animation.AnimationState.SetAnimation(0, targetAnimation, true);
            }
        }

        private void TrySyncCashierVisualState()
        {
            var playerData = PlayerDataModule.Instance?.data;
            if (playerData == null || GameController.Instance == null)
            {
                return;
            }

            bool isUnlocked = GameController.Instance.unlockedBuildingTypes.Contains(structureType);
            if (!isUnlocked &&
                playerData.structUnLockDataDic.TryGetValue(playerData.currentMapID, out var unlockedBuildings))
            {
                isUnlocked = unlockedBuildings.Contains(structureType);
            }

            if (!isUnlocked)
            {
                return;
            }

            if (content != null && !content.activeSelf)
            {
                content.SetActive(true);
            }

            if (structureLock != null && structureLock.gameObject.activeSelf)
            {
                structureLock.gameObject.SetActive(false);
            }

            int activeCount = GetActiveCashierCount();
            bool useSecondContent = playerData.ordenFunction == 1;

            if (useSecondContent)
            {
                bool shouldRefresh = !content_2.activeSelf ||
                                     content_1.activeSelf ||
                                     HasActiveCashier(LingZhangShi1, LingZhangShi2, LingZhangShi3) ||
                                     CountActiveCashiers(LingZhangShi1_1, LingZhangShi2_2, LingZhangShi3_3) != activeCount;
                if (shouldRefresh)
                {
                    ShowContent_2();
                }
            }
            else
            {
                bool shouldRefresh = !content_1.activeSelf ||
                                     content_2.activeSelf ||
                                     HasActiveCashier(LingZhangShi1_1, LingZhangShi2_2, LingZhangShi3_3) ||
                                     CountActiveCashiers(LingZhangShi1, LingZhangShi2, LingZhangShi3) != activeCount;
                if (shouldRefresh)
                {
                    ShowContent_1();
                }
            }
        }

        private bool HasActiveCashier(GameObject npc1, GameObject npc2, GameObject npc3)
        {
            return (npc1 != null && npc1.activeSelf) ||
                   (npc2 != null && npc2.activeSelf) ||
                   (npc3 != null && npc3.activeSelf);
        }

        private int CountActiveCashiers(GameObject npc1, GameObject npc2, GameObject npc3)
        {
            int count = 0;
            if (npc1 != null && npc1.activeSelf) count++;
            if (npc2 != null && npc2.activeSelf) count++;
            if (npc3 != null && npc3.activeSelf) count++;
            return count;
        }

        private IEnumerator InitWhenReady()
        {
            const float maxWaitTime = 5f;
            float waitTime = 0f;
            while (waitTime < maxWaitTime)
            {
                if (PlayerDataModule.Instance?.data != null &&
                    DataController.Instance != null &&
                    GameController.Instance != null)
                {
                    Init();
                    if (lockstate == StructureState.Unlocked)
                    {
                        initWhenReadyCoroutine = null;
                        yield break;
                    }
                }

                waitTime += Time.unscaledDeltaTime;
                yield return null;
            }

            initWhenReadyCoroutine = null;
        }

        private int GetActiveCashierCount()
        {
            var cashierData = PlayerDataModule.Instance?.data?.cashierData;
            if (cashierData == null)
            {
                return 1;
            }

            cashierData.maxpeopleLevel = Mathf.Max(1, cashierData.maxpeopleLevel);
            cashierData.peopleLevel = Mathf.Clamp(Mathf.Max(1, cashierData.peopleLevel), 1, cashierData.maxpeopleLevel);
            cashierData.totalNum = Mathf.Clamp(Mathf.Max(1, cashierData.totalNum, cashierData.peopleLevel), 1, cashierData.maxpeopleLevel);
            cashierData.workingNum = Mathf.Clamp(cashierData.workingNum, 0, cashierData.totalNum);
            return Mathf.Clamp(cashierData.totalNum, 1, 3);
        }

        private void HandleCustomerArrived(params object[] args)
        {
            if (args.Length < 1) return;

            CustomerController c = args[0] as CustomerController;
            customerList.Add(c);

            TryProcessNextCustomer();
        }

        private void TryProcessNextCustomer()
        {
            while (customerList.Count > 0 && workingWaiters < maxWaiters)
            {
                CustomerController customer = customerList[0];
                customerList.RemoveAt(0);
                workingWaiters++;
                StartCoroutine(HandleSingleCustomer(customer));
            }
        }

        private IEnumerator HandleSingleCustomer(CustomerController customer)
        {
            if (customer == null || customer.salesStall == null)
            {
                workingWaiters = Mathf.Max(0, workingWaiters - 1);
                TryProcessNextCustomer();
                yield break;
            }

            var playerData = PlayerDataModule.Instance?.data;
            if (playerData == null)
            {
                workingWaiters = Mathf.Max(0, workingWaiters - 1);
                TryProcessNextCustomer();
                yield break;
            }

            if (playerData.cashierData == null)
            {
                playerData.cashierData = new CashierData();
            }

            float progressTime = 0f;

            customer.fillBg.gameObject.SetActive(true);
            customer.fill.transform.localScale = new Vector3(0, 1, 1);

            while (true)
            {
                float productionTime = Mathf.Max(0.01f, playerData.cashierData.currentWorkingSpeed);
                progressTime += Time.deltaTime * Mathf.Max(0.01f, speed);
                float value = Mathf.Clamp01(progressTime / productionTime);
                customer.fill.transform.localScale = new Vector3(value, 1, 1);
                if (value >= 1f)
                {
                    break;
                }

                yield return null;
            }

            customer.fillBg.gameObject.SetActive(false);
            GoodsType goodsType = customer.salesStall.currentGoodsType;
            customer.state = NpcState.JieZhangChengGong;
            customer.RefreshMovementByState();

            EventCenter.Instance.TriggerEvent(
                EventMessages.SellTask,
                customer.salesStall.currentGoodsType,
                customer.data.carryNum
            );

            ProductStationData productStationdata = PlayerDataModule.Instance.GetProductStationDataByGoods(goodsType);
            if (productStationdata == null)
            {
                productStationdata = new ProductStationData(playerData.currentMapID, BuildingType.None, goodsType);
            }

            ProductionStation productionStation = GameController.Instance != null && GameController.Instance.productionStationList != null
                ? GameController.Instance.productionStationList.Find(x => x != null && x.goodsType == goodsType)
                : null;
            CardUpProgress cardData = null;
            if (productionStation != null)
            {
                switch (productionStation.buildingType)
                {
                    case BuildingType.YuShaHu_1:
                        cardData = playerData.cardUpProgressesList.Find(x => x.developType == CardDevelopType.UpgradeYuShaHu_1);
                        break;
                    case BuildingType.YuShaHu_2:
                        cardData = playerData.cardUpProgressesList.Find(x => x.developType == CardDevelopType.UpgradeYuShaHu_2);
                        break;
                    case BuildingType.YuShaHu_3:
                        cardData = playerData.cardUpProgressesList.Find(x => x.developType == CardDevelopType.UpgradeYuShaHu_3);
                        break;
                    case BuildingType.YuShaHu_4:
                        cardData = playerData.cardUpProgressesList.Find(x => x.developType == CardDevelopType.UpgradeYuShaHu_4);
                        break;
                    case BuildingType.LianQiLu_1:
                        cardData = playerData.cardUpProgressesList.Find(x => x.developType == CardDevelopType.UpgradeLianQiLu_1);
                        break;
                    case BuildingType.LianQiLu_2:
                        cardData = playerData.cardUpProgressesList.Find(x => x.developType == CardDevelopType.UpgradeLianQiLu_2);
                        break;
                    case BuildingType.LianQiLu_3:
                        cardData = playerData.cardUpProgressesList.Find(x => x.developType == CardDevelopType.UpgradeLianQiLu_3);
                        break;
                }
            }

            float mapPrice = 1f;
            if (DataController.Instance != null &&
                DataController.Instance.mapDataDic != null &&
                DataController.Instance.mapDataDic.TryGetValue(playerData.currentMapID, out var mapData))
            {
                mapPrice = mapData.price;
            }

            float basePrice = WorldData.goodsPriceDic.TryGetValue(goodsType, out var priceValue) ? priceValue : 0f;
            float totalNum;

            if (cardData != null)
            {
                totalNum =
                              basePrice * mapPrice * customer.data.carryNum
                              * playerData.cashierData.earning * (cardData.level * 0.2f + 1) + (productStationdata.priceLevel - 1) * 25;
            }
            else
            {
                totalNum =
                     basePrice * mapPrice * customer.data.carryNum
                        * playerData.cashierData.earning + (productStationdata.priceLevel - 1) * 25;
            }

            PrintingMoney(totalNum);
            workingWaiters--;
            TryProcessNextCustomer();
            if (PlayerDataModule.Instance.data.guideStep == GuideStep.ToLingZhangTai)
            {
                PlayerDataModule.Instance.data.guideStep = GuideStep.UpgradePot;
                UIController.Instance.Show<PlayerGuide>();
            }
        }



        public void PrintingMoney(float value)
        {
            GameObject productObj = GameObject.Instantiate(_assetHandle.Get<GameObject>("Production"));
            Transform activeReceiveTransform = GetActiveReceiveTransform();
            productObj.transform.position = activeReceiveTransform != null ? activeReceiveTransform.position : transform.position;
            Production product = productObj.GetComponent<Production>();
            product.Init(GoodsType.TongBi, (int)value);
            product.SetStation(this);
            RegisterCoin(product);
            Vector2 targetPos = grid.GetNextPosition();
            if (product.spriteRenderer != null)
            {
                product.spriteRenderer.sortingOrder = GetCoinSortingOrderByIndex(Mathf.Max(0, grid.currentIndex - 1));
            }
            product.FlyTo(targetPos, (() =>
            {
                product.canPickup = true;
                product.state = ItemState.OnWorkbench;
            }));

        }

        public Vector2 GetPickupRootPosition()
        {
            return grid.basePosition;
        }

        public void RegisterCoin(Production coin)
        {
            if (coin == null) return;
            if (!coinList.Contains(coin))
            {
                coinList.Add(coin);
            }
        }

        public int GetCoinSortingBaseOrder()
        {
            if (content_2 != null && content_2.activeInHierarchy)
            {
                if (rend4 != null)
                {
                    return rend4.sortingOrder;
                }
                if (LingZhangShi1_1 != null)
                {
                    return 30000 - Mathf.RoundToInt((transform.position.y + LingZhangShi1_1.transform.localPosition.y) * 100) + 4;
                }
            }
            else
            {
                if (rend1 != null)
                {
                    return rend1.sortingOrder;
                }
                if (LingZhangShi1 != null)
                {
                    return 30000 - Mathf.RoundToInt((transform.position.y + LingZhangShi1.transform.localPosition.y) * 100) + 4;
                }
            }
            return sprite != null ? sprite.sortingOrder + 3 : 30000 - Mathf.RoundToInt(transform.position.y * 100) + 3;
        }
        public int GetCoinSortingOrderByIndex(int index)
        {
            index = Mathf.Max(0, index);
            int columns = Mathf.Max(1, grid.columns);
            int rows = Mathf.Max(1, grid.rows);
            int layerSize = columns * rows;
            int layer = index / layerSize;
            return GetCoinSortingBaseOrder() + layer;
        }
        public void UnregisterCoin(Production coin)
        {
            if (coin == null) return;
            coinList.Remove(coin);
        }

        public void SortCoinsByHeight()
        {
            RefreshCoinLayout();
        }

        private Transform GetActiveReceiveTransform()
        {
            if (content_2 != null && content_2.activeInHierarchy && receiveTransform1 != null)
            {
                return receiveTransform1;
            }

            if (content_1 != null && content_1.activeInHierarchy && receiveTransform != null)
            {
                return receiveTransform;
            }

            return receiveTransform1 != null ? receiveTransform1 : receiveTransform;
        }

        private void RefreshCoinLayout()
        {
            coinList.RemoveAll(c => c == null);
            coinList.Sort((a, b) =>
            {
                if (a == null && b == null) return 0;
                if (a == null) return -1;
                if (b == null) return 1;
                int yCompare = a.transform.position.y.CompareTo(b.transform.position.y);
                if (yCompare != 0) return yCompare;
                return a.transform.position.x.CompareTo(b.transform.position.x);
            });

            for (int i = 0; i < coinList.Count; i++)
            {
                Production coin = coinList[i];
                if (coin == null) continue;
                Vector2 pos = grid.GetPositionByIndex(i);
                coin.transform.position = new Vector3(pos.x, pos.y, coin.transform.position.z);
                if (coin.spriteRenderer != null)
                {
                    coin.spriteRenderer.sortingOrder = GetCoinSortingOrderByIndex(i);
                }
            }

            grid.currentIndex = coinList.Count;
        }

        public bool TryAttractTopCoin(Transform picker, Transform receivePoint)
        {
            if (picker == null || receivePoint == null) return false;

            coinList.RemoveAll(c => c == null);

            for (int i = coinList.Count - 1; i >= 0; i--)
            {
                var coin = coinList[i];
                if (coin == null) continue;
                if (!coin.canPickup) continue;
                if (coin.isTaken) continue;
                if (coin.state != ItemState.OnWorkbench) continue;

                bool wasTaken = coin.isTaken;
                coin.StartAttract(picker, receivePoint);
                if (!wasTaken && coin.isTaken)
                {
                    grid.ReleaseOne();
                    return true;
                }
            }

            return false;
        }
        public void HandleStructureSpeedUp(params object[] args)
        {
            BuildingType t = (BuildingType)args[0];
            if (t != structureType)
            {
                return;
            }
            speed = 10f;
        }
        public void HandleStructureSpeedDown(params object[] args)
        {
            BuildingType t = (BuildingType)args[0];
            if (t != structureType)
            {
                return;
            }
            speed = 1f;
        }
    }
}

