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
      public int id; //灵储阁Id
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
              EventCenter.Instance.TriggerEvent(EventMessages.LingChuGeItemDelivery , type , ownnum);
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
          int ownnum = ModuleMgr.Instance.GetModule<PlayerDataModule>().data.warehouselist.Find(x => x.id == id).ownItemList.Get((int)type);
          numtxt.text = ownnum.ToString();
      }
    }
}
