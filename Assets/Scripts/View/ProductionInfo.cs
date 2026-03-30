using System.Collections;
using Controller.Structure;
using Module;
using Module.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace View
{
    public class ProductionInfo : MonoBehaviour
    {
        public Image fillImage;
        public TextMeshProUGUI productionText;
        private Coroutine loopRoutine;

        public ProductionStation container;

        public void Init(int currentMaterialCount, StructureBase structureBase)
        {
            productionText.text = currentMaterialCount.ToString();
            // 生产循环不在 Init 自动开始（由 StartProductionLoop 控制）
            if (loopRoutine != null)
            {
                StopCoroutine(loopRoutine);
                loopRoutine = null;
            }
            container = structureBase as ProductionStation;
            fillImage.fillAmount = 0f;
        }
        private Coroutine followRoutine;



      

        public void UpdateText()
        {
            productionText.text = container.currentMaterialCount.ToString();
        }

        public void StartProductionLoop(ProductionStation container,
            BuildingType type)
        {
            if (loopRoutine != null)
                return;

            loopRoutine = StartCoroutine(ProductionLoop(container, type));
        }

        private IEnumerator ProductionLoop(ProductionStation container, BuildingType type)
        {
            while (container.currentMaterialCount > 0)
            {
                yield return StartCoroutine(PlayProgressBar(type));
                container.currentMaterialCount -= 1;
                productionText.text = container.currentMaterialCount.ToString();

                Debug.Log($"生产完成一个 {type}");
            }

            loopRoutine = null;
            container.OnProductionFinished();
        }

        private IEnumerator PlayProgressBar(BuildingType type)
        {
            float progress = 0f;
            fillImage.fillAmount = 0;
            while (progress < 1f)
            {
                float currentProductionTime = GetCurrentProductionTime(type);
                progress += Time.deltaTime / currentProductionTime;
                fillImage.fillAmount = Mathf.Clamp01(progress);
                yield return null;
            }
            fillImage.fillAmount = 1f;
            EventCenter.Instance.TriggerEvent(EventMessages.ProductionComplete, type);
        }

        private float GetCurrentProductionTime(BuildingType type)
        {
            var stationData = PlayerDataModule.Instance.GetProductStationData(type);
            int timelevel = stationData != null ? stationData.timelevel : 1;
            if (!WorldData.productStationWorkingTimeDic.TryGetValue(timelevel, out float productionTime))
            {
                productionTime = WorldData.productStationWorkingTimeDic[1];
            }

            if (PlayerDataModule.Instance.data.speedTime > 0f)
            {
                productionTime = 0.2f;
            }

            return Mathf.Max(0.01f, productionTime);
        }
    }
}
