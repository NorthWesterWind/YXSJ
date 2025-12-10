using System.Collections.Generic;
using Controller.Structure;
using Module;
using UnityEngine;
using Utils;

namespace Controller
{
    public class YunDiGeController : StructureBase
    {
        public List<FreightClerkController> freightClerkList = new();
        public Transform bornTransform;

        protected override void Start()
        {
            for (int i = 0; i < ModuleMgr.Instance.GetModule<PlayerDataModule>().data.totalNum; i++)
            {
                GameObject obj = GameObject.Instantiate(_assetHandle.Get<GameObject>("FreightClerk"));
                obj.transform.position = bornTransform.position;
                var cc = obj.GetComponent<FreightClerkController>();
                cc.Init();
                freightClerkList.Add(cc);
            }
        }

        void Update()
        {
        }
    }
}