using System.Collections.Generic;
using Controller.Pickups;
using Module;
using Module.Data;
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
        public float baseProductionTime; // 基础生产时间
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
        public SpriteRenderer turnIcon;
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
            if (GameController.Instance.unlockedBuildingTypes.Contains(buildingType))
            {
                return;
            }
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
            if (_assetHandle == null)
                _assetHandle = GetComponent<AssetHandle>();
            baseProductionTime = WorldData.productStationWorkingTimeDic[PlayerDataModule.Instance.data.ProductStationDataList.Find(x => x.buildingType == buildingType).timelevel];

            productionInfo.Init(currentMaterialCount, this);
            if (currentMaterialCount == 0)
            {
                productionInfo.gameObject.SetActive(false);
            }
            grid.basePosition = productPosition.position;
            productIcon.sprite = _assetHandle.Get<Sprite>(Extensions.GetGoodsResNameByType(goodsType));
            materialIcon.sprite = _assetHandle.Get<Sprite>(Extensions.GetDropItemResNameByType(dropItemType));

            int newOrder = 30000 - Mathf.RoundToInt(transform.position.y * 100);
            int order = newOrder + 2;
            icon.GetComponent<MeshRenderer>().sortingOrder = order;
            productIcon.sortingOrder = order;
            materialIcon.sortingOrder = order;
            turnIcon.sortingOrder = order;
            GetComponent<Canvas>().sortingOrder = order + 2;

            icon.initialSkinName = GetBuildingIcon().ToString();
            GameController.Instance.unlockedBuildingTypes.Add(buildingType);
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
            productionInfo.StartProductionLoop(this, structureType);
            icon.AnimationState.SetAnimation(0, "animation", true);
            if (dropItemType == DropItemType.ShuangYunZhiFragment &&
                PlayerDataModule.Instance.data.guideStep == GuideStep.DeliverMaterial)
            {
                PlayerDataModule.Instance.data.guideStep = GuideStep.BuildAccountDesk;
                UIController.Instance.Show<PlayerGuide>();
            }
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
            GameObject productObj = GameObject.Instantiate(_assetHandle.Get<GameObject>("Production"));
            productObj.transform.position = recivePosition.position;
            Production product = productObj.GetComponent<Production>();
            EventCenter.Instance.TriggerEvent(EventMessages.ProduceTask, goodsType);
            product.Init(goodsType);
            product.SetStation(this);
            product.spriteRenderer.sortingOrder = sprite.sortingOrder + 3;
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
          
        }


        public List<Production> TakeProduct(FreightClerkController freightClerk)
        {
            List<Production> list = new List<Production>();

            // 防止玩家已经拿走所有产品导致的空列表
            if (productionList.Count == 0)
            {
                return list; // 返回空列表
            }

            int num = freightClerk.currentCapacity;

            if (productionList.Count <= num)
            {
                list.AddRange(productionList);
                productionList.Clear();
            }
            else
            {
                list.AddRange(productionList.GetRange(0, num));
                productionList.RemoveRange(0, num);
            }

            for (int i = 0; i < list.Count; i++)
            {
                list[i].canPickup = false;             // 标记为不可拾取，防止在飞行途中被玩家抢夺
                list[i].SetState(ItemState.HeldByAssistant);
                grid.ReleaseOne();
                list[i].FlyTo(freightClerk.points[i].position);
            }

            return list;
        }
    }




    [System.Serializable]
    public class PlacementGrid
    {
        [Header("网格配置")]
        [Tooltip("每行的列数")]
        public int columns = 3;

        [Tooltip("每层的行数")]
        public int rows = 3;

        [Header("间距配置")]
        [Tooltip("列之间的水平间距")]
        public float xSpacing = 0.3f;

        [Tooltip("行之间的垂直间距（用于模拟深度）")]
        public float rowSpacing = 0.15f;

        [Tooltip("层之间的垂直间距（堆叠高度）")]
        public float layerSpacing = 0.4f;

        [Header("运行时数据")]
        public Vector2 basePosition;
        public int currentIndex = 0;

        /// <summary>
        /// 获取下一个摆放位置
        /// 逻辑：先填满第一行 → 填满所有行 → 开始新的一层向上堆叠
        /// </summary>
        public Vector2 GetNextPosition()
        {
            int layerSize = columns * rows; // 每层可放的物品数
            int index = currentIndex++;

            // 计算当前是第几层（从0开始）
            int layer = index / layerSize;

            // 计算在当前层中的索引（0 ~ layerSize-1）
            int indexInLayer = index % layerSize;

            // 计算在当前层中是第几行、第几列
            int row = indexInLayer / columns;
            int col = indexInLayer % columns;

            // 计算实际位置
            float x = basePosition.x + col * xSpacing;

            // y坐标 = 基础位置 + 层高度 + 行深度
            // 层高度：让商品向上堆叠
            // 行深度：同一层内，后面的行y稍高，产生2.5D深度感
            float y = basePosition.y + (layer * layerSpacing) + (row * rowSpacing);

            return new Vector2(x, y);
        }

        /// <summary>
        /// 释放一个位置（商品被拿走时调用）
        /// </summary>
        public void ReleaseOne()
        {
            if (currentIndex > 0)
                currentIndex--;
        }

        /// <summary>
        /// 重置网格（清空所有物品时调用）
        /// </summary>
        public void Reset()
        {
            currentIndex = 0;
        }
    }



}