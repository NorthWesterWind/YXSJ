using System.Collections.Generic;
using System.Linq;
using Controller;
using Module;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace View.MapFunction
{
    public class MapSelectView : BaseView
    {
        public TextMeshProUGUI tongbitxt;
        public UIButton closeBtn;
        public Transform content;
        public List<MapItem> mapItems = new List<MapItem>();

        public GameObject loadView;
        public Image fillImage;

        public override void UpdateViewWithArgs(params object[] args)
        {
            base.UpdateViewWithArgs(args);

            if (_assetHandle == null)
            {
                _assetHandle = GetComponent<AssetHandle>();
            }

            HandleUpdatePlayerInfo();
            Extensions.ClearChildren(content);
            var list = DataController.Instance.mapDataDic.Values.ToList();
            for (int i = list.Count - 1; i >= 0; i--)
            {
                GameObject obj = Instantiate(_assetHandle.Get<GameObject>("mapItem"), content, false);
                var mapItem = obj.GetComponent<MapItem>();
                mapItem.Init(list[i]);
                mapItems.Add(mapItem);
            }

            loadView.SetActive(false);
        }

        protected override void AddEventListener()
        {
            base.AddEventListener();
            closeBtn.onClick.RemoveAllListeners();
            closeBtn.onClick.AddListener((() => { Hide(); }));
            EventCenter.Instance.AddListener(EventMessages.UpdatePlayerMoneyInfo, HandleUpdatePlayerInfo);
            EventCenter.Instance.AddListener(EventMessages.ShowLoadView, HandleShowLoadView);
            EventCenter.Instance.AddListener(EventMessages.UpdateLoadView, HandleUpdateLoadView);
        }

        public override void RemoveEventListener()
        {
            EventCenter.Instance.RemoveListener(EventMessages.UpdatePlayerMoneyInfo, HandleUpdatePlayerInfo);
            EventCenter.Instance.RemoveListener(EventMessages.ShowLoadView, HandleShowLoadView);
            EventCenter.Instance.RemoveListener(EventMessages.UpdateLoadView, HandleUpdateLoadView);
        }

        public void HandleShowLoadView(params object[] args)
        {
            loadView.SetActive(true);
            fillImage.fillAmount = 0;
        }

        public void HandleUpdateLoadView(params object[] args)
        {
            fillImage.fillAmount = (float)args[0];
        }

        protected override void OnHideComplete()
        {
            base.OnHideComplete();
            EventCenter.Instance.TriggerEvent(EventMessages.ShowPlayerInfoViewCartoon);
        }

        private void HandleUpdatePlayerInfo(params object[] args)
        {
            tongbitxt.text = Extensions.FormatNumber(PlayerDataModule.Instance.data.tongbi);
        }
    }
}