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
            if(PlayerDataModule.Instance.data.guideStep != GuideStep.Finished)
            {
                 content.SetActive(false);
            }

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
            }
            List<TaskData> dataList = DataController.Instance.GetTaskGroupIds();
            TaskData task = dataList.Find(x => x.taskId == PlayerDataModule.Instance.data.nowTaskId);

            if (task != null)
            {
                taskInfoTxt.text = task.info;
                if (PlayerDataModule.Instance.data.taskProgressDic.ContainsKey(task.taskId))
                {
                    //有进度
                    taskProgressTxt.text = "(" + PlayerDataModule.Instance.data.taskProgressDic.ContainsKey(task.taskId) + "/" + task.keyValue + ")";
                }
                else
                {
                    taskProgressTxt.text = "(0/" + task.keyValue + ")";
                }
                iconImage.sprite = _assetHandle.Get<Sprite>(Extensions.GetTaskInfoResNameByTypeWithId(task.type, task.aimId));
                if (task.type == TaskType.Upgrade || task.type == TaskType.Construct)
                {
                    iconImage.rectTransform.sizeDelta = new Vector2(160, 160);
                    //iconImage.rectTransform.position = new Vector3(23, 0, 0);
                }
                else
                {
                    iconImage.rectTransform.sizeDelta = new Vector2(130, 130);
                    //iconImage.rectTransform.position = new Vector3(32, 0, 0);
                }
            }
        }

        private void OnClickShowBtn()
        {
            UIController.Instance.Show<TaskPop>();
        }


        #endregion



        void Update()
        {

        }
    }
}
