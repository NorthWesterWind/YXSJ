using System;
using System.Collections.Generic;
using Controller;
using Module.Data;
using UnityEngine;
using Utils;

namespace View.LingChuGe
{
    public class LingChuGeInfo : MonoBehaviour
    {
        public CanvasGroup canvasGroup;
        public Transform topTransform;

        public AssetHandle assetHandle;
        public List<MonsterFamily> targetTypeList = new();
        public  LingChuGeController lingChuGeController;

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

        private void OnDisable()
        {
            EventCenter.Instance.TriggerEvent(EventMessages.LingChuGeStopDelivery);
        }


        public void Init(WarehouseCategory warehouseCategory ,   LingChuGeController c)
        {
            targetTypeList.Clear();
            targetTypeList = warehouseCategory.targetTypeList;
            lingChuGeController = c;
            if (assetHandle == null)
            {
                assetHandle = GetComponent<AssetHandle>();
            }

            Extensions.ClearChildren(topTransform);
            foreach (var value in warehouseCategory.targetTypeList)
            {
                GameObject obj = GameObject.Instantiate(assetHandle.Get<GameObject>("topItem"), topTransform, false);
                obj.GetComponent<TopItem>().Init( lingChuGeController ,value);
            
            }
        }
    }
}