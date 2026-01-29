using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Controller;
using Module;
using Module.Data;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Utils;
using View;
using World.Controller;

public class LoginView : BaseView
{
    public UIButton ageBtn;

    public GameObject fillBg;
    public Image fillImage;

    public GameObject loginContent;
    public TMP_InputField accountInput;
    public TMP_InputField passwordInput;
    public UIButton beginRegisterBtn;
    public UIButton loginBtn;

    public GameObject registerContent;
    public TMP_InputField registerAccountInput;
    public TMP_InputField registerPasswordInput;
    public UIButton registerReturnBtn;
    public UIButton registerBtn;

    public GameObject realNameContent;
    public TMP_InputField realNameInput;
    public TMP_InputField realAccountInput;
    public UIButton realNameReturnBtn;
    public UIButton realNameBtn;

    public GameObject setNameContent;
    public TMP_InputField setNameInput;
    public UIButton setNameBtn;
    public UIButton ZhuXiaoBtn;
    protected override void Awake()
    {
        base.Awake();
        StartCoroutine(LoadSensitiveWordsCoroutine());
    }
    public override void UpdateViewWithArgs(params object[] args)
    {
        base.UpdateViewWithArgs(args);
        accountInput.text = "";
        passwordInput.text = "";
        registerAccountInput.text = "";
        registerPasswordInput.text = "";
        realNameInput.text = "";
        realAccountInput.text = "";
        setNameInput.text = "";
        HideAllPanels();
        SwitchToLoginPanel();
        Debug.Log($"Persistent Path: {Application.persistentDataPath}");
    }


    public void HideAllPanels()
    {
        loginContent.SetActive(false);
        registerContent.SetActive(false);
        realNameContent.SetActive(false);
        setNameContent.SetActive(false);

        fillBg.gameObject.SetActive(false);
    }
    public void SwitchToLoginPanel()
    {
        HideAllPanels();
        loginContent.SetActive(true);
        accountInput.text = "";
        passwordInput.text = "";
    }
    public void SwitchToRegisterPanel()
    {
        HideAllPanels();
        registerContent.SetActive(true);
        registerAccountInput.text = "";
        registerPasswordInput.text = "";
    }
    public void SwitchToRealNamePanel()
    {
        HideAllPanels();
        realNameContent.SetActive(true);
        realNameInput.text = "";
        realAccountInput.text = "";
    }
    public void SwitchToSetNamePanel()
    {
        HideAllPanels();
        setNameContent.SetActive(true);
        setNameInput.text = "";
    }

