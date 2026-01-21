using System.Collections;
using Controller.Player;
using Module.Data;
using UnityEngine;
using Utils;

namespace Controller.Pickups
{
    public class DropController : BasePickup, IPickable
    {
        public DropItemType itemType;
        public SpriteRenderer spriteRenderer;

        [Header("飞行参数")]


        private System.Action _onArrive;
        public void Init(DropItemType type)
        {
            itemType = type;
            canPickup = false;
            itemName = "DropObj";
            _onArrive = null;
            ScenePickupController.Instance.materials.Add(this);
            spriteRenderer.sprite = _assetHandle.Get<Sprite>(Extensions.GetDropItemResNameByType(type));


        }
        void Update()
        {
            int order = 30100 - Mathf.RoundToInt(transform.position.y * 100);

            spriteRenderer.sortingOrder = order ;
        }

        /// <summary>
        /// 对外唯一入口：让物品飞向目标
        /// </summary>
        public void FlyTo(Transform picker, Transform receivePoint, System.Action onArrive = null)
        {
            if (!gameObject.activeInHierarchy) return;

            this.picker = picker;
            this.pickerReceivePoint = receivePoint;
            this._onArrive = onArrive;
            StopAllCoroutines();
            StartCoroutine(FlyCoroutine(true, picker.gameObject));
        }
        public void FlyTo(Transform receivePoint)
        {
            if (!gameObject.activeInHierarchy) return;
            this.pickerReceivePoint = receivePoint;
            StopAllCoroutines();
            StartCoroutine(FlyCoroutine());
        }

        public void ForceStop()
        {
            StopAllCoroutines();
        }
        private IEnumerator FlyCoroutine(bool isPlayer = false, GameObject player = null)
        {
            Vector2 start = transform.position;
            Vector2 end = pickerReceivePoint.position;
            Vector2 control = Vector2.Lerp(start, end, 0.5f) + Vector2.up * flyHeight;
            float timer = 0f;
            while (timer < flyDuration)
            {
                if (picker == null || pickerReceivePoint == null)
                    yield break;
                float t = flyCurve.Evaluate(timer / flyDuration);
                Vector2 pos =
                    (1 - t) * (1 - t) * start +
                    2 * (1 - t) * t * control +
                    t * t * end;
                transform.position = pos;
                timer += Time.deltaTime;
                yield return null;
            }
            transform.position = end;
            _onArrive?.Invoke();
            if (isPlayer)
            {
                OnPicked(player);
            }
            // 从场景列表移除
            ScenePickupController.Instance.materials.Remove(this);
            ObjectPoolManager.Instance.ReturnObject(itemName, gameObject);
        }

        public void OnPicked(GameObject pickerObj)
        {
            if (pickerObj.TryGetComponent(out PlayerController player))
            {
                player.AddDropItem(itemType);
            }
            else if (pickerObj.TryGetComponent(out CollectorController collector))
            {
                collector.AddDropItem(itemType);
            }
        }
    }
}
