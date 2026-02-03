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
       public TextMeshProUGUI peopletxt;   // 这里用来显示当前存放总量 / 容量
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

           // 统计当前存放的总数量与容量（ownItemList 是自定义字典，内部用 list 存储）
           int totalStored = 0;
           foreach (var kv in _data.ownItemList.list)
           {
               totalStored += kv.value;
           }
           peopletxt.text = $"已存放: {totalStored}/{_data.capacity}";

           // 如果你希望在下方列表中展示每种物资的详细数量，
           // 可以在这里根据 ownItemList 或当前地图里的 monsterFamilyList 实例化具体的条目 prefab。
           // 这里只保留清空逻辑，避免旧节点残留。
           Extensions.ClearChildren(content.transform);
       }
    }
}
