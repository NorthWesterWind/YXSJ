using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Utils;
using World.Controller;

namespace View
{
    public class SettingView : BaseView
    {
        public Slider musicSlider;
        public Slider soundSlider;
        public UIButton returnBtn;
        public UIButton showLogoutBtn;
        public UIButton showQuitBtn;
        public UIButton hideQuitBtn;
        public UIButton hideQuitBg;
        public UIButton hideLogoutBtn;
        public UIButton hideLogoutBg;

        public UIButton quitBtn;
        public UIButton loginoutBtn;

        public GameObject QuitContent;
        public GameObject SwitchContent;

        public override void UpdateViewWithArgs(params object[] args)
        {
            base.UpdateViewWithArgs(args);
            musicSlider.value = AudioSourceController.Instance.musicVolume;
            soundSlider.value = AudioSourceController.Instance.soundVolume;
            QuitContent.SetActive(false);
            SwitchContent.SetActive(false);
        }

        protected override void AddEventListener()
        {
            base.AddEventListener();
            returnBtn.onClick.RemoveAllListeners();
            returnBtn.onClick.AddListener((() => { Hide(); }));
            loginoutBtn.onClick.AddListener(Logout);
            quitBtn.onClick.AddListener((() =>
                         {
#if UNITY_EDITOR
                             UnityEditor.EditorApplication.isPlaying = false; // 在编辑器中停止播放
#else
    Application.Quit(); // 在打包后的应用中退出
#endif
                         }));
            musicSlider.onValueChanged.RemoveAllListeners();
            musicSlider.onValueChanged.AddListener((value) =>
            {
                AudioSourceController.Instance.SetMusicVolume(musicSlider.value);
            });
            soundSlider.onValueChanged.RemoveAllListeners();
            soundSlider.onValueChanged.AddListener((value) =>
            {
                AudioSourceController.Instance.SetSoundVolume(soundSlider.value);
            });

            showLogoutBtn.onClick.RemoveAllListeners();
            showLogoutBtn.onClick.AddListener((() =>
            {
                SwitchContent.SetActive(true);
            }));

            showQuitBtn.onClick.RemoveAllListeners();
            showQuitBtn.onClick.AddListener((() =>
            {
                QuitContent.SetActive(true);
            }));

            hideQuitBtn.onClick.RemoveAllListeners();
            hideQuitBtn.onClick.AddListener((() =>
            {
                QuitContent.SetActive(false);
            }));
            hideLogoutBg.onClick.RemoveAllListeners();
            hideLogoutBg.onClick.AddListener((() =>
            {
                SwitchContent.SetActive(false);
            }));
            hideQuitBg.onClick.RemoveAllListeners();
            hideQuitBg.onClick.AddListener((() =>
            {
                QuitContent.SetActive(false);
            }));
            hideLogoutBtn.onClick.RemoveAllListeners();
            hideLogoutBtn.onClick.AddListener((() =>
            {
                SwitchContent.SetActive(false);
            }));
        }

        protected override void Start()
        {
            base.Start();

        }

        private void Logout()
        {
            StartCoroutine(LoadNextSceneCoroutine());
        }

        private IEnumerator LoadNextSceneCoroutine()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;

            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("Login");
            while (!asyncLoad.isDone)
            {
                yield return null;
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == "Login")
            {
                // 显示UI
                UIController.Instance.Show<LoginView>();
            }

            // 用完就移除，防止多次注册
            SceneManager.sceneLoaded -= OnSceneLoaded;
            AudioSourceController.Instance.StopSound();
        }
        protected override void OnHideComplete()
        {
            base.OnHideComplete();
            EventCenter.Instance.TriggerEvent(EventMessages.ShowPlayerInfoViewCartoon);
        }
    }
}