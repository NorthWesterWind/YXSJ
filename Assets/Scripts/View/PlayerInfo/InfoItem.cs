using System;
using Controller;
using Module;
using Module.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using CharacterController = Controller.Player.CharacterController;

namespace View.PlayerInfo
{
    public class InfoItem : MonoBehaviour
    {
        private AssetHandle _assetHandle;
        public Image itemIcon;
        public TextMeshProUGUI infoText;
        public InfoType infoType;
        private CharacterController _cc;

        private void Awake()
        {
            _assetHandle = GetComponent<AssetHandle>();
            EventCenter.Instance.AddListener(EventMessages.UpdateInfoItem ,HandleUpdateInfo);
        }

        private void OnDestroy()
        {
            EventCenter.Instance.RemoveListener(EventMessages.UpdateInfoItem,HandleUpdateInfo);
        }

        public void HandleUpdateInfo(params object[] args)
        {
            InfoType type  =  (InfoType)args[0];
            if (type != infoType)
            {
                return;
            }
            Init();
        }
        public void Init(CharacterController character = null)
        {
            if (character != null)
            {
                _cc = character;
            }
          
            itemIcon.sprite = _assetHandle.Get<Sprite>("金币");

            switch (infoType)
            {
                case InfoType.JinYuanBao:
                    infoText.text = "金元宝:" +
                                    Extensions.FormatNumber(character.dataModule.data.goldIngot);
                    break;
                case InfoType.YinQian:
                    infoText.text = "银钱:" +
                                    Extensions.FormatNumber(character.dataModule.data.silverCoin);
                    break;
                case InfoType.LingJing:
                    infoText.text = "灵晶:" +
                                    Extensions.FormatNumber(character.dataModule.data.lingJing);
                    break;
                case InfoType.ShuangYunZhiFragment:
                    infoText.text =
                        "霜云芝:" + Extensions.FormatNumber(character.dropDic[DropItemType.ShuangYunZhiFragment]);
                    break;
                case InfoType.YueLuCaoFragment:
                    infoText.text =
                        "月露草:" + Extensions.FormatNumber(character.dropDic[DropItemType.YueLuCaoFragment]);
                    break;
                case InfoType.ZiXinHuaFragment:
                    infoText.text =
                        "栀心花:" + Extensions.FormatNumber(character.dropDic[DropItemType.ZiXinHuaFragment]);
                    break;
                case InfoType.YuHuiHeFragment:
                    infoText.text =
                        "玉穗禾:" + Extensions.FormatNumber(character.dropDic[DropItemType.YuHuiHeFragment]);
                    break;
                case InfoType.XingWenGuoFragment:
                    infoText.text =
                        "星纹果:" + Extensions.FormatNumber(character.dropDic[DropItemType.XingWenGuoFragment]);
                    break;
                case InfoType.WuRongJunFragment:
                    infoText.text =
                        "雾茸菌:" + Extensions.FormatNumber(character.dropDic[DropItemType.WuRongJunFragment]);
                    break;
                case InfoType.LingXuShengFragment:
                    infoText.text =
                        "灵须参:" + Extensions.FormatNumber(character.dropDic[DropItemType.LingXuShengFragment]);
                    break;
                case InfoType.XueBanHuaFragment:
                    infoText.text =
                        "雪瓣花:" + Extensions.FormatNumber(character.dropDic[DropItemType.XueBanHuaFragment]);
                    break;
                case InfoType.MuLingYaFragment:
                    infoText.text =
                        "木灵芽:" + Extensions.FormatNumber(character.dropDic[DropItemType.MuLingYaFragment]);
                    break;
                case InfoType.JingRuiCaoFragment:
                    infoText.text =
                        "晶蕊草:" + Extensions.FormatNumber(character.dropDic[DropItemType.JingRuiCaoFragment]);
                    break;
                case InfoType.TieKuangShiFragment:
                    infoText.text =
                        "铁矿石:" + Extensions.FormatNumber(character.dropDic[DropItemType.TieKuangShiFragment]);
                    break;
                case InfoType.YinKuangShiFragment:
                    infoText.text =
                        "银矿石:" + Extensions.FormatNumber(character.dropDic[DropItemType.YinKuangShiFragment]);
                    break;
                case InfoType.TongKuangShiFragment:
                    infoText.text =
                        "铜矿石:" + Extensions.FormatNumber(character.dropDic[DropItemType.TongKuangShiFragment]);
                    break;
                case InfoType.ZiJingShiFragment:
                    infoText.text =
                        "紫晶石:" + Extensions.FormatNumber(character.dropDic[DropItemType.ZiJingShiFragment]);
                    break;
                case InfoType.YueJingShiFragment:
                    infoText.text =
                        "月晶石:" + Extensions.FormatNumber(character.dropDic[DropItemType.YueJingShiFragment]);
                    break;

                case InfoType.YunZhiCha:
                    infoText.text =
                        "云芝茶:" + Extensions.FormatNumber(character.goodsDic[GoodsType.YunZhiCha]);
                    break;
                case InfoType.YueLuCha:
                    infoText.text =
                        "月露茶:" + Extensions.FormatNumber(character.goodsDic[GoodsType.YueLuCha]);
                    break;
                case InfoType.ZiXinCha:
                    infoText.text =
                        "栀心茶:" + Extensions.FormatNumber(character.goodsDic[GoodsType.ZiXinCha]);
                    break;
                case InfoType.YuHeCha:
                    infoText.text =
                        "玉禾茶:" + Extensions.FormatNumber(character.goodsDic[GoodsType.YuHeCha]);
                    break;
                case InfoType.XingWenCha:
                    infoText.text =
                        "星纹茶:" + Extensions.FormatNumber(character.goodsDic[GoodsType.XingWenCha]);
                    break;
                case InfoType.WuRongCha:
                    infoText.text =
                        "雾茸茶:" + Extensions.FormatNumber(character.goodsDic[GoodsType.WuRongCha]);
                    break;
                case InfoType.LingXuCha:
                    infoText.text =
                        "灵须茶:" + Extensions.FormatNumber(character.goodsDic[GoodsType.LingXuCha]);
                    break;
                case InfoType.XueBanCha:
                    infoText.text =
                        "雪瓣茶:" + Extensions.FormatNumber(character.goodsDic[GoodsType.XueBanCha]);
                    break;
                case InfoType.MuLingCha:
                    infoText.text =
                        "木灵茶:" + Extensions.FormatNumber(character.goodsDic[GoodsType.MuLingCha]);
                    break;
                case InfoType.JingRuiCha:
                    infoText.text =
                        "晶蕊茶:" + Extensions.FormatNumber(character.goodsDic[GoodsType.JingRuiCha]);
                    break;
                case InfoType.QingYanJian:
                    infoText.text =
                        "青岩剑:" + Extensions.FormatNumber(character.goodsDic[GoodsType.QingYanJian]);
                    break;
                case InfoType.YinSiDao:
                    infoText.text =
                        "银丝刀:" + Extensions.FormatNumber(character.goodsDic[GoodsType.YinSiDao]);
                    break;
                case InfoType.TongWenDao:
                    infoText.text =
                        "铜纹刀:" + Extensions.FormatNumber(character.goodsDic[GoodsType.TongWenDao]);
                    break;
                case InfoType.ZiWuJian:
                    infoText.text =
                        "紫雾剑:" + Extensions.FormatNumber(character.goodsDic[GoodsType.ZiWuJian]);
                    break;
                case InfoType.YueXinJing:
                    infoText.text =
                        "月心镜:" + Extensions.FormatNumber(character.goodsDic[GoodsType.YueXinJing]);
                    break;
            }
        }

        public void SetType(InfoType type)
        {
            infoType = type;
        }
    }
}