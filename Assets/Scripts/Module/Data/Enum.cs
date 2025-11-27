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
        public enum BuildingType
        {
                None = 0,
                LingZhangTai = 1,
                LingChuGe_1 = 2,
                YunDiGe = 3,
                LingChaJia_1 =4,
                YuShaHu_1 = 5,
                LingChaJia_2 = 6,
                YuShaHu_2 = 7,
                LingChaJia_3 = 8,
                YuShaHu_3 = 9,
                LingChaJia_4 = 10,
                YuShaHu_4 = 11,
                LianQiLu_1 = 12,
                LingQiJia_1 = 13,
                LianQiLu_2 = 14,
                LingQiJia_2 = 15,
                LianQiLu_3 = 16,
                LingQiJia_3 = 17,
                YuanBaoKuangDong = 18,
                LingChuGe_2 = 19,
        }

        public enum EmployeeType
        {
               LingZhangShi,
               XunCaiTu,
               YunDiZhe,
               
        }   
        public enum GoodsType
        {
                None = 0,
                //灵茶
                YunZhiCha = 1,
                YueLuCha = 2,
                ZhiXinCha = 3,
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

        public enum TaskType
        {
                None = 0,
                Produce = 1,
                Upgrade = 2,
                Construct = 3,
                Sell = 4,
                Harvest = 5,  
                Makemoney = 6,
                Unlock = 7,
        }

        public enum AreaType
        {
                
        }
}
