using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace View.PlayerInfo
{
    public class TaskMainView : MonoBehaviour
    {
        public Image iconImage;
        public TextMeshProUGUI taskInfoTxt;
        public TextMeshProUGUI taskProgressTxt;
        public UIButton showBtn;
        void Start()
        {
            AddEvent();
        }

        public void AddEvent()
        {
            showBtn.onClick.RemoveAllListeners();
            showBtn.onClick.AddListener(OnClickShowBtn);
        }

        #region 事件监听
        public void HandleTaskProgressUpdate(params object[] args)
        {
            
        }

        private void OnClickShowBtn()
        {
           // UIController.Instance.Show<>();
        }
        

        #endregion
        
        
       
        void Update()
        {
        
        }
    }
}
