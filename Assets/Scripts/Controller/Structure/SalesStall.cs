using System.Collections.Generic;
using Controller.Pickups;
using Module.Data;
using TMPro;
using UnityEngine;


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
        public TextMeshPro numTxt;
        public List<Production> productList = new();
        protected override void Start()
        {
            base.Start();
            Init();
        }

        public void Init()
        {
            grid.basePosition = baseTransform.position;
            GameController.Instance.goodBuild.Add(currentGoodsType, this);
        }

        public void AddGoods( Production p)
        {   
            p.SetState(ItemState.OnShelf);
            p.canPickup = true;
            productList.Add(p);
            UpdateTxt();
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
            }

            UpdateTxt();
            return true;
        }

        
        public void UpdateTxt()
        {
            numTxt.text = productList.Count.ToString();
        }

        public void PlaceProduct(Production p)
        {
            var targetPos = grid.GetNextPosition();
            p.FlyTo(targetPos);
            p.SetState(ItemState.OnShelf);
        }
    }
}