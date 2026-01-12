using System.Collections;
using Controller.Structure;
using Module.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace View
{
    public class ProductionInfo : MonoBehaviour
    {
       // public Image fillImage;
        public TextMeshProUGUI productionText;
        private float _productionTime;


        private Coroutine loopRoutine;

        private float speed = 1f;

        private float baseTime;
        public ProductionStation container;

        public void Init(float baseTime, float speed, int currentMaterialCount, StructureBase structureBase)
        {
            this.baseTime = baseTime;
            this.speed = speed;
            productionText.text = currentMaterialCount.ToString();
            // 生产循环不在 Init 自动开始（由 StartProductionLoop 控制）
            if (loopRoutine != null)
            {
                StopCoroutine(loopRoutine);
                loopRoutine = null;
            }
            container = structureBase as ProductionStation;
           // fillImage.fillAmount = 0f;
        }
        void LateUpdate()
        {
            if (!container) return;

            Vector3 screenPos =
                Camera.main.WorldToScreenPoint(container.infoTransform.position);

            // 🔑 像素对齐，防抖核心
            screenPos.x = Mathf.Round(screenPos.x);
            screenPos.y = Mathf.Round(screenPos.y);

            transform.position = screenPos;
        }



        public void UpdateText()
        {
            productionText.text = container.currentMaterialCount.ToString();
        }

        public void StartProductionLoop(ProductionStation container,
            BuildingType type,
            float baseTime,
            float speed)
        {
            this.speed = speed;
            this.baseTime = baseTime;

            if (loopRoutine != null)
                return;

            loopRoutine = StartCoroutine(ProductionLoop(container, type));
        }

        public void UpdateSpeed(float newSpeed)
        {
            speed = newSpeed;
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
            float t = 0f;
            float productionTime = baseTime / speed;

            // fillImage.fillAmount = 0;

            while (t < productionTime)
            {
                t += Time.deltaTime;
                float value = t / productionTime;
              //  fillImage.fillAmount = 2.9f * value;
                yield return null;
            }

            //fillImage.fillAmount = 2.9f;
            EventCenter.Instance.TriggerEvent(EventMessages.ProductionComplete, type);
        }
    }
}
