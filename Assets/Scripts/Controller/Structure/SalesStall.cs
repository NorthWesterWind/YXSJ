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
        public SpriteRenderer  productIcon;
        public SpriteRenderer  productIconbg;

        protected override void Start()
        {
            base.Start();
            Init();
        }

        public void Init()
        {
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
            var lockData = structureLocks.Find(s => s.buildingType == buildingType);
            if (lockData != null)
            {
                var progressData = playerData.structureLockDataList.Find(s => s.buildType == buildingType && s.lockId == lockData.lockId && s.mapId == playerData.currentMapID);
                if (progressData != null && progressData.isUnlock)
                {
                    content.SetActive(true);
                    structureLock.gameObject.SetActive(false);
                    grid.basePosition = baseTransform.position;
                    GameController.Instance.goodBuild.Add(currentGoodsType, this);
                }
                else
                {
                    content.SetActive(false);
                    structureLock.gameObject.SetActive(true);
                    structureLock.InitInfo(lockData);
                }
            }
            productIcon.sprite = _assetHandle.Get<Sprite>(Extensions.GetGoodsResNameByType(currentGoodsType));
            productIcon.sortingOrder = sprite.sortingOrder+2;
            productIconbg.sortingOrder = sprite.sortingOrder+1;
        }

        public void AddGoods(Production p)
        {
            p.SetState(ItemState.OnShelf);
            p.canPickup = true;
            productList.Add(p);
            currentGoodsCount++;
        }

        /// <summary>
        /// 尝试购买指定数量商品，成功返回实际商品列表，失败返回空列表
        /// </summary>
        public bool TryPurchase(int count, List<Production> outList)
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