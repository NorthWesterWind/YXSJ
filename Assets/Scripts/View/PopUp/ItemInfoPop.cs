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
        public Image icon;
        public GameObject fillContent;
        public Image fill;
        public TextMeshProUGUI filltxt;
        public bool isWeapon = false;

        private BtnState state = BtnState.None;
        private WeaponData weaponData;
        public StotageBagData stotageBagData;

        public override void UpdateViewWithArgs(params object[] args)
        {
            if (args == null || args.Length == 0 || args[0] == null)
            {
                return;
            }

            weaponData = args[0] as WeaponData;
            stotageBagData = args[0] as StotageBagData;
            isWeapon = weaponData != null;

            RefreshViewState();
        }

        private void RefreshViewState()
        {
            PlayerData playerData = PlayerDataModule.Instance.data;
            if (isWeapon && weaponData != null)
            {
                tiptxt.text = weaponData.name;
                locktxt.text = weaponData.unlockStr;
                icon.sprite = _assetHandle.Get<Sprite>(weaponData.name);

                if (playerData.ownWeaponList.Contains(weaponData.id))
                {
                    state = playerData.currentWeapon == weaponData.id ? BtnState.YiZhuangBei : BtnState.ZhuangBei;
                    btntxt.text = state == BtnState.YiZhuangBei ? "已装备" : "装备";
                    locktxt.text = "攻击力：" + (playerData.atk + weaponData.atkValue);
                    fillContent.SetActive(false);
                    return;
                }

                fillContent.SetActive(true);
                RefreshUnlockState(weaponData.lockType, weaponData.value);
                return;
            }

            if (stotageBagData == null)
            {
                return;
            }

            tiptxt.text = stotageBagData.name;
            locktxt.text = stotageBagData.unlockStr;
            icon.sprite = _assetHandle.Get<Sprite>(stotageBagData.name);

            if (playerData.ownBagList.Contains(stotageBagData.id))
            {
                state = playerData.currentBag == stotageBagData.id ? BtnState.YiZhuangBei : BtnState.ZhuangBei;
                btntxt.text = state == BtnState.YiZhuangBei ? "已装备" : "装备";
                locktxt.text = "储物容量：" + (playerData.bagCapacity + stotageBagData.capacity);
                fillContent.SetActive(false);
                return;
            }

            fillContent.SetActive(true);
            RefreshUnlockState(stotageBagData.lockType, stotageBagData.value);
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
                    SetUnlockProgress(playerData.cardLevelMax, targetValue);
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
                    EquipCurrentItem();
                    RefreshViewState();
                    EventCenter.Instance.TriggerEvent(EventMessages.UpdatePlayerValueInfo);
                    EventCenter.Instance.TriggerEvent(EventMessages.UpdatePlayerEquimentInfo);
                    return;
                }

                if (state == BtnState.JieSuo)
                {
                    UnlockCurrentItem();
                    RefreshViewState();
                    EventCenter.Instance.TriggerEvent(EventMessages.UpdatePlayerEquimentInfo);
                }
            });
        }

        private void EquipCurrentItem()
        {
            PlayerData playerData = PlayerDataModule.Instance.data;
            if (isWeapon && weaponData != null)
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

            if (DataController.Instance.storageBagDataDic.TryGetValue(playerData.currentBag, out var currentBagData))
            {
                playerData.addBagCapacity -= currentBagData.capacity;
            }

            playerData.currentBag = stotageBagData.id;
            playerData.addBagCapacity += stotageBagData.capacity;
        }

        private void UnlockCurrentItem()
        {
            PlayerData playerData = PlayerDataModule.Instance.data;
            if (isWeapon && weaponData != null)
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
