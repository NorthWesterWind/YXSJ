using System;
using Module;
using Module.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace View.LingChuGe
{
    public class BottomItem : MonoBehaviour
    {
        public Image icon;
        public int num;
        public WarehouseCategoryType warehouseCategoryType;
        public MonsterType type;
        public WarehouseCategory warehouseCategory;
        public AssetHandle assetHandle;
        public TextMeshProUGUI numtxt;

        public void Init(MonsterType monsterType, WarehouseCategoryType waretype)
        {
            type = monsterType;
            warehouseCategoryType = waretype;
            warehouseCategory = PlayerDataModule.Instance.data.warehouselist
                .Find(x => x.warehouseCategoryType == warehouseCategoryType);
            if (assetHandle == null)
            {
                assetHandle = GetComponent<AssetHandle>();
            }
            //icon.sprite = assetHandle.Get<Sprite>(Extensions.GetMonsterResNameByType(monsterType));
            HandleUpdateLingChuGeInfo();
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
            if (warehouseCategory == null)
            {
                return;
            }
            num = warehouseCategory.ownItemList.Get((int)type);
            numtxt.text = num.ToString();
        }
    }
}
