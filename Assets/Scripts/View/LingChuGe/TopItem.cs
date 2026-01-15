using Module;
using Module.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace View.LingChuGe
{
    public class TopItem : MonoBehaviour
    {
      public Image iconImage;
      public TextMeshProUGUI numtxt;
      public AssetHandle assetHandle;
      public UIButton btn;
      public WarehouseCategoryType  categoryType; //灵储阁Id
      public int ownnum;
      public MonsterType type;
      public void Init(MonsterType monsterType , int num , int id)
      {
          type = monsterType;
          assetHandle = GetComponent<AssetHandle>();
          ownnum = num;
          iconImage.sprite = assetHandle.Get<Sprite>(Extensions.GetMonsterResNameByType(monsterType));
          numtxt.text = ownnum.ToString();
      }
      private void Start()
      {
          btn.onClick.RemoveAllListeners();
          btn.onClick.AddListener((() =>
          {
              EventCenter.Instance.TriggerEvent(EventMessages.LingChuGeDelivery , ExchangeType(type));
          }));
      }
      private void OnEnable()
      {
          EventCenter.Instance.AddListener(EventMessages.UpdateLingChuGeInfo , HandleUpdateLingChuGeInfo);
      }
      
      private void OnDisable()
      {
          EventCenter.Instance.RemoveListener(EventMessages.UpdateLingChuGeInfo , HandleUpdateLingChuGeInfo);
      }

      public void HandleUpdateLingChuGeInfo(params object[] args)
      {
          int ownnum = ModuleMgr.Instance.GetModule<PlayerDataModule>().data.warehouselist.Find(x => x.warehouseCategoryType == categoryType).ownItemList.Get((int)type);
          numtxt.text = ownnum.ToString();
      }


      DropItemType ExchangeType(MonsterType monsterType)
      {
          DropItemType resultype = DropItemType.None;
          switch (monsterType)
          {
              case MonsterType.ShuangYunZhi:
                  resultype = DropItemType.ShuangYunZhiFragment;
                  break;
              case MonsterType.ShuangYunZhiGolden:
                  resultype = DropItemType.ShuangYunZhiFragment;
                  break;
              case MonsterType.ShuangYunZhiBig:
                  resultype = DropItemType.ShuangYunZhiFragment;
                  break;
              case MonsterType.YueLuCao:
                  resultype = DropItemType.YueLuCaoFragment;
                  break;
              case MonsterType.YueLuCaoGolden:
                  resultype = DropItemType.YueLuCaoFragment;
                  break;
              case MonsterType.YueLuCaoBig:
                  resultype = DropItemType.YueLuCaoFragment;
                  break;
              case MonsterType.ZiXinHua:
                  resultype = DropItemType.ZiXinHuaFragment;
                  break;
              case MonsterType.ZiXinHuaGolden:
                  resultype = DropItemType.ZiXinHuaFragment;
                  break;
              case MonsterType.ZiXinHuaBig:
                  resultype = DropItemType.ZiXinHuaFragment;
                  break;
              case MonsterType.YuHuiHe:
                  resultype = DropItemType.YuHuiHeFragment;
                  break;
              case MonsterType.YuHuiHeGolden:
                  resultype = DropItemType.YuHuiHeFragment;
                  break;
              case MonsterType.YuHuiHeBig:
                  resultype = DropItemType.YuHuiHeFragment;
                  break;
              case MonsterType.XingWenGuo:
                  resultype = DropItemType.XingWenGuoFragment;
                  break;
              case MonsterType.XingWenGuoGolden:
                  resultype = DropItemType.XingWenGuoFragment;
                  break;
              case MonsterType.XingWenGuoBig:
                  resultype = DropItemType.XingWenGuoFragment;
                  break;
              case MonsterType.WuRongJun:
                  resultype = DropItemType.WuRongJunFragment;
                  break;
              case MonsterType.WuRongJunBig:
                  resultype = DropItemType.WuRongJunFragment;
                  break;
              case MonsterType.WuRongJunGolden:
                  resultype = DropItemType.WuRongJunFragment;
                  break;
              case MonsterType.LingXuSheng:
                  resultype = DropItemType.LingXuShengFragment;
                  break;
              case MonsterType.LingXuShengGolden:
                  resultype = DropItemType.LingXuShengFragment;
                  break;
              case MonsterType.LingXuShengBig:
                  resultype = DropItemType.LingXuShengFragment;
                  break;
              case MonsterType.XueBanHua:
                  resultype = DropItemType.XueBanHuaFragment;
                  break;
              case MonsterType.XueBanHuaGolden:
                  resultype = DropItemType.XueBanHuaFragment;
                  break;
              case MonsterType.XueBanHuaBig:
                  resultype = DropItemType.XueBanHuaFragment;
                  break;
              case MonsterType.MuLingYa:
                  resultype = DropItemType.MuLingYaFragment;
                  break;
              case MonsterType.MuLingYaGolden:
                  resultype = DropItemType.MuLingYaFragment;
                  break;
              case MonsterType.MuLingYaBig:
                  resultype = DropItemType.MuLingYaFragment;
                  break;
              case MonsterType.JingRuiCao:
                  resultype = DropItemType.JingRuiCaoFragment;
                  break;
              case MonsterType.JingRuiCaoGolden:
                  resultype = DropItemType.JingRuiCaoFragment;
                  break;
              case MonsterType.JingRuiCaoBig:
                  resultype = DropItemType.JingRuiCaoFragment;
                  break;
              case MonsterType.TieKuangShi:
                  resultype = DropItemType.TieKuangShiFragment;
                  break;
              case MonsterType.TieKuangShiGolden:
                  resultype = DropItemType.TieKuangShiFragment;
                  break;
              case MonsterType.TieKuangShiBig:
                  resultype = DropItemType.TieKuangShiFragment;
                  break;
              case MonsterType.YinKuangShi:
                  resultype = DropItemType.YinKuangShiFragment;
                  break;
              case MonsterType.YinKuangShiGolden:
                  resultype = DropItemType.YinKuangShiFragment;
                  break;
              case MonsterType.YinKuangShiBig:
                  resultype = DropItemType.YinKuangShiFragment;
                  break;
              case MonsterType.TongKuangShi:
                  resultype = DropItemType.TongKuangShiFragment;
                  break;
              case MonsterType.TongKuangShiGolden:
                  resultype = DropItemType.TongKuangShiFragment;
                  break;
              case MonsterType.TongKuangShiBig:
                  resultype = DropItemType.TongKuangShiFragment;
                  break;
              case MonsterType.ZiJingShi:
                  resultype = DropItemType.ZiJingShiFragment;
                  break;
              case MonsterType.ZiJingShiGolden:
                  resultype = DropItemType.ZiJingShiFragment;
                  break;
              case MonsterType.ZiJingShiBig:
                  resultype = DropItemType.ZiJingShiFragment;
                  break;
              case MonsterType.YueJingShi:
                  resultype = DropItemType.YueJingShiFragment;
                  break;
              case MonsterType.YueJingShiGolden:
                  resultype = DropItemType.YueJingShiFragment;
                  break;
              case MonsterType.YueJingShiBig:
                  resultype = DropItemType.YueJingShiFragment;
                  break;
          }
          return resultype;
      }
    }
}
