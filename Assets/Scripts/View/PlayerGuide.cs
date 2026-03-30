using System.Collections;
using Controller;
using Controller.Structure;
using Module;
using Module.Data;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

public class PlayerGuide : BaseView
{
    public GameObject bg;
    public GameObject infoContent_1;
    public GameObject infoContent_2;

    public UIButton btn;
    public TextMeshProUGUI infotxt_1;
    public TextMeshProUGUI infotxt_2;
    public SkeletonGraphic jianling;
    public SkeletonGraphic character;
    public VerticalLayoutGroup verticalLayoutGroup1;
    public VerticalLayoutGroup verticalLayoutGroup2;

    private string[] info = new string[]
    {
      "\u3000\u3000新主人，终于等到你啦！我是镇妖剑的剑灵小灵，你被我选中成为新一代镇妖剑主人！",
      "\u3000\u3000镇妖剑？新主人......我该怎么做？",
      "\u3000\u3000主人，别慌呀，有我在！现在我们先去建造一号玉砂壶。",
      "\u3000\u3000太棒啦！接下来我们来打造一号灵茶售卖架，主人你还记得吗？以前街边的灵茶坊里，可都摆着这样的架子，用它来售卖灵茶，就能赚不少铜币呢！",
      "\u3000\u3000接下来，我们就要进入获取灵材区域。前面不远处就是采集霜云芝的区域了，只要拿起“我”攻击它们，就能自动散掉妖气，把灵材变纯净，就可以轻易获取了。要是遇到那种比人还高的巨型灵体也别怕，“我”也能驱散它的妖气获取它哦！",
      "\u3000\u3000好了，我们快把霜云芝送到一号玉砂壶凝炼吧！",
      "\u3000\u3000小灵，接下来应该怎么做？",
      "\u3000\u3000制作灵茶需要时间，我们先去建造灵账台吧！",
      "\u3000\u3000主人你看！灵账使姐姐超靠谱的！以后你去远处采集灵材，店里的灵茶售卖、铜币结算她都能打理。",
      "\u3000\u3000有客人来啦，我们快回去一号玉砂壶取灵茶，然后将灵茶送到一号灵茶售卖架上售卖吧！",
      "\u3000\u3000我们快把云芝茶送到一号灵茶售卖架上售卖吧！",
      "\u3000\u3000移动到灵账台，尝试一下给客人结账吧！",
      "\u3000\u3000好耶好耶！主人你看灵账台里的铜币——这可是咱们靠纯净灵茶赚的第一笔钱！",
      "\u3000\u3000不过主人，就靠现在这壶煮的灵茶，收益还不够呀——你想，以前的灵茶都是用高阶玉砂壶煮的，茶香能飘三条街！咱们升级这壶，能让灵茶的灵气更足、口感更好，自然能卖更高的价钱，攒钱更快，才能做更多复苏灵艺的事呀！",
      "\u3000\u3000我们去升级玉砂壶，用更高的价格卖出灵茶吧！",
      "\u3000\u3000好了，基本的操作主人你已经学会了！接下来呀，就要靠主人自己去探索这片妖力弥漫的妖剑世界，一步步靠近驱散妖力和复苏灵艺的使命啦！",
    };

    private Coroutine typingCoroutine;
    private bool isSkipping = false;
    private bool showAll = false;
    public int currentIndex = 0;



    public override void UpdateViewWithArgs(params object[] args)
    {
        base.UpdateViewWithArgs(args);
        EventCenter.Instance.TriggerEvent(EventMessages.HideGuideFinger);
        currentIndex = (int)PlayerDataModule.Instance.data.guideStep;
        if (currentIndex <= 0 || currentIndex > info.Length)
        {
            Debug.LogWarning($"[PlayerGuide] Invalid guide step index: {currentIndex}, info length: {info.Length}");
            Hide();
            return;
        }
        bg.gameObject.SetActive(true);
        ShowText(info[currentIndex - 1]);
    }

