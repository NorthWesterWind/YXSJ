using System.Collections.Generic;
using System.Linq;
using Controller;
using Module.Data;
using UnityEngine;
using Utils;

namespace View
{
    public class CardClaimInterface : BaseView
    {
       public UIButton closeBtn;
       public Transform content;
       private AssetHandle _assetHandle;
       public override void UpdateViewWithArgs(params object[] args)
       {
           base.UpdateViewWithArgs(args);
           Dictionary<int,int> dic = args[0] as Dictionary<int,int>;
           CurrencyType  type = (CurrencyType)args[1];
           int num = (int)args[2];
           if (_assetHandle == null)
           {
               _assetHandle = GetComponent<AssetHandle>();
           }
           GameObject obj1 = GameObject.Instantiate(_assetHandle.Get<GameObject>("CurrencyCardltem") , content.transform,false);
           obj1.GetComponent<CurrencyCardItem>().Init(type,num);
           foreach (var pair in dic)
           {
               GameObject obj = GameObject.Instantiate(_assetHandle.Get<GameObject>("CardClaimlnterfaceltem") , content.transform,false);
               obj.GetComponent<CardClaimInterfaceItem>().Init(DataController.Instance.cardLevelDataList.FirstOrDefault(c => c.id == pair.Key));
               
           }
       }

       protected override void AddEventListener()
       {
           base.AddEventListener();
           closeBtn.onClick.RemoveAllListeners();
           closeBtn.onClick.AddListener((() =>
           {
               Hide();
           }));
       }

      
    }
}
