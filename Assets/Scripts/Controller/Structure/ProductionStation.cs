using System.Collections.Generic;
using Controller.Pickups;
using Module.Data;
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
        public SpriteRenderer icon;
        protected override void Start()
        {
            base.Start();
            EventCenter.Instance.AddListener(EventMessages.ProductionComplete, HandleProductionComplete);
            _assetHandle = GetComponent<AssetHandle>();
            productionInfo.Init(baseProductionTime, productionSpeed, currentMaterialCount, this);
            grid.basePosition = productPosition.position;
            ObjectPoolManager.Instance.WarmPool("Production", _productObj, 50);
            int newOrder = 3000 - Mathf.FloorToInt(transform.localPosition.y);
            icon.sortingOrder = newOrder + 2;
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
        public float ySpacing = 0.5f;

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