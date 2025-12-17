using System;
using Module.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace View.EmployeeFunction
{
    public class EmployeeInfoItem : MonoBehaviour
    {
       public Image image;
       public UIButton removeBtn;
       public UIButton addBtn;
       public TextMeshProUGUI progresstxt;
       public TextMeshProUGUI nametxt;
       public Image mask1;
       public Image mask2;
       private AssetHandle _assetHandle;
       public MonsterFamily type;
       public int workingnum;
       public WarehouseCategory category;
       public void Init( MonsterFamily familyType , WarehouseCategory warehouseCategory)
       {
           if (_assetHandle == null)
           {
               _assetHandle = GetComponent<AssetHandle>();
           }
           type = familyType;
           //image.sprite = _assetHandle.Get<Sprite>(resName);
           foreach (var value in warehouseCategory.workingCollectorList)
           {
               value.monsterType = type;
               workingnum += 1;
           }
           UpdateInfo();
           category = warehouseCategory;
       }

       private void Start()
       {
           removeBtn.onClick.RemoveAllListeners();
           removeBtn.onClick.AddListener((() =>
           {
               if (workingnum<1)
               {
                   return;
               }
               else
               {
                   foreach (var data in category. workingCollectorList)
                   {
                       if (data.monsterType == type)
                       {
                           data.monsterType = MonsterFamily.None;
                           var temp = data;
                           category.unworkingCollectorList.Add(temp);
                           break;
                       }
                   }
                   UpdateInfo();
                   EventCenter.Instance.TriggerEvent(EventMessages. LingChuGeStopDelivery);
               }
           }));
           
           addBtn.onClick.RemoveAllListeners();
           addBtn.onClick.AddListener((() =>
           {
               if (workingnum > 0)
               {
                   UIController.Instance.Show<TipView>("当前已派遣玄采徒进行采集！");
                   return;
               }
               if (category.unworkingCollectorList.Count < 1)
               {
                   UIController.Instance.Show<TipView>("当前无可以派遣的玄采徒！");
                   return;
               }   
               else
               {
                   var temp = category.unworkingCollectorList[0];
                   temp.monsterType = type;
                   category.unworkingCollectorList.RemoveAt(0);
                   category.workingCollectorList.Add(temp);
                   UpdateInfo();
                  EventCenter.Instance.TriggerEvent(EventMessages.LingChuGeBeginWorking);
               }
           }));
       }

       public void UpdateInfo()
       {
           progresstxt.text = "0/1";
           workingnum = 0;
           foreach (var data in category.workingCollectorList)
           {
               if (data.monsterType == type)
               {
                   progresstxt.text = "1/1";
                   workingnum = 1;
                   break;
               }
           }
           if (workingnum == 0)
           {
               mask1.enabled = false;
           }
           else
           {
               mask1.enabled = true;
           }
       }
    }
}
