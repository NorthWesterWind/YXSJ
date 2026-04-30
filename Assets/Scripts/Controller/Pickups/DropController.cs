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
        public int count = 1;
        public SpriteRenderer spriteRenderer;
        public float spawnTime { get; private set; }
        [SerializeField] private float autoDestroyDelay = 15f;

        [Header("飞行参数")]


        private System.Action _onArrive;
        private float pickableStartTime = -1f;
        public void Init(DropItemType type, int itemCount = 1)
        {
            itemType = type;
            count = Mathf.Max(1, itemCount);
            spawnTime = Time.time;
            pickableStartTime = -1f;
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

        private void Update()
        {
            if (!canPickup || isTaken)
            {
                pickableStartTime = -1f;
                return;
            }

            if (pickableStartTime < 0f)
            {
                pickableStartTime = Time.time;
                return;
            }

            if (Time.time - pickableStartTime < autoDestroyDelay)
            {
                return;
            }

            if (ScenePickupController.Instance != null &&
                ScenePickupController.Instance.materials.Contains(this))
            {
                ScenePickupController.Instance.materials.Remove(this);
            }

            Destroy(gameObject);
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
            int originalSortingOrder = spriteRenderer != null ? spriteRenderer.sortingOrder : 0;
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
                ApplyFlyingSortingOrder(spriteRenderer, picker, end.y, originalSortingOrder);
                transform.position = pos;
                timer += Time.deltaTime;
                yield return null;
            }
            ApplyFlyingSortingOrder(spriteRenderer, picker, end.y, originalSortingOrder);
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
                for (int i = 0; i < count; i++)
                {
                    player.AddDropItem(itemType);
                }
            }
            else if (pickerObj.TryGetComponent(out CollectorController collector))
            {
                for (int i = 0; i < count; i++)
                {
                    collector.AddDropItem(itemType);
                }
            }
        }
    }
}
