
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
       public SpriteRenderer fillImage;
       public TextMeshPro productionText;
       private float _productionTime;
      

       private Coroutine loopRoutine;

       private float speed = 1f;
       
       private float baseTime;
       public ProductionStation container;
       
       public void Init(float baseTime, float speed, int currentMaterialCount , StructureBase structureBase )
       {
           this.baseTime = baseTime;
           this.speed = speed;

           fillImage.size = new Vector2( 0 ,0);
           productionText.text = currentMaterialCount.ToString();
           // 生产循环不在 Init 自动开始（由 StartProductionLoop 控制）
           if (loopRoutine != null)
           {
               StopCoroutine(loopRoutine);
               loopRoutine = null;
           }
           container = structureBase as ProductionStation;
       }

       public void UpdateText()
       {
           productionText.text = container.currentMaterialCount.ToString();
       }
       
       public void StartProductionLoop(ProductionStation container,
           StructureType type,
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

       private IEnumerator ProductionLoop(ProductionStation container, StructureType type)
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

       private IEnumerator PlayProgressBar(StructureType type)
       {
           float t = 0f;
           float productionTime = baseTime / speed; 

           fillImage.size= new Vector2(0,0.08f);

           while (t < productionTime)
           {
               t += Time.deltaTime;
               float value = t / productionTime;
               fillImage.size =  new Vector2( 2.9f*value ,0.08f )  ;
               yield return null;
           }

           fillImage.size= new Vector2(2.9f,0.08f);
           EventCenter.Instance.TriggerEvent(EventMessages.ProductionComplete ,type );
       }
    }
}
