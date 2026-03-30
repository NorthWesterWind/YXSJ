using System;
using System.Collections;
using Controller.Player;
using UnityEngine;
using Utils;

namespace Controller.Pickups
{
    public abstract class BasePickup : MonoBehaviour
    {
        protected const int FlyingSortingOffset = 2;

        public bool canPickup = false;
        public float flyHeight = 1.5f;
        public float flyDuration = 0.5f;
        public AnimationCurve flyCurve;

        protected Transform picker;
        protected Transform pickerReceivePoint;
        public string itemName;
        protected AssetHandle _assetHandle;
        private Renderer _cachedPickupRenderer;

        public bool isTaken = false;   // 被谁使用了
        private Action _onCancel;
        private void Awake()
        {
            _assetHandle = GetComponent<AssetHandle>();
        }

        public void StartAttract(Transform picker, Transform receivePoint, Action onCancel = null)
        {
            if (!canPickup) return;

            if (isTaken) return;
            isTaken = true;

            this.picker = picker;
            this.pickerReceivePoint = receivePoint;
            this._onCancel = onCancel;

            StartCoroutine(FlyToPicker());
        }

        private IEnumerator FlyToPicker()
        {
            Vector2 start = transform.position;
            Vector2 control = start + Vector2.up * flyHeight;
            float timer = 0f;
            Renderer pickupRenderer = GetPickupRenderer();
            int originalSortingOrder = pickupRenderer != null ? pickupRenderer.sortingOrder : 0;

            while (timer < flyDuration)
            {
                if (picker == null || pickerReceivePoint == null || !gameObject.activeInHierarchy)
                {
                    RestoreSortingOrder(pickupRenderer, originalSortingOrder);
                    isTaken = false;
                    _onCancel?.Invoke();
                    yield break;
                }

                float t = flyCurve.Evaluate(timer / flyDuration);
                Vector2 pos = (1 - t) * (1 - t) * start +
                              2 * (1 - t) * t * control +
                              t * t * (Vector2)pickerReceivePoint.position;

                ApplyFlyingSortingOrder(pickupRenderer, picker, pickerReceivePoint.position.y, originalSortingOrder);
                transform.position = pos;

                timer += Time.deltaTime;
                yield return null;
            }
            // 再检查一次，保证池子没提前回收
            if (picker == null || pickerReceivePoint == null || !gameObject.activeInHierarchy)
            {
                RestoreSortingOrder(pickupRenderer, originalSortingOrder);
                isTaken = false;
                _onCancel?.Invoke();
                yield break;
            }
            ApplyFlyingSortingOrder(pickupRenderer, picker, pickerReceivePoint.position.y, originalSortingOrder);
            transform.position = pickerReceivePoint.position;

            // 让具体物品去执行拾取逻辑
            GetComponent<IPickable>().OnPicked(picker.gameObject);
            isTaken = false;
            if (ScenePickupController.Instance.materials.Contains(this))
            {
                ScenePickupController.Instance.materials.Remove(this);
            }
           if (ScenePickupController.Instance.products.Contains(this))
            {
                ScenePickupController.Instance.products.Remove(this);
            }
            Destroy(gameObject);

        }

        protected Renderer GetPickupRenderer()
        {
            if (_cachedPickupRenderer != null)
            {
                return _cachedPickupRenderer;
            }

            if (this is Production production && production.spriteRenderer != null)
            {
                _cachedPickupRenderer = production.spriteRenderer;
                return _cachedPickupRenderer;
            }

            if (this is DropController drop && drop.spriteRenderer != null)
            {
                _cachedPickupRenderer = drop.spriteRenderer;
                return _cachedPickupRenderer;
            }

            _cachedPickupRenderer = GetComponentInChildren<Renderer>();
            return _cachedPickupRenderer;
        }

        protected void ApplyFlyingSortingOrder(Renderer pickupRenderer, Transform target, float fallbackY, int minimumOrder)
        {
            if (pickupRenderer == null)
            {
                return;
            }

            int targetOrder = ResolveSortingOrder(target, fallbackY);
            pickupRenderer.sortingOrder = Mathf.Max(minimumOrder, targetOrder + FlyingSortingOffset);
        }

        protected int ResolveSortingOrder(Transform target, float fallbackY)
        {
            if (target != null)
            {
                if (target.TryGetComponent(out PlayerController player))
                {
                    return player.CurrentSortingOrder;
                }

                if (target.TryGetComponent(out CollectorController collector) && collector.meshRenderer != null)
                {
                    return collector.meshRenderer.sortingOrder;
                }

                if (target.TryGetComponent(out FreightClerkController freightClerk) && freightClerk.renderer != null)
                {
                    return freightClerk.renderer.sortingOrder;
                }

                if (target.TryGetComponent(out CustomerController customer))
                {
                    if (customer._meshRenderer == null && customer.skeletonAnimation != null)
                    {
                        customer._meshRenderer = customer.skeletonAnimation.GetComponent<MeshRenderer>();
                    }

                    if (customer._meshRenderer != null)
                    {
                        return customer._meshRenderer.sortingOrder;
                    }
                }

                if (target.TryGetComponent(out Renderer directRenderer))
                {
                    return directRenderer.sortingOrder;
                }

                Renderer childRenderer = target.GetComponentInChildren<Renderer>();
                if (childRenderer != null)
                {
                    return childRenderer.sortingOrder;
                }

                Renderer parentRenderer = target.GetComponentInParent<Renderer>();
                if (parentRenderer != null)
                {
                    return parentRenderer.sortingOrder;
                }
            }

            return WorldYToSortingOrder(fallbackY);
        }

        protected int WorldYToSortingOrder(float worldY)
        {
            return 30000 - Mathf.RoundToInt(worldY * 100f);
        }

        protected void RestoreSortingOrder(Renderer pickupRenderer, int originalSortingOrder)
        {
            if (pickupRenderer == null)
            {
                return;
            }

            pickupRenderer.sortingOrder = originalSortingOrder;
        }
    }

}
