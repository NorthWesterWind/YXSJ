using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils;

namespace World.View.UI
{
    public class ShanPingView : MonoBehaviour
    {
        // Start is called before the first frame update
        public CanvasGroup canvasGroup;
        void Start()
        {
            canvasGroup.alpha = 0;
            canvasGroup.DOFade(1,  0.5f);
            StartCoroutine(LoadNextSceneCoroutine());
        }

        // Update is called once per frame
        void Update()
        {
        
        }
        
        private IEnumerator LoadNextSceneCoroutine()
        {
            // 注册回调（只注册一次）
            SceneManager.sceneLoaded += OnSceneLoaded;

            yield return new WaitForSeconds(1f);
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("Login");
            while (!asyncLoad.isDone)
            {
                yield return null;
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // 只在加载 Main 场景时执行
            if (scene.name == "Login")
            {
                // 显示UI
                UIController.Instance.Show<LoginView>();
            }
            // 用完就移除，防止多次注册
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
}
