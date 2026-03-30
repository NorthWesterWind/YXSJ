using System;
using System.Collections;
using System.Collections.Generic;
using Controller.Player;
using Controller.Structure;
using Module.Data;
using UnityEngine;
using Utils;

namespace Controller.Pickups
{
    public enum ItemState
    {
        None = 0,
        Flying,
        OnWorkbench,
        OnShelf,
        HeldByAssistant,
        HeldByCustomer
    }

    public class Production : BasePickup, IPickable
    {
        public SpriteRenderer spriteRenderer;
        public float duration = 0.6f; // Fly duration
        public float arcHeight = 1f;  // Arc height
        public float tiltAngle = 25f; // Tilt while flying

        private Vector3 startPos;
        private Vector3 endPos;
        private Coroutine activeFlyCoroutine;

        public bool CanPlayerPick => state == ItemState.OnWorkbench;
        public bool CanAssistantPick => state == ItemState.OnWorkbench;
        public bool CanCustomerPick => state == ItemState.OnShelf;

        public ItemState state;
        public GoodsType goodsType;
        public StructureBase station;
        public AssetHandle assetHandle;
        public int value;

        public void SetState(ItemState newState)
        {
            state = newState;
        }

        public void SetStation(StructureBase _station)
        {
            station = _station;
        }

        public void Init(GoodsType type, int _value = 0)
        {
            goodsType = type;
            value = _value;
            canPickup = false;
            ScenePickupController.Instance.products.Add(this);
            itemName = "Production";
            spriteRenderer.sprite = assetHandle.Get<Sprite>(Extensions.GetGoodsResNameByType(type));
        }

        void OnDestroy()
        {
            Controller.FreightClerkController.UnmarkProductReservedByFreight(this);
            if (station is ProductionStation productionStation)
            {
                productionStation.UnregisterProduct(this);
            }

            if (station is CashierCounter cashierCounter)
            {
                cashierCounter.UnregisterCoin(this);
            }

            if (ScenePickupController.Instance.products.Contains(this))
            {
                ScenePickupController.Instance.products.Remove(this);
            }
        }

        public void FlyTo(Vector3 target, Action callback = null)
        {
            StartFly(FlyRoutine(target, null, callback));
        }

        public void FlyTo(Vector3 target, Transform sortingTarget, Action callback = null)
        {
            StartFly(FlyRoutine(target, sortingTarget, callback));
        }

        IEnumerator FlyRoutine(Vector3 target, Transform sortingTarget, Action callback = null)
        {
            SetState(ItemState.Flying);

            Vector3 start = transform.position;
            float t = 0f;
            float flyTime = 0.5f;
            int originalSortingOrder = spriteRenderer != null ? spriteRenderer.sortingOrder : 0;

            while (t < 1f)
            {
                t += Time.deltaTime / flyTime;
                Vector3 pos = Vector3.Lerp(start, target, t);

                // Arc height
                float h = Mathf.Sin(t * Mathf.PI) * 0.5f;
                pos.y += h;

                // Tilt while flying
                transform.rotation = Quaternion.Euler(0, 0, Mathf.Lerp(0, 25, t));

                if (sortingTarget != null)
                {
                    ApplyFlyingSortingOrder(spriteRenderer, sortingTarget, target.y, originalSortingOrder);
                }
                transform.position = pos;
                yield return null;
            }

            if (sortingTarget != null)
            {
                RestoreSortingOrder(spriteRenderer, originalSortingOrder);
            }
            transform.rotation = Quaternion.identity;
            callback?.Invoke();
        }

        public void FlyTo_1(Vector3 target, float time, Action callback = null)
        {
            StartFly(FlyRoutine_1(target, time, null, callback));
        }

        public void FlyTo_1(Vector3 target, float time, Transform sortingTarget, Action callback = null)
        {
            StartFly(FlyRoutine_1(target, time, sortingTarget, callback));
        }

        private void StartFly(IEnumerator routine)
        {
            if (activeFlyCoroutine != null)
            {
                StopCoroutine(activeFlyCoroutine);
            }

            activeFlyCoroutine = StartCoroutine(RunFlyRoutine(routine));
        }

        private IEnumerator RunFlyRoutine(IEnumerator routine)
        {
            yield return StartCoroutine(routine);
            activeFlyCoroutine = null;
        }

        IEnumerator FlyRoutine_1(Vector3 target, float time = 0.1f, Transform sortingTarget = null, Action callback = null)
        {
            SetState(ItemState.Flying);

            Vector3 start = transform.position;
            float t = 0f;
            float flyTime = time;
            int originalSortingOrder = spriteRenderer != null ? spriteRenderer.sortingOrder : 0;

            while (t < 1f)
            {
                t += Time.deltaTime / flyTime;
                Vector3 pos = Vector3.Lerp(start, target, t);

                // Arc height
                float h = Mathf.Sin(t * Mathf.PI) * 0.5f;
                pos.y += h;

                // Tilt while flying
                transform.rotation = Quaternion.Euler(0, 0, Mathf.Lerp(0, 25, t));

                if (sortingTarget != null)
                {
                    ApplyFlyingSortingOrder(spriteRenderer, sortingTarget, target.y, originalSortingOrder);
                }
                transform.position = pos;
                yield return null;
            }

            if (sortingTarget != null)
            {
                RestoreSortingOrder(spriteRenderer, originalSortingOrder);
            }
            transform.rotation = Quaternion.identity;
            callback?.Invoke();
        }

        public void OnPicked(GameObject picker)
        {
            picker.GetComponent<PlayerController>().AddGoods(goodsType, value);
        }
    }
}
