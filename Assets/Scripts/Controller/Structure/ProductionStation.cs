using System.Collections.Generic;
using Controller.Pickups;
using Module;
using Module.Data;
using Spine;
using Spine.Unity;
using UnityEngine;
using Utils;
using View;

namespace Controller.Structure
{
    public class ProductionStation : StructureBase
    {
        // [Header("进度条位置")]
        // public Transform infoPosition;
        [Header("商品摆放位置")]
        public Transform productPosition;

        public Transform recivePosition;
        public Transform transferPoint;


        public int currentMaterialCount;  //当前材料数量
        public float baseProductionTime = 2.5f; // 基础生产时间
        public float productionSpeed = 1f;    // 外部可修改的速度倍率
        [Header("进度条信息类")]
        public ProductionInfo productionInfo;
        public DropItemType dropItemType;
        public GoodsType goodsType;
        public BuildingType buildingType;
        public GameObject _productObj;
        public PlacementGrid grid = new PlacementGrid();

        public List<Production> productionList = new List<Production>();

        public SpriteRenderer productIcon;
        public SpriteRenderer materialIcon;
        public Transform infoTransform;
        public SkeletonAnimation icon;

        protected override void Start()
        {
            base.Start();
            EventCenter.Instance.AddListener(EventMessages.ProductionComplete, HandleProductionComplete);

            if (productionInfo == null)
            {
                GameObject obj = GameObject.Instantiate(_assetHandle.Get<GameObject>("ProductionInfo"), GameObject.Find("HpCanvas").transform, false);
                productionInfo = obj.GetComponent<ProductionInfo>();
                productionInfo.Init(baseProductionTime, productionSpeed, currentMaterialCount, this);
                if (currentMaterialCount == 0)
                {
                    productionInfo.gameObject.SetActive(false);
                }
            }
            Init();
        }

