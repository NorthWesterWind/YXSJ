using Module;
using Module.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace View.CardView
{
    public class CardDetailPop : BaseView
    {
       public UIButton closeBtn;
       public TextMeshProUGUI title1txt;
       public TextMeshProUGUI contenttxt;
       public TextMeshProUGUI currenttxt;
       public TextMeshProUGUI nexttxt;
       public TextMeshProUGUI infotxt;
       public Image cardImg;
       public TextMeshProUGUI levelTxt;
       public Image fillImg;
       public GameObject fillContent;
       public TextMeshProUGUI filltxt;
       public CardLevelData  cardLevelData;
       public UIButton upgradeBtn;
       public TextMeshProUGUI cardprogresstxt;
       public TextMeshProUGUI goldneedtxt;
       public Image mask;
       public Image topleftLock;
       public TextMeshProUGUI masktxt;
       public AssetHandle assetHandle;

       public int currentNeedCard;
       public int currentNeedGold;
       CardUpProgress cardUpProgress = null ;

       public Image icon;

       public override void UpdateViewWithArgs(params object[] args)
       {
           base.UpdateViewWithArgs(args);
           cardLevelData = args[0] as CardLevelData;
           icon.sprite = assetHandle.Get<Sprite>(cardLevelData.name);
           title1txt.text = cardLevelData.name;
           switch (cardLevelData.developType)
           {
               case CardDevelopType.UpgradeCharacterWithXuanCaiTuAtk:
                   contenttxt.text = "攻击力";
                   break;
               case CardDevelopType.UpgradeCharacterWithXuanCaiTuHp:
                   contenttxt.text = "健康值";
                   break;
               case CardDevelopType.UpgradeGetLingJingShu:
                   contenttxt.text = "采集数";
                   break;
               case CardDevelopType.UpgradeLingZhangTai:
                   contenttxt.text = "打赏";
                   break;
               case CardDevelopType.UpgradeLingChuGe_1:
               case CardDevelopType.UpgradeLingChuGe_2:
                   contenttxt.text = "储物容量";
                   break;
               default:
                   contenttxt.text = "收益";
                   break;
           }

           infotxt.text = cardLevelData.description;
          // cardImg.sprite = assetHandle.Get<Sprite>(cardLevelData.resName);

           PlayerData playerData = PlayerDataModule.Instance.data;
           bool own = false;
         
           foreach (var value in playerData.cardUpProgressesList)
           {
               if (value.id == cardLevelData.id)
               {
                   own = true;
                   cardUpProgress = value;
                   break;
               }
           }
           if (own)
           {
               nexttxt.gameObject.SetActive(true);
               switch (cardLevelData.developType)
               {
                   case CardDevelopType.UpgradeCharacterWithXuanCaiTuAtk:
                       float percent = (cardUpProgress.level + 1) * 0.5f; //+ 50%
                       nexttxt.text = $"<color=#00FF00>x{percent * 100f}%</color>";
                       currenttxt.text = "x" + $" {cardUpProgress.level * 0.5f* 100f}%";
                       break;
                   case CardDevelopType.UpgradeCharacterWithXuanCaiTuHp:
                       nexttxt.text = $"<color=#00FF00>x{(cardUpProgress.level + 1) * 30}</color>"; //+ 30
                       currenttxt.text = "x" + $" {cardUpProgress.level * 30}";
                       break;
                   case CardDevelopType.UpgradeGetLingJingShu:
                       nexttxt.text = $"<color=#00FF00>x{(cardUpProgress.level + 1) * 10}</color>"; //+ 10
                       currenttxt.text = "x" + $" {cardUpProgress.level *10}";
                       break;
                   case CardDevelopType.UpgradeLingZhangTai:
                       nexttxt.text = $"<color=#00FF00>x{(cardUpProgress.level + 1) * 0.2f* 100f}%</color>"; //+ 20%
                       currenttxt.text = "x" + $" {cardUpProgress.level * 0.2f* 100f}%";
                       break;
                   case CardDevelopType.UpgradeLingChuGe_1:
                       nexttxt.text = $"<color=#00FF00>x{(cardUpProgress.level + 1) * 10}</color>"; //+ 10
                       currenttxt.text = "x" + $" {cardUpProgress.level * 10f}";
                       break;
                   case CardDevelopType.UpgradeLingChuGe_2:
                       nexttxt.text = $"<color=#00FF00>x{(cardUpProgress.level + 1) * 10}</color>"; //+ 10
                       currenttxt.text = "x" + $" {cardUpProgress.level * 10f}";
                       break;
                   case CardDevelopType.UpgradeYunDiGe:
                       nexttxt.text = $"<color=#00FF00>x{(cardUpProgress.level + 1) * 1}</color>"; //+ 1
                       currenttxt.text = "x" + $" {cardUpProgress.level}";
                       break;
                   case CardDevelopType.UpgradeYuShaHu_1:
                       nexttxt.text = $"<color=#00FF00>x{Mathf.Pow(3f, cardUpProgress.level + 1)}</color>"; 
                       currenttxt.text = "x" + $" {Mathf.Pow(3f, cardUpProgress.level )}";
                       break;
                   case CardDevelopType.UpgradeYuShaHu_2:
                       nexttxt.text = $"<color=#00FF00>x{Mathf.Pow(3f, cardUpProgress.level + 1)}</color>"; 
                       currenttxt.text = "x" + $" {Mathf.Pow(3f, cardUpProgress.level )}";
                       break;
                   case CardDevelopType.UpgradeYuShaHu_3:
                       nexttxt.text = $"<color=#00FF00>x{Mathf.Pow(3f, cardUpProgress.level + 1)}</color>"; 
                       currenttxt.text = "x" + $" {Mathf.Pow(3f, cardUpProgress.level )}";
                       break;
                   case CardDevelopType.UpgradeYuShaHu_4:
                       nexttxt.text = $"<color=#00FF00>x{Mathf.Pow(3f, cardUpProgress.level + 1)}</color>"; 
                       currenttxt.text = "x" + $" {Mathf.Pow(3f, cardUpProgress.level )}";
                       break;
                   case CardDevelopType.UpgradeLianQiLu_1:
                       nexttxt.text = $"<color=#00FF00>x{Mathf.Pow(3f, cardUpProgress.level + 1)}</color>"; 
                       currenttxt.text = "x" + $" {Mathf.Pow(3f, cardUpProgress.level )}";
                       break;
                   case CardDevelopType.UpgradeLianQiLu_2:
                       nexttxt.text = $"<color=#00FF00>x{Mathf.Pow(3f, cardUpProgress.level + 1)}</color>"; 
                       currenttxt.text = "x" + $" {Mathf.Pow(3f, cardUpProgress.level )}";
                       break;
                   case CardDevelopType.UpgradeLianQiLu_3:
                       nexttxt.text = $"<color=#00FF00>x{Mathf.Pow(3f, cardUpProgress.level + 1)}</color>"; 
                       currenttxt.text = "x" + $" {Mathf.Pow(3f, cardUpProgress.level )}";
                       break;
               }
               
               
               levelTxt.text = cardUpProgress.level.ToString();
               levelTxt.gameObject.SetActive(true);
               mask.gameObject.SetActive(false);
               topleftLock.gameObject.SetActive(false);
               if (cardUpProgress.level == 10)
               {
                   fillContent.SetActive(false);
                   filltxt.text = "已满级";
               }
               else
               {
                   fillContent.SetActive(true);
                   fillImg.fillAmount = cardUpProgress.currentNum * 1f /WorldData.cardUpLevelArr[cardUpProgress.level+1];
               }
               upgradeBtn.gameObject.SetActive(true);
           }
           else
           {
               upgradeBtn.gameObject.SetActive(false);
               nexttxt.gameObject.SetActive(false);
                  switch (cardLevelData.developType)
               {
                   case CardDevelopType.UpgradeCharacterWithXuanCaiTuAtk:
                       currenttxt.text = "x" + $" {0.5f* 100f}%";
                       break;
                   case CardDevelopType.UpgradeCharacterWithXuanCaiTuHp:
                       currenttxt.text = "x30" ;
                       break;
                   case CardDevelopType.UpgradeGetLingJingShu:
                       currenttxt.text = "x10";
                       break;
                   case CardDevelopType.UpgradeLingZhangTai:
                       currenttxt.text = "x" + $"{0.2f* 100f}%";
                       break;
                   case CardDevelopType.UpgradeLingChuGe_1:
                       currenttxt.text = "x10";
                       break;
                   case CardDevelopType.UpgradeLingChuGe_2:
                       currenttxt.text = "x10";
                       break;
                   case CardDevelopType.UpgradeYunDiGe:
                       currenttxt.text = "x1";
                       break;
                   case CardDevelopType.UpgradeYuShaHu_1:
                       currenttxt.text = "x3" ;
                       break;
                   case CardDevelopType.UpgradeYuShaHu_2:
                       currenttxt.text = "x3" ;
                       break;
                   case CardDevelopType.UpgradeYuShaHu_3:
                       currenttxt.text = "x3" ;
                       break;
                   case CardDevelopType.UpgradeYuShaHu_4:
                       currenttxt.text = "x3" ;
                       break;
                   case CardDevelopType.UpgradeLianQiLu_1:
                       currenttxt.text = "x3" ;
                       break;
                   case CardDevelopType.UpgradeLianQiLu_2:
                       currenttxt.text = "x3" ;
                       break;
                   case CardDevelopType.UpgradeLianQiLu_3:
                       currenttxt.text = "x3" ;
                       break;
               }
               
               
               
               
               
               if (cardLevelData.unlockLevel > playerData.accountLevel)
               {
                   //未到达等级解锁条件
                 
                   fillContent.SetActive(false);
                   filltxt.gameObject.SetActive(false);
                   mask.gameObject.SetActive(true);
                   masktxt.text = cardLevelData.unlockLevel.ToString();
               }
               else
               {
                   //到达等级解锁条件，未拥有
                //    topleftLock.gameObject.SetActive(false);
                   fillContent.SetActive(false);
                   filltxt.gameObject.SetActive(false);
                   mask.gameObject.SetActive(false);
               }
               topleftLock.gameObject.SetActive(true);
           }
           int tempvalue;
           if (cardUpProgress != null)
           {
               tempvalue = (cardUpProgress.level+1) > 10 ? 10 : cardUpProgress.level + 1;
           }
           else
           {
               tempvalue = 0;
           }

          
           switch (cardLevelData.levelType)
           {
               case CardLevelType.FanPing:
                   currentNeedGold = WorldData.cardUpgradeCostArr1[ tempvalue ];
                    cardImg.sprite = assetHandle.Get<Sprite>("白卡");
                    mask.sprite = assetHandle.Get<Sprite>("白卡");
                   break;
               case CardLevelType.LingYun:
                   currentNeedGold = WorldData.cardUpgradeCostArr2[ tempvalue ];
                   cardImg.sprite = assetHandle.Get<Sprite>("紫卡");
                    mask.sprite = assetHandle.Get<Sprite>("紫卡");
                   break;
               case CardLevelType.XianYun:
                   currentNeedGold = WorldData.cardUpgradeCostArr3[ tempvalue ];
                   cardImg.sprite = assetHandle.Get<Sprite>("红卡");
                    mask.sprite = assetHandle.Get<Sprite>("红卡");
                   break;
           }

           if (cardUpProgress != null)
           {
               currentNeedCard = WorldData.cardUpLevelArr[cardUpProgress.level-1];
               cardprogresstxt.text = cardUpProgress.currentNum +"/" + currentNeedCard;
           }
           else
           { 
               currentNeedCard = WorldData.cardUpLevelArr[0];
               cardprogresstxt.text = 0+"/" + currentNeedCard;
           }
         
           goldneedtxt.text = currentNeedGold.ToString();
           
           
       }

       protected override void AddEventListener()
       {
           base.AddEventListener();
           closeBtn.onClick.RemoveAllListeners();
           closeBtn.onClick.AddListener((() =>
           {
               Hide();
           }));
           upgradeBtn.onClick.RemoveAllListeners();
           upgradeBtn.onClick.AddListener((() =>
           {
               if (cardUpProgress.currentNum < currentNeedCard)
               {
                   UIController.Instance.Show<TipView>("当前卡片数量不足！");
                   return;
               }

               if (PlayerDataModule.Instance.data.goldIngot < currentNeedGold)
               {
                   UIController.Instance.Show<TipView>("金元宝数量不足！");
                   return;
               }
               cardUpProgress.currentNum -= currentNeedCard;
               PlayerDataModule.Instance.data.goldIngot -= currentNeedGold;
               UIController.Instance.Show<TipView>("升级成功！");
               cardUpProgress.level += 1;
               
               currentNeedCard = WorldData.cardUpLevelArr[cardUpProgress.level-1];
               cardprogresstxt.text = cardUpProgress.currentNum +"/" + currentNeedCard;
               int tempvalue = (cardUpProgress.level+1) > 10 ? 10 : cardUpProgress.level + 1;
               switch (cardLevelData.levelType)
               {
                   case CardLevelType.FanPing:
                       currentNeedGold = WorldData.cardUpgradeCostArr1[ tempvalue ];
                       break;
                   case CardLevelType.LingYun:
                       currentNeedGold = WorldData.cardUpgradeCostArr2[ tempvalue ];
                       break;
                   case CardLevelType.XianYun:
                       
                       currentNeedGold = WorldData.cardUpgradeCostArr3[ tempvalue ];
                       break;
               }
               
               goldneedtxt.text = currentNeedGold.ToString();
           }));
           EventCenter.Instance.TriggerEvent(EventMessages.UpdatePlayerMoneyInfo);
       }
    }
}
