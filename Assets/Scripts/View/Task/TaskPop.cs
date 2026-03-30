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
            RefreshTaskPanel();
            content.DOAnchorPos(new Vector2(0, 0), 0.5f).SetEase(Ease.InBack);
        }

        protected override void AddEventListener()
        {
            base.AddEventListener();
            closeBtn.onClick.AddListener(OnClickClose);

            EventCenter.Instance.AddListener(EventMessages.HasTaskComplete, HandleHasTaskComplete);
            EventCenter.Instance.AddListener(EventMessages.UpdateTaskMainView, HandleUpdateTaskMainView);
            EventCenter.Instance.AddListener(EventMessages.MapTaskDataPrepared, HandleUpdateTaskMainView);

            rewardBtn.onClick.RemoveAllListeners();
            rewardBtn.onClick.AddListener(OnClickRewardBtn);
        }

        public override void RemoveEventListener()
        {
            base.RemoveEventListener();
            EventCenter.Instance.RemoveListener(EventMessages.HasTaskComplete, HandleHasTaskComplete);
            EventCenter.Instance.RemoveListener(EventMessages.UpdateTaskMainView, HandleUpdateTaskMainView);
            EventCenter.Instance.RemoveListener(EventMessages.MapTaskDataPrepared, HandleUpdateTaskMainView);
        }

        void OnClickRewardBtn()
        {
            if (PlayerDataModule.Instance.data.taskPopCompleted >= WorldData.taskboxNeedDic[PlayerDataModule.Instance.data.currentMapID])
            {
                if (PlayerDataModule.Instance.data.currentMapID < 3)
                {
                    PlayerDataModule.Instance.data.goldIngot += DataController.Instance.giftpackDataDic[4].JinYuanBao;
                    var dic = PlayerDataModule.Instance.LotteryCard(DataController.Instance.giftpackDataDic[4]);
                    UIController.Instance.Show<RewardConfirmView>(dic, new Dictionary<CurrencyType, int> { { CurrencyType.JingYuanBao, DataController.Instance.giftpackDataDic[4].JinYuanBao } });
                    EventCenter.Instance.TriggerEvent(EventMessages.UpdatePlayerMoneyInfo);
                }
                else if (PlayerDataModule.Instance.data.currentMapID < 5)
                {
                    PlayerDataModule.Instance.data.goldIngot += DataController.Instance.giftpackDataDic[5].JinYuanBao;
                    var dic = PlayerDataModule.Instance.LotteryCard(DataController.Instance.giftpackDataDic[5]);
                    UIController.Instance.Show<RewardConfirmView>(dic, new Dictionary<CurrencyType, int> { { CurrencyType.JingYuanBao, DataController.Instance.giftpackDataDic[2].JinYuanBao } });
                    EventCenter.Instance.TriggerEvent(EventMessages.UpdatePlayerMoneyInfo);
                }
                else
                {
                    PlayerDataModule.Instance.data.goldIngot += DataController.Instance.giftpackDataDic[6].JinYuanBao;
                    var dic = PlayerDataModule.Instance.LotteryCard(DataController.Instance.giftpackDataDic[6]);
                    UIController.Instance.Show<RewardConfirmView>(dic, new Dictionary<CurrencyType, int> { { CurrencyType.JingYuanBao, DataController.Instance.giftpackDataDic[2].JinYuanBao } });
                    EventCenter.Instance.TriggerEvent(EventMessages.UpdatePlayerMoneyInfo);
                }

                PlayerDataModule.Instance.data.taskPopCompleted -= WorldData.taskboxNeedDic[PlayerDataModule.Instance.data.currentMapID];
                RefreshTaskPanel();
            }
        }

        public void HandleHasTaskComplete(params object[] args)
        {
            var list = GetCurrentCompletedTaskList();
            int tempvalue = 0;
            bool isSet = false;
            foreach (var taskData in PlayerDataModule.Instance.data.listenInTaskList)
            {
                if (!list.Contains(taskData.taskId))
                {
                    tempvalue += 1;
                    if (!isSet)
                    {
                        PlayerDataModule.Instance.data.nowTaskId = taskData.taskId;
                        isSet = true;
                    }
                }
            }

            if (tempvalue == 0)
            {
                int id = 0;
                for (int i = 0; i < PlayerDataModule.Instance.data.listenInTaskList.Count; i++)
                {
                    id = Mathf.Max(id, PlayerDataModule.Instance.data.listenInTaskList[i].taskId);
                }

                if (PlayerDataModule.Instance.data.currentMapID == 1)
                {
                    if (id < 30)
                    {
                        PlayerDataModule.Instance.data.nowTaskId = id + 1;
                        PlayerDataModule.Instance.data.listenInTaskList = DataController.Instance.GetTaskGroupIds();
                        PlayerDataModule.Instance.FillStructureLockProgressData();
                    }
                }
                else if (PlayerDataModule.Instance.data.currentMapID == 2)
                {
                    if (id < 60)
                    {
                        PlayerDataModule.Instance.data.nowTaskId = id + 1;
                        PlayerDataModule.Instance.data.listenInTaskList = DataController.Instance.GetTaskGroupIds();
                        PlayerDataModule.Instance.FillStructureLockProgressData();
                    }
                }
                else if (PlayerDataModule.Instance.data.currentMapID == 3)
                {
                    if (id < 90)
                    {
                        PlayerDataModule.Instance.data.nowTaskId = id + 1;
                        PlayerDataModule.Instance.data.listenInTaskList = DataController.Instance.GetTaskGroupIds();
                        PlayerDataModule.Instance.FillStructureLockProgressData();
                    }
                }
                else if (PlayerDataModule.Instance.data.currentMapID == 4)
                {
                    if (id < 100)
                    {
                        PlayerDataModule.Instance.data.nowTaskId = id + 1;
                        PlayerDataModule.Instance.data.listenInTaskList = DataController.Instance.GetTaskGroupIds();
                        PlayerDataModule.Instance.FillStructureLockProgressData();
                    }
                }
                else if (PlayerDataModule.Instance.data.currentMapID == 5)
                {
                    if (id < 110)
                    {
                        PlayerDataModule.Instance.data.nowTaskId = id + 1;
                        PlayerDataModule.Instance.data.listenInTaskList = DataController.Instance.GetTaskGroupIds();
                        PlayerDataModule.Instance.FillStructureLockProgressData();
                    }
                }

                EventCenter.Instance.TriggerEvent(EventMessages.UpdateTaskMainView);
            }

            RefreshTaskPanel();
        }

        public void HandleUpdateTaskMainView(params object[] args)
        {
            RefreshTaskPanel();
        }

        public void UpdateTaskContent()
        {
            Extensions.ClearChildren(taskContent);
            PlayerData tempdata = PlayerDataModule.Instance.data;
            List<TaskData> dataList = tempdata.listenInTaskList;
            List<TaskData> list1 = new List<TaskData>();
            List<TaskData> list2 = new List<TaskData>();
            var list = GetCurrentCompletedTaskList();

            foreach (TaskData data in dataList)
            {
                if (list.Contains(data.taskId))
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

        private void RefreshTaskPanel()
        {
            PlayerData tempdata = PlayerDataModule.Instance.data;
            if (tempdata == null)
            {
                return;
            }

            _mapData = DataController.Instance.mapDataDic[tempdata.currentMapID];
            var list = GetCurrentCompletedTaskList();
            int count = list.Count;
            mapNameTxt.text = _mapData.name;
            if (_mapData.id == 1 || _mapData.id == 2)
            {
                rewardBtnImg.sprite = assetHandle.Get<Sprite>("玄银宝箱");
            }
            else if (_mapData.id == 3 || _mapData.id == 4)
            {
                rewardBtnImg.sprite = assetHandle.Get<Sprite>("天灵宝箱");
            }
            else
            {
                rewardBtnImg.sprite = assetHandle.Get<Sprite>("紫金宝箱");
            }

            int tempvalue1 = count / _mapData.taskGroupSize;
            mapprogressTxt.text = tempvalue1 + "/" + _mapData.taskGroupNum;
            sliderText.text = tempdata.taskPopCompleted + "/" + WorldData.taskboxNeedDic[tempdata.currentMapID];
            float value = tempdata.taskPopCompleted * 1f / WorldData.taskboxNeedDic[tempdata.currentMapID];
            sliderFill.fillAmount = value;
            redPoint.SetActive(value >= 1f);
            UpdateTaskContent();
        }

        private List<int> GetCurrentCompletedTaskList()
        {
            var list = PlayerDataModule.Instance.data.mapCompletedTaskRecordList_1;
            if (PlayerDataModule.Instance.data.currentMapID == 2)
            {
                list = PlayerDataModule.Instance.data.mapCompletedTaskRecordList_2;
            }
            else if (PlayerDataModule.Instance.data.currentMapID == 3)
            {
                list = PlayerDataModule.Instance.data.mapCompletedTaskRecordList_3;
            }
            else if (PlayerDataModule.Instance.data.currentMapID == 4)
            {
                list = PlayerDataModule.Instance.data.mapCompletedTaskRecordList_4;
            }
            else if (PlayerDataModule.Instance.data.currentMapID == 5)
            {
                list = PlayerDataModule.Instance.data.mapCompletedTaskRecordList_5;
            }

            return list;
        }
    }
}
