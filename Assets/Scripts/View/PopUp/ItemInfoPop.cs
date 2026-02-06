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
        BtnState state = BtnState.None;
        public GameObject fillContent;
        public Image fill;
        public TextMeshProUGUI filltxt;
        public bool isWeapon = false;
        WeaponData weaponData;
        public StotageBagData stotageBagData;

        public override void UpdateViewWithArgs(params object[] args)
        {
            PlayerData playerdata = PlayerDataModule.Instance.data;
            if (args[0] is WeaponData)
            {
                isWeapon = true;
                weaponData = args[0] as WeaponData;
                tiptxt.text = weaponData.name;
                locktxt.text = weaponData.unlockStr;
                icon.sprite = _assetHandle.Get<Sprite>(weaponData.name);
                if (playerdata.ownWeaponList.Contains(weaponData.id))
                {
                    if (playerdata.currentWeapon == weaponData.id)
                    {
                        btntxt.text = "已装备";
                        state = BtnState.YiZhuangBei;
                    }
                    else
                    {
                        state = BtnState.ZhuangBei;
                        btntxt.text = "装备";
                    }
                    locktxt.text = "攻击力：" + (playerdata.atk + weaponData.atkValue);
                    fillContent.SetActive(false);
                }
                else
                {

                    fillContent.SetActive(true);
                    switch (weaponData.lockType)
                    {
                        case UnlockType.accountLevel:
                            fill.fillAmount = playerdata.accountLevel * 1f / weaponData.value;
                            filltxt.text = "(" + playerdata.accountLevel + "/" + weaponData.value + ")";
                            if (playerdata.accountLevel >= weaponData.value)
                            {
                                state = BtnState.JieSuo;
                                btntxt.text = "解锁";
                            }
                            else
                            {
                                state = BtnState.DaiJieSuo;
                                btntxt.text = "待解锁";
                            }
                            break;
                        case UnlockType.CardLevel:
                            fill.fillAmount = playerdata.cardLevelMax * 1f / weaponData.value;
                            filltxt.text = "(" + playerdata.cardLevelMax + "/" + weaponData.value + ")";
                            if (playerdata.cardLevelMax >= weaponData.value)
                            {
                                state = BtnState.JieSuo;
                                btntxt.text = "解锁";
                            }
                            else
                            {
                                state = BtnState.DaiJieSuo;
                                btntxt.text = "待解锁";
                            }
                            break;
                        case UnlockType.talentLevel:
                            fill.fillAmount = playerdata.talentLevel * 1f / weaponData.value;
                            filltxt.text = "(" + playerdata.talentLevel + "/" + weaponData.value + ")";
                            if (playerdata.talentLevel >= weaponData.value)
                            {
                                state = BtnState.JieSuo;
                                btntxt.text = "解锁";
                            }
                            else
                            {
                                state = BtnState.DaiJieSuo;
                                btntxt.text = "待解锁";
                            }
                            break;
                        case UnlockType.UseLingJing:
                            fill.fillAmount = playerdata.useLingJingTotalValue * 1f / weaponData.value;
                            filltxt.text = "(" + playerdata.useLingJingTotalValue + "/" + weaponData.value + ")";
                            if (playerdata.useLingJingTotalValue >= weaponData.value)
                            {
                                state = BtnState.JieSuo;
                                btntxt.text = "解锁";
                            }
                            else
                            {
                                state = BtnState.DaiJieSuo;
                                btntxt.text = "待解锁";
                            }
                            break;
                        case UnlockType.XianYunZhuanPan:
                            fill.fillAmount = playerdata.useZhuanPanTotalValue * 1f / weaponData.value;
                            filltxt.text = "(" + playerdata.useZhuanPanTotalValue + "/" + weaponData.value + ")";
                            if (playerdata.useZhuanPanTotalValue >= weaponData.value)
                            {
                                state = BtnState.JieSuo;
                                btntxt.text = "解锁";
                            }
                            else
                            {
                                state = BtnState.DaiJieSuo;
                                btntxt.text = "待解锁";
                            }
                            break;

                    }

                }
            }
            else if (args[0] is StotageBagData)
            {
                isWeapon = false;
                stotageBagData = args[0] as StotageBagData;
                tiptxt.text = stotageBagData.name;
                locktxt.text = stotageBagData.unlockStr;
                icon.sprite = _assetHandle.Get<Sprite>(stotageBagData.name);
                if (playerdata.ownBagList.Contains(stotageBagData.id))
                {
                    if (playerdata.currentBag == stotageBagData.id)
                    {
                        btntxt.text = "已装备";
                        state = BtnState.YiZhuangBei;
                    }
                    else
                    {
                        state = BtnState.ZhuangBei;
                        btntxt.text = "装备";
                    }

                    fillContent.SetActive(false);
                    locktxt.text = "储物容量：" + (playerdata.bagCapacity + stotageBagData.capacity);
                }
                else
                {

                    fillContent.SetActive(true);
                    switch (stotageBagData.lockType)
                    {
                        case UnlockType.accountLevel:
                            fill.fillAmount = playerdata.accountLevel * 1f / stotageBagData.value;
                            filltxt.text = "(" + playerdata.accountLevel + "/" + stotageBagData.value + ")";
                            if (playerdata.accountLevel >= stotageBagData.value)
                            {
                                state = BtnState.JieSuo;
                                btntxt.text = "解锁";
                            }
                            else
                            {
                                state = BtnState.DaiJieSuo;
                                btntxt.text = "待解锁";
                            }
                            break;
                        case UnlockType.CardLevel:
                            fill.fillAmount = playerdata.cardLevelMax * 1f / stotageBagData.value;
                            filltxt.text = "(" + playerdata.cardLevelMax + "/" + stotageBagData.value + ")";
                            if (playerdata.cardLevelMax >= stotageBagData.value)
                            {
                                state = BtnState.JieSuo;
                                btntxt.text = "解锁";
                            }
                            else
                            {
                                state = BtnState.DaiJieSuo;
                                btntxt.text = "待解锁";
                            }
                            break;
                        case UnlockType.talentLevel:
                            fill.fillAmount = playerdata.talentLevel * 1f / stotageBagData.value;
                            filltxt.text = "(" + playerdata.talentLevel + "/" + stotageBagData.value + ")";
                            if (playerdata.talentLevel >= stotageBagData.value)
                            {
                                state = BtnState.JieSuo;
                                btntxt.text = "解锁";
                            }
                            else
                            {
                                state = BtnState.DaiJieSuo;
                                btntxt.text = "待解锁";
                            }
                            break;
                        case UnlockType.UseLingJing:
                            fill.fillAmount = playerdata.useLingJingTotalValue * 1f / stotageBagData.value;
                            filltxt.text = "(" + playerdata.useLingJingTotalValue + "/" + stotageBagData.value + ")";
                            if (playerdata.useLingJingTotalValue >= stotageBagData.value)
                            {
                                state = BtnState.JieSuo;
                                btntxt.text = "解锁";
                            }
                            else
                            {
                                state = BtnState.DaiJieSuo;
                                btntxt.text = "待解锁";
                            }

                            break;
                        case UnlockType.XianYunZhuanPan:
                            fill.fillAmount = playerdata.useZhuanPanTotalValue * 1f / stotageBagData.value;
                            filltxt.text = "(" + playerdata.useZhuanPanTotalValue + "/" + stotageBagData.value + ")";
                            if (playerdata.useZhuanPanTotalValue >= stotageBagData.value)
                            {
                                state = BtnState.JieSuo;
                                btntxt.text = "解锁";
                            }
                            else
                            {
                                state = BtnState.DaiJieSuo;
                                btntxt.text = "待解锁";
                            }
                            break;

                    }


                }
            }
        }



        protected override void AddEventListener()
        {
            base.AddEventListener();
            closeBtn.onClick.RemoveAllListeners();
            closeBtn.onClick.AddListener((() =>
            {
                Hide();
            }));

            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener((() =>
            {
                if (state == BtnState.ZhuangBei)
                {

                    if (isWeapon)
                    {
                        WeaponData weapondata = DataController.Instance.weaponDataDic[PlayerDataModule.Instance.data.currentWeapon];
                        PlayerDataModule.Instance.data.addAtk -= weapondata.atkValue;
                        PlayerDataModule.Instance.data.currentWeapon = weaponData.id;
                        PlayerDataModule.Instance.data.addAtk += DataController.Instance.weaponDataDic[weaponData.id].atkValue;

                    }
                    else
                    {
                        StotageBagData stotageBagdata = DataController.Instance.storageBagDataDic[PlayerDataModule.Instance.data.currentBag];
                        PlayerDataModule.Instance.data.addBagCapacity -= stotageBagdata.capacity;
                        PlayerDataModule.Instance.data.currentBag = stotageBagData.id;
                        Debug.LogError("playerData.currentBag = " + PlayerDataModule.Instance.data.currentBag);
                        PlayerDataModule.Instance.data.addBagCapacity += DataController.Instance.storageBagDataDic[stotageBagData.id].capacity;
                    }
                    EventCenter.Instance.TriggerEvent(EventMessages.UpdatePlayerValueInfo);
                    state = BtnState.YiZhuangBei;
                    btntxt.text = "已装备";
                    EventCenter.Instance.TriggerEvent(EventMessages.UpdatePlayerEquimentInfo);
                }
                else if (state == BtnState.JieSuo)
                {


                    if (isWeapon)
                    {

                        PlayerDataModule.Instance.data.ownWeaponList.Add(weaponData.id);
                    }
                    else
                    {
                        PlayerDataModule.Instance.data.ownBagList.Add(stotageBagData.id);
                    }
                    state = BtnState.ZhuangBei;
                    btntxt.text = "装备";
                }
            }));

        }
    }
}
