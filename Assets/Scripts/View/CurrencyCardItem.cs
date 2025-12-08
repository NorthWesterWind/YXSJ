using System;
using Module.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace View
{
    public class CurrencyCardItem : MonoBehaviour
    {
        public Image iconImage;
        public TextMeshProUGUI txt;
        private AssetHandle _assetHandle;
        
        public void Init(CurrencyType type , int value)
        {
            if (_assetHandle == null)
            {
                _assetHandle = GetComponent<AssetHandle>();
            }

            switch (type)
            {
                case CurrencyType.JingYuanBao:
                    break;
                case CurrencyType.LingJing:
                    break;
            }
            txt.text = value.ToString();
        }
    }
}