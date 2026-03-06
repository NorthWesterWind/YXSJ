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
        public float spawnTime { get; private set; }

        [Header("飞行参数")]


        private System.Action _onArrive;
        public void Init(DropItemType type)
        {
            itemType = type;
            spawnTime = Time.time;
            canPickup = false;
            itemName = "DropObj";
            _onArrive = null;
            ScenePickupController.Instance.materials.Add(this);
            spriteRenderer.sprite = _assetHandle.Get<Sprite>(Extensions.GetDropItemResNameByType(type));
        }
        
        public bool CanBePickedByCollector(float delay)
        {
            return Time.time - spawnTime >= Mathf.Max(0f, delay);
        }
        void OnDestroy()
        {
            if (ScenePickupController.Instance.materials.Contains(this))
                ScenePickupController.Instance.materials.Remove(this);
        }


        /// <summary>
        /// 对外唯一入口：让物品飞向目标
        /// </summary>
        public void FlyTo(Transform picker, Transform receivePoint, System.Action onArrive = null)
        {
            if (!gameObject.activeInHierarchy || picker == null || receivePoint == null) return;

            this.picker = picker;
            this.pickerReceivePoint = receivePoint;
            this._onArrive = onArrive;
            StopAllCoroutines();
            StartCoroutine(FlyCoroutine(true, picker.gameObject, true));
        }
        public void FlyTo(Transform receivePoint)
        {
            if (!gameObject.activeInHierarchy || receivePoint == null) return;
            this.picker = null;
            this.pickerReceivePoint = receivePoint;
            this._onArrive = null;
            StopAllCoroutines();
            StartCoroutine(FlyCoroutine(false, null, true));
        }

        public void ForceStop()
        {
            StopAllCoroutines();
        }
        private IEnumerator FlyCoroutine(bool isPlayer = false, GameObject player = null, bool destroyAfterArrive = true)
        {
            if (pickerReceivePoint == null)
            {
                yield break;
            }

            Vector2 start = transform.position;
            Vector2 end = pickerReceivePoint.position;
            Vector2 control = Vector2.Lerp(start, end, 0.5f) + Vector2.up * flyHeight;
            float timer = 0f;
            while (timer < flyDuration)
            {
                if (pickerReceivePoint == null)
                    yield break;
                if (isPlayer && picker == null)
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
            if (isPlayer && player != null)
            {
                OnPicked(player);
            }

            if (destroyAfterArrive && gameObject != null)
            {
                if (ScenePickupController.Instance != null &&
                    ScenePickupController.Instance.materials.Contains(this))
                {
                    ScenePickupController.Instance.materials.Remove(this);
                }
                Destroy(gameObject);
            }
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
