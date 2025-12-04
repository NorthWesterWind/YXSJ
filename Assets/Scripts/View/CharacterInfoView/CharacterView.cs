using System.Collections.Generic;
using System.Linq;
using Controller;
using Module;
using Module.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using View.PopUp;

namespace View.CharacterInfoView
{
    public class CharacterView : BaseView
    {
        public GameObject talentRect;
        public GameObject dressRect;
        public UIButton closeButton;
        public UIButton talentBtn;
        public UIButton dressBtn;
        public GameObject talentMask;
        public GameObject dressMask;
        public RectTransform talentContent;
        public Image talentImg;
        public TextMeshProUGUI leveltxt;
        public TextMeshProUGUI talentInfotxt;
        public UIButton uptalentBtn;
        public TextMeshProUGUI Jmztxt;
        public  List<Transform> children = new List<Transform>();
        
        public List<ItemInfo> items = new List<ItemInfo>();
        
        public UIButton weaponDetailBtn;
        public UIButton bagDetailBtn;
        public TextMeshProUGUI atktxt;
        public TextMeshProUGUI bagtxt;
        public TextMeshProUGUI hptxt;
        public Image weaponIcon;
        public Image bagIcon;
        public UIButton detailBtn;
        public List<TalentData> datas = new();
        protected override void Start()
        {
            base.Start();
           
        }

        protected override void AddEventListener()
        {
            base.AddEventListener();
            talentBtn.onClick.RemoveAllListeners();
            talentBtn.onClick.AddListener(() => { ShowTalent(); });
            dressBtn.onClick.RemoveAllListeners();
            dressBtn.onClick.AddListener(() => { ShowDress(); });
            uptalentBtn.onClick.RemoveAllListeners();
            uptalentBtn.onClick.AddListener(OnClickUptalentBtn);
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener((() =>
            {
                Hide();
            }));
            weaponDetailBtn.onClick.RemoveAllListeners();
            weaponDetailBtn.onClick.AddListener(ShowWeapon);
            bagDetailBtn.onClick.RemoveAllListeners();
            bagDetailBtn.onClick.AddListener(ShowBag);
            detailBtn.onClick.RemoveAllListeners();
            detailBtn.onClick.AddListener(OnClickDetailBtn);
        }


        public override void UpdateViewWithArgs(params object[] args)
        {
            base.UpdateViewWithArgs(args);
            ShowTalent();
        }

        protected override void OnHideComplete()
        {
            base.OnHideComplete();
            EventCenter.Instance.TriggerEvent(EventMessages.ShowPlayerInfoViewCartoon);
        }

