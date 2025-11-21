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
                LingZhangTai,
                LingChuGe,
                YunDiGe,
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
                ShuangYunZhiCha = 1,
                YunZhiCha,
                YueLuCha,
                ZhiXinCha,
                YuHeCha,
                XingWenCha,
                WuRongCha,
                LingXuCha,
                XueBanCha,
                MuLingCha,
                JingRuiCha,
                
                //灵器
                QingYanJian,
                YinSiDao,
                TongWenDao,
                ZiWuJian,
                YueXinJing
        }

        public enum DropItemType
        {
                None = 0,
                ShuangYunZhiFragment,
                YueLuCaoFragment,
                ZiXinHuaFragment,
                YuHuiHeFragment,
                XingWenGuoFragment,
                WuRongJunFragment,
                LingXuShengFragment,
                XueBanHuaFragment,
                MuLingYaFragment,
                JingRuiCaoFragment,
                
                //灵器
                TieKuangShiFragment,
                YinKuangShiFragment,
                TongKuangShiFragment,
                ZiJingShiFragment,
                YueJingShiFragment,
                
                
                JingYunBao,
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

        public enum StructureType
        {
                YuShaHu_1,
                LingChaJia_1,
                YuShaHu_2,
                LingChaJia_2,
                YuShaHu_3,
                LingChaJia_3,
                YuShaHu_4,
                LingChaJia_4,
                LianQiLu_1,
                LingQiJia_1,
                LianQiLu_2,
                LingQiJia_2,
                LianQiLu_3,
                LingQiJia_3,
                LingZhangTai,
                LingChuGe_1,
                LingChuGe_2,
                YuDiGe,
                YuanBaoKuangDong
             
        }
}
