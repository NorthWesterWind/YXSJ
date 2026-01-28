using System.Collections.Generic;
using Controller;
using Module;
using Module.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using View.Task;

namespace View.PlayerInfo
{
    public class TaskMainView : MonoBehaviour
    {
        public Image iconImage;
        public TextMeshProUGUI taskInfoTxt;
        public TextMeshProUGUI taskProgressTxt;
        public UIButton showBtn;
        private AssetHandle _assetHandle;
        public GameObject content;

        void Start()
        {
            _assetHandle = GetComponent<AssetHandle>();
            AddEvent();
            HandleUpdateTaskMainView();
        }

        public void AddEvent()
        {
            showBtn.onClick.RemoveAllListeners();
            showBtn.onClick.AddListener(OnClickShowBtn);
            EventCenter.Instance.AddListener(EventMessages.UpdateTaskMainView, HandleUpdateTaskMainView);
            EventCenter.Instance.AddListener(EventMessages.MapTaskDataPrepared, HandleUpdateTaskMainView);
            EventCenter.Instance.AddListener(EventMessages.HidePlayerGuide, HidePlayerGuide);
        }

        private void OnDestroy()
        {
            EventCenter.Instance.RemoveListener(EventMessages.UpdateTaskMainView, HandleUpdateTaskMainView);
            EventCenter.Instance.RemoveListener(EventMessages.MapTaskDataPrepared, HandleUpdateTaskMainView);
            EventCenter.Instance.RemoveListener(EventMessages.HidePlayerGuide, HidePlayerGuide);
        }

        #region 事件监听

        public void HidePlayerGuide(params object[] args)
        {
            content.SetActive(true);
            HandleUpdateTaskMainView();
        }

        public void HandleUpdateTaskMainView(params object[] args)
        {
            if (PlayerDataModule.Instance.data.listenInTaskList.Count == 0)
            {
                //没有监听的任务数据
                PlayerDataModule.Instance.data.listenInTaskList = DataController.Instance.GetTaskGroupIds();
                PlayerDataModule.Instance.FillStructureLockProgressData();
            }

            List<TaskData> dataList = PlayerDataModule.Instance.data.listenInTaskList;

            foreach (var data in dataList)
            {
                if (data.type == TaskType.Construct)
                {
                    if (PlayerDataModule.Instance.data.structureLockProgressDataList.Find(x =>
                            x.buildType == (BuildingType)data.aimId) != null)
                    {
                        if (PlayerDataModule.Instance.data.structureLockProgressDataList
                            .Find(x => x.buildType == (BuildingType)data.aimId).isUnlock)
                        {
                            PlayerDataModule.Instance.data.taskProgressDic[data.taskId] = 1;
                        }
                    }
                }
            }

            TaskData task = null;
            if (PlayerDataModule.Instance.data.nowTaskId == 0)
            {
                foreach (var item in PlayerDataModule.Instance.data.listenInTaskList)
                {
                    if (PlayerDataModule.Instance.data.taskProgressDic.ContainsKey(item.taskId))
                    {
                        if (PlayerDataModule.Instance.data.taskProgressDic[item.taskId] < item.keyValue)
                        {
                            PlayerDataModule.Instance.data.nowTaskId = item.taskId;
                            task = item;
                            break;
                        }

                        continue;
                    }
                    else
                    {
                        PlayerDataModule.Instance.data.nowTaskId = item.taskId;
                        task = item;
                        break;
                    }
                }
            }
            else
            {
                task = dataList.Find(x => x.taskId == PlayerDataModule.Instance.data.nowTaskId);
            }


            if (task != null)
            {
                taskInfoTxt.text = task.info + "。";
                if (PlayerDataModule.Instance.data.taskProgressDic.ContainsKey(task.taskId))
                {
                    //有进度


                    if (PlayerDataModule.Instance.data.taskProgressDic[task.taskId] >= task.keyValue)
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

                        if (list.Contains(task.taskId))
                        {
                            taskProgressTxt.text = "已完成";
                        }
                        else
                        {
                            taskProgressTxt.text = "可领取";
                        }
                    }
                    else
                    {
                        taskProgressTxt.text = "(" + PlayerDataModule.Instance.data.taskProgressDic[task.taskId] + "/" +
                                               task.keyValue + ")";
                    }
                }
                else
                {
                    taskProgressTxt.text = "(0/" + task.keyValue + ")";
                }

                iconImage.sprite =
                    _assetHandle.Get<Sprite>(Extensions.GetTaskInfoResNameByTypeWithId(task.type, task.aimId));
                if (task.type == TaskType.Upgrade || task.type == TaskType.Construct)
                {
                    iconImage.rectTransform.sizeDelta = new Vector2(160, 160);
                }
                else
                {
                    iconImage.rectTransform.sizeDelta = new Vector2(130, 130);
                }
            }
        }

        private void OnClickShowBtn()
        {
            EventCenter.Instance.TriggerEvent(EventMessages.HidePlayerInfoViewCartoon);
            UIController.Instance.Show<TaskPop>();
        }

        #endregion


        void Update()
        {
        }
    }
}