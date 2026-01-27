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
    public class CashierCounter : StructureBase
    {
        public Transform parchaseTransform1;
        public Transform parchaseTransform2;
        public Transform parchaseTransform;

        public PlacementGrid grid;
        public GameObject content_1;
        public GameObject content_2;
        public Transform exportTransform;
        public Transform exportTransform2;
        public Queue<CustomerController> customerQueue = new();
        private float baseTime;
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

        public SpriteRenderer speedPoint_1;
        public SpriteRenderer uiPoint_1;
        public MeshRenderer meshRenderer_1;
        public SpriteRenderer speedPoint_2;
        public SpriteRenderer orderPoint;
         public MeshRenderer meshRenderer_2;
        public SpriteRenderer uiPoint_2;
         public MeshRenderer meshRenderer_3;

        [SerializeField] private int maxWaiters; // 最多服务员
        private int workingWaiters = 0;              // 当前忙的服务员数
        protected override void Start()
        {
            base.Start();
        }

        private void OnEnable()
        {
            EventCenter.Instance.AddListener(EventMessages.CustomerArrived, HandleCustomerArrived);
            EventCenter.Instance.AddListener(EventMessages.StructureSpeedUp, HandleStructureSpeedUp);
            EventCenter.Instance.AddListener(EventMessages.StructureSpeedDown, HandleStructureSpeedDown);
            EventCenter.Instance.AddListener(EventMessages.UpdateSturctureLockInfo, Init);
        }

        private void OnDisable()
        {
            EventCenter.Instance.RemoveListener(EventMessages.CustomerArrived, HandleCustomerArrived);
            EventCenter.Instance.RemoveListener(EventMessages.StructureSpeedUp, HandleStructureSpeedUp);
            EventCenter.Instance.RemoveListener(EventMessages.StructureSpeedDown, HandleStructureSpeedDown);
            EventCenter.Instance.RemoveListener(EventMessages.UpdateSturctureLockInfo, Init);
        }
        void Update()
        {
            if (customerQueue.Count > 0)
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
            Debug.LogError("CashierCounter Init");
            PlayerData playerData = PlayerDataModule.Instance.data;
            lockData = GetLockData(playerData.currentMapID);
            lockstate = GetStructureState(playerData, lockData);
            RefreshView(lockstate, lockData);
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
            switch (state)
            {
                case StructureState.Locked:
                case StructureState.CanUnlock:
                    ShowLock(lockData);
                    break;

                case StructureState.Unlocked:
                    if (PlayerDataModule.Instance.data.ordenFunction == 1)
                    {
                        ShowContent_2();
                    }
                    else
                    {
                        ShowContent_1();
                    }
                    PlayerData playerData = PlayerDataModule.Instance.data;
                    baseTime = playerData.cashierData.currentWorkingSpeed;
                    maxWaiters = playerData.cashierData.totalNum;
                    break;
            }
        }


        public void ShowContent_1()
        {
            content_1.SetActive(true);
            content_2.SetActive(false);
            structureLock.gameObject.SetActive(false);
            parchaseTransform = parchaseTransform1;
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
            switch (PlayerDataModule.Instance.data.cashierData.totalNum)
            {
                case 1:
                    LingZhangShi1.SetActive(true);
                    LingZhangShi2.SetActive(false);
                    LingZhangShi3.SetActive(false);
                    break;
                case 2:
                    LingZhangShi1.SetActive(true);
                    LingZhangShi2.SetActive(true);
                    LingZhangShi3.SetActive(false);
                    break;
                case 3:
                    LingZhangShi1.SetActive(true);
                    LingZhangShi2.SetActive(true);
                    LingZhangShi3.SetActive(true);
                    break;
            }
            int newOrder = 30000 - Mathf.RoundToInt(transform.position.y * 100);
            rend1.sortingOrder = newOrder + 3;
            rend2.sortingOrder = newOrder + 2;
            rend3.sortingOrder = newOrder + 1;
            speedPoint_1.sortingOrder = newOrder + 2;
            uiPoint_1.sortingOrder = newOrder + 2;
            meshRenderer_1.sortingOrder = newOrder + 2;
            grid.basePosition = exportTransform.position;
        }

        public void ShowContent_2()
        {
            parchaseTransform = parchaseTransform2;
        
            content_1.SetActive(false);
            content_2.SetActive(true);
            structureLock.gameObject.SetActive(false);
            grid.basePosition = exportTransform2.position;
            switch (PlayerDataModule.Instance.data.cashierData.totalNum)
            {
                case 1:
                    LingZhangShi1_1.SetActive(true);
                    LingZhangShi2_2.SetActive(false);
                    LingZhangShi3_3.SetActive(false);
                    break;
                case 2:
                    LingZhangShi1_1.SetActive(true);
                    LingZhangShi2_2.SetActive(true);
                    LingZhangShi3_3.SetActive(false);
                    break;
                case 3:
                    LingZhangShi1_1.SetActive(true);
                    LingZhangShi2_2.SetActive(true);
                    LingZhangShi3_3.SetActive(true);
                    break;
            }
            int newOrder = 30000 - Mathf.RoundToInt(transform.position.y * 100);
            rend4.sortingOrder = newOrder + 3;
            rend5.sortingOrder = newOrder + 2;
            rend6.sortingOrder = newOrder + 1;
            speedPoint_2.sortingOrder = newOrder + 2;
            uiPoint_2.sortingOrder = newOrder + 2;
            orderPoint.sortingOrder = newOrder + 2;
            meshRenderer_2.sortingOrder = newOrder + 2;
            meshRenderer_3.sortingOrder = newOrder + 2;
        }

        private void HandleCustomerArrived(params object[] args)
        {
            if (args.Length < 1) return;

            CustomerController c = args[0] as CustomerController;
            customerQueue.Enqueue(c);

            TryProcessNextCustomer();
        }

        private void TryProcessNextCustomer()
        {
            // 没顾客
            if (customerQueue.Count == 0) return;

            // 服务员已满
            if (workingWaiters >= maxWaiters) return;

            // 分配一个顾客
            CustomerController customer = customerQueue.Dequeue();
            workingWaiters++;

            StartCoroutine(HandleSingleCustomer(customer));
        }
        private IEnumerator HandleSingleCustomer(CustomerController customer)
        {
            float t = 0f;
            float productionTime = baseTime / speed;

            customer.fillBg.gameObject.SetActive(true);
            customer.fill.transform.localScale = new Vector3(0, 1, 1);

            while (t < productionTime)
            {
                t += Time.deltaTime;
                float value = t / productionTime;
                customer.fill.transform.localScale = new Vector3(value, 1, 1);
                yield return null;
            }

            customer.fillBg.gameObject.SetActive(false);
            customer.state = NpcState.JieZhangChengGong;
            customer.SetNextPosition();
            customer.agent.Stop();
            customer.agent.SetDestination(customer.nextPosition);

            EventCenter.Instance.TriggerEvent(
                EventMessages.SellTask,
                customer.salesStall.currentGoodsType,
                customer.data.carryNum
            );
            float totalNum = WorldData.goodsPriceDic[customer.salesStall.currentGoodsType] * customer.data.carryNum;
            totalNum *= PlayerDataModule.Instance.data.cashierData.earning;
            PrintingMoney(totalNum);
            workingWaiters--;
            TryProcessNextCustomer();
            if (PlayerDataModule.Instance.data.guideStep == GuideStep.ToLingZhangTai)
            {
                PlayerDataModule.Instance.data.guideStep =  GuideStep.Checkout;
                UIController.Instance.Show<PlayerGuide>();
            }
        }


        public void PrintingMoney(float value)
        {
            GameObject productObj = ObjectPoolManager.Instance.GetObject("Production");
            productObj.transform.position = receiveTransform.position;
            Production product = productObj.GetComponent<Production>();
            product.Init(GoodsType.TongBi, (int)value);
            product.SetStation(this);
            product.spriteRenderer.sortingOrder = sprite.sortingOrder + 3;
            product.FlyTo(grid.GetNextPosition(), (() =>
            {
                product.canPickup = true;
                product.state = ItemState.OnWorkbench;
            }));

        }
        public void HandleStructureSpeedUp(params object[] args)
        {
            BuildingType t = (BuildingType)args[0];
            if (t != structureType)
            {
                return;
            }
            speed = 1.5f;
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
