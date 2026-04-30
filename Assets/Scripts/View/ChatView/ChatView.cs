using System.Collections;
using Module;
using Module.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using View;

public class ChatView : BaseView
{
    public RectTransform content;
    public ScrollRect scrollView;
    public UIButton closeBtn;
    public TMP_InputField inputField;
    public UIButton sendBtn;
    public TextMeshProUGUI nametxt;
    public PlayerData aimData;

    public float pollInterval = 3f; // 轮询间隔（秒）



    private bool isFriend = true;
    private int friendCheckCounter = 0;
    private const int FriendCheckEvery = 3; // 每隔3次消息轮询做一次好友关系检查

    private int lastMessageCount = 0;
    private Coroutine pollCoroutine;
    private CanvasGroup messageViewportCanvasGroup;
    private bool revealMessageAfterRefresh;
    private Coroutine revealFallbackCoroutine;

    void OnEnable()
    {
        EventCenter.Instance.AddListener(EventMessages.UpdateChatRecord, UpdateMessage);
        EnsureMessageViewportCanvasGroup();
        if (content != null)
        {
            // Remove template items immediately to avoid one-frame flicker.
            Extensions.ClearChildrenImmediate(content);
        }
    }
    void OnDisable()
    {
        EventCenter.Instance.RemoveListener(EventMessages.UpdateChatRecord, UpdateMessage);
        StopPolling();
        StopRevealFallback();
    }

    protected override void Start()
    {
        base.Start();
        inputField.characterLimit = 18;

        closeBtn.onClick.AddListener(
           () =>
           {
               Hide();
           });

        sendBtn.onClick.AddListener(
            () =>
            {
                if (!isFriend)
                {
                    UIController.Instance.Show<TipView>("你们已不是好友。");
                    return;
                }
                if (inputField.text.Length > 0)
                {
                    string message = inputField.text;

                    BlockedWordRe blockedWordRe = new BlockedWordRe(ChatUrl.BlockedWordUrl);
                    blockedWordRe.JugmentBlockedWord(message, (data) =>
                    {
                        if (!data.success || data.code != 200 || data.data == null)
                        {
                            UIController.Instance.Show<TipView>("发送失败！");
                            return;
                        }
                        if (data.data.has_sensitive)
                        {
                            UIController.Instance.Show<TipView>($"输入内容包含敏感字符，请修改！");
                            return;
                        }
                        else
                        {
                            inputField.text = string.Empty;
                            AppendMyMessage(message);
                            ChatSendMessage chatSendMessage = new ChatSendMessage(ChatUrl.Send);
                            chatSendMessage.SentMessage(PlayerDataModule.Instance.data.user_id, aimData.user_id, message, "1", (_) => { });
                        }
                    });
                }
            });
    }

    public override void UpdateViewWithArgs(params object[] args)
    {
        base.UpdateViewWithArgs(args);
        aimData = args[0] as PlayerData;
        nametxt.text = aimData.playerName;
        inputField.text = string.Empty;
        lastMessageCount = 0;
        isFriend = true;
        friendCheckCounter = 0;
        sendBtn.interactable = true;
        inputField.interactable = true;
        revealMessageAfterRefresh = true;
        SetMessageViewportVisible(false);
        StartRevealFallback();
        UpdateMessage();    // 首次全量加载
        StartPolling();     // 开始轮询
        CheckFriendship();  // 立即检查一次好友关系
    }

    // 全量刷新（首次加载 / 外部事件触发）
    public void UpdateMessage(params object[] args)
    {
        if (aimData == null)
        {
            revealMessageAfterRefresh = false;
            StopRevealFallback();
            SetMessageViewportVisible(true);
            return;
        }
        Extensions.ClearChildrenImmediate(content);
        lastMessageCount = 0;
        bool shouldReveal = revealMessageAfterRefresh;
        revealMessageAfterRefresh = false;
        Chatrecordrequest req = new Chatrecordrequest(ChatUrl.ChatrecordrequestUrl);
        req.ChatrecordCheck(PlayerDataModule.Instance.data.user_id, aimData.user_id, (ChatDatas datas) =>
        {
            if (datas.data == null || datas.data.messages == null || datas.data.messages.Length == 0)
            {
                ForceContentLayoutRebuild();
                StartCoroutine(ScrollToBottom(shouldReveal));
                return;
            }

            foreach (var data in datas.data.messages)
            {
                AppendMessageItem(data, false);
            }

            ForceContentLayoutRebuild();
            lastMessageCount = datas.data.messages.Length;
            StartCoroutine(ScrollToBottom(shouldReveal));
        });
    }

    // 启动轮询
    private void StartPolling()
    {
        StopPolling();
        pollCoroutine = StartCoroutine(PollMessages());
    }

    private void StopPolling()
    {
        if (pollCoroutine != null)
        {
            StopCoroutine(pollCoroutine);
            pollCoroutine = null;
        }
    }

