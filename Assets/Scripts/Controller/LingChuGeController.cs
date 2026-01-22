using System;
using System.Collections.Generic;
using Controller.Pickups;
using Controller.Player;
using Controller.Structure;
using Module;
using Module.Data;
using UnityEngine;
using Utils;
using View.LingChuGe;

namespace Controller
{
    public class LingChuGeController : StructureBase
    {
        public int capacity;
        public WarehouseCategory  warehouseCategory;
        public Transform receiveTransform;
        public Transform sendTransform;
        public Transform infoTransform;
        public LingChuGeInfo infoitem;
        public Transform collectorTransform;
        public WarehouseCategoryType categoryType; 
        public List<CollectorController>  collectorControllerList = new List<CollectorController>();
        public Dictionary< DropItemType , int> storage;
        public PlayerController characterController;
        
        private bool isDelivering = false;
        private List<DropController> deliveringDrops = new();
        
        public void Init(params object[] args)
        {
            
        }


        protected override void Start()
        {
            base.Start();
            if (characterController == null)
            {
                characterController = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
            }
            EventCenter.Instance.TriggerEvent(EventMessages.LingChuGeBeginWorking);
           
        }

        private void OnEnable()
        {
            EventCenter.Instance.AddListener(EventMessages.LingChuGeBeginWorking , HandleBeginWorking);
            EventCenter.Instance.AddListener(EventMessages.LingChuGeDelivery , HandleLingChuGeDelivery);
            EventCenter.Instance.AddListener(EventMessages.LingChuGeStopDelivery , HandleLingChuGeStopDelivery);
            EventCenter.Instance.AddListener(EventMessages.UpdateSturctureLockInfo, Init);
        }

        private void OnDisable()
        {
            EventCenter.Instance.RemoveListener(EventMessages.LingChuGeBeginWorking , HandleBeginWorking);
            EventCenter.Instance.RemoveListener(EventMessages.LingChuGeDelivery , HandleLingChuGeDelivery);
            EventCenter.Instance.RemoveListener(EventMessages.LingChuGeStopDelivery , HandleLingChuGeStopDelivery);
            EventCenter.Instance.RemoveListener(EventMessages.UpdateSturctureLockInfo, Init);
        }


        private void Update()
        {
            if (Vector2.Distance(characterController.gameObject.transform.position, transform.position) < 8)
            {
                infoitem.gameObject.SetActive(true);
                infoitem.transform.position =  infoTransform.position;
            }
            else
            {
                infoitem.gameObject.SetActive(false);
            }
        }

        public void HandleBeginWorking(params object[] args)
        {
            warehouseCategory = PlayerDataModule.Instance.data.warehouselist.Find(x => x.warehouseCategoryType == categoryType);
            if (infoitem == null)
            {
                infoitem = Instantiate(_assetHandle.Get<GameObject>("LingChuGeInfo") , GameObject.Find("Canvas").transform,false).GetComponent<LingChuGeInfo>();
            }
            infoitem.Init( warehouseCategory , this);
            List<Collector> list = warehouseCategory.workingCollectorList;
            foreach (Collector c in list)
            {
                GameObject obj = Instantiate(_assetHandle.Get<GameObject>("Collector"));
                obj.transform.position = collectorTransform.position;
                obj.GetComponent<CollectorController>().Init(c , this);
                collectorControllerList.Add(obj.GetComponent<CollectorController>());
                
            }
        }

        public void HandleLingChuGeDelivery(params object[] args)
        {
            if (isDelivering) return;

            DropItemType targetType = (DropItemType)args[0];
            int count = storage[targetType];
            count = Mathf.Min(count, characterController.RemainCapacity);

            if (count <= 0) return;

            isDelivering = true;

            for (int i = 0; i < count; i++)
            {
                GameObject obj = Instantiate(_assetHandle.Get<GameObject>("DropItem"));
                obj.transform.position = sendTransform.position;
 
                var drop = obj.GetComponent<DropController>();
                drop.Init(targetType);

                deliveringDrops.Add(drop);

                drop.FlyTo(
                    characterController.transform,
                    characterController.receiveTransform,
                    () =>
                    {
                        // 成功送达回调
                        deliveringDrops.Remove(drop);
                        storage[targetType]--;

                        if (deliveringDrops.Count == 0)
                            isDelivering = false;
                    }
                );
            }
        }

        
        public void HandleLingChuGeStopDelivery(params object[] args)
        {
            if (!isDelivering) return;

            isDelivering = false;

            for (int i = deliveringDrops.Count - 1; i >= 0; i--)
            {
                var drop = deliveringDrops[i];
                if (drop == null) continue;

                // 终止飞行
                drop.ForceStop();

                // 返还库存
                storage[drop.itemType]++;

                // 回收物体
                ObjectPoolManager.Instance.ReturnObject(drop.itemName, drop.gameObject);
            }

            deliveringDrops.Clear();
        }
        
        
        /// <summary>
        /// 取出货物
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="inv"></param>
        public void Store( CollectorController controller , CollectorInventory inv)
        {
            foreach (var kv in inv.dic)
            {
                GameObject obj = Instantiate(_assetHandle.Get<GameObject>("DropObj"));
                obj.transform.position = controller.receiveTransform.position;
                var cc =  obj.GetComponent<DropController>();
                cc.Init(controller.targetType);
                cc.FlyTo(receiveTransform);
                if (!storage.TryAdd(kv.Key, kv.Value))
                    storage[kv.Key] += kv.Value;
            }
            inv.Clear();
        }
    }
}
