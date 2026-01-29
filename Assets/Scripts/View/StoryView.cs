using System;
using System.Collections;
using System.Collections.Generic;
using Controller;
using Module;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Utils;

public class StoryView : BaseView
{
    public UIButton btn;
    public TextMeshProUGUI infotxt;
    public string[] str = new string[]{"\u3000\u3000上古之时，天地灵脉贯通，灵植漫山、矿石蕴聚奇光。彼时灵茶坊、炼器铺遍布街巷，连寻常百姓都能寻得灵材，灵艺技艺代代相传，一派繁荣祥和。然而灵族为了争夺灵脉核心，引入了妖力并使其扩散，导致无数灵植、矿石受到妖力影响，化形四处巡游，甚至变异为具有攻击性的巨型灵体。灵材的异变让灵艺传承陷入绝境，灵植与矿石愈发难寻难取，导致世间灵茶、灵器的制作技艺几近失传。",
        "\u3000\u3000就在灵艺即将断绝之际，一柄沉睡千年的镇妖剑苏醒。此剑天生克制妖力，能驱散灵植、矿石体内的妖气，使其恢复纯净本质。千百年后，你被镇妖剑选中，成为新一代镇妖剑主人。身负“驱散妖力、复苏灵艺”的使命，踏入这片灵材难寻、妖力弥漫的妖剑世界。",
        "\u3000\u3000你将以镇妖剑为引，终结灵族纷争遗留的浩劫，让纯净灵材重现世间；以复苏的灵艺为基，滋养天地灵韵、唤醒世间生机，最终成为既手持长剑守护一方安宁的镇妖者，又以灵艺兴盛带动万灵安居的传奇经营者，让妖剑世界重归繁盛祥和。"};

    private Coroutine typingCoroutine;
    private bool isSkipping = false;
    private bool showAll = false;
    private float typingSpeed = 0.1f;
    public VerticalLayoutGroup verticalLayoutGroup;
    public GameObject fillContent;
    public Image fillImg;
    public GameObject Content;
    public override void UpdateViewWithArgs(params object[] args)
    {
        base.UpdateViewWithArgs(args);
        Content.SetActive(true);
        fillContent.SetActive(false);
        ShowText(str[currentIndex]);
    }
    protected override void AddEventListener()
    {
        base.AddEventListener();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(OnSkipClicked);
    }



    public void ShowText(string text)
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeEffect(text));
    }

    private IEnumerator TypeEffect(string text)
    {

        infotxt.text = text;
        yield return null;
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(
            verticalLayoutGroup.GetComponent<RectTransform>()
        );
        infotxt.maxVisibleCharacters = 0;
        showAll = false;
        for (int i = 0; i <= text.Length; i++)
        {
            if (isSkipping)
                break;
            infotxt.maxVisibleCharacters = i;
            yield return new WaitForSeconds(typingSpeed);
        }
        infotxt.maxVisibleCharacters = text.Length;
        showAll = true;
        isSkipping = false;
    }
    int currentIndex = 0;
    private void OnSkipClicked()
    {
        if (!showAll)
        {
            isSkipping = true;
        }
        else
        {
            currentIndex++;
            if (currentIndex < str.Length)
            {
                ShowText(str[currentIndex]);
            }
            else
            {
                NextContent();
            }
        }
    }

    private void NextContent()
    {
        if (PlayerDataModule.Instance.data.guidIdList.Contains(0))
            return;
        fillContent.SetActive(true);
        Content.SetActive(false);
        PlayerDataModule.Instance.data.guidIdList.Add(0);
        StartCoroutine(LoadNextSceneCoroutine());
    }

    #region 🔸 加载场景逻辑

    private IEnumerator LoadNextSceneCoroutine()
    {

        SceneManager.sceneLoaded += OnSceneLoaded;
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync($"Game_{PlayerDataModule.Instance.data.currentMapID}");
        asyncLoad.allowSceneActivation = false;
        float displayProgress = 0f;
        fillImg.fillAmount = 0f;
        while (!asyncLoad.isDone)
        {
            float targetProgress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
            displayProgress = Mathf.MoveTowards(displayProgress, targetProgress, Time.deltaTime * 0.3f);
            if (displayProgress >= 1f)
            {
                yield return new WaitForSeconds(0.5f);
                asyncLoad.allowSceneActivation = true;
                  fillImg.fillAmount = 1f;
            }
            fillImg.fillAmount = displayProgress;
            yield return null;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == $"Game_{PlayerDataModule.Instance.data.currentMapID}")
        {
            if (!PlayerDataModule.Instance.data.guidIdList.Contains(1))
            {
                UIController.Instance.Show<PlayerGuide>();
            }
            EventCenter.Instance.TriggerEvent(EventMessages.DataPrepared);
            EventCenter.Instance.TriggerEvent(EventMessages.MapDataPrepared);
            EventCenter.Instance.TriggerEvent(EventMessages.MapTaskDataPrepared);
            EventCenter.Instance.TriggerEvent(EventMessages.UpdatePlayerEquimentInfo);
            EventCenter.Instance.TriggerEvent(EventMessages.UpdatePlayerValueInfo);
            EventCenter.Instance.TriggerEvent(EventMessages.CustomerBeginCreate);
            EventCenter.Instance.TriggerEvent(EventMessages.MonsterBeginCreate);
            DataController.Instance.InitMapLock();
            DataController.Instance.UpdateStructureLockInfo();
            EventCenter.Instance.TriggerEvent(EventMessages.UpdateTaskMainView);
        }
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    #endregion

}