        public void OnClickUptalentBtn()
        {
            PlayerData data = ModuleMgr.Instance.GetModule<PlayerDataModule>().data;
            if (data.jingMangZhu < 4)
            {
                UIController.Instance.Show<TipView>("金芒珠数量不足！");
            }
            else
            {
                data.jingMangZhu -= 4;
                data.talentLevel += 1;
                UIController.Instance.Show<TipView>("升级成功！");
                TalentData talentData = DataController.Instance.talentDataDic[ data.talentLevel];
            switch (talentData.type)
            {
                case TalentType.Atk:
                    ModuleMgr.Instance.GetModule<PlayerDataModule>().data.addAtk +=  talentData.value;
                    break;
                case TalentType.Atkhp:
                    ModuleMgr.Instance.GetModule<PlayerDataModule>().data.addhpRecover +=  talentData.value;
                    break;
                case TalentType.Hp:
                    ModuleMgr.Instance.GetModule<PlayerDataModule>().data.addHp +=  talentData.value;
                    break;
                case TalentType.BackpackCapacity:
                    ModuleMgr.Instance.GetModule<PlayerDataModule>().data.addBagCapacity +=  talentData.value;
                    break;
                case TalentType.Movespeed:
                    ModuleMgr.Instance.GetModule<PlayerDataModule>().data.addMoveSpeed +=  talentData.value;
                    break;
                case TalentType.Pickuprange:
                    ModuleMgr.Instance.GetModule<PlayerDataModule>().data.addPickUpRange +=  talentData.value;
                    break;
                case TalentType.Weaponsize:
                    ModuleMgr.Instance.GetModule<PlayerDataModule>().data.addweaponSize +=  talentData.value;
                    break;
            }
            UpdateTalentState();
            }
        }

     
        public void ShowTalent()
        {
            talentRect.SetActive(true);
            dressRect.SetActive(false);
            talentMask.SetActive(false);
            dressMask.SetActive(true);

            if (datas.Count == 0)
            {
                datas = new List<TalentData>(DataController.Instance.talentDataDic.Values);
                datas.Sort((a, b) => a.id.CompareTo(b.id)); 
            }

            if (children.Count == 0)
            {
                for (int i = 0; i < talentContent.childCount; i++)
                {
                    children.Add(talentContent.GetChild(i));
                }
            }
            int count = Mathf.Min(datas.Count, children.Count); // 避免越界
            int j = 0;
            Debug.Log($"yj => {count}");
            Debug.Log($"yj => datas.Count ={datas.Count}");
            for (int i = children.Count - 1; i >= children.Count - count; i--)
            {
                children[i].GetComponent<TalentItemView>().Init(datas[j]);
                j++;
            }

            UpdateTalentState();
        }
        public void UpdateTalentState(params object[] args)
        {
            PlayerData data = ModuleMgr.Instance.GetModule<PlayerDataModule>().data;
            Jmztxt.text = data.jingMangZhu.ToString();
            int level = data.talentLevel;
            //talentImg.sprite = _assetHandle.Get<Sprite>(DataController.Instance.talentDataDic[level].resName);
            leveltxt.text = level.ToString();
            level += 1;
            if (level > 80)
            {
                level = 80;
            }
            TalentData talentData = DataController.Instance.talentDataDic[level];
            switch (talentData.type)
            {
                case TalentType.Atk:
                    talentInfotxt.text = "攻击力\n" + (data.addAtk + data.atk) + "  " +
                                         $"<color=#00FF00>{(data.addAtk + data.atk + talentData.value)}</color>";
                    break;
                case TalentType.Atkhp:
                    talentInfotxt.text = "健康值回复\n" + (data.addhpRecover * 100f).ToString("0") + "%  " +
                                         $"<color=#00FF00> {((data.addhpRecover + talentData.value) * 100f).ToString("0") + "%"} </color>";
                    break;
                case TalentType.Hp:
                    talentInfotxt.text = "健康值\n" + (data.addHp + data.hp) + "  " +
                                         $"<color=#00FF00>{(data.addHp + data.hp + talentData.value)}</color>";
                    break;
                case TalentType.BackpackCapacity:
                    talentInfotxt.text = "储物袋容量\n" + (data.addBagCapacity + data.bagCapacity) + "  " +
                                         $"<color=#00FF00>{(data.addBagCapacity + data.bagCapacity + talentData.value)}</color>";
                    break;
                case TalentType.Movespeed:
                    talentInfotxt.text = "移动速度\n" + (data.addMoveSpeed * 100f).ToString("0") + "%  " +
                                         $"<color=#00FF00> {((data.addMoveSpeed + talentData.value) * 100f).ToString("0") + "%"} </color>";
                    break;
                case TalentType.Pickuprange:
                    talentInfotxt.text = "拾取范围\n" + (data.addPickUpRange * 100f).ToString("0") + "%  " +
                                         $"<color=#00FF00> {((data.addPickUpRange + talentData.value) * 100f).ToString("0") + "%"} </color>";
                    break;
                case TalentType.Weaponsize:
                    talentInfotxt.text = "武器尺寸\n" + (data.addweaponSize * 100f).ToString("0") + "%  " +
                                         $"<color=#00FF00> {((data.addweaponSize + talentData.value) * 100f).ToString("0") + "%"} </color>";
                    break;
            }
        }
        
        public void ShowDress()
        {
            talentRect.SetActive(false);
            dressRect.SetActive(true);
            dressMask.SetActive(false);
            talentMask.SetActive(true);
            ShowWeapon();
            PlayerData data = ModuleMgr.Instance.GetModule<PlayerDataModule>().data;
            atktxt.text = data.addAtk +  data.atk + "";
            bagtxt.text = data.addBagCapacity +  data.bagCapacity + "";
            hptxt.text = data.addHp +  data.hp + "";
            //TODO:卡槽更新
        }

        public void ShowWeapon()
        {
            var list = DataController.Instance.weaponDataDic
                .Values
                .OrderBy(x => x.id)   // 按你的武器编号排序
                .ToList();

            int count = Mathf.Min(items.Count, list.Count);

            for (int i = 0; i < count; i++)
            {
                items[i].Init(list[i]);
            }
        }

        public void ShowBag()
        {
            var list = DataController.Instance.storageBagDataDic
                .Values
                .OrderBy(x => x.id)   // 按你的武器编号排序
                .ToList();

            int count = Mathf.Min(items.Count, list.Count);

            for (int i = 0; i < count; i++)
            {
                items[i].Init(list[i]);
            }
        }

        public void OnClickDetailBtn()
        {
            UIController.Instance.Show<CharacterDetailView>();
        }
    }
}