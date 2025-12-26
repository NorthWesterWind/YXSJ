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
        GouMai
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

        public override void UpdateViewWithArgs(params object[] args)
        {
            PlayerData playerdata = ModuleMgr.Instance.GetModule<PlayerDataModule>().data;
            if (args[0] is WeaponData)
            {
                WeaponData data = args[0] as WeaponData;
                tiptxt.text = data.name;
                locktxt.text = data.unlockStr;
                if (playerdata.ownWeaponList.Contains(data.id))
                {
                    if (playerdata.currentWeapon == data.id)
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
                }
                else
                {
                    if (data.lockType == UnlockType.Purchase)
                    {
                        fillContent.SetActive(false);
                        state = BtnState.GouMai;
                        btntxt.text = "购买";
                    }
                    else
                    {
                        fillContent.SetActive(true);
                        state = BtnState.DaiJieSuo;
                        btntxt.text = "待解锁";
                        switch (data.lockType)
                        {
                            case UnlockType.accountLevel:
                                fill.fillAmount = playerdata.accountLevel * 1f / data.value;
                                filltxt.text = "(" + playerdata.accountLevel + "/" + data.value + ")";
                                break;
                            case UnlockType.CardLevel:
                                fill.fillAmount = playerdata.cardLevelMax * 1f / data.value;
                                filltxt.text = "(" + playerdata.cardLevelMax + "/" + data.value + ")";
                                break;
                            case UnlockType.talentLevel:
                                fill.fillAmount = playerdata.talentLevel * 1f / data.value;
                                filltxt.text = "(" + playerdata.talentLevel + "/" + data.value + ")";
                                break;
                            case UnlockType.UseLingJing:
                                fill.fillAmount = playerdata.useLingJingTotalValue * 1f / data.value;
                                filltxt.text = "(" + playerdata.useLingJingTotalValue + "/" + data.value + ")";
                                break;
                            case UnlockType.XianYunZhuanPan:
                                fill.fillAmount = playerdata.useZhuanPanTotalValue * 1f / data.value;
                                filltxt.text = "(" + playerdata.useZhuanPanTotalValue + "/" + data.value + ")";
                                break;

                        }
                    }
                }
            }
            else if (args[0] is StotageBagData)
            {
                StotageBagData data = args[0] as StotageBagData;
                tiptxt.text = data.name;
                locktxt.text = data.unlockStr;
                if (playerdata.ownBagList.Contains(data.id))
                {
                    if (playerdata.currentBag == data.id)
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
                }
                else
                {
                    if (data.lockType == UnlockType.Purchase)
                    {
                        fillContent.SetActive(false);
                        state = BtnState.GouMai;
                        btntxt.text = "购买";
                    }
                    else
                    {
                        fillContent.SetActive(true);
                        state = BtnState.DaiJieSuo;
                        btntxt.text = "待解锁";
                        switch (data.lockType)
                        {
                            case UnlockType.accountLevel:
                                fill.fillAmount = playerdata.accountLevel * 1f / data.value;
                                filltxt.text = "(" + playerdata.accountLevel + "/" + data.value + ")";
                                break;
                            case UnlockType.CardLevel:
                                fill.fillAmount = playerdata.cardLevelMax * 1f / data.value;
                                filltxt.text = "(" + playerdata.cardLevelMax + "/" + data.value + ")";
                                break;
                            case UnlockType.talentLevel:
                                fill.fillAmount = playerdata.talentLevel * 1f / data.value;
                                filltxt.text = "(" + playerdata.talentLevel + "/" + data.value + ")";
                                break;
                            case UnlockType.UseLingJing:
                                fill.fillAmount = playerdata.useLingJingTotalValue * 1f / data.value;
                                filltxt.text = "(" + playerdata.useLingJingTotalValue + "/" + data.value + ")";
                                break;
                            case UnlockType.XianYunZhuanPan:
                                fill.fillAmount = playerdata.useZhuanPanTotalValue * 1f / data.value;
                                filltxt.text = "(" + playerdata.useZhuanPanTotalValue + "/" + data.value + ")";
                                break;

                        }
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
                
            }));
            
        }
    }
}
