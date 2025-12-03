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
        public TextMeshProUGUI btnText;
        public bool isCompleted = false;
        public TaskData data;
        private AssetHandle _assetHandle;
        void Start()
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(OnClickTask);
            _assetHandle = GetComponent<AssetHandle>();
            EventCenter.Instance.AddListener(EventMessages.UpdateTaskInfo , HandleUpdateTaskInfo);
        }

        private void OnDestroy()
        {
            EventCenter.Instance.RemoveListener(EventMessages.UpdateTaskInfo , HandleUpdateTaskInfo);
        }

        private void OnClickTask()
        {
            if(isCompleted)
                return;
            if (ModuleMgr.Instance.GetModule<PlayerDataModule>().data.taskProgressDic.ContainsKey(data.taskId)
                && ModuleMgr.Instance.GetModule<PlayerDataModule>().data.taskProgressDic[data.taskId] == data.keyValue)
            {
                ModuleMgr.Instance.GetModule<PlayerDataModule>().GetTaskReward(data.rewardId);
                HandleUpdateTaskInfo();
            }
            else
            {
                //触发寻找逻辑
                EventCenter.Instance.TriggerEvent(EventMessages.TriggerSearch);
            }
            
        }

        public void Init(TaskData taskData)
        {
            isCompleted = false;
            data = taskData;
            //iconImage.sprite = _assetHandle.Get<Sprite>(Extensions.GetTaskInfoResNameByTypeWithId(data.type, data.taskId));
            infotxt.text = data.info;
            if (ModuleMgr.Instance.GetModule<PlayerDataModule>().data.completedTaskIdList.Contains(data.taskId))
            {
                isCompleted = true;
                progresstxt.text = "(" + data.keyValue + "/" +  data.keyValue + ")";
                btnText.text = "完成";
            }else if (ModuleMgr.Instance.GetModule<PlayerDataModule>().data.taskProgressDic.ContainsKey(data.taskId))
            {
                btnText.text = "寻找";
                progresstxt.text = "(" + ModuleMgr.Instance.GetModule<PlayerDataModule>().data.taskProgressDic[data.taskId] + "/" +  data.keyValue + ")";
            }
            else
            {
                btnText.text = "寻找";
                progresstxt.text = "(0/" + data.keyValue + ")";
            }
        }
        public void HandleUpdateTaskInfo(params object[] objs)
        {
            int id = (int)objs[0];
            if (id != data.taskId)
            {
                return;
            }
            if (ModuleMgr.Instance.GetModule<PlayerDataModule>().data.completedTaskIdList.Contains(data.taskId))
            {
                isCompleted = true;
                progresstxt.text = "(" + data.keyValue + "/" +  data.keyValue + ")";
                btnText.text = "完成";
            }else if (ModuleMgr.Instance.GetModule<PlayerDataModule>().data.taskProgressDic.ContainsKey(data.taskId))
            {
                btnText.text = "寻找";
                progresstxt.text = "(" + ModuleMgr.Instance.GetModule<PlayerDataModule>().data.taskProgressDic[data.taskId] + "/" +  data.keyValue + ")";
            }
            else
            {
                btnText.text = "寻找";
                progresstxt.text = "(0/" + data.keyValue + ")";
            }
        }
    }
}