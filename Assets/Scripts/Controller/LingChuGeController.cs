using System.Collections.Generic;
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
        
        public int id;
        
        public void Init()
        {
            warehouseCategory = ModuleMgr.Instance.GetModule<PlayerDataModule>().data.warehouselist.Find(x => x.id == id);
            List<Collector> list = warehouseCategory.workingCollectorList;
            foreach (Collector c in list)
            {
                GameObject obj = GameObject.Instantiate(_assetHandle.Get<GameObject>("Collector"));
                obj.transform.position = collectorTransform.position;
                obj.GetComponent<CollectorController>().Init(c , this);
            }
        }
        
        public Dictionary< DropItemType , int> storage;

        public void Store( CollectorController controller , CollectorInventory inv)
        {
            foreach (var kv in inv.dic)
            {
                if (!storage.TryAdd(kv.Key, kv.Value))
                    storage[kv.Key] += kv.Value;
            }
            inv.Clear();
            int remain = capacity;
        }

      
    }
}
