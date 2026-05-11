using System;
using Module.Data;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace View.MapFunction
{
    public class MapInfoItem : MonoBehaviour
    {
        public UIButton btn;
        public string name;
        public string info;
        public Image icon;
        public AssetHandle assetHandle;
        public RectTransform rect;

        public void Init(string resname, object type , Transform  r)
        {
            if (assetHandle == null)
            {
                assetHandle = GetComponent<AssetHandle>();
            }
            icon.sprite = assetHandle.Get<Sprite>(resname);
            rect = r .GetComponent<RectTransform>();
            if (type is BuildingType)
            {
                switch ((BuildingType)type)
                {
                    case BuildingType.YuShaHu_1:
                        name = "一号玉砂壶";
                        info = "可以生产灵茶";
                        break;
                    case BuildingType.YuShaHu_2:
                        name = "二号玉砂壶";
                        info = "可以生产灵茶";
                        break;
                    case BuildingType.YuShaHu_3:
                        name = "三号玉砂壶";
                        info = "可以生产灵茶";
                        break;
                    case BuildingType.YuShaHu_4:
                        name = "四号玉砂壶";
                        info = "可以生产灵茶";
                        break;
                    case BuildingType.LianQiLu_1:
                        name = "一号炼器炉";
                        info = "可以生产灵器";
                        break;
                    case BuildingType.LianQiLu_2:
                        name = "二号炼器炉";
                        info = "可以生产灵器";
                        break;
                    case BuildingType.LianQiLu_3:
                        name = "三号炼器炉";
                        info = "可以生产灵器";
                        break;

                }
            }
            else
            {
                switch ((MonsterFamily)type)
                {
                    case MonsterFamily.ShuangYunZhi:
                        name = "霜云芝";
                        info = "可以生产云芝茶";
                        break;
                    case MonsterFamily.YueLuCao:
                        name = "月露草";
                        info = "可以生产月露茶";
                        break;
                    case MonsterFamily.ZiXinHua:
                        name = "栀心花";
                        info = "可以生产栀心茶";
                        break;
                    case MonsterFamily.YuHuiHe:
                        name = "玉穗禾";
                        info = "可以生产玉禾茶";
                        break;
                    case MonsterFamily.XingWenGuo:
                        name = "星纹果";
                        info = "可以生产星纹茶";
                        break;
                    case MonsterFamily.WuRongJun:
                        name = "雾茸菌";
                        info = "可以生产雾茸茶";
                        break;
                    case MonsterFamily.LingXuSheng:
                        name = "灵须参";
                        info = "可以生产灵须茶";
                        break;
                    case MonsterFamily.XueBanHua:
                        name = "雪瓣花";
                        info = "可以生产雪瓣茶";
                        break;
                    case MonsterFamily.MuLingYa:
                        name = "木灵芽";
                        info = "可以生产木灵茶";
                        break;
                    case MonsterFamily.JingRuiCao:
                        name = "晶蕊草";
                        info = "可以生产晶蕊茶";
                        break;
                    case MonsterFamily.TieKuangShi:
                        name = "铁矿石";
                        info = "可以生产青岩剑";
                        break;
                    case MonsterFamily.TongKuangShi:
                        name = "铜矿石";
                        info = "可以生产铜纹刀";
                        break;
                    case MonsterFamily.YinKuangShi:
                        name = "银矿石";
                        info = "可以生产银丝刀";
                        break;
                    case MonsterFamily.ZiJingShi:
                        name = "紫晶石";
                        info = "可以生产紫雾剑";
                        break;
                    case MonsterFamily.YueJingShi:
                        name = "月晶石";
                        info = "可以生产月心镜";
                        break;
                }
            }
        }

        private void Start()
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener((() =>
            {
                UIController.Instance.Show<TooltipView>( name , info , rect);
            }));
        }
    }
}
