using System.Collections;
using System.Collections.Generic;
using Controller;
using DG.Tweening;
using Module;
using Module.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;


namespace View.Task
{
    public class TaskPop : BaseView
    {
        public TextMeshProUGUI mapNameTxt;
        public TextMeshProUGUI mapprogressTxt;
        public UIButton rewardBtn;
        public Image rewardBtnImg;
        public Image sliderFill;
        public TextMeshProUGUI sliderText;
        private MapData _mapData;
        public Transform taskContent;
        public UIButton closeBtn;
        public RectTransform content;
        public GameObject redPoint;
        public AssetHandle assetHandle;



        private void OnEnable()
        {
            content.anchoredPosition = new Vector2(0, -910);
        }

        public override void UpdateViewWithArgs(params object[] args)
        {
            base.UpdateViewWithArgs(args);
            StopAllCoroutines();
            PlayerData tempdata = PlayerDataModule.Instance.data;
            _mapData = DataController.Instance.mapDataDic[tempdata.currentMapID];
            // if (PlayerDataModule.Instance.data.mapCompletedTaskRecordDic == null)
            // {
            //     PlayerDataModule.Instance.data.mapCompletedTaskRecordDic = new() { { 1, new List<int>() }, { 2, new List<int>() }, { 3, new List<int>() }, { 4, new List<int>() }, { 5, new List<int>() } };
            // }
            int count = PlayerDataModule.Instance.data.mapCompletedTaskRecordDic[_mapData.id].Count;
            mapNameTxt.text = _mapData.name;
            if (_mapData.id == 1 || _mapData.id == 2)
            {
                rewardBtnImg.sprite = assetHandle.Get<Sprite>("宝箱1");
            }
            else if (_mapData.id == 3 || _mapData.id == 4)
            {
                rewardBtnImg.sprite = assetHandle.Get<Sprite>("宝箱2");
            }
            else
            {
                rewardBtnImg.sprite = assetHandle.Get<Sprite>("宝箱3");
            }
            int tempvalue1 = count / _mapData.taskGroupSize;
            mapprogressTxt.text = tempvalue1 + "/" + _mapData.taskGroupNum;
            sliderText.text = tempdata.taskPopCompleted + "/" + WorldData.taskboxNeedDic[tempdata.currentMapID];
            float value = tempdata.taskPopCompleted * 1f / WorldData.taskboxNeedDic[tempdata.currentMapID];
            sliderFill.fillAmount = value;
            if (value >= 1f)
            {
                redPoint.SetActive(true);
            }
            else
            {
                redPoint.SetActive(false);
            }
            content.DOAnchorPos(new Vector2(0, 0), 0.5f).SetEase(Ease.InBack);
            UpdateTaskContent();
        }

        protected override void AddEventListener()
        {
            base.AddEventListener();
            closeBtn.onClick.AddListener(OnClickClose);

            EventCenter.Instance.AddListener(EventMessages.HasTaskComplete, HandleHasTaskComplete);

            rewardBtn.onClick.RemoveAllListeners();
            rewardBtn.onClick.AddListener(OnClickRewardBtn);
        }
        public override void RemoveEventListener()
        {
            base.RemoveEventListener();
            EventCenter.Instance.RemoveListener(EventMessages.HasTaskComplete, HandleHasTaskComplete);
        }

        void OnClickRewardBtn()
        {
            if (PlayerDataModule.Instance.data.taskPopCompleted >= WorldData.taskboxNeedDic[PlayerDataModule.Instance.data.currentMapID])
            {
                if (PlayerDataModule.Instance.data.currentMapID < 3)
                {
                    var dic = PlayerDataModule.Instance.LotteryCard(DataController.Instance.giftpackDataDic[4]);
                    UIController.Instance.Show<RewardConfirmView>(dic, new Dictionary<CurrencyType, int> { { CurrencyType.JingYuanBao, DataController.Instance.giftpackDataDic[2].JinYuanBao } });

                }
                else if (PlayerDataModule.Instance.data.currentMapID < 3)
                {
                    var dic = PlayerDataModule.Instance.LotteryCard(DataController.Instance.giftpackDataDic[5]);
                    UIController.Instance.Show<RewardConfirmView>(dic, new Dictionary<CurrencyType, int> { { CurrencyType.JingYuanBao, DataController.Instance.giftpackDataDic[2].JinYuanBao } });

                }
                else
                {
                    var dic = PlayerDataModule.Instance.LotteryCard(DataController.Instance.giftpackDataDic[6]);
                    UIController.Instance.Show<RewardConfirmView>(dic, new Dictionary<CurrencyType, int> { { CurrencyType.JingYuanBao, DataController.Instance.giftpackDataDic[2].JinYuanBao } });
                }
                PlayerDataModule.Instance.data.taskPopCompleted -= WorldData.taskboxNeedDic[PlayerDataModule.Instance.data.currentMapID];

                sliderText.text = PlayerDataModule.Instance.data.taskPopCompleted + "/" + WorldData.taskboxNeedDic[PlayerDataModule.Instance.data.currentMapID];
                float value = PlayerDataModule.Instance.data.taskPopCompleted * 1f / WorldData.taskboxNeedDic[PlayerDataModule.Instance.data.currentMapID];
                sliderFill.fillAmount = value;
                if (value >= 1f)
                {
                    redPoint.SetActive(true);
                }
                else
                {
                    redPoint.SetActive(false);
                }
            }
        }

