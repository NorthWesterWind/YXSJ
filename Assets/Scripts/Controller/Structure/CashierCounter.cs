using System;
using System.Collections;
using System.Collections.Generic;
using Controller.Pickups;
using Module;
using Module.Data;
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
        public SpriteRenderer fillImage;
        public Transform receiveTransform;
        public GameObject LingZhangShi;
        public MeshRenderer renderer;
        protected override void Start()
        {
            base.Start();
            Init();
        }

        private void OnEnable()
        {
            EventCenter.Instance.AddListener(EventMessages.CustomerArrived, HandleCustomerArrived);
            EventCenter.Instance.AddListener(EventMessages.StructureSpeedUp, HandleStructureSpeedUp);
            EventCenter.Instance.AddListener(EventMessages.StructureSpeedDown, HandleStructureSpeedDown);
        }

        private void OnDisable()
        {
            EventCenter.Instance.RemoveListener(EventMessages.CustomerArrived, HandleCustomerArrived);
            EventCenter.Instance.RemoveListener(EventMessages.StructureSpeedUp, HandleStructureSpeedUp);
            EventCenter.Instance.RemoveListener(EventMessages.StructureSpeedDown, HandleStructureSpeedDown);
        }

        public void Init()
        {
            fillImage.size = new Vector2( 0 ,0);
            grid.basePosition = exportTransform.position;


            PlayerData playerData = ModuleMgr.Instance.GetModule<PlayerDataModule>().data;
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
                var progressData = playerData.structureLockDataList.Find(s => s.buildType == BuildingType.LingZhangTai && s.lockId == lockData.lockId && s.mapId == playerData.currentMapID);
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



            int newOrder = 3500 - Mathf.FloorToInt(transform.localPosition.y);
            renderer.sortingOrder = newOrder;
        }

        private IEnumerator ProcessCustomers()
        {
            while (true)
            {
                if (customerQueue.Count == 0)
                {
                    processCoroutine = null;
                    fillImage.size = new Vector2( 0 ,0);
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

            fillImage.size= new Vector2(0,0.08f);

            while (t < productionTime)
            {
                t += Time.deltaTime;
                float value = t / productionTime;
                fillImage.size =  new Vector2( 2.9f*value ,0.08f )  ;
                yield return null;
            }
            fillImage.size= new Vector2(2.9f,0.08f);
            customer.state = NpcState.JieZhangChengGong;
            customer.SetNextPosition();
            customer.agent.SetDestination(customer.nextPosition);
            PrintingMoney();
        
           
        }


        public void PrintingMoney()
        {
            GameObject productObj = ObjectPoolManager.Instance.GetObject("Production");
            productObj.transform.position = receiveTransform.position;
            Production product =  productObj.GetComponent<Production>();
            product.Init(GoodsType.TongBi);
            product.SetStation(this);
            product.spriteRenderer.sortingOrder = 4000 + grid.currentIndex;
            
            product.FlyTo(grid.GetNextPosition() , (() =>
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
            speed = 2f;
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
