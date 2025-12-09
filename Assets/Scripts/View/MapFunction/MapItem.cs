
 using System;
 using Module;
 using Module.Data;
 using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace View.MapFunction
{
    public class MapItem : MonoBehaviour
    {
        public AssetHandle assetHandle;
        public TextMeshProUGUI tiptxt;
        public Image mapImg;
        public GameObject maskImg;
        public Image completedImg;
        public TextMeshProUGUI pricetxt;
        public Transform content_1;
        public Transform content_2;
        public UIButton unlockBtn;
        public TextMeshProUGUI btntxt;
        public MapData  mapData;

        public void Start()
        {
            unlockBtn.onClick.RemoveAllListeners();
            unlockBtn.onClick.AddListener((() =>
            {
                if(ModuleMgr.Instance.GetModule<PlayerDataModule>().data.accountLevel < mapData.unlockLevel)
                {
                    UIController.Instance.Show<TipView>("等级未达到要求！");
                  
                }
                else
                {
                    if (ModuleMgr.Instance.GetModule<PlayerDataModule>().data.silverCoin >= mapData.unlockCost)
                    {
                        ModuleMgr.Instance.GetModule<PlayerDataModule>().data.silverCoin -= mapData.unlockCost;
                        ModuleMgr.Instance.GetModule<PlayerDataModule>().data.unlockMapList.Add(mapData.id);
                        maskImg.SetActive(false);
                        completedImg.gameObject.SetActive(true);
                        UIController.Instance.Show<TipView>( mapData.name + "解锁成攻！");
                        
                    }
                    else
                    {
                        UIController.Instance.Show<TipView>("银币数量不足！");
                    }
                }
            }));
        }


        public void Init(MapData data)
        {
            if (assetHandle == null)
            {
                assetHandle = GetComponent<AssetHandle>();
            }
            mapData = data;
            tiptxt.text = mapData.name;
            if (ModuleMgr.Instance.GetModule<PlayerDataModule>().data.unlockMapList.Contains(mapData.id))
            {
                completedImg.gameObject.SetActive(true);
                maskImg.SetActive(false);
                unlockBtn.gameObject.SetActive(false);
            }
            else
            {
                unlockBtn.gameObject.SetActive(true);
                completedImg.gameObject.SetActive(false);
                maskImg.SetActive(true);
                if (ModuleMgr.Instance.GetModule<PlayerDataModule>().data.accountLevel < mapData.unlockLevel)
                {
                    btntxt.text = "要求等级:" + mapData.unlockLevel;
                }
                else
                {
                    btntxt.text = "银币:" + mapData.unlockCost;
                }
            }
            pricetxt.text = "价格: x"+ mapData.price;
            if (mapData.monsterTypeList.Count > 0)
            {
                // content_1.gameObject.SetActive(true);
                // for (int i = 0; i < mapData.monsterTypeList.Count; i++)
                // {
                //     GameObject obj = GameObject.Instantiate(assetHandle.Get<GameObject>("mapinfoitem"), content_1.transform,false);
                //     obj.GetComponent<MapInfoItem>().Init();
                // }
            }
            else
            {
                content_1.gameObject.SetActive(false);
            }
            content_2.gameObject.SetActive(false);
         
            
           
        }
    }
}