    protected override void AddEventListener()
    {
        base.AddEventListener();

        accountInput.onValidateInput += ValidateAlphaNumeric;
        accountInput.onValueChanged.AddListener(OnAccountValueChanged);
        registerAccountInput.onValueChanged.AddListener(OnRegisterAccountValueChanged);
        passwordInput.onValidateInput += ValidateAlphaNumeric;
        passwordInput.onValueChanged.AddListener(OnPasswordValueChanged);
        registerPasswordInput.onValidateInput += ValidateAlphaNumeric;
        registerPasswordInput.onValueChanged.AddListener(OnPassword2ValueChanged);
        setNameInput.onValueChanged.AddListener(OnCreateNameValueChanged);
        realAccountInput.onValueChanged.AddListener(OnNumberValueChanged);

        loginBtn.onClick.RemoveAllListeners();
        loginBtn.onClick.AddListener(LoginEvent);

        beginRegisterBtn.onClick.RemoveAllListeners();
        beginRegisterBtn.onClick.AddListener(SwitchToRegisterPanel);

        registerReturnBtn.onClick.RemoveAllListeners();
        registerReturnBtn.onClick.AddListener(SwitchToLoginPanel);
        registerBtn.onClick.RemoveAllListeners();
        registerBtn.onClick.AddListener(OnRegister);

        realNameReturnBtn.onClick.RemoveAllListeners();
        realNameReturnBtn.onClick.AddListener(SwitchToLoginPanel);
        realNameBtn.onClick.RemoveAllListeners();
        realNameBtn.onClick.AddListener(OnRealName);

        setNameBtn.onClick.RemoveAllListeners();
        setNameBtn.onClick.AddListener(OnSetName);

        ageBtn.onClick.RemoveAllListeners();
        ageBtn.onClick.AddListener(OnAge);

    }
    public void OnAge()
    {
        if (fillBg.activeSelf)
        {
            return;
        }

        UIController.Instance.Show<AttentionView>(
            "\u3000\u30001、本游戏是一款以模拟经营为背景的休闲模拟类手机网络游戏，适用于年满8周岁及以上的用户，建议未成年人在家长监护下使用游戏产品。\n\r\u3000\u30002、本游戏以模拟经营题材，核心玩法包含材料收集、建筑升级、角色养成、商品售卖及资源管理，玩家可通过策略进行模拟经营，激励玩家用心钻研和挑战自我。\n\r\u3000\u30003、根据国家新闻出版署《关于防止未成年人沉迷网络游戏的通知》及《关于进一步严格管理 切实防止未成年人沉迷网络游戏的通知》，本游戏已设置实名认证系统和防沉迷系统，并接入国家实名认证系统和防沉迷系统。游戏中部分道具需要付费，规范向未成年人提供付费服务：本游戏不会为未满8周岁的用户提供游戏充值服务；满8周岁未满16周岁的用户，单次充值金额不得超过50元人民币，每月充值金额累计不得超过200元人民币；满16周岁未满18周岁的用户，单次充值金额不得超过100元人民币，每月充值金额累计不得超过400元人民币。\n\r\u3000\u30004、本游戏为模拟经营为主题的休闲模拟类游戏。在游戏中，玩家化身在妖剑世界的一名传奇经营者，提供角色培养、材料收集、资源搭配的过程，有助于玩家日常放松。游戏玩法简单，强化应变决策力，提供放松体验，任务奖励增强玩家自信心与目标感。",
            "适龄提示");
    }

    private void LoginEvent()
    {
        if (string.IsNullOrEmpty(accountInput.text) || string.IsNullOrEmpty(passwordInput.text))
        {
            UIController.Instance.Show<TipView>("账号或密码不能为空!");
            return;
        }

        if (accountInput.text.Length < 4 || accountInput.text.Length > 8)
        {
            UIController.Instance.Show<TipView>("账号长度应为4到8!");
            return;
        }
        if (!IsTextValid(accountInput.text))
        {
            UIController.Instance.Show<TipView>("账号包含敏感词!");
            return;
        }

        PlayerDataModule.Instance.Login(accountInput.text, passwordInput.text, OnLogin);
    }

    private void OnLogin(int fcm)
    {
        if (fcm <= 0)
        {
            HideAllPanels();
            SwitchToRealNamePanel();
            return;
        }

        if (!PlayerDataModule.Instance.data.isCreated)
        {
            HideAllPanels();
            SwitchToSetNamePanel();
        }
        else
            OnCanLogin();
    }

    private void OnCanLogin()
    {
        HideAllPanels();
        StartCoroutine(No18LoadGame());
    }

    private void RealLogin()
    {
        HideAllPanels();
        fillBg.gameObject.SetActive(true);
        fillImage.fillAmount = 0f;
        StartCoroutine(LoadNextSceneCoroutine());
    }

    #region 🔸 加载场景逻辑

    private IEnumerator LoadNextSceneCoroutine()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        AsyncOperation asyncLoad;
        if (!PlayerDataModule.Instance.data.guidIdList.Contains(0))
        {
            asyncLoad = SceneManager.LoadSceneAsync("StoryGuide");

        }
        else
        {
            asyncLoad = SceneManager.LoadSceneAsync($"Game_{PlayerDataModule.Instance.data.currentMapID}");
        }
        asyncLoad.allowSceneActivation = false;
        float displayProgress = 0f;

