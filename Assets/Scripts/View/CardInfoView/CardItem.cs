
using System;
using Module;
using Module.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace View.CardInfoView
{
    public class CardItem : MonoBehaviour
    {
        public Image icon;
        public TextMeshProUGUI leveltxt;
        public TextMeshProUGUI nametxt;
        public TextMeshProUGUI progresstxt;
        public Image fillImg;
        public GameObject fillContent;
        public UIButton btn;
        public CardLevelData data;
        public AssetHandle _assetHandle;
        public Image topLeftLockImage;
        public Image MaskImg;
        public TextMeshProUGUI masktxt;
    
        void Start()
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener((() =>
            {
              //  UIController.Instance.Show<CardDetailPop>(data);
            }));
        }
        void Update()
        {
            
        }

    

        public void Init(CardLevelData _data)
        {
            data = _data;
            PlayerData playerData = ModuleMgr.Instance.GetModule<PlayerDataModule>().data;
            bool own = false;
            CardUpProgress cardUpProgress = null ;
            foreach (var value in playerData.cardUpProgressesList)
            {
                if (value.id == data.id)
                {
                    own = true;
                    cardUpProgress = value;
                    break;
                }
            }

            nametxt.text = data.name;
            if (own)
            {
                leveltxt.text = cardUpProgress.level.ToString();
                leveltxt.gameObject.SetActive(true);
                MaskImg.gameObject.SetActive(false);
                topLeftLockImage.gameObject.SetActive(false);
                if (cardUpProgress.level == 10)
                {
                    fillContent.SetActive(false);
                    progresstxt.text = "已满级";
                }
                else
                {
                    fillContent.SetActive(true);
                    fillImg.fillAmount = cardUpProgress.currentNum * 1f /WorldData.cardUpLevelArr[cardUpProgress.level+1];
                }
                
            }
            else
            {
                if (data.unlockLevel > playerData.accountLevel)
                {
                    //未到达等级解锁条件
                    topLeftLockImage.gameObject.SetActive(false);
                    fillContent.SetActive(false);
                    progresstxt.gameObject.SetActive(false);
                    MaskImg.gameObject.SetActive(true);
                    masktxt.text = data.unlockLevel.ToString();
                }
                else
                {
                    //到达等级解锁条件，未拥有
                    topLeftLockImage.gameObject.SetActive(true);
                    fillContent.SetActive(false);
                    progresstxt.gameObject.SetActive(false);
                    MaskImg.gameObject.SetActive(false);
                }
                
            }
        }
    }
}
