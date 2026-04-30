using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Controller;
using Module;
using Module.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace View.CardView
{
    public class CardInfoView : BaseView
    {
        public TextMeshProUGUI jybtxt;
        public UIButton closeBtn;
        public Image fill;
        public TextMeshProUGUI progressTxt;
        public AssetHandle assetHandle;
        public Transform transform_1;
        public GameObject obj_1;
        public GameObject obj_2;
        public GameObject obj_3;

        public Transform transform_2;
        public Transform transform_3;
        private Coroutine _layoutRefreshCoroutine;

        public override void UpdateViewWithArgs(params object[] args)
        {
            base.UpdateViewWithArgs(args);
            UpdateInfoTxt();
            int unlockedOwnedCardCount = GetUnlockedOwnedCardCount();
            int totalCardCount = DataController.Instance.cardLevelDataList.Count;
            progressTxt.text = unlockedOwnedCardCount + "/" + totalCardCount;
            fill.fillAmount = totalCardCount <= 0 ? 0f : unlockedOwnedCardCount * 1f / totalCardCount;
            UpdateCardItem();
            RefreshLayout();
        }

        public void UpdateCardItem()
        {
            var data = PlayerDataModule.Instance.data;

            // 用 HashSet 一次构建，高性能 O(1) 查找
            HashSet<int> ownedIds = new HashSet<int>(data.cardUpProgressesList.Select(x => x.id));

            List<CardLevelData> list1 = new(); // 已拥有
            List<CardLevelData> list2 = new(); // 未拥有但已解锁
            List<CardLevelData> list3 = new(); // 未拥有且未解锁

            foreach (var card in DataController.Instance.cardLevelDataList)
            {
                if (data.accountLevel < card.unlockLevel)
                {
                    list3.Add(card);
                }
                else if (ownedIds.Contains(card.id))
                {
                    list1.Add(card);
                }
                else
                {
                    list2.Add(card);
                }
            }

            Extensions.ClearChildren(transform_1);
            Extensions.ClearChildren(transform_2);
            Extensions.ClearChildren(transform_3);
            if (list1.Count == 0)
            {
                obj_1.SetActive(false);
            }
            else
            {
                obj_1.SetActive(true);
            }
            if (list2.Count == 0)
            {
                obj_2.SetActive(false);
            }
            else
            {
                obj_2.SetActive(true);
            }
            if (list3.Count == 0)
            {
                obj_3.SetActive(false);
            }
            else
            {
                obj_3.SetActive(true);
            }
            for (int i = 0; i < list1.Count; i++)
            {
                GameObject obj = Instantiate(assetHandle.Get<GameObject>("cardItem"), transform_1, false);
                obj.GetComponent<CardItem>().Init(list1[i]);
            }


            for (int i = 0; i < list2.Count; i++)
            {
                GameObject obj = Instantiate(assetHandle.Get<GameObject>("cardItem"), transform_2, false);
                obj.GetComponent<CardItem>().Init(list2[i]);
            }

            for (int i = 0; i < list3.Count; i++)
            {
                GameObject obj = Instantiate(assetHandle.Get<GameObject>("cardItem"), transform_3, false);
                obj.GetComponent<CardItem>().Init(list3[i]);
            }
        }

        protected override void AddEventListener()
        {
            base.AddEventListener();
            closeBtn.onClick.RemoveAllListeners();
            closeBtn.onClick.AddListener((() => { Hide(); }));
            EventCenter.Instance.AddListener(EventMessages.UpdatePlayerMoneyInfo, UpdateInfoTxt);
        }

        protected override void OnShow()
        {
            base.OnShow();
            RefreshLayout();
        }

        protected override void OnHideComplete()
        {
            base.OnHideComplete();
            EventCenter.Instance.TriggerEvent(EventMessages.ShowPlayerInfoViewCartoon);
        }

        public void UpdateInfoTxt(params object[] args)
        {
            PlayerData playerData = PlayerDataModule.Instance.data;
            jybtxt.text = Extensions.FormatNumber(playerData.goldIngot);
        }

        private int GetUnlockedOwnedCardCount()
        {
            PlayerData playerData = PlayerDataModule.Instance.data;
            if (playerData == null || DataController.Instance == null || DataController.Instance.cardLevelDataList == null)
            {
                return 0;
            }

            int count = 0;
            for (int i = 0; i < DataController.Instance.cardLevelDataList.Count; i++)
            {
                CardLevelData cardLevelData = DataController.Instance.cardLevelDataList[i];
                if (cardLevelData == null || playerData.accountLevel < cardLevelData.unlockLevel)
                {
                    continue;
                }

                CardUpProgress progress = playerData.cardUpProgressesList.Find(x => x.id == cardLevelData.id);
                if (progress == null)
                {
                    continue;
                }

                count++;
            }

            return count;
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
            _layoutRefreshCoroutine = null;
        }

        private void RebuildLayoutTree()
        {
            RectTransform root = transform as RectTransform;
            if (root == null)
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

            LayoutRebuilder.ForceRebuildLayoutImmediate(root);
            Canvas.ForceUpdateCanvases();
        }
    }
}
