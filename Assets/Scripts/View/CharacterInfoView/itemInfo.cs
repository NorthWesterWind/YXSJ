using Module;
using Module.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using View.PopUp;

namespace View.CharacterInfoView
{
    public class ItemInfo : MonoBehaviour
    {
        public Image iconImg;
        public Image selectImg;
        public Image maskImg;
        public Image fill;
        public GameObject fillRect;
        public UIButton btn;
        public WeaponData weaponData;
        public StotageBagData storageBagData;
        public ClothData clothData;
        public TextMeshProUGUI infoTxt;
        public AssetHandle assetHandle;


        void Start()
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener((() =>
            {
                if (weaponData != null)
                {
                    UIController.Instance.Show<ItemInfoPop>(weaponData);
                }
                else if (storageBagData != null)
                {
                    UIController.Instance.Show<ItemInfoPop>(storageBagData);
                }
                else if (clothData != null)
                {
                    UIController.Instance.Show<ItemInfoPop>(clothData);
                }
            }));
        }

        void OnEnable()
        {
            EventCenter.Instance.AddListener(EventMessages.UpdatePlayerEquimentInfo, HandleUpdate);
        }
        void OnDisable()
        {
            EventCenter.Instance.RemoveListener(EventMessages.UpdatePlayerEquimentInfo, HandleUpdate);
        }

        public void Init(params object[] args)
        {
            PlayerData playerdata = PlayerDataModule.Instance.data;
            if (args[0] is WeaponData)
            {
                weaponData = args[0] as WeaponData;
                storageBagData = null;
                selectImg.gameObject.SetActive(playerdata.currentWeapon == weaponData.id);
                maskImg.gameObject.SetActive(!(playerdata.ownWeaponList.Contains(weaponData.id)));
                infoTxt.text = weaponData.name;
                switch (weaponData.lockType)
                {
                    case UnlockType.accountLevel:
                        fillRect.SetActive(true);
                        fill.fillAmount = playerdata.accountLevel * 1f / weaponData.value;
                        break;
                    case UnlockType.CardLevel:
                        fillRect.SetActive(true);
                        fill.fillAmount = GetCurrentCardLevel(playerdata) * 1f / weaponData.value;
                        break;
                    case UnlockType.talentLevel:
                        fillRect.SetActive(true);
                        fill.fillAmount = playerdata.talentLevel * 1f / weaponData.value;
                        break;
                    case UnlockType.UseLingJing:
                        fillRect.SetActive(true);
                        fill.fillAmount = playerdata.useLingJingTotalValue * 1f / weaponData.value;
                        break;
                    case UnlockType.XianYunZhuanPan:
                        fillRect.SetActive(true);
                        fill.fillAmount = playerdata.useZhuanPanTotalValue * 1f / weaponData.value;
                        break;
                    case UnlockType.Purchase:
                        fillRect.SetActive(false);
                        break;


                }
                iconImg.sprite = assetHandle.Get<Sprite>(weaponData.name);
            }
            else if (args[0] is StotageBagData)
            {
                storageBagData = args[0] as StotageBagData;
                weaponData = null;
                selectImg.gameObject.SetActive(playerdata.currentBag == storageBagData.id);
                maskImg.gameObject.SetActive(!(playerdata.ownBagList.Contains(storageBagData.id)));
                infoTxt.text = storageBagData.name;


                switch (storageBagData.lockType)
                {
                    case UnlockType.accountLevel:
                        fillRect.SetActive(true);
                        fill.fillAmount = playerdata.accountLevel * 1f / storageBagData.value;
                        break;
                    case UnlockType.CardLevel:
                        fillRect.SetActive(true);
                        fill.fillAmount = GetCurrentCardLevel(playerdata) * 1f / storageBagData.value;
                        break;
                    case UnlockType.talentLevel:
                        fillRect.SetActive(true);
                        fill.fillAmount = playerdata.talentLevel * 1f / storageBagData.value;
                        break;
                    case UnlockType.UseLingJing:
                        fillRect.SetActive(true);
                        fill.fillAmount = playerdata.useLingJingTotalValue * 1f / storageBagData.value;
                        break;
                    case UnlockType.XianYunZhuanPan:
                        fillRect.SetActive(true);
                        fill.fillAmount = playerdata.useZhuanPanTotalValue * 1f / storageBagData.value;
                        break;
                    case UnlockType.Purchase:
                        fillRect.SetActive(false);
                        break;
                }
                iconImg.sprite = assetHandle.Get<Sprite>(storageBagData.name);
            }
            else if (args[0] is ClothData)
            {
                clothData = args[0] as ClothData;
                weaponData = null;
                storageBagData = null;
                fillRect.SetActive(false);
                maskImg.gameObject.SetActive(!PlayerDataModule.Instance.data.ownClothingList.Contains(clothData.id));
                selectImg.gameObject.SetActive(PlayerDataModule.Instance.data.currentClothing == clothData.id);
                iconImg.sprite = assetHandle.Get<Sprite>(clothData.name);
                infoTxt.text = clothData.name;
            }
        }

        public void HandleUpdate(params object[] args)
        {
            if (weaponData != null)
            {
                selectImg.gameObject.SetActive(PlayerDataModule.Instance.data.currentWeapon == weaponData.id);
                maskImg.gameObject.SetActive(!(PlayerDataModule.Instance.data.ownWeaponList.Contains(weaponData.id)));

            }
            else if (storageBagData != null)
            {
                selectImg.gameObject.SetActive(PlayerDataModule.Instance.data.currentBag == storageBagData.id);
                maskImg.gameObject.SetActive(!(PlayerDataModule.Instance.data.ownBagList.Contains(storageBagData.id)));
            }
            else if (clothData != null)
            {
                maskImg.gameObject.SetActive(!PlayerDataModule.Instance.data.ownClothingList.Contains(clothData.id));
                selectImg.gameObject.SetActive(PlayerDataModule.Instance.data.currentClothing == clothData.id);
            }
        }

        private int GetCurrentCardLevel(PlayerData playerData)
        {
            if (playerData == null || playerData.cardUpProgressesList == null || playerData.cardUpProgressesList.Count == 0)
            {
                return 0;
            }

            int currentCardLevel = 0;
            for (int i = 0; i < playerData.cardUpProgressesList.Count; i++)
            {
                CardUpProgress progress = playerData.cardUpProgressesList[i];
                if (progress == null)
                {
                    continue;
                }

                if (progress.level > currentCardLevel)
                {
                    currentCardLevel = progress.level;
                }
            }

            return currentCardLevel;
        }


        void Update()
        {

        }
    }
}
