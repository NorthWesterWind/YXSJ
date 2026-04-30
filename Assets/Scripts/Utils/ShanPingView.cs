using System.Collections;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils;

namespace World.View.UI
{
    public class ShanPingView : MonoBehaviour
    {
        public CanvasGroup canvasGroup;

        void Start()
        {
            canvasGroup.alpha = 0;
            canvasGroup.DOFade(1, 0.5f);
            LoadNextScene().Forget();
        }
        private async UniTask LoadNextScene()
        {
            await UniTask.Delay(5000);
            await SceneManager.LoadSceneAsync("Login");
            UIController.Instance.Show<LoginView>();
        }
    }
}