        public void Init()
        {

            // PlayerData playerData = ModuleMgr.Instance.GetModule<PlayerDataModule>().data;
            // List<StructureLockData> structureLocks = new();
            // switch (playerData.currentMapID)
            // {
            //     case 1:
            //         structureLocks = DataController.Instance.structureLockDataList_1;
            //         break;
            //     case 2:
            //         structureLocks = DataController.Instance.structureLockDataList_2;
            //         break;
            //     case 3:
            //         structureLocks = DataController.Instance.structureLockDataList_3;
            //         break;
            //     case 4:
            //         structureLocks = DataController.Instance.structureLockDataList_4;
            //         break;
            //     case 5:
            //         structureLocks = DataController.Instance.structureLockDataList_5;
            //         break;
            // }
            // var lockData = structureLocks.Find(s => s.buildingType == buildingType);
            // if (lockData != null)
            // {
            //     var list = playerData.structLockDataDic[playerData.currentMapID];
            //     var list1 = playerData.structUnLockDataDic[playerData.currentMapID];
            //     var list2 = playerData.structCanUnLockDataDic[playerData.currentMapID];
            //     if (list2.Contains(buildingType))
            //     {
            //         isLock = false;
            //         isCanUnlockState = false;
            //     }
            //     else
            //     {
            //         if (list.Contains(buildingType))
            //         {
            //             isLock = true;
            //             isCanUnlockState = false;
            //         }
            //         else
            //         {
            //             isLock = false;
            //             isCanUnlockState = true;
            //         }
            //     }


            //     if (isLock)
            //     {
            //         content.SetActive(false);
            //         structureLock.gameObject.SetActive(true);
            //         structureLock.InitInfo(lockData);
            //     }
            //     else
            //     {
            //         if (isCanUnlockState)
            //         {
            //             content.SetActive(false);
            //             structureLock.gameObject.SetActive(true);
            //             structureLock.InitInfo(lockData);
            //         }
            //         else
            //         {
            //             content.SetActive(true);
            //             structureLock.gameObject.SetActive(false);
            //             productionInfo.Init(baseProductionTime, productionSpeed, currentMaterialCount, this);
            //             grid.basePosition = productPosition.position;
            //             ObjectPoolManager.Instance.WarmPool("Production", _productObj, 50);
            //             productIcon.sprite = _assetHandle.Get<Sprite>(Extensions.GetGoodsResNameByType(goodsType));
            //             materialIcon.sprite = _assetHandle.Get<Sprite>(Extensions.GetDropItemResNameByType(dropItemType));
            //             icon.sortingOrder = sprite.sortingOrder + 2;
            //             productIcon.sortingOrder = sprite.sortingOrder + 2;
            //             materialIcon.sortingOrder = sprite.sortingOrder + 2;
            //             switch (buildingType)
            //             {
            //                 case BuildingType.YuShaHu_1:
            //                     icon.sprite = _assetHandle.Get<Sprite>("1");
            //                     break;
            //                 case BuildingType.YuShaHu_2:
            //                     icon.sprite = _assetHandle.Get<Sprite>("2");
            //                     break;
            //                 case BuildingType.YuShaHu_3:
            //                     icon.sprite = _assetHandle.Get<Sprite>("3");
            //                     break;
            //                 case BuildingType.YuShaHu_4:
            //                     icon.sprite = _assetHandle.Get<Sprite>("4");
            //                     break;
            //                 case BuildingType.LianQiLu_1:
            //                     icon.sprite = _assetHandle.Get<Sprite>("5");
            //                     break;
            //                 case BuildingType.LianQiLu_2:
            //                     icon.sprite = _assetHandle.Get<Sprite>("6");
            //                     break;
            //                 case BuildingType.LianQiLu_3:
            //                     icon.sprite = _assetHandle.Get<Sprite>("7");
            //                     break;

            //             }
            //             grid.basePosition = productPosition.position;
            //         }
            //     }
            // }
            // else
            // {
            //     isLock = false;
            //     isCanUnlockState = false;
            //     content.SetActive(true);
            //     structureLock.gameObject.SetActive(false);
            //     productionInfo.Init(baseProductionTime, productionSpeed, currentMaterialCount, this);
            //     grid.basePosition = productPosition.position;
            //     ObjectPoolManager.Instance.WarmPool("Production", _productObj, 50);
            //     productIcon.sprite = _assetHandle.Get<Sprite>(Extensions.GetGoodsResNameByType(goodsType));
            //     materialIcon.sprite = _assetHandle.Get<Sprite>(Extensions.GetDropItemResNameByType(dropItemType));
            //     icon.sortingOrder = sprite.sortingOrder + 2;
            //     productIcon.sortingOrder = sprite.sortingOrder + 2;
            //     materialIcon.sortingOrder = sprite.sortingOrder + 2;
            //     switch (buildingType)
            //     {
            //         case BuildingType.YuShaHu_1:
            //             icon.sprite = _assetHandle.Get<Sprite>("1");
            //             break;
            //         case BuildingType.YuShaHu_2:
            //             icon.sprite = _assetHandle.Get<Sprite>("2");
            //             break;
            //         case BuildingType.YuShaHu_3:
            //             icon.sprite = _assetHandle.Get<Sprite>("3");
            //             break;
            //         case BuildingType.YuShaHu_4:
            //             icon.sprite = _assetHandle.Get<Sprite>("4");
            //             break;
            //         case BuildingType.LianQiLu_1:
            //             icon.sprite = _assetHandle.Get<Sprite>("5");
            //             break;
            //         case BuildingType.LianQiLu_2:
            //             icon.sprite = _assetHandle.Get<Sprite>("6");
            //             break;
            //         case BuildingType.LianQiLu_3:
            //             icon.sprite = _assetHandle.Get<Sprite>("7");
            //             break;

            //     }
            //     grid.basePosition = productPosition.position;
            // }


            var playerData = PlayerDataModule.Instance.data;

            var lockData = GetLockData(playerData.currentMapID);
            var state = GetStructureState(playerData, lockData);

            RefreshView(state, lockData);

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
            content.SetActive(true);
            structureLock.gameObject.SetActive(false);

            productionInfo.Init(baseProductionTime, productionSpeed, currentMaterialCount, this);
            grid.basePosition = productPosition.position;

            ObjectPoolManager.Instance.WarmPool("Production", _productObj, 50);

            productIcon.sprite = _assetHandle.Get<Sprite>(Extensions.GetGoodsResNameByType(goodsType));
            materialIcon.sprite = _assetHandle.Get<Sprite>(Extensions.GetDropItemResNameByType(dropItemType));

            int order = sprite.sortingOrder + 2;
            icon.GetComponent<MeshRenderer>().sortingOrder = order;
            productIcon.sortingOrder = order;
            materialIcon.sortingOrder = order;

            icon.initialSkinName = GetBuildingIcon().ToString();
        }


