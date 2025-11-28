using System.Collections.Generic;
using Controller.Structure;
using Module.Data;
using UnityEngine;
using Utils;

namespace Controller
{
    public class  GameController : MonoSingleton<GameController>
    {
        [Header("每个地图的出生点")] 
        public Dictionary<MonsterType,Vector2> bornPositions = new ();
        [Header("地图中建筑信息")]
        public Dictionary<BuildingType, StructureBase> buildings = new ();
        [Header("商品类型对应的售卖摊位")]
        public Dictionary<GoodsType , StructureBase> goodBuild = new ();

        public int currentMapID = 1;

        public Vector2 TestPoint;
        private AssetHandle _assetHandle;
        public GameObject obj;
        public MonsterData monsterData;
        public override void Awake()
        {
            base.Awake();
            bornPositions = new ();
            buildings = new ();
            goodBuild = new ();
        }

        private void Start()
        {
            _assetHandle = GetComponent<AssetHandle>();
      
        }
       
    }
}