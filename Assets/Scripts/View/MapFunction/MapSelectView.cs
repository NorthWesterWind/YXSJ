using System.Collections;
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
        public RectTransform content;
        public RectTransform viewport;
        public Vector2 contentOffset;
        public bool centerCurrentMapItem = true;
        public List<MapItem> mapItems = new List<MapItem>();
        private Vector2 _defaultContentAnchoredPosition;
        private bool _cachedDefaultContentPosition;

        public GameObject loadView;
        public Image fillImage;
        public HorizontalLayoutGroup horizontalLayoutGroup;
        private Coroutine _layoutRefreshCoroutine;

        public override void UpdateViewWithArgs(params object[] args)
        {
            base.UpdateViewWithArgs(args);

            if (_assetHandle == null)
            {
                _assetHandle = GetComponent<AssetHandle>();
            }

            HandleUpdatePlayerInfo();
            Extensions.ClearChildren(content);
            mapItems.Clear();
            CacheDefaultContentPosition();
            content.anchoredPosition = _defaultContentAnchoredPosition;
            var list = DataController.Instance.mapDataDic.Values.ToList();
            for (int i = list.Count - 1; i >= 0; i--)
            {
                GameObject obj = Instantiate(_assetHandle.Get<GameObject>("mapItem"), content, false);
                var mapItem = obj.GetComponent<MapItem>();
                mapItem.Init(list[i]);
                mapItems.Add(mapItem);
            }

            loadView.SetActive(false);
            RefreshLayout();
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

        protected override void OnShow()
        {
            base.OnShow();
            RefreshLayout();
        }

        private void HandleUpdatePlayerInfo(params object[] args)
        {
            tongbitxt.text = Extensions.FormatNumber(PlayerDataModule.Instance.data.tongbi);
        }

        private void RefreshLayout()
        {
            if (_layoutRefreshCoroutine != null)
            {
                StopCoroutine(_layoutRefreshCoroutine);
            }

            _layoutRefreshCoroutine = StartCoroutine(RefreshLayoutCoroutine());
        }

        private IEnumerator RefreshLayoutCoroutine()
        {
            yield return null;
            RebuildLayoutTree();
            yield return null;
            RebuildLayoutTree();

            if (centerCurrentMapItem)
            {
                CenterCurrentMapItem();
            }

            _layoutRefreshCoroutine = null;
        }

        private void RebuildLayoutTree()
        {
            if (content == null)
            {
                return;
            }

            Canvas.ForceUpdateCanvases();

            ContentSizeFitter[] fitters = GetComponentsInChildren<ContentSizeFitter>(true);
            for (int i = 0; i < fitters.Length; i++)
            {
                if (fitters[i] == null)
                {
                    continue;
                }

                RectTransform rect = fitters[i].transform as RectTransform;
                if (rect != null)
                {
                    LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
                }
            }

            HorizontalOrVerticalLayoutGroup[] layoutGroups = GetComponentsInChildren<HorizontalOrVerticalLayoutGroup>(true);
            for (int i = 0; i < layoutGroups.Length; i++)
            {
                if (layoutGroups[i] == null)
                {
                    continue;
                }

                RectTransform rect = layoutGroups[i].transform as RectTransform;
                if (rect != null)
                {
                    LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
                }
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(content);
            Canvas.ForceUpdateCanvases();
        }

        private void CenterCurrentMapItem()
        {
            var currentMapId = PlayerDataModule.Instance.data.currentMapID;
            var currentItem = mapItems.FirstOrDefault(x => x != null && x.mapData != null && x.mapData.id == currentMapId);
            if (currentItem == null)
            {
                return;
            }

            var viewportRect = viewport != null ? viewport : content.parent as RectTransform;
            if (viewportRect == null)
            {
                return;
            }

            var itemRect = currentItem.transform as RectTransform;
            if (itemRect == null)
            {
                return;
            }

            Vector2 itemLocal = viewportRect.InverseTransformPoint(itemRect.position);
            Vector2 centerLocal = viewportRect.rect.center;
            Vector2 delta = centerLocal - itemLocal + contentOffset;
            content.anchoredPosition += delta;
        }

        private void CacheDefaultContentPosition()
        {
            if (_cachedDefaultContentPosition || content == null)
            {
                return;
            }

            _defaultContentAnchoredPosition = content.anchoredPosition;
            _cachedDefaultContentPosition = true;
        }
    }
}