        private int GetBuildingIcon()
        {
            return buildingType switch
            {
                BuildingType.YuShaHu_1 => 1,
                BuildingType.YuShaHu_2 => 2,
                BuildingType.YuShaHu_3 => 3,
                BuildingType.YuShaHu_4 => 4,
                BuildingType.LianQiLu_1 => 1,
                BuildingType.LianQiLu_2 => 2,
                BuildingType.LianQiLu_3 => 3,
                _ => 1
            };
        }





        private void Update()
        {

        }

        public void AddMaterial(int count)
        {
            currentMaterialCount += count;
            productionInfo.UpdateText();
            // 强制激活 UI
            if (!productionInfo.gameObject.activeSelf)
                productionInfo.gameObject.SetActive(true);
            productionInfo.StartProductionLoop(this, structureType, baseProductionTime, productionSpeed);
            icon.AnimationState.SetAnimation(0, "animation", true);
            
        }

        public void SetSpeed(float speed)
        {
            productionSpeed = speed;
            productionInfo.UpdateSpeed(speed);
        }
        public void OnProductionFinished()
        {
            currentMaterialCount = 0;
            productionInfo.gameObject.SetActive(false); // 在这里关闭 UI
            icon.AnimationState.ClearTracks();
        }
        private void HandleProductionComplete(params object[] args)
        {
            BuildingType t = (BuildingType)args[0];
            if (t != structureType)
            {
                return;
            }
            GameObject productObj = ObjectPoolManager.Instance.GetObject("Production");
            productObj.transform.position = recivePosition.position;
            Production product = productObj.GetComponent<Production>();
            product.Init(goodsType);
            product.SetStation(this);
            product.spriteRenderer.sortingOrder = grid.currentIndex + 4000;
            productionList.Add(product);
            product.FlyTo(grid.GetNextPosition(), (() =>
            {
                product.canPickup = true;
                product.SetState(ItemState.OnWorkbench);
            }));

            if (currentMaterialCount == 0)
            {
                OnProductionFinished();
            }

        }




        private void OnDestroy()
        {
            if (productionInfo != null)
                Destroy(productionInfo.gameObject);
            EventCenter.Instance.RemoveListener(EventMessages.ProductionComplete, HandleProductionComplete);
        }


        public List<Production> TakeProduct(FreightClerkController freightClerk)
        {
            int num = freightClerk.currentCapacity;
            List<Production> list = new List<Production>();
            if (productionList.Count < num)
            {
                list = productionList;
                productionList.Clear();
            }
            else
            {
                list.AddRange(productionList.GetRange(0, num));
                productionList.RemoveRange(0, num);
            }

            for (int i = 0; i < list.Count; i++)
            {
                list[i].FlyTo(freightClerk.points[i].position);
            }

            return list;
        }
    }




    [System.Serializable]
    public class PlacementGrid
    {
        public int columns = 3;
        public int rows = 3;
        public int layers = 3;

        public float xSpacing = 1f;
        public float ySpacing = 0.2f;

        public Vector2 basePosition;

        public int currentIndex = 0;
        private float layerSpacing = 0.5f;

        public Vector2 GetNextPosition()
        {
            int layerSize = columns * rows;
            int maxIndex = layerSize * layers;

            if (currentIndex >= maxIndex)
                currentIndex = 0;  // 循环

            int index = currentIndex++;

            int layer = index / layerSize;
            int layerIndex = index % layerSize;

            int row = layerIndex / columns;
            int col = layerIndex % columns;

            float x = basePosition.x + col * xSpacing;
            float y = basePosition.y + layer * layerSpacing + row * ySpacing;

            return new Vector2(x, y);
        }

        public void ReleaseOne()
        {
            if (currentIndex > 0)
                currentIndex--;
        }

        public void Reset()
        {
            currentIndex = 0;
        }
    }



}