using System.Collections.Generic;
using Controller;
using Module;
using Module.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace View.Task
{
    public class TaskViewItem : MonoBehaviour
    {
        public Image iconImage;
        public TextMeshProUGUI infotxt;
        public TextMeshProUGUI progresstxt;
        public UIButton btn;
        public TextMeshProUGUI btntxt;

        public bool isCompleted = false;
        public bool canGetReward = false;
        public TaskData data;
        private AssetHandle _assetHandle;

        public TextMeshProUGUI jmztxt;
        public GameObject TongBiObj;
        public TextMeshProUGUI TongBiTxt;
        public GameObject JingYuanBaoObj;
        public TextMeshProUGUI JingYuanBaoTxt;
        public UIButton testBtn;
        void Start()
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(OnClickTask);
            _assetHandle = GetComponent<AssetHandle>();

            // testBtn.onClick.RemoveAllListeners();
            // testBtn.onClick.AddListener(OnClickTest);
        }
        void OnEnable()
        {
            EventCenter.Instance.AddListener(EventMessages.UpdateTaskInfo, HandleUpdateTaskInfo);
        }
        void OnDisable()
        {
            EventCenter.Instance.RemoveListener(EventMessages.UpdateTaskInfo, HandleUpdateTaskInfo);
        }

        private void OnClickTask()
        {
            if (isCompleted)
                return;
            if (canGetReward)
            {
                List<int> list = PlayerDataModule.Instance.data.mapCompletedTaskRecordList_1;
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
                list.Add(data.taskId);
                PlayerDataModule.Instance.GetTaskReward(data.rewardId);
                HandleUpdateTaskInfo(data.taskId);
                
                EventCenter.Instance.TriggerEvent(EventMessages.HasTaskComplete , data);
            }

            // else
            // {
            //     //触发寻找逻辑
            //     EventCenter.Instance.TriggerEvent(EventMessages.TriggerSearch);
            // }

        }
        public void OnClickTest()
        {
            
        }

        public void Init(TaskData taskData)
        {
            isCompleted = false;
            canGetReward = false;
            data = taskData;
            infotxt.text = data.info + "。";
            RewardData rewardData = DataController.Instance.taskRewardDataDic[data.rewardId];
            jmztxt.text = "x"  + rewardData.Jmz.ToString();
            if(rewardData.Tq > 0)
            {
                TongBiObj.SetActive(true);
                TongBiTxt.text = "x"  + rewardData.Tq.ToString();
            }
            else
            {
                TongBiObj.SetActive(false);
            }
            if(rewardData.Jyb > 0)
            {
                JingYuanBaoObj.SetActive(true);
                JingYuanBaoTxt.text =  "x"  + rewardData.Jyb.ToString();
            }
            else
            {
                JingYuanBaoObj.SetActive(false);
            }
            PlayerData playerdata = PlayerDataModule.Instance.data;
            List<int> list = playerdata.mapCompletedTaskRecordList_1;
            if (playerdata.currentMapID == 2)
            {
                list = playerdata.mapCompletedTaskRecordList_2;
            }
            else if (playerdata.currentMapID == 3)
            {
                list = playerdata.mapCompletedTaskRecordList_3;
            }
            else if (playerdata.currentMapID == 4)
            {
                list = playerdata.mapCompletedTaskRecordList_4;
            }
            else if (playerdata.currentMapID == 5)
            {
                list = playerdata.mapCompletedTaskRecordList_5;
            }
            if (list.Contains(data.taskId))
            {
                isCompleted = true;

                progresstxt.text = "(" + (int)data.keyValue + "/" + data.keyValue + ")";
                btntxt.text = "已完成";
            }
            else if (PlayerDataModule.Instance.data.taskProgressDic.ContainsKey(data.taskId))
            {
                if (PlayerDataModule.Instance.data.taskProgressDic[data.taskId] >= data.keyValue)
                {
                    canGetReward = true;
                    btntxt.text = "领取";
                    progresstxt.text = "(" + (int)data.keyValue + "/" + data.keyValue + ")";
                }
                else
                {
                    canGetReward = false;
                    btntxt.text = "进行中";
                    progresstxt.text = "(" + (int)PlayerDataModule.Instance.data.taskProgressDic[data.taskId] + "/" + data.keyValue + ")";
                }
            }
            else
            {
                btntxt.text = "进行中";
                progresstxt.text = "(0/" + data.keyValue + ")";
            }
            if (_assetHandle == null)
            {
                _assetHandle = GetComponent<AssetHandle>();
            }
            iconImage.sprite = _assetHandle.Get<Sprite>(Extensions.GetTaskInfoResNameByTypeWithId(data.type, data.aimId));
            if (data.type == TaskType.Upgrade || data.type == TaskType.Construct)
            {
                iconImage.rectTransform.sizeDelta = new Vector2(160, 160);
            }
            else
            {
                iconImage.rectTransform.sizeDelta = new Vector2(130, 130);
            }
        }
        public void HandleUpdateTaskInfo(params object[] objs)
        {
            int id = (int)objs[0];
            if (id != data.taskId)
            {
                return;
            }
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

             if (list.Contains(data.taskId))
            {
                isCompleted = true;

                progresstxt.text = "(" + (int)data.keyValue + "/" + data.keyValue + ")";
                btntxt.text = "已完成";
            }
            else if (PlayerDataModule.Instance.data.taskProgressDic.ContainsKey(data.taskId))
            {
                if (PlayerDataModule.Instance.data.taskProgressDic[data.taskId] >= data.keyValue)
                {
                    canGetReward = true;
                    btntxt.text = "领取";
                    progresstxt.text = "(" + (int)data.keyValue + "/" + data.keyValue + ")";
                }
                else
                {
                    canGetReward = false;
                    btntxt.text = "进行中";
                    progresstxt.text = "(" + (int)PlayerDataModule.Instance.data.taskProgressDic[data.taskId] + "/" + data.keyValue + ")";
                }
            }
            else
            {
                btntxt.text = "进行中";
                progresstxt.text = "(0/" + (int)data.keyValue + ")";
            }
        }
    }
}