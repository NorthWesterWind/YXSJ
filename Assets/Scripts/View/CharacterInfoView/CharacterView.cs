using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Controller;
using Module;
using Module.Data;
using Spine.Unity;
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
        public TextMeshProUGUI talentInfotxt1;
        public TextMeshProUGUI talentInfotxt2;
        public UIButton uptalentBtn;
        public TextMeshProUGUI Jmztxt;
        public List<Transform> children = new List<Transform>();

        public Transform itemTransform;

        public UIButton weaponDetailBtn;
        public UIButton bagDetailBtn;
        public UIButton clothBtn;
        public TextMeshProUGUI atktxt;
        public TextMeshProUGUI bagtxt;
        public TextMeshProUGUI hptxt;
        public Image weaponIcon;
        public Image bagIcon;
        public UIButton detailBtn;
        public List<TalentData> datas = new();

        public SkeletonGraphic skeletonGraphic;
        public Image dressMask1;
        public Image dressMask2;
        public Image clothMask;

        private Coroutine _scrollTalentCoroutine;

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
            clothBtn.onClick.RemoveAllListeners();
            clothBtn.onClick.AddListener(ShowCloth);
            detailBtn.onClick.RemoveAllListeners();
            detailBtn.onClick.AddListener(OnClickDetailBtn);
            EventCenter.Instance.AddListener(EventMessages.UpdatePlayerEquimentInfo, UpdateSoltState);
            EventCenter.Instance.AddListener(EventMessages.UpdatePlayerClothingInfo, UpdateClothInfo);
        }

        public void UpdateClothInfo(params object[] args)
        {
            skeletonGraphic.initialSkinName = PlayerDataModule.Instance.data.currentClothing.ToString();
            skeletonGraphic.Initialize(true);
        }
        public override void RemoveEventListener()
        {
            base.RemoveEventListener();
            EventCenter.Instance.RemoveListener(EventMessages.UpdatePlayerEquimentInfo, UpdateSoltState);
            EventCenter.Instance.RemoveListener(EventMessages.UpdatePlayerClothingInfo, UpdateClothInfo);
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
            PlayerData data = PlayerDataModule.Instance.data;
            if (data.talentPoint < 4)
            {
                UIController.Instance.Show<TipView>("翠芒珠材料不足！");
            }
            else
            {
                data.talentPoint -= 4;
                data.talentLevel += 1;
                UIController.Instance.Show<TipView>("升级成功！");
                TalentData talentData = DataController.Instance.talentDataDic[data.talentLevel];
                switch (talentData.type)
                {
                    case TalentType.Atk:
                        PlayerDataModule.Instance.data.addAtk += talentData.value;
                        break;
                    case TalentType.Atkhp:
                        PlayerDataModule.Instance.data.addhpRecover += talentData.value;
                        break;
                case TalentType.Hp:
                    PlayerDataModule.Instance.data.addHp += talentData.value;
                    break;
                case TalentType.BackpackCapacity:
                    PlayerDataModule.Instance.RefreshTalentDerivedStats();
                    break;
                case TalentType.Movespeed:
                    PlayerDataModule.Instance.data.addMoveSpeed += talentData.value;
                    break;
                    case TalentType.Pickuprange:
                        PlayerDataModule.Instance.data.addPickUpRange += talentData.value;
                        break;
                    case TalentType.Weaponsize:
                        PlayerDataModule.Instance.data.addweaponSize += talentData.value;
                        break;
                }
                ShowTalent();
                EventCenter.Instance.TriggerEvent(EventMessages.UpdatePlayerValueInfo);
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
            for (int i = children.Count - 1; i >= children.Count - count; i--)
            {
                children[i].GetComponent<TalentItemView>().Init(datas[j]);
                j++;
            }

            UpdateTalentState();
            ScrollToCurrentTalentItem();
        }
        public void UpdateTalentState(params object[] args)
        {
            PlayerData data = PlayerDataModule.Instance.data;
            Jmztxt.text = data.talentPoint.ToString();
            int level = data.talentLevel + 1;
            if (level > 80)
            {
                level = 80;
                uptalentBtn.interactable = false;
            }
            talentImg.sprite = _assetHandle.Get<Sprite>(DataController.Instance.talentDataDic[level].resName);
            leveltxt.text = level.ToString();
            if (level > 80)
            {
                level = 80;
            }
            TalentData talentData = DataController.Instance.talentDataDic[level];
            switch (talentData.type)
            {
                case TalentType.Atk:
                    talentInfotxt.text = "攻击力";
                    talentInfotxt1.text = (data.addAtk + data.atk) + "";
                    talentInfotxt2.text = $"<color=#00FF00>{(data.addAtk + data.atk + talentData.value)}</color>";
                    break;
                case TalentType.Atkhp:
                    talentInfotxt.text = "击败灵植或矿石健康值回复";
                    talentInfotxt1.text = (data.addhpRecover).ToString();
                    talentInfotxt2.text = $"<color=#00FF00>{(data.addhpRecover + talentData.value)} </color>";
                    break;
                case TalentType.Hp:
                    talentInfotxt.text = "健康值";
                    talentInfotxt1.text = (data.addHp + data.hp) + "";
                    talentInfotxt2.text = $"<color=#00FF00>{(data.addHp + data.hp + talentData.value)}</color>";
                    break;
                case TalentType.BackpackCapacity:
                    talentInfotxt.text = "储物袋容量";
                    talentInfotxt1.text = PlayerDataModule.Instance.GetTotalBagCapacity() + "";
                    talentInfotxt2.text = $"<color=#00FF00>{(PlayerDataModule.Instance.GetTotalBagCapacity() + talentData.value)}</color>";
                    break;
                case TalentType.Movespeed:
                    talentInfotxt.text = "移动速度";
                    talentInfotxt1.text = (data.addMoveSpeed * 100f).ToString("0") + "%";
                    talentInfotxt2.text = $"<color=#00FF00>{((data.addMoveSpeed + talentData.value) * 100f).ToString("0") + "%"} </color>";
                    break;
                case TalentType.Pickuprange:
                    talentInfotxt.text = "拾取范围";
                    talentInfotxt1.text = (data.addPickUpRange * 100f).ToString("0") + "%";
                    talentInfotxt2.text = $"<color=#00FF00>{((data.addPickUpRange + talentData.value) * 100f).ToString("0") + "%"} </color>";
                    break;
                case TalentType.Weaponsize:
                    talentInfotxt.text = "武器尺寸";
                    talentInfotxt1.text = (data.addweaponSize * 100f).ToString("0") + "%";
                    talentInfotxt2.text = $"<color=#00FF00>{((data.addweaponSize + talentData.value) * 100f).ToString("0") + "%"} </color>";
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
            PlayerData data = PlayerDataModule.Instance.data;
            atktxt.text = data.addAtk + data.atk + "";
            bagtxt.text = PlayerDataModule.Instance.GetTotalBagCapacity() + "";
            hptxt.text = data.addHp + data.hp + "";
            UpdateSoltState();
            UpdateClothInfo();
        }

        public void ShowWeapon()
        {
            var list = DataController.Instance.weaponDataDic
                .Values
                .OrderBy(x => x.id)   // 按你的武器编号排序
                .ToList();
            Extensions.ClearChildrenImmediate(itemTransform);
            for (int i = 0; i < list.Count; i++)
            {
                GameObject obj = GameObject.Instantiate<GameObject>(_assetHandle.Get<GameObject>("itemInfo"), itemTransform);
                obj.GetComponent<ItemInfo>().Init(list[i]);
            }
            dressMask1.gameObject.SetActive(false);
            dressMask2.gameObject.SetActive(true);
            clothMask.gameObject.SetActive(true);
        }

        public void ShowBag()
        {
            var list = DataController.Instance.storageBagDataDic
                .Values
                .OrderBy(x => x.id)   // 按你的武器编号排序
                .ToList();
            Extensions.ClearChildrenImmediate(itemTransform);
            for (int i = 0; i < list.Count; i++)
            {
                GameObject obj = GameObject.Instantiate<GameObject>(_assetHandle.Get<GameObject>("itemInfo"), itemTransform);
                obj.GetComponent<ItemInfo>().Init(list[i]);
            }
            dressMask1.gameObject.SetActive(true);
            dressMask2.gameObject.SetActive(false);
            clothMask.gameObject.SetActive(true);
        }

        public void ShowCloth()
        {
            var list = DataController.Instance.clothDataDic
               .Values
               .OrderBy(x => x.id)
               .ToList();
            Extensions.ClearChildrenImmediate(itemTransform);
            for (int i = 0; i < list.Count; i++)
            {
                GameObject obj = GameObject.Instantiate<GameObject>(_assetHandle.Get<GameObject>("itemInfo"), itemTransform);
                obj.GetComponent<ItemInfo>().Init(list[i]);
            }
            dressMask1.gameObject.SetActive(true);
            dressMask2.gameObject.SetActive(true);
            clothMask.gameObject.SetActive(false);
        }

        public void UpdateSoltState(params object[] args)
        {
            skeletonGraphic.AnimationState.SetAnimation(0, "待机", true);
            WeaponData weaponData = DataController.Instance.weaponDataDic[PlayerDataModule.Instance.data.currentWeapon];
            skeletonGraphic.Skeleton.SetAttachment(weaponData.slotName, weaponData.attachmentName);
            StotageBagData bagData = DataController.Instance.storageBagDataDic[PlayerDataModule.Instance.data.currentBag];
            skeletonGraphic.Skeleton.SetAttachment(bagData.slotName, bagData.attachmentName);

            weaponIcon.sprite = _assetHandle.Get<Sprite>(weaponData.name);
            bagIcon.sprite = _assetHandle.Get<Sprite>(bagData.name);
            PlayerData data = PlayerDataModule.Instance.data;
            atktxt.text = data.addAtk + data.atk + "";
            bagtxt.text = PlayerDataModule.Instance.GetTotalBagCapacity() + "";
            hptxt.text = data.addHp + data.hp + "";
        }

        public void OnClickDetailBtn()
        {
            UIController.Instance.Show<CharacterDetailView>();
        }

        private void ScrollToCurrentTalentItem()
        {
            if (talentContent == null || children.Count == 0)
            {
                return;
            }

            if (_scrollTalentCoroutine != null)
            {
                StopCoroutine(_scrollTalentCoroutine);
            }

            _scrollTalentCoroutine = StartCoroutine(ScrollToCurrentTalentItemRoutine());
        }

        private IEnumerator ScrollToCurrentTalentItemRoutine()
        {
            yield return null;

            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(talentContent);

            ScrollRect scrollRect = talentContent.GetComponentInParent<ScrollRect>();
            if (scrollRect == null)
            {
                _scrollTalentCoroutine = null;
                yield break;
            }

            RectTransform viewport = scrollRect.viewport != null ? scrollRect.viewport : scrollRect.GetComponent<RectTransform>();
            RectTransform target = GetCurrentTalentItem();
            if (viewport == null || target == null)
            {
                _scrollTalentCoroutine = null;
                yield break;
            }

            CenterTalentItemInViewport(scrollRect, viewport);
            _scrollTalentCoroutine = null;
        }

        private RectTransform GetCurrentTalentItem()
        {
            int currentLevel = Mathf.Clamp(PlayerDataModule.Instance.data.talentLevel, 1, datas.Count);

            for (int i = 0; i < children.Count; i++)
            {
                TalentItemView talentItemView = children[i].GetComponent<TalentItemView>();
                if (talentItemView != null && talentItemView.data != null && talentItemView.data.id == currentLevel)
                {
                    return children[i] as RectTransform;
                }
            }

            return null;
        }

        private void CenterTalentItemInViewport(ScrollRect scrollRect, RectTransform viewport)
        {
            if (viewport == null)
            {
                return;
            }

            RectTransform target = GetCurrentTalentItem();
            if (target == null)
            {
                return;
            }

            Vector2 targetLocal = viewport.InverseTransformPoint(target.position);
            Vector2 centerLocal = viewport.rect.center;
            Vector2 delta = centerLocal - targetLocal;
            Vector2 anchoredPosition = talentContent.anchoredPosition + new Vector2(0f, delta.y);

            float maxPosY = 0f;
            float minPosY = Mathf.Min(0f, viewport.rect.height - talentContent.rect.height);
            anchoredPosition.y = Mathf.Clamp(anchoredPosition.y, minPosY, maxPosY);
            talentContent.anchoredPosition = anchoredPosition;
            scrollRect.StopMovement();
        }
    }
}
