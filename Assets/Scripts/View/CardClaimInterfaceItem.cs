using Module;
using Module.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using View.CardView;

namespace View
{
    public class CardClaimInterfaceItem : MonoBehaviour
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

        void Start()
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener((() => { UIController.Instance.Show<CardDetailPop>(data); }));
        }

        public void Init(CardLevelData _data)
        {
            data = _data;
            PlayerData playerData = PlayerDataModule.Instance.data;

            CardUpProgress cardUpProgress = null;
            foreach (var value in playerData.cardUpProgressesList)
            {
                if (value.id == data.id)
                {
                    cardUpProgress = value;
                    break;
                }
            }
            nametxt.text = data.name;
            leveltxt.text = cardUpProgress.level.ToString();
           
            if (playerData.accountLevel >= _data.unlockLevel)
            {
                leveltxt.gameObject.SetActive(true);
                topLeftLockImage.gameObject.SetActive(false);
            }
            else
            {
                leveltxt.gameObject.SetActive(false);
                topLeftLockImage.gameObject.SetActive(true);
            }
          
            if (cardUpProgress.level == 10)
            {
                fillContent.SetActive(false);
                progresstxt.text = "已满级";
            }
            else
            {
                fillContent.SetActive(true);
                fillImg.fillAmount = cardUpProgress.currentNum * 1f /
                                     WorldData.cardUpLevelArr[cardUpProgress.level -1];
                progresstxt.text = cardUpProgress.currentNum +"/" +  WorldData.cardUpLevelArr[cardUpProgress.level - 1];
            }
        }
    }
}