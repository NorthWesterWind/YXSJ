using System.Collections;
using UnityEngine;

namespace View._3D
{
    public enum ItemState
    {
        Flying,
        OnWorkbench,
        OnShelf,
        HeldByPlayer,
        HeldByAssistant,
        HeldByCustomer
    }
    public class Production : MonoBehaviour
    {
       public SpriteRenderer spriteRenderer;
       public float duration = 0.6f; // 飞行时间
       public float arcHeight = 1f;  // 抛物高度
       public float tiltAngle = 25f; // 飞行倾斜角

       private Vector3 startPos;
       private Vector3 endPos;
        
       public bool CanPlayerPick => state == ItemState.OnWorkbench;
       public bool CanAssistantPick => state == ItemState.OnWorkbench;
       public bool CanCustomerPick => state == ItemState.OnShelf;
       
       public ItemState state;
       public void SetState(ItemState newState)
       {
           state = newState;
       }

       public void FlyTo(Vector3 target)
       {
           StartCoroutine(FlyRoutine(target));
       }


       IEnumerator FlyRoutine(Vector3 target)
       {
           SetState(ItemState.Flying);

           Vector3 start = transform.position;
           float t = 0;
           float duration = 0.5f;

           while (t < 1f)
           {
               t += Time.deltaTime / duration;
               Vector3 pos = Vector3.Lerp(start, target, t);

               // 抛物线高度
               float h = Mathf.Sin(t * Mathf.PI) * 0.5f;
               pos.y += h;

               // 倾斜（飞行中）
               transform.rotation = Quaternion.Euler(0, 0, Mathf.Lerp(0, 25, t));

               transform.position = pos;
               yield return null;
           }

           // 落地直立
           transform.rotation = Quaternion.identity;
       }
    }
}
