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

        public override void Start()
        {
            base.Start();
        }

        public StructureLockData lockData;
        public StructureState lockstate;
        public void Init(params object[] args)
        {
            var playerData = PlayerDataModule.Instance.data;
            if (GameController.Instance.unlockedBuildingTypes.Contains(buildingType))
            {
                var unlocked = playerData.structUnLockDataDic[playerData.currentMapID];
                if (!unlocked.Contains(buildingType))
                {
                    unlocked.Add(buildingType);
                }
                playerData.structLockDataDic[playerData.currentMapID].Remove(buildingType);
                playerData.structCanUnLockDataDic[playerData.currentMapID].Remove(buildingType);
            }
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
            isLock = state == StructureState.Locked;
            isCanUnlockState = state == StructureState.CanUnlock;
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
            var stationData = PlayerDataModule.Instance.data.ProductStationDataList
                .Find(x => x.buildingType == buildingType);
            if (stationData == null)
            {
                stationData = new ProductStationData(buildingType, goodsType);
                PlayerDataModule.Instance.data.ProductStationDataList.Add(stationData);
            }
            if (!WorldData.productStationWorkingTimeDic.TryGetValue(stationData.timelevel, out baseProductionTime))
            {
                baseProductionTime = WorldData.productStationWorkingTimeDic[1];
            }

            productionInfo.Init(currentMaterialCount, this);
            if (currentMaterialCount == 0)
            {
                productionInfo.gameObject.SetActive(false);
            }
            if (productPosition != null)
            {
                grid.basePosition = productPosition.position;
            }
            else
            {
                grid.basePosition = transform.position;
            }
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
            var unlocked = PlayerDataModule.Instance.data.structUnLockDataDic[PlayerDataModule.Instance.data.currentMapID];
            if (!unlocked.Contains(buildingType))
            {
                unlocked.Add(buildingType);
            }
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
            // 强制激活UI
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
            productionInfo.gameObject.SetActive(false); // 在这里关闭UI
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
            Vector2 targetPos = grid.GetNextPosition();
            if (product.spriteRenderer != null)
            {
                product.spriteRenderer.sortingOrder = grid.GetLastSortingOrder(sprite.sortingOrder, 3);
            }
            RegisterProduct(product);
            product.FlyTo(targetPos, (() =>
            {
                product.canPickup = true;
                product.SetState(ItemState.OnWorkbench);
                SortProductsByHeight();
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

            int num = Mathf.Min(freightClerk.currentCapacity, freightClerk.points.Count);
            if (num <= 0)
            {
                return list;
            }

            SortProductsByHeight();

            // 只挑选“未被占用”且“可被搬运”的商品，避免与玩家同帧抢夺
            for (int i = productionList.Count - 1; i >= 0 && list.Count < num; i--)
            {
                var production = productionList[i];
                if (production == null) continue;
                if (production.isTaken) continue;
                if (!production.canPickup) continue;
                if (production.state != ItemState.OnWorkbench) continue;

                // 预占：一旦生效，玩家无法再抢到该商品
                production.isTaken = true;
                production.canPickup = false;
                production.SetState(ItemState.HeldByAssistant);
                FreightClerkController.MarkProductReservedByFreight(production);
                list.Add(production);
                productionList.RemoveAt(i);
            }

            // 从生产台列表移除已预占的商品
            for (int i = 0; i < list.Count; i++)
            {
                var production = list[i];
                grid.ReleaseOne();
                var carryPoint = freightClerk.points[i];
                var carryTarget = carryPoint != null ? carryPoint.position : freightClerk.transform.position;
                production.FlyTo(carryTarget, () =>
                {
                    if (production != null)
                    {
                        // 到达搬运工挂点后释放占用标记，但保持不可被玩家拾取
                        if (carryPoint != null)
                        {
                            production.transform.SetParent(carryPoint, true);
                            production.transform.localPosition = Vector3.zero;
                        }
                        production.isTaken = false;
                        production.canPickup = false;
                        production.SetState(ItemState.HeldByAssistant);
                    }
                });
            }

            return list;
        }

        public Vector2 GetPickupRootPosition()
        {
            return grid.basePosition;
        }

        public void RegisterProduct(Production product)
        {
            if (product == null) return;
            if (!productionList.Contains(product))
            {
                productionList.Add(product);
            }
        }

        public void UnregisterProduct(Production product)
        {
            if (product == null) return;
            productionList.Remove(product);
        }

        public void SortProductsByHeight()
        {
            productionList.RemoveAll(p => p == null);
            productionList.Sort((a, b) =>
            {
                if (a == null && b == null) return 0;
                if (a == null) return -1;
                if (b == null) return 1;
                return a.transform.position.y.CompareTo(b.transform.position.y);
            });
        }

        private Production GetTopPickableProduct()
        {
            SortProductsByHeight();
            for (int i = productionList.Count - 1; i >= 0; i--)
            {
                var production = productionList[i];
                if (production == null) continue;
                if (production.isTaken) continue;
                if (!production.canPickup) continue;
                if (production.state != ItemState.OnWorkbench) continue;
                if (Controller.FreightClerkController.IsProductReservedByFreight(production)) continue;
                return production;
            }

            return null;
        }

        public bool TryAttractTopProduct(Transform picker, Transform receivePoint, System.Action onCancel = null)
        {
            if (picker == null || receivePoint == null) return false;

            var production = GetTopPickableProduct();
            if (production == null) return false;

            bool wasTaken = production.isTaken;
            production.StartAttract(picker, receivePoint, onCancel);
            if (!wasTaken && production.isTaken)
            {
                grid.ReleaseOne();
                productionList.Remove(production);
                return true;
            }

            return false;
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
        /// 逻辑：先填满第一行，填满所有行后开始新的一层向上堆叠
        /// </summary>
        public Vector2 GetNextPosition()
        {
            int index = currentIndex++;
            return GetPositionByIndex(index);
        }

        public Vector2 GetPositionByIndex(int index)
        {
            index = Mathf.Max(0, index);
            columns = Mathf.Max(1, columns);
            rows = Mathf.Max(1, rows);

            int layerSize = columns * rows; // 每层可放的物品数

            // 计算当前是第几层（从0开始）
            int layer = index / layerSize;

            // 计算在当前层中的索引: 0 ~ layerSize-1
            int indexInLayer = index % layerSize;

            // 计算在当前层中是第几行、第几列
            int row = indexInLayer / columns;
            int col = indexInLayer % columns;

            // 计算实际位置
            float x = basePosition.x + col * xSpacing;

            // y坐标 = 基础位置 + 层高 + 行深
            // 层高度：每层使用固定高度（不跨越整行），避免形成墙面感
            // 行深度：同一层内的行间距
            float layerHeight = layerSpacing;
            float y = basePosition.y + (layer * layerHeight) + (row * rowSpacing);

            return new Vector2(x, y);
        }
        public int GetSortingOrderByIndex(int baseOrder, int baseOffset, int index)
        {
            index = Mathf.Max(0, index);
            columns = Mathf.Max(1, columns);
            rows = Mathf.Max(1, rows);

            int layerSize = columns * rows;
            int layer = index / layerSize;
            int indexInLayer = index % layerSize;
            int row = indexInLayer / columns;

            int depth = (rows - 1 - row) + (layer * rows);
            return baseOrder + baseOffset + depth;
        }

        public int GetLastSortingOrder(int baseOrder, int baseOffset)
        {
            int index = Mathf.Max(0, currentIndex - 1);
            return GetSortingOrderByIndex(baseOrder, baseOffset, index);
        }

        public int GetSortingOrderByPosition(int baseOrder, int baseOffset, Vector2 position)
        {
            columns = Mathf.Max(1, columns);
            rows = Mathf.Max(1, rows);

            float yOffset = position.y - basePosition.y;
            if (rowSpacing <= 0f)
            {
                return baseOrder + baseOffset;
            }

            int depth = Mathf.RoundToInt(yOffset / rowSpacing);
            return baseOrder + baseOffset - depth;
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










