namespace Module.Data
{
        public enum MonsterType
        {
                None = 0,
                ShuangYunZhi = 1,
                ShuangYunZhiGolden = 2,
                ShuangYunZhiBig = 3,
                YueLuCao = 4,
                YueLuCaoGolden = 5,
                YueLuCaoBig = 6,
                ZiXinHua = 7,
                ZiXinHuaGolden = 8,
                ZiXinHuaBig = 9,
                YuHuiHe = 10,
                YuHuiHeGolden = 11,
                YuHuiHeBig = 12,
                XingWenGuo = 13,
                XingWenGuoGolden = 14,
                XingWenGuoBig = 15 ,
                WuRongJun = 16,
                WuRongJunGolden = 17,
                WuRongJunBig = 18,
                LingXuSheng = 19,
                LingXuShengGolden = 20,
                LingXuShengBig = 21,
                XueBanHua = 22,
                XueBanHuaGolden = 23,
                XueBanHuaBig = 24,
                MuLingYa = 25,
                MuLingYaGolden = 26,
                MuLingYaBig = 27,
                JingRuiCao = 28,
                JingRuiCaoGolden = 29,
                JingRuiCaoBig = 30,
                
                //矿石
                TieKuangShi = 31,
                TieKuangShiGolden = 32,
                TieKuangShiBig = 33,
                YinKuangShi = 34,
                YinKuangShiGolden = 35,
                YinKuangShiBig = 36,
                TongKuangShi = 37,
                TongKuangShiGolden = 38,
                TongKuangShiBig = 39,
                ZiJingShi = 40,
                ZiJingShiGolden = 41,
                ZiJingShiBig = 42,
                YueJingShi = 43,
                YueJingShiGolden = 44,
                YueJingShiBig = 45,
        }

        public enum MonsterBehavior
        {
                Normal,
                Giant,
                Golden
        }
        
        /// <summary>
        /// 建筑物枚举类
        /// </summary>
        public enum BuildingType
        {
                None = 0,
                LingZhangTai = 1, //灵账台
                LingChuGe_1 = 2,  //一号灵储阁
                YunDiGe = 3,    //云递阁
                LingChaJia_1 =4,  //一号灵茶架
                YuShaHu_1 = 5,     //一号玉砂壶
                LingChaJia_2 = 6,   //二号灵茶架
                YuShaHu_2 = 7,      //二号玉砂壶
                LingChaJia_3 = 8,    //三号灵茶架
                YuShaHu_3 = 9,       //三号玉砂壶
                LingChaJia_4 = 10,   //四号灵茶架
                YuShaHu_4 = 11,     //四号玉砂壶
                LianQiLu_1 = 12,    //一号炼器炉
                LingQiJia_1 = 13,   //一号灵器架
                LianQiLu_2 = 14,    //二号炼器炉
                LingQiJia_2 = 15,   //二号灵器架
                LianQiLu_3 = 16,    //三号炼器炉
                LingQiJia_3 = 17,   //三号灵器架
                YuanBaoKuangDong = 18, //元宝矿洞
                LingChuGe_2 = 19,   //二号灵储阁
        }
        
        /// <summary>
        /// 助手枚举类
        /// </summary>
        public enum EmployeeType
        {
               LingZhangShi,
               XunCaiTu,
               YunDiZhe,
               
        }   
        
        /// <summary>
        /// 商品枚举类
        /// </summary>
        public enum GoodsType
        {
                None = 0,
                //灵茶
                YunZhiCha = 1,
                YueLuCha = 2,
                ZiXinCha = 3,
                YuHeCha = 4,
                XingWenCha = 5,
                WuRongCha = 6,
                LingXuCha = 7,
                XueBanCha = 8,
                MuLingCha = 9,
                JingRuiCha = 10,
                
                //灵器
                QingYanJian = 11,
                YinSiDao = 12,
                TongWenDao = 13,
                ZiWuJian = 14,
                YueXinJing = 15,
                
                
                JingYunBao = 16,
                YingQian = 17,
        }
        