    // 轮询协程：每隔 pollInterval 秒拉取一次，仅追加新消息
    private IEnumerator PollMessages()
    {
        while (true)
        {
            yield return new WaitForSeconds(pollInterval);
            if (aimData == null) continue;

            // 每隔 FriendCheckEvery 次轮询做一次好友关系检查
            friendCheckCounter++;
            if (friendCheckCounter % FriendCheckEvery == 0)
                CheckFriendship();

            if (!isFriend) continue;

            Chatrecordrequest req = new Chatrecordrequest(ChatUrl.ChatrecordrequestUrl);
            req.ChatrecordCheck(PlayerDataModule.Instance.data.user_id, aimData.user_id, (ChatDatas datas) =>
            {
                if (datas.data == null || datas.data.messages == null) return;
                int newCount = datas.data.messages.Length;
                if (newCount <= lastMessageCount) return;

                for (int i = lastMessageCount; i < newCount; i++)
                    AppendMessageItem(datas.data.messages[i], false);

                ForceContentLayoutRebuild();
                lastMessageCount = newCount;
                StartCoroutine(ScrollToBottom(false));
            });
        }
    }

    // 追加单条消息 UI
    private void AppendMessageItem(MessageData data, bool rebuildLayout = true)
    {
        if (data.is_sent_by_me)
        {
            GameObject chatItem = GameObject.Instantiate(_assetHandle.Get<GameObject>("myMessage"), content, false);
            chatItem.GetComponent<MessageItem>().InitInfo(PlayerDataModule.Instance.data.headId.ToString(), data.time_formatted, data.content);
        }
        else
        {
            GameObject chatItem = GameObject.Instantiate(_assetHandle.Get<GameObject>("otherMessage"), content, false);
            chatItem.GetComponent<MessageItem>().InitInfo(aimData.headId.ToString(), data.time_formatted, data.content);
        }
        if (rebuildLayout)
            ForceContentLayoutRebuild();
    }

    private void AppendMyMessage(string message)
    {
        string time = System.DateTime.Now.ToString("MM-dd HH:mm");
        GameObject chatItem = GameObject.Instantiate(_assetHandle.Get<GameObject>("myMessage"), content, false);
        chatItem.GetComponent<MessageItem>().InitInfo(PlayerDataModule.Instance.data.headId.ToString(), time, message);
        ForceContentLayoutRebuild();
        lastMessageCount++; // 本地已展示，轮询时跳过这条
        StartCoroutine(ScrollToBottom(false));
    }

    private void ForceContentLayoutRebuild()
    {
        if (content != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(content);
    }

    private void EnsureMessageViewportCanvasGroup()
    {
        if (messageViewportCanvasGroup != null) return;
        if (scrollView == null || scrollView.viewport == null) return;

        messageViewportCanvasGroup = scrollView.viewport.GetComponent<CanvasGroup>();
        if (messageViewportCanvasGroup == null)
            messageViewportCanvasGroup = scrollView.viewport.gameObject.AddComponent<CanvasGroup>();
    }

    private void SetMessageViewportVisible(bool visible)
    {
        EnsureMessageViewportCanvasGroup();
        if (messageViewportCanvasGroup == null) return;

        messageViewportCanvasGroup.alpha = visible ? 1f : 0f;
        messageViewportCanvasGroup.blocksRaycasts = visible;
        messageViewportCanvasGroup.interactable = visible;
    }

    private void StartRevealFallback()
    {
        StopRevealFallback();
        revealFallbackCoroutine = StartCoroutine(RevealFallbackAfterDelay(1.5f));
    }

    private void StopRevealFallback()
    {
        if (revealFallbackCoroutine == null) return;
        StopCoroutine(revealFallbackCoroutine);
        revealFallbackCoroutine = null;
    }

    private IEnumerator RevealFallbackAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SetMessageViewportVisible(true);
        revealFallbackCoroutine = null;
    }

    private IEnumerator ScrollToBottom(bool revealAfter)
    {
        if (scrollView == null) yield break;

        Canvas.ForceUpdateCanvases();
        scrollView.verticalNormalizedPosition = 0f;
        yield return new WaitForEndOfFrame();
        Canvas.ForceUpdateCanvases();
        scrollView.verticalNormalizedPosition = 0f;

        if (revealAfter)
        {
            StopRevealFallback();
            SetMessageViewportVisible(true);
        }
    }

    // ── 好友关系检查 ──────────────────────────────────────────────────────
    private void CheckFriendship()
    {
        if (aimData == null) return;

        FriendsListRe req = new FriendsListRe(FriendsURL.FrinedsListUrl);
        req.GetFriendsList(PlayerDataModule.Instance.data.user_id, (listData) =>
        {
            if (listData?.data?.friends == null)
            {
                SetFriendStatus(false);
                return;
            }
            bool found = false;
            foreach (var fd in listData.data.friends)
            {
                if (fd.user_id == aimData.user_id) { found = true; break; }
            }
            SetFriendStatus(found);
        }, 1, 500);
    }

    private void SetFriendStatus(bool status)
    {
        if (isFriend == status) return;
        isFriend = status;
        sendBtn.interactable = status;
        inputField.interactable = status;

        if (!status)
            UIController.Instance.Show<TipView>("对方已将你删除!");
    }
}
