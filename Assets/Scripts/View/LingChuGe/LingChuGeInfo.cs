using System.Collections.Generic;
using Module.Data;
using UnityEngine;
using Utils;

namespace View.LingChuGe
{
    public class LingChuGeInfo : MonoBehaviour
    {
        public CanvasGroup canvasGroup;
        public Transform bottomTransform;
        public Transform topTransform;

        public AssetHandle assetHandle;
        public List<MonsterType> targetTypeList = new();

        private void Start()
        {
        }

        public void HideInfo()
        {
            canvasGroup.alpha = 0;
        }

        public void ShowInfo()
        {
            canvasGroup.alpha = 1;
        }


        public void Init(WarehouseCategory warehouseCategory)
        {
            targetTypeList.Clear();
            targetTypeList = warehouseCategory.targetTypeList;
            if (assetHandle == null)
            {
                assetHandle = GetComponent<AssetHandle>();
            }

            Extensions.ClearChildren(topTransform);
            Extensions.ClearChildren(bottomTransform);
            foreach (var value in warehouseCategory.targetTypeList)
            {
                GameObject obj = GameObject.Instantiate(assetHandle.Get<GameObject>("topItem"), topTransform, false);
                obj.GetComponent<TopItem>()
                    .Init(value, warehouseCategory.ownItemList.Get((int)value), warehouseCategory.id);

                GameObject obj2 =
                    GameObject.Instantiate(assetHandle.Get<GameObject>("bottomItem"), bottomTransform, false);
                obj2.GetComponent<BottomItem>().Init(value, warehouseCategory.id);
            }
        }
    }
}