    public void ShowText(string text)
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeEffect(text));
    }
    protected override void AddEventListener()
    {
        base.AddEventListener();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(OnSkipClicked);
    }

    private IEnumerator TypeEffect(string text)
    {
        switch (currentIndex)
        {
            case 1:
                infoContent_1.gameObject.SetActive(true);
                infoContent_2.gameObject.SetActive(false);
                infotxt_1.text = info[0];
                yield return null;
                Canvas.ForceUpdateCanvases();
                LayoutRebuilder.ForceRebuildLayoutImmediate(
                    verticalLayoutGroup1.GetComponent<RectTransform>()
                );
                infotxt_1.maxVisibleCharacters = 0;
                jianling.AnimationState.SetAnimation(0, "idle", true);
                break;
            case 2:

                infoContent_1.gameObject.SetActive(false);
                infoContent_2.gameObject.SetActive(true);
                infotxt_2.text = info[1];
                yield return null;

                Canvas.ForceUpdateCanvases();
                LayoutRebuilder.ForceRebuildLayoutImmediate(
                    verticalLayoutGroup2.GetComponent<RectTransform>()
                );
                infotxt_2.maxVisibleCharacters = 0;
                character.AnimationState.SetAnimation(0, "待机", true);
                break;
            case 3:

                infoContent_2.gameObject.SetActive(false);
                infoContent_1.gameObject.SetActive(true);
                infotxt_1.text = info[2];
                yield return null;
                Canvas.ForceUpdateCanvases();
                LayoutRebuilder.ForceRebuildLayoutImmediate(
                    verticalLayoutGroup1.GetComponent<RectTransform>()
                );
                infotxt_1.maxVisibleCharacters = 0;
                jianling.AnimationState.SetAnimation(0, "idle", true);
                break;
            case 4:
                infoContent_1.gameObject.SetActive(true);
                infoContent_2.gameObject.SetActive(false);
                infotxt_1.text = info[3];
                yield return null;
                Canvas.ForceUpdateCanvases();
                LayoutRebuilder.ForceRebuildLayoutImmediate(
                    verticalLayoutGroup1.GetComponent<RectTransform>()
                );
                infotxt_1.maxVisibleCharacters = 0;
                jianling.AnimationState.SetAnimation(0, "idle", true);
                break;
            case 5:
                infoContent_1.gameObject.SetActive(true);
                infoContent_2.gameObject.SetActive(false);
                infotxt_1.text = info[4];
                yield return null;
                Canvas.ForceUpdateCanvases();
                LayoutRebuilder.ForceRebuildLayoutImmediate(
                    verticalLayoutGroup1.GetComponent<RectTransform>()
                );
                infotxt_1.maxVisibleCharacters = 0;
                jianling.AnimationState.SetAnimation(0, "idle", true);
                break;
            case 6:
                infoContent_1.gameObject.SetActive(true);
                infoContent_2.gameObject.SetActive(false);
                infotxt_1.text = info[5];
                yield return null;
                Canvas.ForceUpdateCanvases();
                LayoutRebuilder.ForceRebuildLayoutImmediate(
                    verticalLayoutGroup1.GetComponent<RectTransform>()
                );
                infotxt_1.maxVisibleCharacters = 0;
                jianling.AnimationState.SetAnimation(0, "idle", true);
                break;
            case 7:
                infoContent_2.gameObject.SetActive(true);
                infoContent_1.gameObject.SetActive(false);
                infotxt_2.text = info[6];
                yield return null;
                Canvas.ForceUpdateCanvases();
                LayoutRebuilder.ForceRebuildLayoutImmediate(
                    verticalLayoutGroup2.GetComponent<RectTransform>()
                );
                infotxt_2.maxVisibleCharacters = 0;
                character.AnimationState.SetAnimation(0, "待机", true);
                break;
            case 8:
                infoContent_1.gameObject.SetActive(true);
                infoContent_2.gameObject.SetActive(false);
                infotxt_1.text = info[7];
                yield return null;

                Canvas.ForceUpdateCanvases();
                LayoutRebuilder.ForceRebuildLayoutImmediate(
                    verticalLayoutGroup1.GetComponent<RectTransform>()
                );
                infotxt_1.maxVisibleCharacters = 0;
                jianling.AnimationState.SetAnimation(0, "idle", true);
                break;
            case 9:
                infoContent_1.gameObject.SetActive(true);
                infoContent_2.gameObject.SetActive(false);
                infotxt_1.text = info[8];
                yield return null;
                Canvas.ForceUpdateCanvases();
                LayoutRebuilder.ForceRebuildLayoutImmediate(
                    verticalLayoutGroup1.GetComponent<RectTransform>()
                );

                infotxt_1.maxVisibleCharacters = 0;
                jianling.AnimationState.SetAnimation(0, "idle", true);
                break;
            case 10:
                infoContent_1.gameObject.SetActive(true);
                infoContent_2.gameObject.SetActive(false);
                infotxt_1.text = info[9];
                yield return null;
                Canvas.ForceUpdateCanvases();
                LayoutRebuilder.ForceRebuildLayoutImmediate(
                    verticalLayoutGroup1.GetComponent<RectTransform>()
                );

                infotxt_1.maxVisibleCharacters = 0;
                jianling.AnimationState.SetAnimation(0, "idle", true);
                break;
            case 11:
                infoContent_1.gameObject.SetActive(true);
                infoContent_2.gameObject.SetActive(false);
                infotxt_1.text = info[10];
                yield return null;
                Canvas.ForceUpdateCanvases();
                LayoutRebuilder.ForceRebuildLayoutImmediate(
                    verticalLayoutGroup1.GetComponent<RectTransform>()
                );

                infotxt_1.maxVisibleCharacters = 0;
                jianling.AnimationState.SetAnimation(0, "idle", true);
                break;
            case 12:
                infoContent_1.gameObject.SetActive(true);
                infoContent_2.gameObject.SetActive(false);
                infotxt_1.text = info[11];
                yield return null;
                Canvas.ForceUpdateCanvases();
                LayoutRebuilder.ForceRebuildLayoutImmediate(
                    verticalLayoutGroup1.GetComponent<RectTransform>()
                );
                infotxt_1.maxVisibleCharacters = 0;
                jianling.AnimationState.SetAnimation(0, "idle", true);
                break;
            case 13:
                infoContent_1.gameObject.SetActive(true);
                infoContent_2.gameObject.SetActive(false);
                infotxt_1.text = info[12];
                yield return null;
                Canvas.ForceUpdateCanvases();
                LayoutRebuilder.ForceRebuildLayoutImmediate(
                    verticalLayoutGroup1.GetComponent<RectTransform>()
                );
                infotxt_1.maxVisibleCharacters = 0;
                jianling.AnimationState.SetAnimation(0, "idle", true);
                break;
            case 14:
                infoContent_1.gameObject.SetActive(true);
                infoContent_2.gameObject.SetActive(false);
                infotxt_1.text = info[13];
                yield return null;
                Canvas.ForceUpdateCanvases();
                LayoutRebuilder.ForceRebuildLayoutImmediate(
                    verticalLayoutGroup1.GetComponent<RectTransform>()
                );

                infotxt_1.maxVisibleCharacters = 0;
                jianling.AnimationState.SetAnimation(0, "idle", true);
                break;
            case 15:
                infoContent_1.gameObject.SetActive(true);
                infoContent_2.gameObject.SetActive(false);
                infotxt_1.text = info[14];
                yield return null;
                Canvas.ForceUpdateCanvases();
                LayoutRebuilder.ForceRebuildLayoutImmediate(
                    verticalLayoutGroup1.GetComponent<RectTransform>()
                );

                infotxt_1.maxVisibleCharacters = 0;
                jianling.AnimationState.SetAnimation(0, "idle", true);
                break;
            case 16:
                infoContent_1.gameObject.SetActive(true);
                infoContent_2.gameObject.SetActive(false);
                infotxt_1.text = info[15];
                yield return null;
                Canvas.ForceUpdateCanvases();
                LayoutRebuilder.ForceRebuildLayoutImmediate(
                    verticalLayoutGroup1.GetComponent<RectTransform>()
                );
                infotxt_1.maxVisibleCharacters = 0;
                jianling.AnimationState.SetAnimation(0, "idle", true);
                break;
        }

        showAll = false;

        for (int i = 0; i <= text.Length; i++)
        {
            if (isSkipping)
                break;

            switch (currentIndex)
            {
                case 1:
                case 3:
                case 4:
                case 5:
                case 6:
                case 8:
                case 9:
                case 10:
                case 11:
                case 12:
                case 13:
                case 14:
                case 15:
                case 16:

                    infotxt_1.maxVisibleCharacters = i;
                    yield return new WaitForSeconds(0.1f);
                    break;
                case 2:
                case 7:
                    infotxt_2.maxVisibleCharacters = i;
                    yield return new WaitForSeconds(0.1f);
                    break;
            }


        }
        switch (currentIndex)
        {
            case 1:
            case 3:
            case 4:
            case 5:
            case 6:
            case 8:
            case 9:
            case 10:
            case 11:
            case 12:
            case 13:
            case 14:
            case 15:
            case 16:
                infotxt_1.maxVisibleCharacters = text.Length;
                break;
            case 2:
            case 7:
                infotxt_2.maxVisibleCharacters = text.Length;
                break;
        }
        showAll = true;
        isSkipping = false;
    }

    private void OnSkipClicked()
    {
        if (!showAll)
        {
            // 当前文字未显示完 → 快速显示
            isSkipping = true;
        }
        else
        {

            if (currentIndex - 1 < info.Length)
            {
                switch (currentIndex)
                {
                    case 3:
                        TriggerGuide_1();
                        break;
                    case 4:
                        TriggerGuide_2();
                        break;
                    case 5:
                        TriggerGuide_3();
                        break;
                    case 6:
                        TriggerGuide_4();
                        break;
                    case 8:
                        TriggerGuide_5();
                        break;
                    case 10:
                        TriggerGuide_6();
                        break;
                    case 11:
                        TriggerGuide_7();
                        break;
                    case 12:
                        Hide();
                        break;
                    case 15:
                        TriggerGuide_9();

                        break;
                    case 16:
                        NextContent();
                        break;
                    default:
                        currentIndex++;
                        ShowText(info[currentIndex - 1]);
                        break;
                }

            }
            else
            {
                NextContent();
            }
        }
    }

    private void NextContent()
    {
        Hide();
        PlayerDataModule.Instance.data.guideStep = GuideStep.Over;
    }


    private void TriggerGuide_1()
    {
        //建造一号玉砂壶
        Transform collectPoint = GameController.Instance.buildings[BuildingType.YuShaHu_1].transform;
        EventCenter.Instance.TriggerEvent(EventMessages.ShowGuideFinger, new Vector2(collectPoint.position.x, collectPoint.position.y ));

        StructureLockData data1 = DataController.Instance.structureLockDataList_1.Find(x => x.buildingType == BuildingType.YuShaHu_1);
        StructureLockProgressData progress1 = new StructureLockProgressData(BuildingType.YuShaHu_1,
            data1.needMoney, data1.lockId, PlayerDataModule.Instance.data.currentMapID);
        if (PlayerDataModule.Instance.data.structureLockProgressDataList.Find(x => x.buildType == BuildingType.YuShaHu_1) == null)
        {
            PlayerDataModule.Instance.data.structureLockProgressDataList.Add(progress1);
            PlayerDataModule.Instance.data.structLockDataDic[1].Remove(BuildingType.YuShaHu_1);
            PlayerDataModule.Instance.data.structCanUnLockDataDic[1].Add(BuildingType.YuShaHu_1);
            DataController.Instance.UpdateStructureLockInfo();
        }
        Hide();
    }
    private void TriggerGuide_2()
    {
        //建造一号灵茶架
        Transform collectPoint = GameController.Instance.buildings[BuildingType.LingChaJia_1].transform;
        // GuideManager.Instance.StartStep(GuideStep.BuildTeaStand, collectPoint);
         EventCenter.Instance.TriggerEvent(EventMessages.ShowGuideFinger, new Vector2(collectPoint.position.x, collectPoint.position.y ));

        StructureLockData data1 = DataController.Instance.structureLockDataList_1.Find(x => x.buildingType == BuildingType.LingChaJia_1);
        StructureLockProgressData progress1 = new StructureLockProgressData(BuildingType.LingChaJia_1,
        data1.needMoney, data1.lockId, PlayerDataModule.Instance.data.currentMapID);

        if (PlayerDataModule.Instance.data.structureLockProgressDataList.Find(x => x.buildType == BuildingType.LingChaJia_1) == null)
        {
            PlayerDataModule.Instance.data.structureLockProgressDataList.Add(progress1);
            PlayerDataModule.Instance.data.structLockDataDic[1].Remove(BuildingType.LingChaJia_1);
            PlayerDataModule.Instance.data.structCanUnLockDataDic[1].Add(BuildingType.LingChaJia_1);
            DataController.Instance.UpdateStructureLockInfo();
        }
        Hide();
    }
    private void TriggerGuide_3()
    {
        //收集霜云芝
        Transform collectPoint = GameController.Instance.factoryControllers[Module.Data.MonsterType.ShuangYunZhi].transform;
        //  GuideManager.Instance.StartStep(GuideStep.CollectMaterial, collectPoint);
         EventCenter.Instance.TriggerEvent(EventMessages.ShowGuideFinger, new Vector2(collectPoint.position.x, collectPoint.position.y ));
        Hide();
    }
    private void TriggerGuide_4()
    {
        //运送霜云芝
        Transform collectPoint = GameController.Instance.buildings[BuildingType.YuShaHu_1].transform;
        //  GuideManager.Instance.StartStep(GuideStep.DeliverMaterial, collectPoint);
         EventCenter.Instance.TriggerEvent(EventMessages.ShowGuideFinger, new Vector2(collectPoint.position.x, collectPoint.position.y ));
        Hide();
    }
    private void TriggerGuide_5()
    {
        //建造灵账台
        Transform collectPoint = GameController.Instance.buildings[BuildingType.LingZhangTai].transform;
         EventCenter.Instance.TriggerEvent(EventMessages.ShowGuideFinger, new Vector2(collectPoint.position.x, collectPoint.position.y - 2 ));
        // GuideManager.Instance.StartStep(GuideStep.BuildAccountDesk, collectPoint);
        StructureLockData data1 = DataController.Instance.structureLockDataList_1.Find(x => x.buildingType == BuildingType.LingZhangTai);
        StructureLockProgressData progress1 = new StructureLockProgressData(BuildingType.LingZhangTai,
        data1.needMoney, data1.lockId, PlayerDataModule.Instance.data.currentMapID);
        if (PlayerDataModule.Instance.data.structureLockProgressDataList.Find(x => x.buildType == BuildingType.LingZhangTai) == null)
        {
            PlayerDataModule.Instance.data.structureLockProgressDataList.Add(progress1);
            PlayerDataModule.Instance.data.structLockDataDic[1].Remove(BuildingType.LingZhangTai);
            PlayerDataModule.Instance.data.structCanUnLockDataDic[1].Add(BuildingType.LingZhangTai);
            DataController.Instance.UpdateStructureLockInfo();
            if (PlayerDataModule.Instance.data.cashierData == null)
            {
                PlayerDataModule.Instance.data.cashierData = new CashierData();
            }
        }
        Hide();
    }
    private void TriggerGuide_6()
    {
        //取灵茶
        Transform collectPoint = GameController.Instance.buildings[BuildingType.YuShaHu_1].transform;
         EventCenter.Instance.TriggerEvent(EventMessages.ShowGuideFinger, new Vector2(collectPoint.position.x, collectPoint.position.y ));
        // GuideManager.Instance.StartStep(GuideStep.TakeTea, collectPoint);
        if (GameController.Instance.buildings[BuildingType.YuShaHu_1].GetComponent<ProductionStation>().currentMaterialCount == 0)
        {
            GameController.Instance.buildings[BuildingType.YuShaHu_1].GetComponent<ProductionStation>().AddMaterial(3);
        }

        Hide();
    }
    private void TriggerGuide_7()
    {
        //上架灵茶
        Transform collectPoint = GameController.Instance.buildings[BuildingType.LingChaJia_1].transform;
         EventCenter.Instance.TriggerEvent(EventMessages.ShowGuideFinger, new Vector2(collectPoint.position.x, collectPoint.position.y ));
        //GuideManager.Instance.StartStep(GuideStep.SellTea, collectPoint);
           if (GameController.Instance.buildings[BuildingType.YuShaHu_1].GetComponent<ProductionStation>().currentMaterialCount == 0)
        {
            GameController.Instance.buildings[BuildingType.YuShaHu_1].GetComponent<ProductionStation>().AddMaterial(3);
        }

        Hide();
    }

    // private void TriggerGuide_8()
    // {
    //     //进行结账
    //     Transform collectPoint = GameController.Instance.buildings[BuildingType.LingZhangTai].transform;
    //     // GuideManager.Instance.StartStep(GuideStep.Checkout, collectPoint);
    //     Hide();
    // }
    private void TriggerGuide_9()
    {
        //升级一号玉砂壶
        Transform collectPoint = GameController.Instance.buildings[BuildingType.YuShaHu_1].transform;
         EventCenter.Instance.TriggerEvent(EventMessages.ShowGuideFinger, new Vector2(collectPoint.position.x, collectPoint.position.y ));
        // GuideManager.Instance.StartStep(GuideStep.UpgradePot, collectPoint);
        PlayerDataModule.Instance.data.guideStep = GuideStep.UpgradePot;
        Hide();
    }

    public void HideContent(string res, string info)
    {
        bg.gameObject.SetActive(false);
    }
}
