using System.Collections.Generic;
using System.Linq;
using Controller;
using Module;
using Module.Data;
using TMPro;
using UnityEngine;
using Utils;

namespace View.CardView
{
    public class CardInfoView : BaseView
    {
        public TextMeshProUGUI jybtxt;
        public UIButton closeBtn;
        public TextMeshProUGUI progressTxt;
        public AssetHandle assetHandle;
        public Transform transform_1;
        public GameObject obj_1;
        public GameObject obj_2;
        public GameObject obj_3;

        public Transform transform_2;
        public Transform transform_3;

        public override void UpdateViewWithArgs(params object[] args)
        {
            base.UpdateViewWithArgs(args);
            UpdateInfoTxt();
            progressTxt.text = PlayerDataModule.Instance.data.cardUpProgressesList.Count + "/" +
                               DataController.Instance.cardLevelDataList.Count;
            UpdateCardItem();
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
                if (ownedIds.Contains(card.id))
                {
                    list1.Add(card);
                }
                else if (data.accountLevel >= card.unlockLevel)
                {
                    list2.Add(card);
                }
                else
                {
                    list3.Add(card);
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
    }
}