        public void HandleHasTaskComplete(params object[] args)
        {
            _mapData = DataController.Instance.mapDataDic[PlayerDataModule.Instance.data.currentMapID];
            int count = PlayerDataModule.Instance.data.mapCompletedTaskRecordDic[_mapData.id].Count;
            int tempvalue1 = count / _mapData.taskGroupSize;
            mapprogressTxt.text = tempvalue1 + "/" + _mapData.taskGroupNum;
            sliderText.text = PlayerDataModule.Instance.data.taskPopCompleted + "/" + WorldData.taskboxNeedDic[PlayerDataModule.Instance.data.currentMapID];
            float value = PlayerDataModule.Instance.data.taskPopCompleted * 1f / WorldData.taskboxNeedDic[PlayerDataModule.Instance.data.currentMapID];
            sliderFill.fillAmount = value;
            if (value >= 1f)
            {
                redPoint.SetActive(true);
            }
            else
            {
                redPoint.SetActive(false);
            }
            int tempvalue = 0;
            foreach (var _data in PlayerDataModule.Instance.data.listenInTaskList)
            {
                if (!PlayerDataModule.Instance.data.mapCompletedTaskRecordDic[_mapData.id].Contains(_data.taskId))
                {
                    tempvalue += 1;
                }
            }
            if (tempvalue == 0)
            {
                PlayerDataModule.Instance.data.listenInTaskList = DataController.Instance.GetTaskGroupIds();
                PlayerDataModule.Instance.FillStructureLockProgressData();
                UpdateTaskContent();
            }
        }

        public void UpdateTaskContent()
        {
            Extensions.ClearChildren(taskContent);
            PlayerData tempdata = PlayerDataModule.Instance.data;
            List<TaskData> dataList = tempdata.listenInTaskList;
            List<TaskData> list1 = new List<TaskData>();
            List<TaskData> list2 = new List<TaskData>();
            foreach (TaskData data in dataList)
            {
                if (tempdata.mapCompletedTaskRecordDic[_mapData.id].Contains(data.taskId))
                {
                    list2.Add(data);
                }
                else
                {
                    list1.Add(data);
                }
            }

            for (int i = 0; i < list1.Count; i++)
            {
                GameObject obj = Instantiate(_assetHandle.Get<GameObject>("taskViewItem"), taskContent, false);
                obj.GetComponent<TaskViewItem>().Init(list1[i]);
            }
            for (int i = 0; i < list2.Count; i++)
            {
                GameObject obj = Instantiate(_assetHandle.Get<GameObject>("taskViewItem"), taskContent, false);
                obj.GetComponent<TaskViewItem>().Init(list2[i]);
            }
        }
        void Update()
        {
        }

        private void OnClickClose()
        {
            StartCoroutine(ShowAnimation());
        }

        private IEnumerator ShowAnimation()
        {
            EventCenter.Instance.TriggerEvent(EventMessages.UpdateTaskMainView);
            content.DOAnchorPos(new Vector2(0, -910), 0.4f)
                .SetEase(Ease.InBack);
            yield return new WaitForSeconds(0.4f);
            Hide();
        }
        protected override void OnHideComplete()
        {
            base.OnHideComplete();

            EventCenter.Instance.TriggerEvent(EventMessages.ShowPlayerInfoViewCartoon);
        }
    }
}