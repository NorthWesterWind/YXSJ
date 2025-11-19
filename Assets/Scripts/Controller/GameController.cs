using System;
using System.Collections.Generic;
using Module.Data;
using UnityEngine;
using Utils;

namespace Controller
{
    public class GameController : MonoSingleton<GameController>
    {
        [Header("每个地图的出生点")] 
        public Dictionary<MonsterType,Vector2> bornPositions = new Dictionary<MonsterType,Vector2>();
        public Dictionary<BuildingType, GameObject> buildings = new Dictionary<BuildingType, GameObject>();
        [Header("商品类型对应的售卖摊位")]
        public Dictionary<GoodsType , GameObject> goodBuild = new Dictionary<GoodsType, GameObject>();

        public int currentMapID = 1;

        public Vector2 TestPoint;
        private AssetHandle _assetHandle;
        public GameObject obj;
        public MonsterData monsterData;
        private void Start()
        {
            _assetHandle = GetComponent<AssetHandle>();
           // BeginCreatMonster();
        }
        // public void BeginCreatMonster()
        // {
        //     for (int i = 0; i < 1; i++)
        //     {
        //         GameObject monster = Instantiate(obj);
        //         monster.transform.position = TestPoint;
        //         monster.GetComponent<MonsterController>().Init(monsterData , TestPoint);
        //     }
        // }
    }
}