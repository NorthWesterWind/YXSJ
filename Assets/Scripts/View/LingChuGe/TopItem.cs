using Controller;
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

        public int ownnum;
        public MonsterFamily type;
        public LingChuGeController lingChuGeController;
        public WarehouseCategory warehouseCategory;
        public void Init(LingChuGeController lcc, MonsterFamily monsterType)
        {
            if (assetHandle == null)
            {
                assetHandle = GetComponent<AssetHandle>();
            }
            lingChuGeController = lcc;
            warehouseCategory = lcc.warehouseCategory;
            type = monsterType;
            ownnum = warehouseCategory.ownItemList.Get((int)monsterType);
            iconImage.sprite = assetHandle.Get<Sprite>(Extensions.GetMonsterPictureNameByType(monsterType));
            HandleUpdateLingChuGeInfo();

        }

        private void Start()
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener((() =>
            {
                EventCenter.Instance.TriggerEvent(EventMessages.LingChuGeDelivery, Extensions.ExchangeType(type));
            }));
        }
        private void OnEnable()
        {
            EventCenter.Instance.AddListener(EventMessages.UpdateLingChuGeInfo, HandleUpdateLingChuGeInfo);
        }

        private void OnDisable()
        {
            EventCenter.Instance.RemoveListener(EventMessages.UpdateLingChuGeInfo, HandleUpdateLingChuGeInfo);
            EventCenter.Instance.TriggerEvent(EventMessages.LingChuGeStopDelivery);
        }

        public void HandleUpdateLingChuGeInfo(params object[] args)
        {
            int ownnum = warehouseCategory.ownItemList.Get((int)type);
            numtxt.text = ownnum.ToString();
        }



    }
}
