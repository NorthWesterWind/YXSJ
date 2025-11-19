using Module.Data;
using UnityEngine;
using Utils;
using View;

namespace Controller
{
    public enum InteractionType
    {
        Immediate, // 接触即触发
        OnStop     // 停止时触发
    }
    /// <summary>
    /// 用于挂载到可交互对象身上
    /// </summary>
    public class InteractionController : MonoBehaviour
    {
        public InteractionType interactionType;
        
        public ShowUIType  showUIType;
        public void Interact()
        {
            Debug.Log($"交互触发：{name}");
            switch (showUIType)
            {
                case ShowUIType.TestView:
                    UIController.Instance.Show<TestView>();
                    break;
            }
        }
    }
}
