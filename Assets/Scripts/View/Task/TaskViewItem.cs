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
        PlayerData data;

        void Start()
        {
            data = ModuleMgr.Instance.GetModule<PlayerDataModule>().data;
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(OnClickTask);
        }

        private void OnClickTask()
        {
            UIController.Instance.Show<TaskPop>();
        }
    }
}