using Controller.Structure;
using Module.Data;
using Spine.Unity;
using UnityEngine;
using Utils;
using View;
using View.OrderFunction;

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

        public ShowUIType showUIType;
        public BuildingType buildingType;
      
        public bool SpeedUp;
        public void Interact()
        {
            if (SpeedUp)
            {
                EventCenter.Instance.TriggerEvent(EventMessages.StructureSpeedUp, buildingType);

                return;
            }

            switch (showUIType)
            {
                case ShowUIType.TestView:
                    UIController.Instance.Show<TestView>();
                    break;
                case ShowUIType.LianQiLu:
                case ShowUIType.YuShaHu:
                    UIController.Instance.Show<ProductionStationPop>(buildingType, (GameController.Instance.buildings[buildingType] as ProductionStation).goodsType);
                    break;
                case ShowUIType.LingZhangTai:
                    UIController.Instance.Show<LingZhangTaiPop>();
                    break;
                case ShowUIType.YunDiGe:
                    UIController.Instance.Show<YunDiGePop>();
                    break;
                case ShowUIType.LingChuGe:
                     UIController.Instance.Show<LingChuGePop>();
                    break;
                case  ShowUIType.OrderView:
                    UIController.Instance.Show<OrderFunctionView>();
                    break;

            }
        }
        public void CloseInteract()
        {
             if (SpeedUp)
            {
                EventCenter.Instance.TriggerEvent(EventMessages.StructureSpeedDown, buildingType);

                return;
            }
            switch (showUIType)
            {
                case ShowUIType.TestView:
                    UIController.Instance.Hide<TestView>();
                    break;
                case ShowUIType.LianQiLu:
                case ShowUIType.YuShaHu:
                    UIController.Instance.Hide<ProductionStationPop>();
                    break;
                case ShowUIType.LingZhangTai:
                    // UIController.Instance.Hide<LingZhangTaiPop>();
                    break;
                case ShowUIType.YunDiGe:
                    // UIController.Instance.Hide<YunDiGePop>();
                    break;
                case ShowUIType.LingChuGe:
                    // UIController.Instance.Hide<LingChuGePop>();
                    break;
            }
        }
    }
}
