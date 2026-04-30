using Controller;
using Module;
using Module.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace View.PopUp
{
    public enum BtnState
    {
        None,
        YiZhuangBei,
        ZhuangBei,
        DaiJieSuo,
        JieSuo,
    }

    public class ItemInfoPop : BaseView
    {
        public TextMeshProUGUI tiptxt;
        public UIButton closeBtn;
        public TextMeshProUGUI locktxt;
        public TextMeshProUGUI btntxt;
        public UIButton btn;
        public UIButton purchaseBtn;
        public TextMeshProUGUI freeTxt;
        public Image icon;
        public GameObject fillContent;
        public Image fill;
        public TextMeshProUGUI filltxt;

        private BtnState state = BtnState.None;
        private WeaponData weaponData;
        public StotageBagData stotageBagData;
        public ClothData clothData;

        public override void UpdateViewWithArgs(params object[] args)
        {
            if (args == null || args.Length == 0 || args[0] == null)
            {
                return;
            }

            if (args[0] is WeaponData)
            {
                weaponData = args[0] as WeaponData;
                stotageBagData = null;
                clothData = null;
            }
            else if (args[0] is StotageBagData)
            {
                stotageBagData = args[0] as StotageBagData;
                weaponData = null;
                clothData = null;
            }
            else if (args[0] is ClothData)
            {
                clothData = args[0] as ClothData;
                weaponData = null;
                stotageBagData = null;
            }

            RefreshViewState();
        }

        private void RefreshViewState()
        {
            PlayerData playerData = PlayerDataModule.Instance.data;

            if (weaponData != null)
            {
                tiptxt.text = weaponData.name;
                locktxt.text = weaponData.unlockStr+"。";
                icon.sprite = _assetHandle.Get<Sprite>(weaponData.name);
                purchaseBtn.gameObject.SetActive(false);
                btn.gameObject.SetActive(true);

                if (playerData.ownWeaponList.Contains(weaponData.id))
                {
                    state = playerData.currentWeapon == weaponData.id ? BtnState.YiZhuangBei : BtnState.ZhuangBei;
                    btntxt.text = state == BtnState.YiZhuangBei ? "已装备" : "装备";
                    locktxt.text = "攻击力：" + (playerData.atk + weaponData.atkValue)+"。";
                    fillContent.SetActive(false);
                    return;
                }

                fillContent.SetActive(true);
                RefreshUnlockState(weaponData.lockType, weaponData.value);
                return;
            }

            if (stotageBagData != null)
            {
                tiptxt.text = stotageBagData.name;
                locktxt.text = stotageBagData.unlockStr+"。";
                icon.sprite = _assetHandle.Get<Sprite>(stotageBagData.name);
                purchaseBtn.gameObject.SetActive(false);
                btn.gameObject.SetActive(true);

                if (playerData.ownBagList.Contains(stotageBagData.id))
                {
                    state = playerData.currentBag == stotageBagData.id ? BtnState.YiZhuangBei : BtnState.ZhuangBei;
                    btntxt.text = state == BtnState.YiZhuangBei ? "已装备" : "装备";
                    locktxt.text = "储物容量：" + (playerData.bagCapacity + stotageBagData.capacity)+"。";
                    fillContent.SetActive(false);
                    return;
                }

                fillContent.SetActive(true);
                RefreshUnlockState(stotageBagData.lockType, stotageBagData.value);
                return;
            }

            if (clothData != null)
            {
                tiptxt.text = clothData.name;
                locktxt.text = clothData.unlockStr+"。";
                icon.sprite = _assetHandle.Get<Sprite>(clothData.name);
                fillContent.SetActive(false);

                if (playerData.ownClothingList.Contains(clothData.id))
                {
                    state = playerData.currentClothing == clothData.id ? BtnState.YiZhuangBei : BtnState.ZhuangBei;
                    btntxt.text = state == BtnState.YiZhuangBei ? "换下" : "穿戴";
                    locktxt.text = "健康值增加：" + clothData.hpValue+"。";
                    purchaseBtn.gameObject.SetActive(false);
                    btn.gameObject.SetActive(true);
                    return;
                }

                purchaseBtn.gameObject.SetActive(true);
                freeTxt.text = clothData.value.ToString();
                btn.gameObject.SetActive(false);
                state = BtnState.DaiJieSuo;
            }
        }

        private void RefreshUnlockState(UnlockType unlockType, int targetValue)
        {
            PlayerData playerData = PlayerDataModule.Instance.data;
            switch (unlockType)
            {
                case UnlockType.accountLevel:
                    SetUnlockProgress(playerData.accountLevel, targetValue);
                    break;
                case UnlockType.CardLevel:
                    SetUnlockProgress(GetCurrentCardLevel(playerData), targetValue);
                    break;
                case UnlockType.talentLevel:
                    SetUnlockProgress(playerData.talentLevel, targetValue);
                    break;
                case UnlockType.UseLingJing:
                    SetUnlockProgress(playerData.useLingJingTotalValue, targetValue);
                    break;
                case UnlockType.XianYunZhuanPan:
                    SetUnlockProgress(playerData.useZhuanPanTotalValue, targetValue);
                    break;
                case UnlockType.Purchase:
                    fill.fillAmount = 1f;
                    filltxt.text = string.Empty;
                    state = BtnState.JieSuo;
                    btntxt.text = "解锁";
                    break;
                default:
                    fill.fillAmount = 0f;
                    filltxt.text = string.Empty;
                    state = BtnState.DaiJieSuo;
                    btntxt.text = "待解锁";
                    break;
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

        private void SetUnlockProgress(int currentValue, int targetValue)
        {
            int safeTarget = Mathf.Max(1, targetValue);
            fill.fillAmount = Mathf.Clamp01(currentValue * 1f / safeTarget);
            filltxt.text = "(" + currentValue + "/" + safeTarget + ")";

            if (currentValue >= targetValue)
            {
                state = BtnState.JieSuo;
                btntxt.text = "解锁";
            }
            else
            {
                state = BtnState.DaiJieSuo;
                btntxt.text = "待解锁";
            }
        }

        protected override void AddEventListener()
        {
            base.AddEventListener();

            closeBtn.onClick.RemoveAllListeners();
            closeBtn.onClick.AddListener(() =>
            {
                Hide();
            });

            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() =>
            {
                if (state == BtnState.ZhuangBei)
                {
                    if (clothData != null)
                    {
                        EquipClothing();
                        return;
                    }

                    EquipCurrentItem();
                    RefreshViewState();
                    EventCenter.Instance.TriggerEvent(EventMessages.UpdatePlayerValueInfo);
                    EventCenter.Instance.TriggerEvent(EventMessages.UpdatePlayerEquimentInfo);
                    return;
                }

                if (state == BtnState.YiZhuangBei)
                {
                    if (clothData != null)
                    {
                        UnequipClothing();
                        return;
                    }
                }

                if (state == BtnState.JieSuo)
                {
                    UnlockCurrentItem();
                    RefreshViewState();
                    EventCenter.Instance.TriggerEvent(EventMessages.UpdatePlayerEquimentInfo);
                }
            });

            purchaseBtn.onClick.RemoveAllListeners();
            purchaseBtn.onClick.AddListener(() =>
            {
                if (PlayerDataModule.Instance.data.lingJing >= clothData.value)
                {
                    UIController.Instance.Show<TipView>("购买成功！");
                    PlayerDataModule.Instance.data.lingJing -= clothData.value;
                    PlayerDataModule.Instance.data.ownClothingList.Add(clothData.id);
                    RefreshViewState();
                    EventCenter.Instance.TriggerEvent(EventMessages.UpdatePlayerEquimentInfo);
                    PlayerDataModule.Instance.data.useLingJingTotalValue += clothData.value;
                    EventCenter.Instance.TriggerEvent(EventMessages.UpdatePlayerMoneyInfo);
                }
                else
                {
                    UIController.Instance.Show<TipView>("灵晶不足！");
                }
            });
        }

        private void EquipCurrentItem()
        {
            PlayerData playerData = PlayerDataModule.Instance.data;
            if (weaponData != null)
            {
                if (DataController.Instance.weaponDataDic.TryGetValue(playerData.currentWeapon, out var currentWeaponData))
                {
                    playerData.addAtk -= currentWeaponData.atkValue;
                }

                playerData.currentWeapon = weaponData.id;
                playerData.addAtk += weaponData.atkValue;
                return;
            }

            if (stotageBagData == null)
            {
                return;
            }

            playerData.currentBag = stotageBagData.id;
            playerData.equippedBagCapacity = PlayerDataModule.Instance.GetBagCapacityById(stotageBagData.id);
        }

        private void EquipClothing()
        {
            PlayerData playerData = PlayerDataModule.Instance.data;
            AdjustClothingHp(playerData.currentClothing, -1);
            playerData.currentClothing = clothData.id;
            AdjustClothingHp(playerData.currentClothing, 1);

            EventCenter.Instance.TriggerEvent(EventMessages.UpdatePlayerClothingInfo);
            RefreshViewState();
            EventCenter.Instance.TriggerEvent(EventMessages.UpdatePlayerValueInfo);
            EventCenter.Instance.TriggerEvent(EventMessages.UpdatePlayerEquimentInfo);
        }

        private void UnequipClothing()
        {
            PlayerData playerData = PlayerDataModule.Instance.data;
            AdjustClothingHp(playerData.currentClothing, -1);
            playerData.currentClothing = 3;

            EventCenter.Instance.TriggerEvent(EventMessages.UpdatePlayerClothingInfo);
            RefreshViewState();
            EventCenter.Instance.TriggerEvent(EventMessages.UpdatePlayerValueInfo);
            EventCenter.Instance.TriggerEvent(EventMessages.UpdatePlayerEquimentInfo);
        }

        private void AdjustClothingHp(int clothingId, int direction)
        {
            if (clothData == null || clothingId == 3)
            {
                return;
            }

            if (DataController.Instance.clothDataDic.TryGetValue(clothingId, out var currentClothData))
            {
                PlayerDataModule.Instance.data.addHp += currentClothData.hpValue * direction;
            }
        }

        private void UnlockCurrentItem()
        {
            PlayerData playerData = PlayerDataModule.Instance.data;
            if (weaponData != null)
            {
                if (!playerData.ownWeaponList.Contains(weaponData.id))
                {
                    playerData.ownWeaponList.Add(weaponData.id);
                }

                return;
            }

            if (stotageBagData == null)
            {
                return;
            }

            if (!playerData.ownBagList.Contains(stotageBagData.id))
            {
                playerData.ownBagList.Add(stotageBagData.id);
            }
        }
    }
}
