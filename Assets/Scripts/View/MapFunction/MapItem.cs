
using System;
using JetBrains.Annotations;
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
        public TextMeshProUGUI masktxt;
        // public Image completedImg;
        public TextMeshProUGUI pricetxt;
        public Transform content_1;
        public Transform content_2;
        public UIButton unlockBtn;
        public TextMeshProUGUI btntxt;
        public MapData mapData;
        public GameObject fruit;
        public GameObject constuct;

        public void Start()
        {
            unlockBtn.onClick.RemoveAllListeners();
            unlockBtn.onClick.AddListener((() =>
            {
                if (PlayerDataModule.Instance.data.accountLevel < mapData.unlockLevel)
                {
                    UIController.Instance.Show<TipView>("等级未达到要求！");

                }
                else
                {
                    if (PlayerDataModule.Instance.data.tongbi >= mapData.unlockCost)
                    {
                        PlayerDataModule.Instance.data.tongbi -= mapData.unlockCost;
                        PlayerDataModule.Instance.data.unlockMapList.Add(mapData.id);
                        maskImg.SetActive(false);
                        // completedImg.gameObject.SetActive(true);
                        UIController.Instance.Show<TipView>(mapData.name + "解锁成攻！");

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
            mapImg.sprite = assetHandle.Get<Sprite>(mapData.name);
            maskImg.GetComponent<Image>().sprite = assetHandle.Get<Sprite>(mapData.name + "灰");


            if (PlayerDataModule.Instance.data.unlockMapList.Contains(mapData.id))
            {
                //  completedImg.gameObject.SetActive(true);
                maskImg.SetActive(false);
                masktxt.gameObject.SetActive(false);
                unlockBtn.gameObject.SetActive(false);
            }
            else
            {
                unlockBtn.gameObject.SetActive(true);
                // completedImg.gameObject.SetActive(false);
                maskImg.SetActive(true);
                masktxt.gameObject.SetActive(true);
                masktxt.text = $"{mapData.unlockLevel}级后解锁";
                if (PlayerDataModule.Instance.data.accountLevel < mapData.unlockLevel)
                {
                    btntxt.text = "要求等级:" + mapData.unlockLevel;
                }
                else
                {
                    btntxt.text = "银币:" + mapData.unlockCost;
                }
            }
            pricetxt.text = "x" + mapData.price;
            Extensions.ClearChildren(content_1);
            Extensions.ClearChildren(content_2);
            if (mapData.monsterFamilyList.Count > 0)
            {
               fruit.gameObject.SetActive(true);
                for (int i = 0; i < mapData.monsterFamilyList.Count; i++)
                {
                    GameObject obj = GameObject.Instantiate(assetHandle.Get<GameObject>("mapinfoitem"), content_1.transform, false);
                    obj.GetComponent<MapInfoItem>().Init(Extensions.GetMonsterPictureNameByType((MonsterFamily)mapData.monsterFamilyList[i]), (MonsterFamily)mapData.monsterFamilyList[i], obj.transform);

                }
            }
            else
            {
                fruit.gameObject.SetActive(false);
            }
            if (mapData.buildTypeList.Count > 0)
            {
                constuct.gameObject.SetActive(true);
                for (int i = 0; i < mapData.buildTypeList.Count; i++)
                {
                    GameObject obj = GameObject.Instantiate(assetHandle.Get<GameObject>("mapinfoitem"), content_2.transform, false);
                    obj.GetComponent<MapInfoItem>().Init(Extensions.GetStructureResNameByType((BuildingType)mapData.buildTypeList[i]), (BuildingType)mapData.buildTypeList[i], obj.transform);
                }
            }
            else
            {
                constuct.gameObject.SetActive(false);
            }

        }
    }
}
