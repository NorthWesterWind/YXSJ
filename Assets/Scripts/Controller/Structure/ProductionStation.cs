using System;
using Module.Data;
using UnityEngine;
using Utils;
using View;
using View._3D;

namespace Controller.Structure
{
    public class ProductionStation : StructureBase
    {
        // [Header("进度条位置")]
        // public Transform infoPosition;
        [Header("商品摆放位置")]
        public Transform productPosition;
        
        public Transform recivePosition;
        
        public int currentMaterialCount;  //当前材料数量
        public float baseProductionTime = 2.5f; // 基础生产时间
        public float productionSpeed = 1f;    // 外部可修改的速度倍率
        [Header("进度条信息类")]
        public ProductionInfo productionInfo;
        public DropItemType dropItemType;
        public GoodsType goodsType;
        
        public GameObject _productObj;
        private AssetHandle _assetHandle;
        public PlacementGrid grid = new PlacementGrid();
        
        protected override void Start()
        {
            base.Start();
            EventCenter.Instance.AddListener(EventMessages.ProductionComplete , HandleProductionComplete);
           // ObjectPoolManager.Instance.WarmPool("Production" , _productObj , 40 );
            _assetHandle = GetComponent<AssetHandle>();
            productionInfo.Init(baseProductionTime , productionSpeed , currentMaterialCount ,this );
            grid.basePosition = productPosition.position;
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
            StructureType t = (StructureType)args[0];
            if (t != structureType)
            {
                return;
            }
         //   GameObject  productObj = ObjectPoolManager.Instance.GetObject("Production");
            GameObject  productObj = Instantiate(_productObj);
            productObj.transform.position = recivePosition.position;
            Production product =  productObj.GetComponent<Production>();
            product.FlyTo(grid.GetNextPosition());
            product.SetState(ItemState.OnWorkbench);
            if (currentMaterialCount == 0)
            {
                OnProductionFinished();
            }
        }
        
      
       

        private void OnDestroy()
        {
            Destroy(productionInfo.gameObject);
        }
    }
   
     

   
    [System.Serializable]
    public class PlacementGrid
    {
        private int columns = 3;     // 每层的列数
        private int rows = 3;        // 每层的行数（可以理解为X方向）
        public float xSpacing = 1f; // X方向间距
        public float ySpacing = 1f; // Y方向间距（层高）

        private int currentIndex = 0;
        public Vector2 basePosition; // 2D 起点

        public Vector2 GetNextPosition()
        {
            int index = currentIndex++;

            int layer = index / (rows * columns);      // 第几层
            int layerIndex = index % (rows * columns); // 在当前层内的索引
            int row = layerIndex / columns;            // 当前层的行
            int col = layerIndex % columns;            // 当前层的列
            float x = basePosition.x + col * xSpacing ;
            float y = basePosition.y + layer * ySpacing + row * 0.5f; // 行方向可以加0，如果你想竖直放置可以用行乘间距

            return new Vector2(x, y);
        }


        public void ResetGrid()
        {
            currentIndex = 0;
        }
    }


}