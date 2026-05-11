using TMPro;
using Utils;
using UnityEngine;

public class ForceQuitView : BaseView
{
    public TextMeshProUGUI infotxt;
    public UIButton quitBtn;
    public TextMeshProUGUI titleTxt;

    public override void UpdateViewWithArgs(params object[] args)
    {
        base.UpdateViewWithArgs(args);
        infotxt.text = args[0] as string;
        titleTxt.text = args[1] as string;
    }

    protected override void AddEventListener()
    {
        base.AddEventListener();
        quitBtn.onClick.AddListener((() =>
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false; // 在编辑器中停止播放
#else
    Application.Quit(); // 在打包后的应用中退出
#endif
        }));
    }
}
