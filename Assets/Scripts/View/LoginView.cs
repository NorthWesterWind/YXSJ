using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

public class LoginView : BaseView
{
    public UIButton ageBtn;

    public GameObject fillBg;
    public Image fillImage;

    public GameObject loginContent;
    public TMP_InputField  accountInput;
    public TMP_InputField  passwordInput;
    public UIButton beginRegisterBtn;
    public UIButton loginBtn;

    public GameObject registerContent;
    public TMP_InputField  registerAccountInput;
    public TMP_InputField  registerPasswordInput;
    public UIButton registerReturnBtn;
    public UIButton registerBtn;

    public GameObject realNameContent;
    public TMP_InputField   realNameInput;
    public TMP_InputField   realAccountInput;
    public UIButton realNameReturnBtn;
    public UIButton realNameBtn;    

    public GameObject setNameContent;
    public TMP_InputField   setNameInput;
    public UIButton setNameBtn;

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


        public void  HideAllPanels()
        {
            loginContent.SetActive(false);
            registerContent.SetActive(false);
            realNameContent.SetActive(false);
            setNameContent.SetActive(false);
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
        loginBtn.onClick.RemoveAllListeners();
        loginBtn.onClick.AddListener(OnLogin);

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
    }


    private void OnLogin()
    {
        
    }
    private void OnRegister()
    {
        
    }
    private void OnRealName()
    {
        
    }
    private void OnSetName()
    {
        
    }
}
