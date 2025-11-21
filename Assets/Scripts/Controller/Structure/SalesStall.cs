using Module.Data;
using UnityEngine;
using View._3D;

namespace Controller.Structure
{
    public class SalesStall : StructureBase
    {
        public Transform receiveTransform;
        public GoodsType currentGoodsType;
        public int currentGoodsCount;

        public void Init(GoodsType goodsType)
        {
            currentGoodsType = goodsType;
        }

        public void AddGoods(GoodsType goodsType, int goodsCount)
        {
            if (goodsType == currentGoodsType)
            {
                return;
            }

            currentGoodsCount += goodsCount;
        }

        public void RemoveGoods(int goodsCount)
        {
            if (currentGoodsCount < goodsCount)
            {
                return;
            }
            currentGoodsCount -= goodsCount;
        }
        
        public PlacementGrid grid;

        public void PlaceProduct(Production p)
        {
            var targetPos = grid.GetNextPosition();
            p.FlyTo(targetPos);
            p.SetState(ItemState.OnShelf);
        }
    }
}