using System;
using System.Collections;
using UnityEngine;
using Utils;

namespace Controller.Pickups
{
    public abstract class BasePickup : MonoBehaviour
    {
        public bool canPickup = false;
        public float flyHeight = 2f;      
        public float flyDuration = 0.5f;  
        public AnimationCurve flyCurve;

        protected Transform picker;
        protected Transform pickerReceivePoint;
        public string itemName;
        protected AssetHandle _assetHandle;

        private void Awake()
        {
            _assetHandle = GetComponent<AssetHandle>();
        }

        public void StartAttract(Transform picker, Transform receivePoint)
        {
            if (!canPickup) return;
            this.picker = picker;
            this.pickerReceivePoint = receivePoint;

            StartCoroutine(FlyToPicker());
        }

        private IEnumerator FlyToPicker()
        {
            Vector2 start = transform.position;
            Vector2 control = start + Vector2.up * flyHeight;
            float timer = 0f;

            while (timer < flyDuration)
            {
                float t = flyCurve.Evaluate(timer / flyDuration);
                Vector2 pos = (1 - t) * (1 - t) * start +
                              2 * (1 - t) * t * control +
                              t * t * (Vector2)pickerReceivePoint.position;

                transform.position = pos;

                timer += Time.deltaTime;
                yield return null;
            }
            // 再检查一次，保证池子没提前回收
            if (picker == null || pickerReceivePoint == null || !gameObject.activeInHierarchy)
                yield break;
            transform.position = pickerReceivePoint.position;

            // 让具体物品去执行拾取逻辑
            GetComponent<IPickable>().OnPicked(picker.gameObject);
            ObjectPoolManager.Instance.ReturnObject(itemName , gameObject);
            if (ScenePickupController.Instance.materials.Contains(this))
            {
                ScenePickupController.Instance.materials.Remove(this);
            }
            else
            {
                ScenePickupController.Instance.products.Remove(this);
            }
           
        }
    }

}
