using System;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace View.MapFunction
{
    public class MapInfoItem : MonoBehaviour
    {
        public UIButton btn;
        public string name;
        public string info;
        public Image icon;
        public AssetHandle assetHandle;

        public void Init(  string resname)
        {
            if (assetHandle == null)
            {
                assetHandle = GetComponent<AssetHandle>();
            }
            icon.sprite = assetHandle.Get<Sprite>(resname);
                                
        }

        private void Start()
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener((() =>
            {
                UIController.Instance.Show<TooltipView>();
            }));
        }
    }
}
