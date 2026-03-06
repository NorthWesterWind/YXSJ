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
        public DropItemType type;
        public LingChuGeController lingChuGeController;
        public WarehouseCategory warehouseCategory;
        private bool clickBound;

        public void Init(LingChuGeController lcc, DropItemType dropItemType)
        {
            if (assetHandle == null)
            {
                assetHandle = GetComponent<AssetHandle>();
            }

            if (btn == null)
            {
                btn = GetComponent<UIButton>();
            }

            lingChuGeController = lcc;
            warehouseCategory = lcc != null ? lcc.warehouseCategory : null;
            type = dropItemType;
            if (warehouseCategory != null && warehouseCategory.ownItemList != null)
            {
                ownnum = warehouseCategory.ownItemList.Get((int)dropItemType);
            }
            if (iconImage != null && assetHandle != null)
            {
                iconImage.sprite = assetHandle.Get<Sprite>(Extensions.GetDropItemResNameByType(dropItemType));
            }
            BindClick();
            HandleUpdateLingChuGeInfo();

        }

        private void BindClick()
        {
            if (clickBound || btn == null)
            {
                return;
            }
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener((() =>
            {
                EventCenter.Instance.TriggerEvent(EventMessages.LingChuGeDelivery, type);
            }));
            clickBound = true;
        }

        private void OnEnable()
        {
            EventCenter.Instance.AddListener(EventMessages.UpdateLingChuGeInfo, HandleUpdateLingChuGeInfo);
            BindClick();
        }

        private void OnDisable()
        {
            EventCenter.Instance.RemoveListener(EventMessages.UpdateLingChuGeInfo, HandleUpdateLingChuGeInfo);
            EventCenter.Instance.TriggerEvent(EventMessages.LingChuGeStopDelivery);
        }

        public void HandleUpdateLingChuGeInfo(params object[] args)
        {
            if (warehouseCategory == null || warehouseCategory.ownItemList == null || numtxt == null)
            {
                return;
            }

            int ownnum = warehouseCategory.ownItemList.Get((int)type);
            numtxt.text = ownnum.ToString();
        }



    }
}
