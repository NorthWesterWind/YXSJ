using System.Collections.Generic;
using Controller.Pickups;
using Module;
using Module.Data;
using UnityEngine;
using Utils;


namespace Controller.Structure
{
    public class SalesStall : StructureBase
    {
        public Transform receiveTransform;
        public GoodsType currentGoodsType;
        public int currentGoodsCount;
        public Transform baseTransform;
        public Transform parchaseTransform;
        public PlacementGrid grid;
        public List<Production> productList = new();
        public Transform transferPoint;
        public BuildingType buildingType;
        public SpriteRenderer productIcon;
        public SpriteRenderer productIconbg;

        public List<CustomerController> customerQueue = new();

        protected override void Start()
        {
            base.Start();
        }
        void OnEnable()
        {
            EventCenter.Instance.AddListener(EventMessages.UpdateSturctureLockInfo,Init);
            EventCenter.Instance.AddListener(EventMessages.CustomerArrivedSell, HandleCustomerArrived);
            EventCenter.Instance.AddListener(EventMessages.  CustomerLeave, HandleCustomerLeft);
          
        }
        void OnDisable()
        {
            EventCenter.Instance.RemoveListener(EventMessages.UpdateSturctureLockInfo, Init);
            EventCenter.Instance.RemoveListener(EventMessages.CustomerArrivedSell, HandleCustomerArrived);
              EventCenter.Instance.RemoveListener(EventMessages.CustomerLeave, HandleCustomerLeft);
        }
        
        public void HandleCustomerLeft(params object[] args)
        {
            if(args[0 ]as SalesStall  != this) return;
            CustomerController c = args[1] as CustomerController;
            if (customerQueue.Contains(c))
            {
                customerQueue.Remove(c);
            }
        }
        private void HandleCustomerArrived(params object[] args)
        {
            if (args.Length < 1) return;

            CustomerController c = args[0] as CustomerController;
            customerQueue.Add(c);
            TryServeNextCustomer();
        }

        public void Init(params object[] args)
        {

            var playerData = PlayerDataModule.Instance.data;
            var lockData = GetLockData(playerData.currentMapID);
            var state = GetStructureState(playerData, lockData);
            RefreshView(state, lockData);
        }

        public Vector2 GetPurchasePosition()
        {
            Vector2 pos = new Vector2(parchaseTransform.position.x + Random.Range(-1f, 1f), parchaseTransform.position.y + Random.Range(-1f, 0f));
            return pos;
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
                    ShowContent();
                    break;
            }
        }

        private void ShowContent()
        {
            if(content.activeSelf)
                return;
            content.SetActive(true);
            structureLock.gameObject.SetActive(false);
            grid.basePosition = baseTransform.position;
            productIcon.sprite = _assetHandle.Get<Sprite>(Extensions.GetGoodsResNameByType(currentGoodsType));
            int newOrder = 30000 - Mathf.RoundToInt(transform.position.y * 100);
            productIcon.sortingOrder = newOrder + 2;
            productIconbg.sortingOrder = newOrder + 1;
        }

        public StructureLockData GetLockData(int mapId)
        {
            var list = DataController.Instance.GetStructureLockList(mapId);
            return list?.Find(s => s.buildingType == buildingType);
        }
        private StructureState GetStructureState(PlayerData playerData, StructureLockData lockData)
        {
            if (lockData == null)
                return StructureState.Unlocked;

            var locked = playerData.structLockDataDic[playerData.currentMapID];
            var unlocked = playerData.structUnLockDataDic[playerData.currentMapID];
            var canUnlock = playerData.structCanUnLockDataDic[playerData.currentMapID];

            if (unlocked.Contains(buildingType))
                return StructureState.Unlocked;

            if (locked.Contains(buildingType))
                return StructureState.Locked;

            return StructureState.CanUnlock;
        }

        public void AddGoods(Production p)
        {
            p.SetState(ItemState.OnShelf);
            p.canPickup = true;
            productList.Add(p);
            currentGoodsCount++;
            TryServeNextCustomer();
            if (PlayerDataModule.Instance.data.guideStep == GuideStep.SellTea)
            {
                PlayerDataModule.Instance.data.guideStep = GuideStep.ToLingZhangTai;
                UIController.Instance.Show<PlayerGuide>();
            }
        }

        private void TryServeNextCustomer()
        {
            
            // 没顾客
            if (customerQueue.Count == 0)
                return;

            // 没商品
            if (productList.Count == 0)
                return;

            CustomerController customer = customerQueue[0];
            if (customer.data.carryNum > productList.Count)
                return;

            if (TryPurchase(customer, customer.data.carryNum, customer.purchaseList))
            {
                 customerQueue.RemoveAt(0);
                // 继续服务下一个
                 TryServeNextCustomer();
            }
        }


        /// <summary>
        /// 尝试购买指定数量商品，成功返回实际商品列表，失败返回空列表
        /// </summary>
        public bool TryPurchase( CustomerController customer ,int count, List<Production> outList)
        {
            if (productList.Count < count)
                return false;

            outList.Clear();

            // 循环 count 次，每次移除尾部元素
            for (int i = 0; i < count; i++)
            {
                int lastIndex = productList.Count - 1;
                Production p = productList[lastIndex];
                productList.RemoveAt(lastIndex);
                p.FlyTo(customer.receiveTransform.position);
                grid.ReleaseOne();
                outList.Add(p);
                currentGoodsCount--;
            }
            return true;
        }
        public void PlaceProduct(Production p)
        {
            var targetPos = grid.GetNextPosition();
            p.FlyTo(targetPos);
            p.SetState(ItemState.OnShelf);
        }

        public void ReceiveProduct(FreightClerkController controller)
        {
            List<Production> list = controller.productList;
            for (int i = 0; i < list.Count; i++)
            {
                PlaceProduct(list[i]);
            }
            controller.productList.Clear();
        }
    }
}