        while (!asyncLoad.isDone)
        {
            float targetProgress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
            displayProgress = Mathf.MoveTowards(displayProgress, targetProgress, Time.deltaTime * 0.3f);
            fillImage.fillAmount = displayProgress;

            if (asyncLoad.progress >= 0.9f && displayProgress >= 0.99f)
            {
                displayProgress = Mathf.MoveTowards(displayProgress, 1f, Time.deltaTime * 0.3f);
                fillImage.fillAmount = displayProgress;

                if (displayProgress >= 1f)
                {
                    yield return new WaitForSeconds(0.5f);
                    asyncLoad.allowSceneActivation = true;
                }
            }

            yield return null;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayerDataModule.Instance.BeginAutoSave();
        if (!PlayerDataModule.Instance.data.guidIdList.Contains(0))
        {
            if (scene.name == "StoryGuide")
            {
                UIController.Instance.Show<StoryView>();
            }

        }
        else
        {
            if (scene.name == $"Game_{PlayerDataModule.Instance.data.currentMapID}")
            {
                if (PlayerDataModule.Instance.data.currentMapID == 1 && PlayerDataModule.Instance.data.guideStep != GuideStep.Over)
                {
                    //剧情引导
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
        }
        AudioSourceController.Instance.PlaySound();
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    #endregion




    private void OnRegister()
    {
        string str1 = registerAccountInput.text;
        string str2 = registerPasswordInput.text;
        if (string.IsNullOrEmpty(str1) || string.IsNullOrEmpty(str2))
        {
            UIController.Instance.Show<TipView>("账号或密码不能为空!");
            return;
        }

        if (registerAccountInput.text.Length < 4 || registerPasswordInput.text.Length > 8)
        {
            UIController.Instance.Show<TipView>("账号长度应为4到8!");
            return;
        }

        if (!IsTextValid(registerAccountInput.text))
        {
            UIController.Instance.Show<TipView>("账号包含敏感词!");
            return;
        }

        if (registerPasswordInput.text.Length < 4)
        {
            UIController.Instance.Show<TipView>("密码长度不能少于4位!");
            return;
        }

        PlayerDataModule.Instance.Register(str1, str2,
            OnRegisterSuccess, OnRegisterFail);
    }

    private void OnRegisterSuccess()
    {
        UIController.Instance.Show<TipView>("注册成功!");
        SwitchToLoginPanel();
    }

    private void OnRegisterFail(string msg)
    {
        UIController.Instance.Show<TipView>(msg);
    }

    private void OnRealName()
    {

        if (string.IsNullOrEmpty(realNameInput.text) || string.IsNullOrEmpty(realAccountInput.text))
        {
            UIController.Instance.Show<TipView>("姓名或身份证号不能为空!");
            return;
        }
        if (realAccountInput.text.Length < 18 || realAccountInput.text.Length > 18)
        {
            UIController.Instance.Show<TipView>("请输入18位有效身份证号数字！");
            return;
        }

        PlayerDataModule.Instance.RealName(realAccountInput.text, realNameInput.text,
           "0",
           response =>
           {
               switch (response.state)
               {
                   case 1:
                       UIController.Instance.Show<TipView>(response.msg);
                       int age = response.age;
                       PlayerDataModule.Instance.data.age = age;
                       PlayerDataModule.Instance.SavePlayerDataAsync();
                       PlayerDataModule.Instance.SavePlayerDataToSever();

                       if (!PlayerDataModule.Instance.data.isCreated)
                       {
                           HideAllPanels();
                           SwitchToSetNamePanel();
                       }
                       else
                           OnCanLogin();
                       break;
                   case 3:
                       UIController.Instance.Show<TipView>("实名失败输入18位身份证号数字！");
                       break;
                   case 4:
                       UIController.Instance.Show<TipView>("用户不存在！");
                       break;
                   default:
                       UIController.Instance.Show<TipView>("实名失败！");
                       return;
               }
           });
    }

    private void OnSetName()
    {

        string name = setNameInput.text;
        if (string.IsNullOrEmpty(name))
        {
            UIController.Instance.Show<TipView>("昵称不能为空!");
            return;
        }

        if (!Extensions.IsAllChinese(name))
        {
            UIController.Instance.Show<TipView>("昵称只允许输入中文!");
            return;
        }

        if (name.Length > 6)
        {
            UIController.Instance.Show<TipView>("超出昵称长度限制6!");
            return;
        }

        if (!IsTextValid(name))
        {
            UIController.Instance.Show<TipView>("昵称包含敏感词!");
            return;
        }

        HideAllPanels();
        PlayerData playerData = PlayerDataModule.Instance.data;
        playerData.userName = name;
        playerData.isCreated = true;
        PlayerDataModule.Instance.SavePlayerDataAsync();
        PlayerDataModule.Instance.SavePlayerDataToSever();
        StartCoroutine(No18LoadGame());


    }



    private char ValidateAlphaNumeric(string text, int charIndex, char addedChar)
    {
        return char.IsLetterOrDigit(addedChar) ? addedChar : '\0';
    }



    private void OnRegisterAccountValueChanged(string text)
    {
        if (Input.compositionString.Length > 0)
            return; // 等输入法结束

        string filtered = Regex.Replace(text, @"[^a-zA-Z0-9]", "");

        if (filtered != text)
        {
            registerAccountInput.text = filtered;
            registerAccountInput.caretPosition = filtered.Length; // 保持光标在最后
        }
    }

    private void OnAccountValueChanged(string text)
    {
        if (Input.compositionString.Length > 0)
            return; // 等输入法结束

        string filtered = Regex.Replace(text, @"[^a-zA-Z0-9]", "");

        if (filtered != text)
        {
            accountInput.text = filtered;
            accountInput.caretPosition = filtered.Length;
        }
    }


    private void OnPasswordValueChanged(string text)
    {
        if (Input.compositionString.Length > 0)
            return; // 等输入法结束

        string filtered = Regex.Replace(text, @"[^a-zA-Z0-9]", "");

        if (filtered != text)
        {
            passwordInput.text = filtered;
            passwordInput.caretPosition = filtered.Length; // 保持光标在最后
        }
    }

    // 工具函数：去除所有非字母数字字符
    private string RemoveNonAlphaNumeric(string input)
    {
        return Regex.Replace(input, @"[^a-zA-Z0-9]", "");
    }

    private void OnPassword2ValueChanged(string text)
    {
        if (Input.compositionString.Length > 0)
            return; // 等输入法结束

        string filtered = Regex.Replace(text, @"[^a-zA-Z0-9]", "");

        if (filtered != text)
        {
            registerPasswordInput.text = filtered;
            registerPasswordInput.caretPosition = filtered.Length; // 保持光标在最后
        }
    }

    private void OnCreateNameValueChanged(string text)
    {
        var filtered = new System.Text.StringBuilder();
        // 如果被改动了，就更新输入框
        if (filtered.ToString() != text)
        {
            filtered.Append(text);
            setNameInput.text = filtered.ToString();
        }
    }
    private void OnNumberValueChanged(string text)
    {
        // 用 StringBuilder 过滤出数字
        var filtered = new System.Text.StringBuilder();
        if (Input.compositionString.Length > 0)
        {
            filtered.Clear();
            return;
        }

        foreach (char c in text)
        {
            // 只允许英文(a-zA-Z)或数字(0-9)，且长度限制为10
            if ((c >= 'a' && c <= 'z') ||
                (c >= 'A' && c <= 'Z') ||
                (c >= '0' && c <= '9'))
            {
                filtered.Append(c);
            }
        }

        // 如果被改动了，就更新输入框
        if (filtered.ToString() != text)
        {
            realAccountInput.text = filtered.ToString();
        }
    }




    private List<string> sensitiveWordsFilePaths = new List<string>
        {
            "SensitiveWords.txt",
        };

    private List<string> sensitiveWords = new List<string>();
    private bool isInitialized = false;



    private IEnumerator LoadSensitiveWordsCoroutine()
    {
        sensitiveWords.Clear();
        isInitialized = false;

        Debug.Log("开始异步加载敏感词文件...");

        // 遍历文件路径列表，加载每一个文件
        foreach (string filePath in sensitiveWordsFilePaths)
        {
            string fullPath = Path.Combine(Application.streamingAssetsPath, filePath);

            // 【关键修改】使用 UnityWebRequest 替代 File.IO
            UnityWebRequest www = UnityWebRequest.Get(fullPath);

            // 发送请求并等待其完成
            yield return www.SendWebRequest();

            // 检查请求结果
            if (www.result == UnityWebRequest.Result.Success)
            {
                // 请求成功，获取文件内容
                string fileContent = www.downloadHandler.text;

                // 【关键修改】从返回的文本中按行分割，模拟 ReadAllLines
                string[] words = fileContent.Split(new[] { "\r\n", "\r", "\n" },
                    System.StringSplitOptions.RemoveEmptyEntries);

                int countBefore = sensitiveWords.Count;
                foreach (string word in words)
                {
                    string trimmedWord = word.Trim();
                    if (!string.IsNullOrEmpty(trimmedWord))
                    {
                        sensitiveWords.Add(trimmedWord.ToLower());
                    }
                }

                Debug.Log($"成功加载文件 '{filePath}'，新增 {sensitiveWords.Count - countBefore} 个敏感词。");
            }
            else
            {
                // 如果请求失败，打印错误
                Debug.LogWarning($"加载敏感词文件失败: {fullPath} | 错误: {www.error}");
            }
        }

        // 所有文件加载完成后，设置初始化标志
        isInitialized = true;
        Debug.Log($"成功加载所有敏感词，总数量: {sensitiveWords.Count}");
    }

    /// <summary>
    /// 检测输入文本是否合法（返回 true = 合法）
    /// </summary>
    /// <summary>
    /// 检查文本是否合法（true = 合法）
    /// </summary>
    public bool IsTextValid(string input)
    {
        if (!isInitialized)
        {
            Debug.LogWarning("敏感词检测器未初始化完成");
            return true;
        }

        if (string.IsNullOrEmpty(input))
        {
            return true;
        }

        string lowerInput = input.ToLower();

        // --- 第一步：直接匹配检查 ---
        // 这一步能捕获到像 "4399" 这样包含数字的敏感词
        int index = 0;
        for (int i = 0; i < sensitiveWords.Count; i++)
        {
            string sensitiveWord = sensitiveWords[i];

            if (lowerInput.Contains(sensitiveWord))
            {
                Debug.Log($"[直接匹配] 检测到敏感词: {sensitiveWord}");
                Debug.Log($"[直接匹配】检测到敏感词：{sensitiveWord},差不多在敏感词列表第‘{i}'位)");
                return false;
            }
        }

        // --- 第二步：过滤干扰字符后检查 ---
        // 只有在第一步没发现问题时才执行，用于防范 "c n m" 或 "c23nm" 这样的绕过

        // 移除所有非字母和非中文字符的“干扰项”
        string filteredInput = Regex.Replace(lowerInput, @"[^a-z\u4e00-\u9fa5]", "");

        // 如果过滤后和过滤前一样，且第一步已检查过，那么无需再查
        if (filteredInput == lowerInput)
        {
            return true;
        }

        foreach (string sensitiveWord in sensitiveWords)
        {
            if (filteredInput.Contains(sensitiveWord))
            {
                Debug.Log($"[绕过匹配] 通过过滤 '{input}' -> '{filteredInput}' 检测到敏感词 '{sensitiveWord}'");
                return false;
            }
        }

        // 如果两步检查都通过了，则判定为合法
        return true;
    }


    #region 🔸 未成年人时间限制

    // 判断是否允许未成年人登录的接口逻辑
    private IEnumerator No18LoadGame()
    {
        //  RealLogin();
        //  EventCenter.Instance.TriggerEvent(EventMessages.BeginJugmentRemainTime);
        //     yield break;
        if (PlayerDataModule.Instance.data.age < 8)
        {
            UIController.Instance.Show<ForceQuitView>(
                "\u3000\u3000尊敬的玩家，您当前账号为未成年人账号，已被纳入防沉迷系统。根据国家新闻出版署下发《关于防止未成年人沉迷网络游戏的通知》及《关于进一步严格管理 切实防止未成年人沉迷网络游戏的通知》的要求，本游戏不为未满8周岁的用户提供游戏服务，您的年龄未满8周岁，无法登录本游戏。");

            yield break;
        }

        if (PlayerDataModule.Instance.data.age >= 18)
        {
            RealLogin();
            yield break;
        }

        DateTime now = DateTime.Now;
        string rawParams = $"time={now:yyyyMMdd}&time1={now:yyyy-MM-dd}";
        byte[] payload = Encoding.UTF8.GetBytes(rawParams);

        using UnityWebRequest request = new UnityWebRequest("https://banhao2.dyhyyx.com/php/holiday.php", "POST")
        {
            uploadHandler = new UploadHandlerRaw(payload),
            downloadHandler = new DownloadHandlerBuffer()
        };
        request.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded; charset=UTF-8");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            UIController.Instance.Show<TipView>("网络错误!");
            yield break;
        }

        Debug.Log("服务器返回原始数据: " + request.downloadHandler.text);

        try
        {
            RootObject data = JsonUtility.FromJson<RootObject>(request.downloadHandler.text);
            if (data.time.code != 0) yield break;

            int dayType = data.time.type.type;
            int weekDay = data.time.type.week;
            bool isWeekend = weekDay == 6 || weekDay == 7;
            bool isFriday = weekDay == 5;
            bool isHoliday = dayType == 2;
            bool isTimeValid = now.Hour >= 20 && now.Hour < 21;
            bool isWorkingDay = dayType == 3;
            HideAllPanels();
            if (isTimeValid && (isHoliday || isFriday || isWeekend) && !isWorkingDay)
            {
                EventCenter.Instance.TriggerEvent(EventMessages.BeginJugmentRemainTime);

                // TODO: 未成年人弹窗提示
                UIController.Instance.Show<AttentionView>(
                    "\u3000\u3000根据国家新闻出版署下发《关于防止未成年人沉迷网络游戏的通知》及《关于进一步严格管理 切实防止未成年人沉迷网络游戏的通知》，本游戏严格控制未成年人使用游戏时段，仅每周五、周六、周日和法定节假日的20时至21时提供1小时网络游戏服务。8周岁以上未满16周岁的未成车人用户，游戏中单次充值全额不得超过50元人民币，每月充值金额累计不得超过200元人民；16周岁以上未满18周岁的未成年人用户，单次充值全额不得超过100元人民币，每月充值金额累计不得超过400元人民币。",
                    "防沉迷提示", (Action)RealLogin);
            }
            else
            {
                string reason = !isTimeValid
                    ? $"当前时间 {now:HH:mm} 不在允许时段(20:00-21:00)"
                    : isWorkingDay
                        ? "今天是调休工作日，不允许游戏"
                        : "今天不是允许的游戏日期（需周五、六、日或节假日）";
                Debug.Log($"限制原因：{reason}");
                UIController.Instance.Show<ForceQuitView>(
                    "\u3000\u3000尊敬的玩家，您目前为未成年人账号，已被纳入防沉迷系统。根据国家新闻出版署下发《关于防止未成年人沉迷网络游戏的通知》及《关于进一步严格管理 切实防止未成年人沉迷网络游戏的通知》，本游戏严格控制未成年人使用游戏时段，仅每周五、周六、周日和法定节假日的20时至21时提供1小时网络游戏服务。您当前处于防沉迷保护中，当前时段为未成年人限制在线时段，您暂时无法登录游戏，系统强制下线。");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"解析节假日数据异常: {ex.Message}");
            UIController.Instance.Show<TipView>("系统繁忙，请稍后再试。");
        }
    }

    [Serializable]
    public class RootObject
    {
        public TimeData time;
    }

    [Serializable]
    public class TimeData
    {
        public int code;
        public TypeData type;
        public object holiday;
    }

    [Serializable]
    public class TypeData
    {
        public int type;
        public string name;
        public int week;
    }
    #endregion
}

