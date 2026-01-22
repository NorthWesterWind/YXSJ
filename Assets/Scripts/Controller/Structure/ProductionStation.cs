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

    
        void OnEnable()
        {
            EventCenter.Instance.AddListener(EventMessages.UpdateSturctureLockInfo, Init);
            EventCenter.Instance.AddListener(EventMessages.ProductionComplete, HandleProductionComplete);
        }
        void OnDisable()
        {
            EventCenter.Instance.RemoveListener(EventMessages.ProductionComplete, HandleProductionComplete);
            EventCenter.Instance.RemoveListener(EventMessages.UpdateSturctureLockInfo, Init);
        }

        protected override void Start()
        {
            base.Start();
        }

        public StructureLockData lockData;
        public StructureState lockstate;
        public void Init(params object[] args)
        {
            var playerData = PlayerDataModule.Instance.data;
            lockData = GetLockData(playerData.currentMapID);
            lockstate = GetStructureState(playerData, lockData);
            RefreshView(lockstate, lockData);
           
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