        /// <summary>
        /// 掉落物枚举类
        /// </summary>
        public enum DropItemType
        {
                None = 0,
                ShuangYunZhiFragment = 1,
                YueLuCaoFragment = 2,
                ZiXinHuaFragment = 3,
                YuHuiHeFragment = 4,
                XingWenGuoFragment = 5,
                WuRongJunFragment = 6,
                LingXuShengFragment = 7,
                XueBanHuaFragment = 8,
                MuLingYaFragment = 9,
                JingRuiCaoFragment = 10,
                
                //灵器
                TieKuangShiFragment = 11,
                YinKuangShiFragment = 12,
                TongKuangShiFragment = 13,
                ZiJingShiFragment = 14,
                YueJingShiFragment = 15,
                YingQian,
        }
        
        
        
        public enum MonsterState
        {
                None,
                Idle,
                Patrol,
                Flee,
                Attack
        }

        public enum MapType
        {
                None = 0 , 
                ChunHuiGu = 1,
                BiLanGu = 2,
                JinLuGu = 3,
                YunDingGu = 4,
                WangShuGu = 5
        }

        public enum CustomerType
        {
                None = 0 ,
                YunZhiTangDiZi,
                YunZhiTangZhangLao,
                QingLanGuDiZi,
                QingLanGuZhangLao,
                SuCaiGeDiZi,
                SuCaiGeZhangLao,
                CangYunGeDiZi,
                CangYunGeZhangLao,
        }
        
        public enum InteractionType
        {
                None = 0, 
                FactoryA,
                FactoryB,
                SellA,
                SellB,
        }

        public enum ShowUIType
        {
                TestView
        }

        // public enum StructureType
        // {
        //         None = 0 ,
        //         YuShaHu_1 = 1,
        //         LingChaJia_1 = 2,
        //         YuShaHu_2 = 3,
        //         LingChaJia_2 = 4,
        //         YuShaHu_3 = 5,
        //         LingChaJia_3 = 6,
        //         YuShaHu_4 = 7,
        //         LingChaJia_4 = 8,
        //         LianQiLu_1 = 9,
        //         LingQiJia_1 = 10,
        //         LianQiLu_2 = 11,
        //         LingQiJia_2 = 12,
        //         LianQiLu_3 = 13,
        //         LingQiJia_3 = 14,
        //         LingZhangTai = 15,
        //       
        //      
        // }
        
        /// <summary>
        /// 任务类型
        /// </summary>
        public enum TaskType
        {
                None = 0,
                Produce = 1, //生产类型任务
                Upgrade = 2, //升级建筑类型任务
                Construct = 3, //建造类型任务
                Sell = 4,   //商品出售类型任务
                Harvest = 5,  //玩家收货物品类型任务
                Makemoney = 6, //玩家获得银币类型任务
                Unlock = 7,  //解锁区域类型任务
        }

        public enum AreaType
        {
                
        }

        public enum InfoType
        {
                None,
                JinYuanBao,
                LingJing,
                YinQian,
                ShuangYunZhiFragment,
                YueLuCaoFragment ,
                ZiXinHuaFragment ,
                YuHuiHeFragment ,
                XingWenGuoFragment ,
                WuRongJunFragment,
                LingXuShengFragment,
                XueBanHuaFragment ,
                MuLingYaFragment ,
                JingRuiCaoFragment,
                TieKuangShiFragment,
                YinKuangShiFragment,
                TongKuangShiFragment,
                ZiJingShiFragment,
                YueJingShiFragment,
                
                YunZhiCha ,
                YueLuCha ,
                ZiXinCha,
                YuHeCha,
                XingWenCha,
                WuRongCha,
                LingXuCha,
                XueBanCha,
                MuLingCha,
                JingRuiCha,
                QingYanJian ,
                YinSiDao ,
                TongWenDao ,
                ZiWuJian ,
                YueXinJing,
        }
        
}
