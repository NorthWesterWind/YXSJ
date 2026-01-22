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
        public Transform parchaseTransform;
        public PlacementGrid grid;
        public Transform exportTransform;
        public Queue<CustomerController> customerQueue = new();
        private Coroutine processCoroutine;
        public float baseTime;
        public float speed = 1f;

        public Transform receiveTransform;
        public GameObject LingZhangShi;
        public SkeletonAnimation skeletonAnimation;
        public MeshRenderer rend;

        public SpriteRenderer point_1;
        public SpriteRenderer point_2;
        public SpriteRenderer point_3;
        public SpriteRenderer point_4;

    

        
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
                var currentAnimation = skeletonAnimation.AnimationState.GetCurrent(0);

                if (currentAnimation == null || currentAnimation.Animation.Name != "idle穿搭")
                {
                    skeletonAnimation.AnimationState.SetAnimation(0, "idle穿搭", true);
                }



            }
            else
            {
                var currentAnimation = skeletonAnimation.AnimationState.GetCurrent(0);
                if (currentAnimation == null || currentAnimation.Animation.Name != "待机")
                {
                    skeletonAnimation.AnimationState.SetAnimation(0, "待机", true);
                }
            }
        }


        public void Init(params object[] args)
        {
            grid.basePosition = exportTransform.position;
            PlayerData playerData =PlayerDataModule.Instance.data;
            List<StructureLockData> structureLocks = new();
            switch (playerData.currentMapID)
            {
                case 1:
                    structureLocks = DataController.Instance.structureLockDataList_1;
                    break;
                case 2:
                    structureLocks = DataController.Instance.structureLockDataList_2;
                    break;
                case 3:
                    structureLocks = DataController.Instance.structureLockDataList_3;
                    break;
                case 4:
                    structureLocks = DataController.Instance.structureLockDataList_4;
                    break;
                case 5:
                    structureLocks = DataController.Instance.structureLockDataList_5;
                    break;
            }
            var lockData = structureLocks.Find(s => s.buildingType == BuildingType.LingZhangTai);
            if (lockData != null)
            {
                var progressData = playerData.structureLockProgressDataList.Find(s => s.buildType == BuildingType.LingZhangTai && s.lockId == lockData.lockId && s.mapId == playerData.currentMapID);
                if (progressData != null && progressData.isUnlock)
                {
                    content.SetActive(true);
                    structureLock.gameObject.SetActive(false);
                }
                else
                {
                    content.SetActive(false);
                    structureLock.gameObject.SetActive(true);
                    structureLock.InitInfo(lockData);
                }
            }
            int newOrder = 30000 - Mathf.RoundToInt(transform.position.y * 100);
            rend.sortingOrder = newOrder + 2;
            point_1.sortingOrder = newOrder + 2;
            point_2.sortingOrder = newOrder + 2;
            point_3.sortingOrder = newOrder + 2;
            point_4.sortingOrder = newOrder + 2;

        }

        private IEnumerator ProcessCustomers()
        {
            while (true)
            {
                if (customerQueue.Count == 0)
                {
                    processCoroutine = null;
                    yield break;
                }
                CustomerController customer = customerQueue.Dequeue();
                yield return StartCoroutine(HandleSingleCustomer(customer));
            }
        }

        private void HandleCustomerArrived(params object[] args)
        {
            if (args.Length < 1)
            {
                return;
            }

            CustomerController c = args[0] as CustomerController;
            customerQueue.Enqueue(c);
            // 没有正在处理 → 开启循环
            if (processCoroutine == null)
            {
                processCoroutine = StartCoroutine(ProcessCustomers());
            }
        }

        private IEnumerator HandleSingleCustomer(CustomerController customer)
        {
            float t = 0f;
            float productionTime = baseTime / speed;
            customer.fillBg.gameObject.SetActive(true);
            customer.fill.gameObject.transform.localScale = new Vector3(0, 1, 1);
            while (t < productionTime)
            {
                t += Time.deltaTime;
                float value = t / productionTime;
                customer.fill.gameObject.transform.localScale = new Vector3(1f * value, 1, 1);
                yield return null;
            }
            customer.fillBg.gameObject.SetActive(false);
            customer.state = NpcState.JieZhangChengGong;
            customer.SetNextPosition();
            customer.agent.Stop();
            customer.agent.SetDestination(customer.nextPosition);
            PrintingMoney();
        }


        public void PrintingMoney()
        {
            GameObject productObj = ObjectPoolManager.Instance.GetObject("Production");
            productObj.transform.position = receiveTransform.position;
            Production product = productObj.GetComponent<Production>();
            product.Init(GoodsType.TongBi);
            product.SetStation(this);
            product.spriteRenderer.sortingOrder = 4000 + grid.currentIndex;

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
