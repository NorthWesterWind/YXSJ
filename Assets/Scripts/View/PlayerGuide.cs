using System.Collections;
using Controller;
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

    public string[] info = new string[]
    {
      "\u3000\u3000新主人，终于等到你啦！我是镇妖剑的剑灵小灵，你被我选中成为新一代镇妖剑主人！",
      "\u3000\u3000镇妖剑？新主人…我该怎么做？",
      "\u3000\u3000主人，别慌呀，有我在！现在我们先去建造一号玉砂壶。",
      "\u3000\u3000太棒啦！接下来我们建一号灵茶售卖架——你还记得吗？以前街上的灵茶坊都摆满了这种架子，可以用来售卖灵茶获得多多的银钱哦！",
      "\u3000\u3000接下来，我们就要进入获取灵材区域。前面不远处就是采集霜云芝的区域了，只要拿起“我”攻击它们，就能自动散掉妖气，把灵材变纯净，就可以轻易获取了。要是遇到那种比人还高的巨型灵体也别怕，“我”也能驱散它的妖气获取它哦！",
      "\u3000\u3000好了，我们快把霜云芝送到一号玉砂壶凝炼吧！",
      "\u3000\u3000小灵，接下来应该怎么做？",
      "\u3000\u3000制作灵茶需要时间，我们先去建造灵账台吧！",
      "\u3000\u3000主人你看！灵账使姐姐超靠谱的！以后你去远处采集灵材，店里的灵茶售卖、银钱结算她都能打理。",
      "\u3000\u3000有客人来啦，我们快回去一号玉砂壶取灵茶，然后将灵茶送到一号灵茶售卖架上售卖吧！",
      "\u3000\u3000我们快把云芝茶送到一号灵茶售卖架上售卖吧！",
      "\u3000\u3000移动到灵账台，尝试一下给客人结账吧！",
      "\u3000\u3000好耶好耶！主人你看灵账台里的银钱——这可是咱们靠纯净灵茶赚的第一笔钱！",
      "\u3000\u3000不过主人，就靠现在这壶煮的灵茶，收益还不够呀——你想，以前的灵茶都是用高阶玉砂壶煮的，茶香能飘三条街！咱们升级这壶，能让灵茶的灵气更足、口感更好，自然能卖更高的价钱，攒钱更快，才能做更多复苏灵艺的事呀！",
      "\u3000\u3000我们去升级玉砂壶，用更高的价格卖出灵茶吧！",
      "\u3000\u3000好了，基本的操作主人你已经学会了！接下来呀，就要靠主人自己去探索这片妖力弥漫的妖剑世界，一步步靠近“驱散妖力、复苏灵艺”的使命啦！",
    };

    private Coroutine typingCoroutine;
    private bool isSkipping = false;
    private bool showAll = false;
    private int currentIndex = 0;



    public override void UpdateViewWithArgs(params object[] args)
    {
        base.UpdateViewWithArgs(args);
        currentIndex = (int)PlayerDataModule.Instance.data.guideStep;
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
                infotxt_1.maxVisibleCharacters = 0;
                jianling.AnimationState.SetAnimation(0, "idle", true);
                break;
            case 2:
                infoContent_2.gameObject.SetActive(true);
                infoContent_1.gameObject.SetActive(false);
                infotxt_2.text = info[1];
                infotxt_2.maxVisibleCharacters = 0;
                character.AnimationState.SetAnimation(0, "待机", true);
                break;
            case 3:
                infoContent_1.gameObject.SetActive(true);
                infoContent_2.gameObject.SetActive(false);
                infotxt_1.text = info[2];
                infotxt_1.maxVisibleCharacters = 0;
                jianling.AnimationState.SetAnimation(0, "idle", true);
                break;
            case 4:
                infoContent_1.gameObject.SetActive(true);
                infoContent_2.gameObject.SetActive(false);
                infotxt_1.text = info[3];
                infotxt_1.maxVisibleCharacters = 0;
                jianling.AnimationState.SetAnimation(0, "idle", true);
                break;
            case 5:
                infoContent_1.gameObject.SetActive(true);
                infoContent_2.gameObject.SetActive(false);
                infotxt_1.text = info[4];
                infotxt_1.maxVisibleCharacters = 0;
                jianling.AnimationState.SetAnimation(0, "idle", true);
                break;
            case 6:
                infoContent_1.gameObject.SetActive(true);
                infoContent_2.gameObject.SetActive(false);
                infotxt_1.text = info[5];
                infotxt_1.maxVisibleCharacters = 0;
                jianling.AnimationState.SetAnimation(0, "idle", true);
                break;
            case 7:
                infoContent_2.gameObject.SetActive(true);
                infoContent_1.gameObject.SetActive(false);
                infotxt_2.text = info[6];
                infotxt_2.maxVisibleCharacters = 0;
                character.AnimationState.SetAnimation(0, "待机", true);
                break;
            case 8:
                infoContent_1.gameObject.SetActive(true);
                infoContent_2.gameObject.SetActive(false);
                infotxt_1.text = info[7];
                infotxt_1.maxVisibleCharacters = 0;
                jianling.AnimationState.SetAnimation(0, "idle", true);
                break;
            case 9:
                infoContent_1.gameObject.SetActive(true);
                infoContent_2.gameObject.SetActive(false);
                infotxt_1.text = info[8];
                infotxt_1.maxVisibleCharacters = 0;
                jianling.AnimationState.SetAnimation(0, "idle", true);
                break;
            case 10:
                infoContent_1.gameObject.SetActive(true);
                infoContent_2.gameObject.SetActive(false);
                infotxt_1.text = info[9];
                infotxt_1.maxVisibleCharacters = 0;
                jianling.AnimationState.SetAnimation(0, "idle", true);
                break;
            case 11:
                infoContent_1.gameObject.SetActive(true);
                infoContent_2.gameObject.SetActive(false);
                infotxt_1.text = info[10];
                infotxt_1.maxVisibleCharacters = 0;
                jianling.AnimationState.SetAnimation(0, "idle", true);
                break;
            case 12:
                infoContent_1.gameObject.SetActive(true);
                infoContent_2.gameObject.SetActive(false);
                infotxt_1.text = info[11];
                infotxt_1.maxVisibleCharacters = 0;
                jianling.AnimationState.SetAnimation(0, "idle", true);
                break;
            case 13:
                infoContent_1.gameObject.SetActive(true);
                infoContent_2.gameObject.SetActive(false);
                infotxt_1.text = info[12];
                infotxt_1.maxVisibleCharacters = 0;
                jianling.AnimationState.SetAnimation(0, "idle", true);
                break;
            case 14:
                infoContent_1.gameObject.SetActive(true);
                infoContent_2.gameObject.SetActive(false);
                infotxt_1.text = info[13];
                infotxt_1.maxVisibleCharacters = 0;
                jianling.AnimationState.SetAnimation(0, "idle", true);
                break;
            case 15:
                infoContent_1.gameObject.SetActive(true);
                infoContent_2.gameObject.SetActive(false);
                infotxt_1.text = info[14];
                infotxt_1.maxVisibleCharacters = 0;
                jianling.AnimationState.SetAnimation(0, "idle", true);
                break;
            case 16:
                infoContent_1.gameObject.SetActive(true);
                infoContent_2.gameObject.SetActive(false);
                infotxt_1.text = info[15];
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
            currentIndex++;
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
                        TriggerGuide_8();

                        break;
                    case 15:
                        TriggerGuide_9();

                        break;
                }

                ShowText(info[currentIndex - 1]);
            }
            else
            {
                NextContent();
            }
        }
    }

    private void NextContent()
    {
        EventCenter.Instance.TriggerEvent(EventMessages.HidePlayerGuide);
        Hide();
    }


    private void TriggerGuide_1()
    {
        //建造一号玉砂壶
        Transform collectPoint = GameController.Instance.buildings[BuildingType.YuShaHu_1].transform;
        GuideManager.Instance.StartStep(GuideStep.BuildYushaPot, collectPoint);
        HideContent("YuShaHu_1", "建造一号玉砂壶。");
    }
    private void TriggerGuide_2()
    {
        //建造一号灵茶架
        Transform collectPoint = GameController.Instance.buildings[BuildingType.LingChaJia_1].transform;
        GuideManager.Instance.StartStep(GuideStep.BuildTeaStand, collectPoint);
        HideContent("LingChaJia", "建造一号灵茶架。");
    }
    private void TriggerGuide_3()
    {
        //收集霜云芝
        Transform collectPoint = GameController.Instance.factoryControllers[Module.Data.MonsterType.ShuangYunZhi].transform;
        GuideManager.Instance.StartStep(GuideStep.CollectMaterial, collectPoint);
        HideContent("ShuangYunZhi", "收集3个霜云芝。");
    }
    private void TriggerGuide_4()
    {
        //运送霜云芝
        Transform collectPoint = GameController.Instance.buildings[BuildingType.YuShaHu_1].transform;
        GuideManager.Instance.StartStep(GuideStep.DeliverMaterial, collectPoint);
        HideContent("YuShaHu_1", "将霜云芝放入一号玉砂壶。");
    }
    private void TriggerGuide_5()
    {
        //建造灵账台
        Transform collectPoint = GameController.Instance.buildings[BuildingType.LingZhangTai].transform;
        GuideManager.Instance.StartStep(GuideStep.BuildAccountDesk, collectPoint);
        HideContent("LingZhangTai", "建造灵账台。");
    }
    private void TriggerGuide_6()
    {
        //取灵茶
        Transform collectPoint = GameController.Instance.buildings[BuildingType.YuShaHu_1].transform;
        GuideManager.Instance.StartStep(GuideStep.TakeTea, collectPoint);
        HideContent("ShuangYunCha", "取得霜云茶。");
    }
    private void TriggerGuide_7()
    {
        //上架灵茶
        Transform collectPoint = GameController.Instance.buildings[BuildingType.LingChaJia_1].transform;
        GuideManager.Instance.StartStep(GuideStep.TakeTea, collectPoint);
        HideContent("LingChaJia_1", "上架霜云茶。");
    }

    private void TriggerGuide_8()
    {
        //进行结账
        Transform collectPoint = GameController.Instance.buildings[BuildingType.LingZhangTai].transform;
        GuideManager.Instance.StartStep(GuideStep.TakeTea, collectPoint);
        HideContent("LingZhangTai", "前往收银台结账。");
    }
    private void TriggerGuide_9()
    {
        //升级一号玉砂壶
        Transform collectPoint = GameController.Instance.buildings[BuildingType.YuShaHu_1].transform;
        GuideManager.Instance.StartStep(GuideStep.TakeTea, collectPoint);
        HideContent("YuShaHu_1", "升级一号玉砂壶。");
    }

    public void HideContent(string res, string info)
    {
        bg.gameObject.SetActive(false);
    }
}
