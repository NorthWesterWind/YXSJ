using Controller.Player;
using Module.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace View.PlayerInfo
{
    public class InfoItem : MonoBehaviour
    {
        private AssetHandle _assetHandle;
        public Image itemIcon;
        public TextMeshProUGUI infoText;
        public InfoType infoType;
        public PlayerController _cc;

        private void Awake()
        {
            _assetHandle = GetComponent<AssetHandle>();
            EventCenter.Instance.AddListener(EventMessages.UpdateInfoItem, HandleUpdateInfo);
        }

        private void OnDestroy()
        {
            EventCenter.Instance.RemoveListener(EventMessages.UpdateInfoItem, HandleUpdateInfo);
        }

        public void HandleUpdateInfo(params object[] args)
        {
            InfoType type = (InfoType)args[0];
            if (type != infoType)
            {
                return;
            }

            Init();
        }

        private int GetDropCount(DropItemType type)
        {
            if (_cc == null || _cc.dropDic == null)
            {
                return 0;
            }

            return _cc.dropDic.TryGetValue(type, out int count) ? count : 0;
        }

        private int GetGoodsCount(GoodsType type)
        {
            if (_cc == null || _cc.goodsDic == null)
            {
                return 0;
            }

            return _cc.goodsDic.TryGetValue(type, out int count) ? count : 0;
        }

        public void Init(PlayerController player = null)
        {
            if (player != null)
            {
                _cc = player;
            }

            if (_cc == null)
            {
                _cc = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
            }

            switch (infoType)
            {
                case InfoType.JinYuanBao:
                    infoText.text = Extensions.FormatNumber(_cc.dataModule.data.goldIngot);
                    itemIcon.sprite = _assetHandle.Get<Sprite>("JingYuanBao");
                    break;
                case InfoType.TongBi:
                    infoText.text = Extensions.FormatNumber(_cc.dataModule.data.tongbi);
                    itemIcon.sprite = _assetHandle.Get<Sprite>("TongBi");
                    break;
                case InfoType.LingJing:
                    infoText.text = Extensions.FormatNumber(_cc.dataModule.data.lingJing);
                    itemIcon.sprite = _assetHandle.Get<Sprite>("LingJing");
                    break;
                case InfoType.ShuangYunZhiFragment:
                    infoText.text =
                        //"霜云芝:" +
                        Extensions.FormatNumber(GetDropCount(DropItemType.ShuangYunZhiFragment));
                    itemIcon.sprite = _assetHandle.Get<Sprite>("ShuangYunZhi");
                    break;
                case InfoType.YueLuCaoFragment:
                    infoText.text =
                        //"月露草:" +
                        Extensions.FormatNumber(GetDropCount(DropItemType.YueLuCaoFragment));
                    itemIcon.sprite = _assetHandle.Get<Sprite>("YueLuCao");
                    break;
                case InfoType.ZiXinHuaFragment:
                    infoText.text =
                        //"栀心花:" +
                        Extensions.FormatNumber(GetDropCount(DropItemType.ZiXinHuaFragment));
                    itemIcon.sprite = _assetHandle.Get<Sprite>("ZiXinHua");
                    break;
                case InfoType.YuHuiHeFragment:
                    infoText.text =
                        //"玉穗禾:" +
                        Extensions.FormatNumber(GetDropCount(DropItemType.YuHuiHeFragment));
                    itemIcon.sprite = _assetHandle.Get<Sprite>("YuHuiHe");
                    break;
                case InfoType.XingWenGuoFragment:
                    infoText.text =
                        //"星纹果:" +
                        Extensions.FormatNumber(GetDropCount(DropItemType.XingWenGuoFragment));
                    itemIcon.sprite = _assetHandle.Get<Sprite>("XingWenGuo");
                    break;
                case InfoType.WuRongJunFragment:
                    infoText.text =
                        //"雾茸菌:" +
                        Extensions.FormatNumber(GetDropCount(DropItemType.WuRongJunFragment));
                    itemIcon.sprite = _assetHandle.Get<Sprite>("WuRongJun");
                    break;
                case InfoType.LingXuShengFragment:
                    infoText.text =
                        //"灵须参:" +
                        Extensions.FormatNumber(GetDropCount(DropItemType.LingXuShengFragment));
                    itemIcon.sprite = _assetHandle.Get<Sprite>("LingXuSheng");
                    break;
                case InfoType.XueBanHuaFragment:
                    infoText.text =
                        //"雪瓣花:" +
                        Extensions.FormatNumber(GetDropCount(DropItemType.XueBanHuaFragment));
                    itemIcon.sprite = _assetHandle.Get<Sprite>("XueBanHua");
                    break;
                case InfoType.MuLingYaFragment:
                    infoText.text =
                        //"木灵芽:" +
                        Extensions.FormatNumber(GetDropCount(DropItemType.MuLingYaFragment));
                    itemIcon.sprite = _assetHandle.Get<Sprite>("MuLingYa");
                    break;
                case InfoType.JingRuiCaoFragment:
                    infoText.text =
                        //"晶蕊草:" +
                        Extensions.FormatNumber(GetDropCount(DropItemType.JingRuiCaoFragment));
                    itemIcon.sprite = _assetHandle.Get<Sprite>("JingRuiCao");
                    break;
                case InfoType.TieKuangShiFragment:
                    infoText.text =
                        //"铁矿石:" +
                        Extensions.FormatNumber(GetDropCount(DropItemType.TieKuangShiFragment));
                    itemIcon.sprite = _assetHandle.Get<Sprite>("TieKuangShi");
                    break;
                case InfoType.YinKuangShiFragment:
                    infoText.text =
                        //"银矿石:" +
                        Extensions.FormatNumber(GetDropCount(DropItemType.YinKuangShiFragment));
                    itemIcon.sprite = _assetHandle.Get<Sprite>("YinKuangShi");
                    break;
                case InfoType.TongKuangShiFragment:
                    infoText.text =
                        //"铜矿石:" +
                        Extensions.FormatNumber(GetDropCount(DropItemType.TongKuangShiFragment));
                    itemIcon.sprite = _assetHandle.Get<Sprite>("TongKuangShi");
                    break;
                case InfoType.ZiJingShiFragment:
                    infoText.text =
                        //"紫晶石:" +
                        Extensions.FormatNumber(GetDropCount(DropItemType.ZiJingShiFragment));
                    itemIcon.sprite = _assetHandle.Get<Sprite>("ZiJingShi");
                    break;
                case InfoType.YueJingShiFragment:
                    infoText.text =
                        //"月晶石:" +
                        Extensions.FormatNumber(GetDropCount(DropItemType.YueJingShiFragment));
                    itemIcon.sprite = _assetHandle.Get<Sprite>("YueJingShi");
                    break;
                case InfoType.YunZhiCha:
                    infoText.text =
                        //"云芝茶:" +
                        Extensions.FormatNumber(GetGoodsCount(GoodsType.YunZhiCha));
                    itemIcon.sprite = _assetHandle.Get<Sprite>("YunZhiCha");
                    break;
                case InfoType.YueLuCha:
                    infoText.text =
                        //"月露茶:" +
                        Extensions.FormatNumber(GetGoodsCount(GoodsType.YueLuCha));
                    itemIcon.sprite = _assetHandle.Get<Sprite>("YueLuCha");
                    break;
                case InfoType.ZiXinCha:
                    infoText.text =
                        //"栀心茶:" +
                        Extensions.FormatNumber(GetGoodsCount(GoodsType.ZiXinCha));
                    itemIcon.sprite = _assetHandle.Get<Sprite>("ZiXinCha");
                    break;
                case InfoType.YuHeCha:
                    infoText.text =
                        //"玉穗茶:" +
                        Extensions.FormatNumber(GetGoodsCount(GoodsType.YuHuiCha));
                    itemIcon.sprite = _assetHandle.Get<Sprite>("YuHeCha");
                    break;
                case InfoType.XingWenCha:
                    infoText.text =
                        //"星纹茶:" +
                        Extensions.FormatNumber(GetGoodsCount(GoodsType.XingWenCha));
                    itemIcon.sprite = _assetHandle.Get<Sprite>("XingWenCha");
                    break;
                case InfoType.WuRongCha:
                    infoText.text =
                        //"雾茸茶:" +
                        Extensions.FormatNumber(GetGoodsCount(GoodsType.WuRongCha));
                    itemIcon.sprite = _assetHandle.Get<Sprite>("WuRongCha");
                    break;
                case InfoType.LingXuCha:
                    infoText.text =
                        //"灵须茶:" +
                        Extensions.FormatNumber(GetGoodsCount(GoodsType.LingXuCha));
                    itemIcon.sprite = _assetHandle.Get<Sprite>("LingXuCha");
                    break;
                case InfoType.XueBanCha:
                    infoText.text =
                        //"雪瓣茶:" +
                        Extensions.FormatNumber(GetGoodsCount(GoodsType.XueBanCha));
                    itemIcon.sprite = _assetHandle.Get<Sprite>("XueBanCha");
                    break;
                case InfoType.MuLingCha:
                    infoText.text =
                        //"木灵茶:" +
                        Extensions.FormatNumber(GetGoodsCount(GoodsType.MuLingCha));
                    itemIcon.sprite = _assetHandle.Get<Sprite>("MuLingCha");
                    break;
                case InfoType.JingRuiCha:
                    infoText.text =
                        //"晶蕊茶:" +
                        Extensions.FormatNumber(GetGoodsCount(GoodsType.JingRuiCha));
                    itemIcon.sprite = _assetHandle.Get<Sprite>("JingRuiCha");
                    break;
                case InfoType.QingYanJian:
                    infoText.text =
                        //"青岩剑:" +
                        Extensions.FormatNumber(GetGoodsCount(GoodsType.QingYanJian));
                    itemIcon.sprite = _assetHandle.Get<Sprite>("QingYanJian");
                    break;
                case InfoType.YinSiDao:
                    infoText.text =
                        //"银丝刀:" +
                        Extensions.FormatNumber(GetGoodsCount(GoodsType.YinSiDao));
                    itemIcon.sprite = _assetHandle.Get<Sprite>("YinSiDao");
                    break;
                case InfoType.TongWenDao:
                    infoText.text =
                        //"铜纹刀:" +
                        Extensions.FormatNumber(GetGoodsCount(GoodsType.TongWenDao));
                    itemIcon.sprite = _assetHandle.Get<Sprite>("TongWenDao");
                    break;
                case InfoType.ZiWuJian:
                    infoText.text =
                        //"紫雾剑:" +
                        Extensions.FormatNumber(GetGoodsCount(GoodsType.ZiWuJian));
                    itemIcon.sprite = _assetHandle.Get<Sprite>("ZiWuJian");
                    break;
                case InfoType.YueXinJing:
                    infoText.text =
                        //"月心镜:" +
                        Extensions.FormatNumber(GetGoodsCount(GoodsType.YueXinJing));
                    itemIcon.sprite = _assetHandle.Get<Sprite>("YueXinJing");
                    break;
            }
        }

        public void SetType(InfoType type)
        {
            infoType = type;
        }
    }
}
