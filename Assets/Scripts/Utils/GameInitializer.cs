using UnityEngine;

namespace Utils
{
    public class GameInitializer : MonoBehaviour
    {
        void Awake()
        {
            QualitySettings.vSyncCount = 0; // 关闭 VSync
            int refreshRate = Screen.currentResolution.refreshRate;
            Application.targetFrameRate = refreshRate;
            DontDestroyOnLoad(this);
            Screen.orientation = ScreenOrientation.Portrait;

            Screen.autorotateToPortrait = true;
            Screen.autorotateToPortraitUpsideDown = false;
            Screen.autorotateToLandscapeLeft = false;
            Screen.autorotateToLandscapeRight = false;
        }
    }
}
