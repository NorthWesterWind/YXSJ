using Controller;
using Module;
using Module.Data;
using TMPro;
using UnityEngine;
using Utils;

namespace View.EmployeeFunction
{
    public class LingChuGeInfoItem : MonoBehaviour
    {
       public TextMeshProUGUI tiptxt;
       public TextMeshProUGUI peopletxt;
       public Transform content;
       WarehouseCategory _data;
       AssetHandle _assetHandle;
       
       public void Init(WarehouseCategory data)
       {
           _data = data;
           if (_assetHandle == null)
           {
               _assetHandle = GetComponent<AssetHandle>();
           }
           if (data.warehouseCategoryType ==  WarehouseCategoryType.LingChuGe_1)
           {
               tiptxt.text = "一号灵储阁";
           }
           else
           {
               tiptxt.text = "二号灵储阁";
           }
           peopletxt.text =  ( _data.workingCollectorList.Count) +"/" +  (_data.unworkingCollectorList.Count + _data.workingCollectorList.Count);
           MapData mapData = DataController.Instance.mapDataDic[PlayerDataModule.Instance.data.currentMapID];
           Extensions.ClearChildren(content.transform);
           for (int i = 0; i < mapData.monsterFamilyList.Count; i++)
           {
               GameObject obj = Instantiate(_assetHandle.Get<GameObject>("EmployeeInfoItem") , content.transform , false);
               //obj.GetComponent<EmployeeInfoItem>().Init((MonsterFamily)mapData.monsterFamilyList[i] , data);
           }
       }
    }
}
