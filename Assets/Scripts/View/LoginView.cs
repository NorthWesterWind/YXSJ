using System;
using System.Collections;
using System.Collections.Generic;
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
using View.CardView;
using View.CharacterInfoView;
using View.EmployeeFunction;
using View.MapFunction;
using View.OrderFunction;
using View.Task;
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
    public GameObject zhuxiaoContent;

    public TMP_InputField zhuxiaoAccountInput;
    public TMP_InputField zhuxiaoPasswordInput;
    public UIButton ZhuXiaoConfirmBtn;
    public UIButton ZhuXiaoCancleBtn;

    public UIButton headIconBtn;
    public Image headIcon;

    protected override void Awake()
    {
        base.Awake();
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
        zhuxiaoContent.SetActive(false);
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
        RefreshSetNameHeadIcon();
    }

    public void SwitchToZhuXiaoPanel()
    {
        HideAllPanels();
        zhuxiaoContent.SetActive(true);
        zhuxiaoAccountInput.text = "";
        zhuxiaoPasswordInput.text = "";
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
        zhuxiaoAccountInput.onValidateInput += ValidateAlphaNumeric;
        zhuxiaoAccountInput.onValueChanged.AddListener(OnZhuXiaoAccountValueChanged);

        zhuxiaoPasswordInput.onValidateInput += ValidateAlphaNumeric;
        zhuxiaoPasswordInput.onValueChanged.AddListener(OnZhuXiaoPasswordValueChanged);

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
        realNameBtn.onClick.AddListener(OnRealNameStrict);

        ZhuXiaoCancleBtn.onClick.RemoveAllListeners();
        ZhuXiaoCancleBtn.onClick.AddListener(() =>
        {
            SwitchToLoginPanel();
        });

        setNameBtn.onClick.RemoveAllListeners();
        setNameBtn.onClick.AddListener(OnSetName);

        headIconBtn.onClick.RemoveAllListeners();
        headIconBtn.onClick.AddListener(OnClickHeadIconBtn);

        EventCenter.Instance.AddListener(EventMessages.UpdateHeadIcon, HandleUpdateHeadIcon);

        ageBtn.onClick.RemoveAllListeners();
        ageBtn.onClick.AddListener(OnAge);
        ZhuXiaoBtn.onClick.RemoveAllListeners();
        ZhuXiaoBtn.onClick.AddListener(SwitchToZhuXiaoPanel);
        ZhuXiaoConfirmBtn.onClick.RemoveAllListeners();
        ZhuXiaoConfirmBtn.onClick.AddListener(() =>
        {
            string name = zhuxiaoAccountInput.text;
            string password = zhuxiaoPasswordInput.text;
            if (string.IsNullOrEmpty(name))
            {
                UIController.Instance.Show<TipView>("账号不能为空！");
                return;
            }
            if (string.IsNullOrEmpty(password))
            {
                UIController.Instance.Show<TipView>("密码不能为空！");
                return;
            }

            LoginUtil.Instance.ClearUser(name, password, responseclear =>
            {
                if (responseclear.state == 1)
                {
                    UIController.Instance.Show<TipView>(responseclear.msg);
                    zhuxiaoAccountInput.text = "";
                    zhuxiaoPasswordInput.text = "";
                }
                else if (responseclear.state == 2)
                {
                    UIController.Instance.Show<TipView>("账号信息不存在！");
                }
            });

        });

    }

    public override void RemoveEventListener()
    {
        EventCenter.Instance.RemoveListener(EventMessages.UpdateHeadIcon, HandleUpdateHeadIcon);
        if (headIconBtn != null) headIconBtn.onClick.RemoveListener(OnClickHeadIconBtn);
        base.RemoveEventListener();
    }

    private void OnClickHeadIconBtn()
    {
        UIController.Instance.Show<HeadIconChangeView>(PlayerDataModule.Instance.data);
    }

    private void HandleUpdateHeadIcon(params object[] args)
    {
        RefreshSetNameHeadIcon();
    }

    private void RefreshSetNameHeadIcon()
    {
        if (headIcon == null || PlayerDataModule.Instance == null || PlayerDataModule.Instance.data == null) return;

        AssetHandle assetHandle = _assetHandle != null ? _assetHandle : GetComponent<AssetHandle>();
        if (assetHandle == null) return;

        headIcon.sprite = assetHandle.Get<Sprite>(PlayerDataModule.Instance.data.headId.ToString());
    }

    public void OnAge()
    {
        if (fillBg.activeSelf)
        {
            return;
        }

        UIController.Instance.Show<AttentionView>(
            "\u3000\u3000（1）本游戏是一款以仙侠世界为背景的模拟经营类手游，适用于年满8周岁及以上的用户。建议未成年人在家长监护下使用游戏产品。\n\r\u3000\u3000（2）本游戏核心玩法包含材料收集、建筑升级、角色养成、物品售卖及资源管理等内容。玩家可通过策略规划开展经营，鼓励玩家用心钻研、挑战自我。\n\r\u3000\u3000（3）根据国家新闻出版署《关于防止未成年人沉迷网络游戏的通知》及《关于进一步严格管理 切实防止未成年人沉迷网络游戏的通知》，本游戏中有用户实名认证系统，未通过实名认证的用户不可进入游戏，认证为未成年人的用户将接受以下管理:\n\r\u3000\u3000认证为未成年人的用户，除周五、周六、周日及法定节假日每日20时至21时以外，其他时间均不可进入游戏。游戏中部分玩法和道具需要付费。未满8周岁的用户不能付费；8周岁以上未满16周岁的未成年人用户，单次充值金额不得超过50元人民币，每月充值金额累计不得超过200元人民币；16周岁以上未满18周岁的未成年人用户，单次充值金额不得超过100元人民币，每月充值金额累计不得超过400元人民币。\n\r\u3000\u3000（4）本游戏为仙侠主题的模拟经营类游戏，以妖剑世界的灵艺复苏为核心脉络，玩家将化身为妖剑世界的经营者，体验材料收集、建筑经营、资源调配的全过程，玩法轻松易懂，可帮助玩家放松日常压力，同时通过目标引导提升成就感。",
            "适龄提示");
    }

    private void LoginEvent()
    {
        if (string.IsNullOrEmpty(accountInput.text) || string.IsNullOrEmpty(passwordInput.text))
        {
            UIController.Instance.Show<TipView>("账号或密码不能为空！");
            return;
        }

        if (accountInput.text.Length < 4 || accountInput.text.Length > 8)
        {
            UIController.Instance.Show<TipView>("账号长度应为4到8个字符！");
            return;
        }

        LoginUtil.Instance.CheckBlockedWords(accountInput.text, (data) =>
        {
            if (data.code != 200)
            {
                UIController.Instance.Show<TipView>("网络状态异常！");
                return;
            }
            else
            {
                if (data.data.has_sensitive)
                {
                    UIController.Instance.Show<TipView>("账号包含敏感词！");
                    Debug.LogWarning($"账号 '{accountInput.text}' 包含敏感词 '{data.data.hit_word}'，原因类型: {data.data.reason_type}，具体原因: {data.data.reason}");
                    return;
                }
                else
                {
                    PlayerDataModule.Instance.Login(accountInput.text, passwordInput.text, OnLogin);
                }
            }
        });


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
        {
            OnCanLogin();
        }

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


                UIController.Instance.Preload<SettingView>();
                UIController.Instance.Preload<StoreView>();
                UIController.Instance.Preload<ZhuanPanView>();
                UIController.Instance.Preload<SevenDayView>();
                UIController.Instance.Preload<RewardConfirmView>();
                UIController.Instance.Preload<DungeonLevelView>();
                UIController.Instance.Preload<CharacterView>();
                UIController.Instance.Preload<CardInfoView>();
                UIController.Instance.Preload<CharacterView>();
                UIController.Instance.Preload<MapSelectView>();
                UIController.Instance.Preload<EmployeeFunctionView>();
                UIController.Instance.Preload<OrderFunctionView>();
                UIController.Instance.Preload<TaskPop>();
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
            UIController.Instance.Show<TipView>("账号或密码不能为空！");
            return;
        }

        if (registerAccountInput.text.Length < 4 || registerPasswordInput.text.Length > 8)
        {
            UIController.Instance.Show<TipView>("账号长度应为4到8个字符！");
            return;
        }
        if (registerPasswordInput.text.Length < 4)
        {
            UIController.Instance.Show<TipView>("密码长度不能少于4位！");
            return;
        }

        // if (!IsTextValid(registerAccountInput.text))
        // {
        //     UIController.Instance.Show<TipView>("账号包含敏感词!");
        //     return;
        // }
        LoginUtil.Instance.CheckBlockedWords(registerAccountInput.text, (data) =>
        {
            if (data.code != 200)
            {
                UIController.Instance.Show<TipView>("网络状态异常！");
                return;
            }
            else
            {
                if (data.data.has_sensitive)
                {
                    UIController.Instance.Show<TipView>("账号包含敏感词！");
                    Debug.LogWarning($"注册账号 '{registerAccountInput.text}' 包含敏感词 '{data.data.hit_word}'，原因类型: {data.data.reason_type}，具体原因: {data.data.reason}");
                    return;
                }
                else
                {
                    PlayerDataModule.Instance.Register(str1, str2,
                        OnRegisterSuccess, OnRegisterFail);
                }
            }
        });

    }

    private void OnRegisterSuccess()
    {
        UIController.Instance.Show<TipView>("注册成功！");
        SwitchToLoginPanel();
    }

    private void OnRegisterFail(string msg)
    {
        UIController.Instance.Show<TipView>(msg);
    }

    private void OnRealNameStrict()
    {
        string realName = realNameInput.text?.Trim();
        string idCard = realAccountInput.text?.Trim().ToUpperInvariant();

        if (string.IsNullOrEmpty(realName) || string.IsNullOrEmpty(idCard))
        {
            UIController.Instance.Show<TipView>("姓名或身份证号不能为空！");
            return;
        }

        if (!Extensions.IsAllChinese(realName) || realName.Length < 2 || realName.Length > 8)
        {
            UIController.Instance.Show<TipView>("请输入2-8位中文姓名！");
            return;
        }

        if (!IsValidChineseIdCard(idCard))
        {
            UIController.Instance.Show<TipView>("请输入18位有效身份证号！");
            return;
        }

        realNameInput.text = realName;
        realAccountInput.text = idCard;
        OnRealName();
    }

    private static readonly int[] IdCardWeight = { 7, 9, 10, 5, 8, 4, 2, 1, 6, 3, 7, 9, 10, 5, 8, 4, 2 };
    private static readonly char[] IdCardCheckCode = { '1', '0', 'X', '9', '8', '7', '6', '5', '4', '3', '2' };

    private bool IsValidChineseIdCard(string idCard)
    {
        if (string.IsNullOrWhiteSpace(idCard))
        {
            return false;
        }

        if (!Regex.IsMatch(idCard, @"^\d{17}[\dX]$"))
        {
            return false;
        }

        if (!DateTime.TryParseExact(idCard.Substring(6, 8), "yyyyMMdd",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out DateTime birthday))
        {
            return false;
        }

        if (birthday < new DateTime(1900, 1, 1) || birthday > DateTime.Today)
        {
            return false;
        }

        int sum = 0;
        for (int i = 0; i < 17; i++)
        {
            sum += (idCard[i] - '0') * IdCardWeight[i];
        }

        char checkCode = IdCardCheckCode[sum % 11];
        return idCard[17] == checkCode;
    }

    private void OnRealName()
    {

        if (string.IsNullOrEmpty(realNameInput.text) || string.IsNullOrEmpty(realAccountInput.text))
        {
            UIController.Instance.Show<TipView>("姓名或身份证号不能为空！");
            return;
        }
        if (realAccountInput.text.Length < 18 || realAccountInput.text.Length > 18)
        {
            UIController.Instance.Show<TipView>("请输入18位有效身份证号数字！");
            return;
        }

        StartCoroutine(SendAuthRequest(realNameInput.text, realAccountInput.text));
    }


    private IEnumerator SendAuthRequest(string name, string idCard)
    {
        string rawParams = $"idnum={idCard}&name={name}";
        byte[] payload = Encoding.UTF8.GetBytes(rawParams);

        using UnityWebRequest request = new UnityWebRequest("https://banhao2.dyhyyx.com/php/yanzheng1.php", "POST")
        {
            uploadHandler = new UploadHandlerRaw(payload),
            downloadHandler = new DownloadHandlerBuffer()
        };
        request.SetRequestHeader("Content-Type",
            "application/x-www-form-urlencoded; charset=UTF-8");

        yield return request.SendWebRequest();

        Debug.Log("服务器返回原始数据: " + request.downloadHandler.text);
        //loadingPanel.SetActive(false);

        if (request.result == UnityWebRequest.Result.Success)
        {
            HandleResponse(request.downloadHandler.text);
        }
        else
        {
            Debug.Log($"网络错误: {request.error}！");
            UIController.Instance.Show<TipView>($"网络错误！");
        }
    }

    // 处理服务器响应
    private void HandleResponse(string responseText)
    {
        AuthResponse response = JsonUtility.FromJson<AuthResponse>(responseText);
        if (response == null)
        {
            Debug.Log("服务器返回数据解析失败");
            UIController.Instance.Show<TipView>("服务器未响应！");
            return;
        }

        // 检查错误码
        if (response.error_code != 0)
        {
            Debug.Log($"认证失败: {response.reason}");
            UIController.Instance.Show<TipView>($"认证失败！");
            return;
        }

        // 检查是否验证通过
        if (!response.result.isok)
        {
            UIController.Instance.Show<TipView>("身份证信息不匹配！");
            return;
        }

        // 全部验证通过
        Debug.Log($"认证成功！\n姓名：{response.result.realname}\n地区：{response.result.IdCardInfor.area}");

        PlayerDataModule.Instance.RealName(realAccountInput.text, realNameInput.text,
           "0",
           response =>
           {
               switch (response.state)
               {
                   case 1:
                       UIController.Instance.Show<TipView>(response.msg + "!");
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
                       {
                           OnCanLogin();
                       }
                       break;
                   case 3:
                       UIController.Instance.Show<TipView>("实名失败输入18位身份证号！");
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
            UIController.Instance.Show<TipView>("昵称不能为空！");
            return;
        }

        if (!Extensions.IsAllChinese(name))
        {
            UIController.Instance.Show<TipView>("昵称只允许输入中文！");
            return;
        }

        if (name.Length > 6)
        {
            UIController.Instance.Show<TipView>("昵称不能超过6位中文字符。");
            return;
        }

        LoginUtil.Instance.CheckBlockedWords(name, (data) =>
        {
            if (data.code != 200)
            {
            UIController.Instance.Show<TipView>("网络状态异常！");
                return;
            }
            else
            {
                if (data.data.has_sensitive)
                {
                    UIController.Instance.Show<TipView>($"输入内容包含敏感字符，请修改！");
                    Debug.LogWarning($"昵称 '{name}' 包含敏感词 '{data.data.hit_word}'，原因类型: {data.data.reason_type}，具体原因: {data.data.reason}");
                    return;
                }
                else
                {
                    HideAllPanels();
                    PlayerData playerData = PlayerDataModule.Instance.data;
                    playerData.playerName = name;
                    playerData.isCreated = true;
                    PlayerDataModule.Instance.SavePlayerDataAsync();
                    PlayerDataModule.Instance.SavePlayerDataToSever();
                    StartCoroutine(No18LoadGame());
                }
            }
        });




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
    private void OnZhuXiaoAccountValueChanged(string text)
    {
        if (Input.compositionString.Length > 0)
            return; // 等输入法结束

        string filtered = Regex.Replace(text, @"[^a-zA-Z0-9]", "");

        if (filtered != text)
        {
            zhuxiaoAccountInput.text = filtered;
            zhuxiaoAccountInput.caretPosition = filtered.Length;
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
    private void OnZhuXiaoPasswordValueChanged(string text)
    {
        if (Input.compositionString.Length > 0)
            return; // 等输入法结束

        string filtered = Regex.Replace(text, @"[^a-zA-Z0-9]", "");

        if (filtered != text)
        {
            zhuxiaoPasswordInput.text = filtered;
            zhuxiaoPasswordInput.caretPosition = filtered.Length; // 保持光标在最后
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
            //setNameInput.text = filtered.ToString();
        }
    }
    private void OnNumberValueChanged(string text)
    {
        // 用 StringBuilder 过滤出数字
        var filtered = new StringBuilder(18);
        if (Input.compositionString.Length > 0)
        {
            return;
        }

        foreach (char c in text)
        {
            // 只允许英文(a-zA-Z)或数字(0-9)，且长度限制为10
            if (filtered.Length >= 18)
            {
                break;
            }

            if (char.IsDigit(c))
            {
                filtered.Append(c);
                continue;
            }

            if ((c == 'x' || c == 'X') && filtered.Length == 17)
            {
                filtered.Append('X');
            }
        }

        // 如果被改动了，就更新输入框
        if (filtered.ToString() != text)
        {
            realAccountInput.text = filtered.ToString();
            realAccountInput.caretPosition = filtered.Length;
        }
    }




    private List<string> sensitiveWordsFilePaths = new List<string>
        {
            "SensitiveWords.txt",
        };

    private List<string> sensitiveWords = new List<string>();
    private bool isInitialized = false;





    /// <summary>
    /// 检测输入文本是否合法（返回 true = 合法）
    /// </summary>
    /// <summary>
    /// 检查文本是否合法（true = 合法）
    /// </summary>
    // public bool IsTextValid(string input)
    // {
    //     if (!isInitialized)
    //     {
    //         Debug.LogWarning("敏感词检测器未初始化完成");
    //         return true;
    //     }

    //     if (string.IsNullOrEmpty(input))
    //     {
    //         return true;
    //     }

    //     string lowerInput = input.ToLower();

    //     // --- 第一步：直接匹配检查 ---
    //     // 这一步能捕获到像 "4399" 这样包含数字的敏感词
    //     int index = 0;
    //     for (int i = 0; i < sensitiveWords.Count; i++)
    //     {
    //         string sensitiveWord = sensitiveWords[i];

    //         if (lowerInput.Contains(sensitiveWord))
    //         {
    //             Debug.Log($"[直接匹配] 检测到敏感词: {sensitiveWord}");
    //             Debug.Log($"[直接匹配】检测到敏感词：{sensitiveWord},差不多在敏感词列表第‘{i}'位)");
    //             return false;
    //         }
    //     }

    //     // --- 第二步：过滤干扰字符后检查 ---
    //     // 只有在第一步没发现问题时才执行，用于防范 "c n m" 或 "c23nm" 这样的绕过

    //     // 移除所有非字母和非中文字符的“干扰项”
    //     string filteredInput = Regex.Replace(lowerInput, @"[^a-z\u4e00-\u9fa5]", "");

    //     // 如果过滤后和过滤前一样，且第一步已检查过，那么无需再查
    //     if (filteredInput == lowerInput)
    //     {
    //         return true;
    //     }

    //     foreach (string sensitiveWord in sensitiveWords)
    //     {
    //         if (filteredInput.Contains(sensitiveWord))
    //         {
    //             Debug.Log($"[绕过匹配] 通过过滤 '{input}' -> '{filteredInput}' 检测到敏感词 '{sensitiveWord}'");
    //             return false;
    //         }
    //     }

    //     // 如果两步检查都通过了，则判定为合法
    //     return true;
    // }


    #region 🔸 未成年人时间限制

    // 判断是否允许未成年人登录的接口逻辑
    private IEnumerator No18LoadGame()
    {
        //  RealLogin();
        //   EventCenter.Instance.TriggerEvent(EventMessages.BeginJugmentRemainTime);
        //     yield break;\

        // UIController.Instance.Show<AttentionView>(
        //          "\u3000\u3000根据国家新闻出版署下发《关于防止未成年人沉迷网络游戏的通知》及《关于进一步严格管理 切实防止未成年人沉迷网络游戏的通知》，本游戏严格控制未成年人使用游戏时段，仅每周五、周六、周日和法定节假日的20时至21时提供1小时网络游戏服务。8周岁以上未满16周岁的未成年人用户，游戏中单次充值金额不得超过50元人民币，每月充值金额累计不得超过200元人民币；16周岁以上未满18周岁的未成年人用户，单次充值金额不得超过100元人民币，每月充值金额累计不得超过400元人民币。",
        //          "未成年提示", (Action)RealLogin);
        // EventCenter.Instance.TriggerEvent(EventMessages.BeginJugmentRemainTime);
        // yield break;

        if (PlayerDataModule.Instance.data.age < 8)
        {
            UIController.Instance.Show<ForceQuitView>(
                "\u3000\u3000尊敬的玩家，检测到您当前登录账号未满8周岁。为更好地保护未成年人，本游戏不向未满8周岁的用户提供游戏服务，您无法登录本游戏。", "防沉迷提示");

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
            UIController.Instance.Show<TipView>("网络错误！");
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
                    "\u3000\u3000根据国家新闻出版署下发《关于防止未成年人沉迷网络游戏的通知》及《关于进一步严格管理 切实防止未成年人沉迷网络游戏的通知》，本游戏严格控制未成年人使用游戏时段，仅每周五、周六、周日和法定节假日的20时至21时提供1小时网络游戏服务。8周岁以上未满16周岁的未成年人用户，游戏中单次充值金额不得超过50元人民币，每月充值金额累计不得超过200元人民币；16周岁以上未满18周岁的未成年人用户，单次充值金额不得超过100元人民币，每月充值金额累计不得超过400元人民币。",
                    "未成年提示", (Action)RealLogin);
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
                    "\u3000\u3000尊敬的玩家，您目前为未成年人账号，已被纳入防沉迷系统。根据国家新闻出版署下发《关于防止未成年人沉迷网络游戏的通知》及《关于进一步严格管理 切实防止未成年人沉迷网络游戏的通知》，本游戏严格控制未成年人使用游戏时段，仅每周五、周六、周日和法定节假日的20时至21时提供1小时网络游戏服务。", "健康游戏提示");
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

