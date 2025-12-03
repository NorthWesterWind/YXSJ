using Module;
using Module.Data;
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
       public StorageBagData storageBagData;
       void Start()
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener((() =>
            {
                if (weaponData != null)
                {
                    UIController.Instance.Show<ItemInfoPop>(weaponData);
                }
                else
                {
                    UIController.Instance.Show<ItemInfoPop>(storageBagData);
                }
            }));
        }

        public void Init(params object[] args)
        {
            PlayerData playerdata = ModuleMgr.Instance.GetModule<PlayerDataModule>().data;
            if (args[0] is WeaponData)
            {
                weaponData = args[0] as WeaponData;
                storageBagData = null;
                selectImg.gameObject.SetActive(playerdata.currentWeapon == weaponData.id);
                maskImg.gameObject.SetActive(!(playerdata.ownWeaponList.Contains(weaponData.id)));
                switch ( weaponData.lockType)
                {
                    case UnlockType.accountLevel:
                        fillRect.SetActive(true);
                        fill.fillAmount = playerdata.accountLevel * 1f / weaponData.value;
                        break;
                    case UnlockType.CardLevel:
                        fillRect.SetActive(true);
                        fill.fillAmount = playerdata.cardLevelMax * 1f / weaponData.value;
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
            }
            else
            {
                storageBagData =  args[0] as StorageBagData;
                weaponData = null;
                selectImg.gameObject.SetActive(playerdata.currentBag == storageBagData.id);
                maskImg.gameObject.SetActive(!(playerdata.ownWeaponList.Contains(storageBagData.id)));
                switch ( storageBagData.lockType)
                {
                    case UnlockType.accountLevel:
                        fillRect.SetActive(true);
                        fill.fillAmount = playerdata.accountLevel * 1f / storageBagData.value;
                        break;
                    case UnlockType.CardLevel:
                        fillRect.SetActive(true);
                        fill.fillAmount = playerdata.cardLevelMax * 1f / storageBagData.value;
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
            }
        }

      
        void Update()
        {
        
        }
    }
}
