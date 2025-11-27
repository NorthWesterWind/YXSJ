using System;
using System.Collections;
using System.Collections.Generic;
using Controller.Pickups;
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
        protected override void Start()
        {
            base.Start();
            Init();
        }

        private void OnEnable()
        {
            EventCenter.Instance.AddListener(EventMessages.CustomerArrived, HandleCustomerArrived);
        }

        private void OnDisable()
        {
            EventCenter.Instance.RemoveListener(EventMessages.CustomerArrived, HandleCustomerArrived);
        }

        public void Init()
        {
            fillImage.size = new Vector2( 0 ,0);
            GameController.Instance.buildings.Add(BuildingType.LingZhangTai, this);
            grid.basePosition = exportTransform.position;
        }

        private IEnumerator ProcessCustomers()
        {
            while (true)
            {
                // 队列为空 → 跳出循环，协程停止
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
            product.Init(GoodsType.YingQian);
            product.SetStation(this);
            product.spriteRenderer.sortingOrder = 4000 + grid.currentIndex;
            product.FlyTo(grid.GetNextPosition() , (() =>
            {
                product.canPickup = true;
                product.state = ItemState.OnWorkbench;
            }));
        }
    }
}
