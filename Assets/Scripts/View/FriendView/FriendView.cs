using System.Collections;
using System.Collections.Generic;
using Module;
using Module.Data;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using View;
using JObject = Newtonsoft.Json.Linq.JObject;

public class FriendView : BaseView
{
    public GameObject content_1;
    public Transform content_1Transform;
    public ScrollRect friendlistScrollView;

    public GameObject content_2;
    public Transform content_2Transform;
    public UIButton addAllBtn;
    public UIButton reduseAllBtn;


    public GameObject content_3;
    public Transform content_3Transform;
    public TMP_InputField inputField;
    public UIButton selectBtn;

    public UIButton myfriendBtn;
    public GameObject myfriendBtnMask;
    public UIButton listBtn;
    public GameObject listBtnMask;
    public UIButton addfriendBtn;
    public GameObject addfriendBtnMask;


    private bool isLoadingMore = false;
    private int currentPage = 1;
    private const float loadMoreThreshold = 0.85f;
    private bool hasMoreData = true;


    private HashSet<int> cachedFriendIds = new HashSet<int>();
    private int friendCheckCycle = 0;
    private const int FriendListCheckEvery = 2;
    private bool isFriendListChecking = false;
    private Coroutine friendReqAutoRefreshCoroutine;
    private const float FriendReqAutoRefreshInterval = 5f;
    private bool isRefreshingFriendReq = false;
    private float friendReqLastRequestTime = -999f;
    private const float FriendReqRequestCooldown = 1f;
    private const float FriendReqRequestTimeout = 35f;
    private readonly Dictionary<int, FriendReqItem> friendReqItemMap = new Dictionary<int, FriendReqItem>();
    public List<FriendReqItem> friendReqItems = new List<FriendReqItem>();
    public GameObject reqBtnRedPoint;
    public GameObject listBtnRedPoint;
    public UIButton closeBtn;
    private bool isBatchHandlingReq = false;
    private bool isSearchingFriend = false;
    private bool hasUnreadMessage = false;
    private bool hasFriendRequest = false;
    private int friendSearchRequestVersion = 0;
    private Coroutine friendSearchTimeoutCoroutine;
    private const float FriendSearchTimeout = 10f;
    private const float FriendListRevealDelay = 0.15f;
    private readonly HashSet<int> renderedSearchUserIds = new HashSet<int>();
    private readonly HashSet<int> friendedSearchUserIds = new HashSet<int>();
    private readonly Dictionary<int, GameObject> renderedSearchItemMap = new Dictionary<int, GameObject>();
    private Coroutine contentRevealCoroutine;
    private int contentRevealVersion = 0;
    private int friendListRequestVersion = 0;
    private readonly HashSet<int> renderedFriendUserIds = new HashSet<int>();

    protected override void AddEventListener()
    {
        base.AddEventListener();
        myfriendBtn.onClick.RemoveAllListeners();
        myfriendBtn.onClick.AddListener(ShowContent_1);
        listBtn.onClick.RemoveAllListeners();
        listBtn.onClick.AddListener(ShowContent_2);
        addfriendBtn.onClick.RemoveAllListeners();
        addfriendBtn.onClick.AddListener(ShowContent_3);

        selectBtn.onClick.RemoveAllListeners();
        selectBtn.onClick.AddListener(SearchFriend);
        addAllBtn.onClick.RemoveAllListeners();
        addAllBtn.onClick.AddListener(() => HandleAllFriendReq("accept"));
        reduseAllBtn.onClick.RemoveAllListeners();
        reduseAllBtn.onClick.AddListener(() => HandleAllFriendReq("reject"));
        closeBtn.onClick.RemoveAllListeners();
        closeBtn.onClick.AddListener(() => { Hide(); });

        if (friendlistScrollView != null)
        {
            friendlistScrollView.onValueChanged.RemoveListener(OnScrollValueChanged);
            friendlistScrollView.onValueChanged.AddListener(OnScrollValueChanged);
        }

        EventCenter.Instance.AddListener(EventMessages.RefreshFriendList, RefreshFriendList);
        EventCenter.Instance.AddListener(EventMessages.DeleteReqItem, DeleteReqItem);
    }

    public override void UpdateViewWithArgs(params object[] args)
    {
        base.UpdateViewWithArgs(args);
        StartUnreadCheck();
        RequestUnread();
        RequestNewFriend();
        ShowContent_1();
    }

    public override void RemoveEventListener()
    {
        if (friendlistScrollView != null)
        {
            friendlistScrollView.onValueChanged.RemoveListener(OnScrollValueChanged);
        }

        EventCenter.Instance.RemoveListener(EventMessages.RefreshFriendList, RefreshFriendList);
        EventCenter.Instance.RemoveListener(EventMessages.DeleteReqItem, DeleteReqItem);
        base.RemoveEventListener();
    }


    protected override void OnHideComplete()
    {
        base.OnHideComplete();
        EventCenter.Instance.TriggerEvent(EventMessages.ShowPlayerInfoViewCartoon);
    }
    public void ShowContent_1()
    {
        StopFriendReqAutoRefresh();
        StopFriendSearchRequest();
        content_1.SetActive(true);
        content_2.SetActive(false);
        content_3.SetActive(false);
        HideContentBeforeLayout(content_1);
        myfriendBtnMask.SetActive(true);
        listBtnMask.SetActive(false);
        addfriendBtnMask.SetActive(false);
        hasUnreadMessage = false;
        RefreshRedPoints();
        RefreshFriendList();
    }

    public void ShowContent_2()
    {
        StopFriendReqAutoRefresh();
        StopFriendSearchRequest();
        StopFriendListRequest();
        content_1.SetActive(false);
        content_2.SetActive(true);
        content_3.SetActive(false);
        HideContentBeforeLayout(content_2);
        ClearFriendReqItems();
        myfriendBtnMask.SetActive(false);
        listBtnMask.SetActive(true);
        addfriendBtnMask.SetActive(false);
        RefreshRedPoints();
        RefreshFriendReq();
        StartFriendReqAutoRefresh();
        RevealContentAfterLayout(content_2, content_2Transform as RectTransform, FriendListRevealDelay);
    }

    public void ShowContent_3()
    {
        StopFriendReqAutoRefresh();
        StopFriendSearchRequest();
        StopFriendListRequest();
        inputField.text = "";
        content_1.SetActive(false);
        content_2.SetActive(false);
        content_3.SetActive(true);
        HideContentBeforeLayout(content_3);
        ClearFriendSearchItems();
        myfriendBtnMask.SetActive(false);
        listBtnMask.SetActive(false);
        addfriendBtnMask.SetActive(true);
        RefreshRedPoints();
        RefreshFriendSeach();
        RevealContentAfterLayout(content_3, content_3Transform as RectTransform, FriendListRevealDelay);
    }

    private void HideContentBeforeLayout(GameObject content)
    {
        CanvasGroup canvasGroup = GetOrAddCanvasGroup(content);
        if (canvasGroup == null) return;

        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
    }

    private void RevealContentAfterLayout(GameObject content, RectTransform layoutRoot, float extraDelay = 0f)
    {
        contentRevealVersion++;
        if (contentRevealCoroutine != null)
        {
            StopCoroutine(contentRevealCoroutine);
            contentRevealCoroutine = null;
        }

        contentRevealCoroutine = StartCoroutine(RevealContentAfterLayoutCoroutine(content, layoutRoot, contentRevealVersion, extraDelay));
    }

    private IEnumerator RevealContentAfterLayoutCoroutine(GameObject content, RectTransform layoutRoot, int revealVersion, float extraDelay)
    {
        yield return null;

        if (extraDelay > 0f)
        {
            yield return new WaitForSeconds(extraDelay);
        }

        if (revealVersion != contentRevealVersion || content == null || !content.activeSelf)
        {
            yield break;
        }

        Canvas.ForceUpdateCanvases();
        if (layoutRoot != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(layoutRoot);
        }

        RectTransform contentRect = content.transform as RectTransform;
        if (contentRect != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
        }

        Canvas.ForceUpdateCanvases();

        CanvasGroup canvasGroup = GetOrAddCanvasGroup(content);
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
        }

        contentRevealCoroutine = null;
    }

    private CanvasGroup GetOrAddCanvasGroup(GameObject target)
    {
        if (target == null) return null;

        CanvasGroup canvasGroup = target.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = target.AddComponent<CanvasGroup>();
        }

        return canvasGroup;
    }

    private void StopFriendListRequest()
    {
        friendListRequestVersion++;
        isLoadingMore = false;
        renderedFriendUserIds.Clear();
    }

    public void RefreshFriendList(params object[] args)
    {
        if (!content_1.activeSelf) return;

        int requestVersion = ++friendListRequestVersion;
        currentPage = 1;
        hasMoreData = true;
        isLoadingMore = true;
        cachedFriendIds.Clear();
        renderedFriendUserIds.Clear();
        ClearFriendList();
        LoadFriendListPage(currentPage, requestVersion);
    }



    #region 濂藉弸鍒楄〃婊氬姩鍔犺浇
    private void OnScrollValueChanged(Vector2 scrollPosition)
    {
        CheckScrollPosition();
    }

    private void CheckScrollPosition()
    {
        if (!hasMoreData || isLoadingMore || friendlistScrollView == null)
            return;
        float normalizedPosition = friendlistScrollView.verticalNormalizedPosition;
        if (normalizedPosition <= (1 - loadMoreThreshold))
        {
            LoadMoreFriends();
        }
    }


    private void LoadMoreFriends()
    {
        if (isLoadingMore || !hasMoreData)
            return;

        isLoadingMore = true;
        currentPage++;

        LoadFriendListPage(currentPage, friendListRequestVersion);
    }


    private void LoadFriendListPage(int page, int requestVersion)
    {
        FriendsListRe friendRequest = new FriendsListRe(FriendsURL.FrinedsListUrl);
        friendRequest.GetFriendsList(PlayerDataModule.Instance.data.user_id, (ListData) =>
        {
            if (requestVersion != friendListRequestVersion || content_1 == null || !content_1.activeSelf)
            {
                return;
            }

            if (ListData == null || ListData.data == null || ListData.data.friends == null || ListData.data.friends.Length == 0)
            {
                hasMoreData = false;
                isLoadingMore = false;
                RevealFriendListAfterLatestRequest(requestVersion);
                return;
            }

            foreach (FriendData friendData in ListData.data.friends)
            {
                if (friendData == null || !renderedFriendUserIds.Add(friendData.user_id)) continue;

                cachedFriendIds.Add(friendData.user_id);
                GameObject friendListItem = GameObject.Instantiate(_assetHandle.Get<GameObject>("friendlistitem"), content_1Transform, false);
                friendListItem.GetComponent<FriendListItem>().InitInfo(friendData);
            }


            if (ListData.data.friends.Length < 60)
            {
                hasMoreData = false;
            }

            isLoadingMore = false;
            RevealFriendListAfterLatestRequest(requestVersion);

        }, page, 60);
    }

    private void RevealFriendListAfterLatestRequest(int requestVersion)
    {
        if (requestVersion != friendListRequestVersion || content_1 == null || !content_1.activeSelf) return;
        RevealContentAfterLayout(content_1, content_1Transform as RectTransform, FriendListRevealDelay);
    }


    private void ClearFriendList()
    {
        if (content_1Transform == null)
            return;

        Extensions.ClearChildrenImmediate(content_1Transform);
    }
    #endregion


    public void RefreshFriendReq()
    {
        if (!content_2.activeSelf) return;
        if (isRefreshingFriendReq)
        {
            if (Time.unscaledTime - friendReqLastRequestTime <= FriendReqRequestTimeout)
                return;

            isRefreshingFriendReq = false;
        }

        if (Time.unscaledTime - friendReqLastRequestTime < FriendReqRequestCooldown)
            return;

        isRefreshingFriendReq = true;
        friendReqLastRequestTime = Time.unscaledTime;

        FriendRequest friendRequest = new FriendRequest(FriendsURL.FriendRequestUrl);
        friendRequest.FriendRequestCheck(PlayerDataModule.Instance.data.user_id, "received", (FriendRequestData) =>
        {
            isRefreshingFriendReq = false;
            if (FriendRequestData == null || FriendRequestData.data == null || FriendRequestData.data.requests == null)
            {
                SyncFriendReqItems(null);
                return;
            }

            SyncFriendReqItems(FriendRequestData.data.requests);
        });
    }

    private void SyncFriendReqItems(RequestData[] requests)
    {
        if (content_2Transform == null) return;

        if (requests == null || requests.Length == 0)
        {
            ClearFriendReqItems();
            UpdateReqRedPoint(false);
            return;
        }

        HashSet<int> aliveIds = new HashSet<int>();

        for (int i = 0; i < requests.Length; i++)
        {
            RequestData data = requests[i];
            if (data == null) continue;

            aliveIds.Add(data.id);
            FriendReqItem item;
            if (!friendReqItemMap.TryGetValue(data.id, out item) || item == null)
            {
                GameObject friendReqItemObj = GameObject.Instantiate(
                    _assetHandle.Get<GameObject>("friendlReqItem"),
                    content_2Transform,
                    false);
                item = friendReqItemObj.GetComponent<FriendReqItem>();
                item.InitInfo(data);
                friendReqItemMap[data.id] = item;
                friendReqItems.Add(item);
            }
            else if (!IsSameRequestData(item.requestData, data))
            {
                item.InitInfo(data);
            }

            item.transform.SetSiblingIndex(i);
        }

        List<int> removeIds = new List<int>();
        foreach (var pair in friendReqItemMap)
        {
            if (pair.Value == null || !aliveIds.Contains(pair.Key))
            {
                removeIds.Add(pair.Key);
            }
        }

        foreach (int reqId in removeIds)
        {
            if (friendReqItemMap.TryGetValue(reqId, out FriendReqItem item) && item != null)
            {
                friendReqItems.Remove(item);
                Destroy(item.gameObject);
            }

            friendReqItemMap.Remove(reqId);
        }

        CleanupFriendReqItemRefs();
        UpdateReqRedPoint(friendReqItems.Count > 0);
    }

    private void ClearFriendReqItems()
    {
        for (int i = friendReqItems.Count - 1; i >= 0; i--)
        {
            FriendReqItem item = friendReqItems[i];
            if (item != null)
            {
                Destroy(item.gameObject);
            }
        }

        friendReqItems.Clear();
        friendReqItemMap.Clear();
    }

    private void RemoveFriendReqItem(FriendReqItem item)
    {
        if (item == null)
        {
            CleanupFriendReqItemRefs();
            UpdateReqRedPoint(friendReqItems.Count > 0);
            return;
        }

        friendReqItems.Remove(item);
        if (item.requestData != null)
        {
            friendReqItemMap.Remove(item.requestData.id);
        }
        else
        {
            int keyToRemove = -1;
            foreach (var pair in friendReqItemMap)
            {
                if (pair.Value == item)
                {
                    keyToRemove = pair.Key;
                    break;
                }
            }

            if (keyToRemove != -1)
            {
                friendReqItemMap.Remove(keyToRemove);
            }
        }

        if (item != null)
        {
            Destroy(item.gameObject);
        }

        CleanupFriendReqItemRefs();
        UpdateReqRedPoint(friendReqItems.Count > 0);
    }

    private void CleanupFriendReqItemRefs()
    {
        for (int i = friendReqItems.Count - 1; i >= 0; i--)
        {
            if (friendReqItems[i] == null)
            {
                friendReqItems.RemoveAt(i);
            }
        }

        List<int> nullKeys = new List<int>();
        foreach (var pair in friendReqItemMap)
        {
            if (pair.Value == null)
            {
                nullKeys.Add(pair.Key);
            }
        }

        foreach (int key in nullKeys)
        {
            friendReqItemMap.Remove(key);
        }
    }

    private bool IsSameRequestData(RequestData current, RequestData incoming)
    {
        if (current == null || incoming == null) return false;
        return current.id == incoming.id
               && current.status == incoming.status
               && current.message == incoming.message
               && current.updated_at == incoming.updated_at
               && current.user_more == incoming.user_more;
    }

    private void UpdateReqRedPoint(bool hasUnread)
    {
        hasFriendRequest = hasUnread;
        RefreshRedPoints();
    }

    private void UpdateListRedPoint(bool hasUnread)
    {
        hasUnreadMessage = hasUnread;
        RefreshRedPoints();
    }

    private void RefreshRedPoints()
    {
        if (reqBtnRedPoint != null)
        {
            reqBtnRedPoint.SetActive(hasFriendRequest && (content_2 == null || !content_2.activeSelf));
        }

        if (listBtnRedPoint != null)
        {
            listBtnRedPoint.SetActive(hasUnreadMessage && (content_1 == null || !content_1.activeSelf));
        }

        EventCenter.Instance.TriggerEvent(EventMessages.RefreshExpeditionFriendRedPoint, hasFriendRequest || hasUnreadMessage);
    }

    private void DeleteReqItem(params object[] args)
    {
        if (args == null || args.Length == 0)
        {
            CleanupFriendReqItemRefs();
            UpdateReqRedPoint(friendReqItems.Count > 0);
            return;
        }

        RemoveFriendReqItem(args[0] as FriendReqItem);
    }

    private void HandleAllFriendReq(string action)
    {
        if (isBatchHandlingReq) return;

        CleanupFriendReqItemRefs();
        if (friendReqItems.Count == 0)
        {
            UIController.Instance.Show<TipView>("暂无好友申请。");
            UpdateReqRedPoint(false);
            return;
        }

        isBatchHandlingReq = true;
        SetFriendReqButtonsInteractable(false);

        List<FriendReqItem> pendingItems = new List<FriendReqItem>(friendReqItems);
        int finishedCount = 0;
        int successCount = 0;
        int totalCount = pendingItems.Count;

        for (int i = 0; i < pendingItems.Count; i++)
        {
            FriendReqItem item = pendingItems[i];
            if (item == null || item.requestData == null)
            {
                finishedCount++;
                continue;
            }

            int requestId = item.requestData.id;
            ConSentFriend friendRequest = new ConSentFriend(FriendsURL.ConSentFriendUrl);
            friendRequest.ConSentFriendCheck(PlayerDataModule.Instance.data.user_id, requestId, action, (data) =>
            {
                finishedCount++;
                if (data != null && data.code == 200)
                {
                    successCount++;
                    if (friendReqItemMap.TryGetValue(requestId, out FriendReqItem targetItem))
                    {
                        RemoveFriendReqItem(targetItem);
                    }
                }

                if (finishedCount >= totalCount)
                {
                    isBatchHandlingReq = false;
                    SetFriendReqButtonsInteractable(true);
                    UIController.Instance.Show<TipView>(action == "accept" ? $"已同意{successCount}个好友申请。" : $"已拒绝{successCount}个好友申请。");

                    if (successCount > 0)
                    {
                        EventCenter.Instance.TriggerEvent(EventMessages.RefreshFriendList);
                    }

                    RefreshFriendReq();
                }
            });
        }

        if (finishedCount >= totalCount)
        {
            isBatchHandlingReq = false;
            SetFriendReqButtonsInteractable(true);
            CleanupFriendReqItemRefs();
            UpdateReqRedPoint(friendReqItems.Count > 0);
        }
    }

    private void SetFriendReqButtonsInteractable(bool interactable)
    {
        if (addAllBtn != null) addAllBtn.interactable = interactable;
        if (reduseAllBtn != null) reduseAllBtn.interactable = interactable;

        for (int i = 0; i < friendReqItems.Count; i++)
        {
            FriendReqItem item = friendReqItems[i];
            if (item == null) continue;
            if (item.confirmBtn != null) item.confirmBtn.interactable = interactable;
            if (item.refuseBtn != null) item.refuseBtn.interactable = interactable;
        }
    }

    public void RefreshFriendSeach()
    {
        int requestVersion = ++friendSearchRequestVersion;
        StopFriendSearchTimeout();
        isSearchingFriend = false;
        if (selectBtn != null) selectBtn.interactable = true;
        renderedSearchUserIds.Clear();
        friendedSearchUserIds.Clear();
        renderedSearchItemMap.Clear();
        ClearFriendSearchItems();
        GetStrangerList getStrangerList = new GetStrangerList(FriendsURL.GetStrangersUrl);
        getStrangerList.GetStrangerListCheck(PlayerDataModule.Instance.data.user_id, (GetStrangerListData) =>
        {
            if (requestVersion != friendSearchRequestVersion) return;
            if (!content_3.activeSelf) return;
            if (GetStrangerListData == null || GetStrangerListData.ranking_list == null || GetStrangerListData.ranking_list.Length == 0) return;
            foreach (var resultData in GetStrangerListData.ranking_list)
            {
                CreateRankSearchItem(resultData);
            }
        });
    }

    private void SearchFriend()
    {
        string keyword = inputField == null ? string.Empty : inputField.text.Trim();
        if (string.IsNullOrEmpty(keyword))
        {
            RefreshFriendSeach();
            return;
        }

        int requestVersion = ++friendSearchRequestVersion;
        isSearchingFriend = true;
        if (selectBtn != null) selectBtn.interactable = false;
        StopFriendSearchTimeout();
        friendSearchTimeoutCoroutine = StartCoroutine(FriendSearchTimeoutLoop(requestVersion));
        renderedSearchUserIds.Clear();
        friendedSearchUserIds.Clear();
        renderedSearchItemMap.Clear();
        ClearFriendSearchItems();

        LoginUtil.Instance.CheckBlockedWords(keyword, (blockedData) =>
        {
            if (requestVersion != friendSearchRequestVersion || !isSearchingFriend) return;
            if (blockedData == null || blockedData.code != 200 || blockedData.data == null)
            {
                isSearchingFriend = false;
                StopFriendSearchTimeout();
                if (selectBtn != null) selectBtn.interactable = true;
                UIController.Instance.Show<TipView>("网络状态异常");
                return;
            }

            if (blockedData.data.has_sensitive)
            {
                isSearchingFriend = false;
                StopFriendSearchTimeout();
                if (selectBtn != null) selectBtn.interactable = true;
                UIController.Instance.Show<TipView>("输入内容包含敏感字符，请修改！");
                Debug.LogWarning($"好友搜索关键词 '{keyword}' 包含敏感词 '{blockedData.data.hit_word}'，原因类型: {blockedData.data.reason_type}，具体原因: {blockedData.data.reason}");
                return;
            }

            ExecuteFriendSearch(keyword, requestVersion);
        });
    }

    private void ExecuteFriendSearch(string keyword, int requestVersion)
    {
        if (requestVersion != friendSearchRequestVersion || !isSearchingFriend) return;

        int pendingRequestCount = 2;
        bool hasShownNoResultTip = false;

        void FinishSearchRequest()
        {
            if (requestVersion != friendSearchRequestVersion) return;
            pendingRequestCount--;
            if (pendingRequestCount > 0) return;

            isSearchingFriend = false;
            StopFriendSearchTimeout();
            if (selectBtn != null) selectBtn.interactable = true;

            if (!content_3.activeSelf) return;
            if (renderedSearchUserIds.Count == 0 && !hasShownNoResultTip)
            {
                hasShownNoResultTip = true;
                UIController.Instance.Show<TipView>("未搜索到玩家。");
            }
        }

        SearchForFriends strangerRequest = new SearchForFriends(FriendsURL.SearchFriendsUrl);
        strangerRequest.SearchForFriendCheck(PlayerDataModule.Instance.data.user_id, keyword, (strangerData) =>
        {
            if (requestVersion != friendSearchRequestVersion) return;
            if (strangerData != null && strangerData.data != null)
            {
                RenderStrangerSearchResults(strangerData.data.strangers);
            }

            FinishSearchRequest();
        });

        SearchForFriended friendedRequest = new SearchForFriended(FriendsURL.SearchFriendedUrl);
        friendedRequest.SearchForFriendCheck(PlayerDataModule.Instance.data.user_id, keyword, (friendedData) =>
        {
            if (requestVersion != friendSearchRequestVersion) return;
            if (friendedData != null && friendedData.data != null)
            {
                RenderFriendedSearchResults(friendedData.data.friends);
            }

            FinishSearchRequest();
        });
    }

    private void RenderFriendedSearchResults(FriendedData[] friendedResults)
    {
        if (friendedResults == null) return;
        foreach (FriendedData resultData in friendedResults)
        {
            CreateFriendedSearchItem(resultData);
        }
    }

    private void RenderStrangerSearchResults(StrangerData[] strangerResults)
    {
        if (strangerResults == null) return;
        foreach (StrangerData resultData in strangerResults)
        {
            CreateStrangerSearchItem(resultData);
        }
    }

    private void CreateRankSearchItem(RankList resultData)
    {
        if (resultData == null || resultData.id == PlayerDataModule.Instance.data.user_id || string.IsNullOrEmpty(resultData.user_more)) return;
        GameObject friendSeachItem = GameObject.Instantiate(_assetHandle.Get<GameObject>("friendSeachItem"), content_3Transform, false);
        friendSeachItem.GetComponent<FriendSeachItem>().InitInfo(resultData);
    }

    private bool CreateStrangerSearchItem(StrangerData resultData)
    {
        if (resultData == null || resultData.user_id == PlayerDataModule.Instance.data.user_id) return false;
        if (friendedSearchUserIds.Contains(resultData.user_id)) return false;
        if (!renderedSearchUserIds.Add(resultData.user_id)) return false;

        GameObject friendSeachItem = GameObject.Instantiate(_assetHandle.Get<GameObject>("friendSeachItem"), content_3Transform, false);
        friendSeachItem.GetComponent<FriendSeachItem>().InitInfo_1(resultData);
        renderedSearchItemMap[resultData.user_id] = friendSeachItem;
        return true;
    }

    private bool CreateFriendedSearchItem(FriendedData resultData)
    {
        if (resultData == null || resultData.user_id == PlayerDataModule.Instance.data.user_id) return false;
        friendedSearchUserIds.Add(resultData.user_id);

        if (renderedSearchItemMap.TryGetValue(resultData.user_id, out GameObject oldItem) && oldItem != null)
        {
            oldItem.SetActive(false);
            Destroy(oldItem);
        }

        renderedSearchItemMap.Remove(resultData.user_id);
        renderedSearchUserIds.Remove(resultData.user_id);
        if (!renderedSearchUserIds.Add(resultData.user_id)) return false;

        GameObject friendSeachItem = GameObject.Instantiate(_assetHandle.Get<GameObject>("friendSeachItem"), content_3Transform, false);
        friendSeachItem.GetComponent<FriendSeachItem>().InitInfo_2(resultData);
        renderedSearchItemMap[resultData.user_id] = friendSeachItem;
        return true;
    }

    private IEnumerator FriendSearchTimeoutLoop(int requestVersion)
    {
        yield return new WaitForSeconds(FriendSearchTimeout);
        if (requestVersion != friendSearchRequestVersion || !isSearchingFriend) yield break;

        isSearchingFriend = false;
        if (selectBtn != null) selectBtn.interactable = true;
        if (renderedSearchUserIds.Count == 0)
        {
            UIController.Instance.Show<TipView>("搜索超时，请稍后重试。");
        }
    }

    private void StopFriendSearchRequest()
    {
        friendSearchRequestVersion++;
        isSearchingFriend = false;
        renderedSearchUserIds.Clear();
        friendedSearchUserIds.Clear();
        renderedSearchItemMap.Clear();
        StopFriendSearchTimeout();
        if (selectBtn != null) selectBtn.interactable = true;
    }

    private void StopFriendSearchTimeout()
    {
        if (friendSearchTimeoutCoroutine != null)
        {
            StopCoroutine(friendSearchTimeoutCoroutine);
            friendSearchTimeoutCoroutine = null;
        }
    }

    private void ClearFriendSearchItems()
    {
        if (content_3Transform == null) return;

        for (int i = content_3Transform.childCount - 1; i >= 0; i--)
        {
            Transform child = content_3Transform.GetChild(i);
            if (child == null) continue;
            child.gameObject.SetActive(false);
            Destroy(child.gameObject);
        }
    }


    private Coroutine unreadCoroutine;

    public void StartUnreadCheck()
    {
        if (unreadCoroutine != null)
            return;
        RefreshRedPoints();
        unreadCoroutine = StartCoroutine(UnreadLoop());
    }

    private IEnumerator UnreadLoop()
    {
        while (true)
        {
            RequestUnread();
            RequestNewFriend();
            friendCheckCycle++;
            if (friendCheckCycle % FriendListCheckEvery == 0)
                CheckFriendListChanges();
            yield return new WaitForSeconds(5f);
        }
    }

    private bool isRequesting = false;
    private void RequestUnread()
    {
        if (isRequesting) return;
        isRequesting = true;
        GetUnreadCount getUnreadCount = new GetUnreadCount(FriendsURL.GetUnreadCountUrl);
        getUnreadCount.LoadMsg(PlayerDataModule.Instance.data.user_id, (data) =>
        {
            bool hasUnread = data != null && data.data != null && data.data.total_unread > 0;
            UpdateListRedPoint(hasUnread);

            isRequesting = false;
        });
    }


    private bool isReqing = false;

    private void RequestNewFriend()
    {
        if (isReqing) return;

        isReqing = true;

        FriendRequest getUnreadCount = new FriendRequest(FriendsURL.FriendRequestUrl);
        getUnreadCount.FriendRequestCheck(PlayerDataModule.Instance.data.user_id, "received", (data) =>
        {
            bool hasUnread = data != null && data.data != null && data.data.requests != null && data.data.requests.Length > 0;
            UpdateReqRedPoint(hasUnread);
            isReqing = false;
        });
    }

    private void StopUnreadCheck()
    {
        if (unreadCoroutine != null)
        {
            StopCoroutine(unreadCoroutine);
            unreadCoroutine = null;
        }
        isFriendListChecking = false;
    }

    private void StartFriendReqAutoRefresh()
    {
        if (friendReqAutoRefreshCoroutine != null)
            return;

        friendReqAutoRefreshCoroutine = StartCoroutine(FriendReqAutoRefreshLoop());
    }

    private IEnumerator FriendReqAutoRefreshLoop()
    {
        while (content_2 != null && content_2.activeSelf)
        {
            RefreshFriendReq();
            yield return new WaitForSeconds(FriendReqAutoRefreshInterval);
        }

        friendReqAutoRefreshCoroutine = null;
    }

    private void StopFriendReqAutoRefresh()
    {
        if (friendReqAutoRefreshCoroutine != null)
        {
            StopCoroutine(friendReqAutoRefreshCoroutine);
            friendReqAutoRefreshCoroutine = null;
        }

        isRefreshingFriendReq = false;
    }


    private void CheckFriendListChanges()
    {
        if (isFriendListChecking || cachedFriendIds.Count == 0) return;
        isFriendListChecking = true;

        FriendsListRe req = new FriendsListRe(FriendsURL.FrinedsListUrl);
        req.GetFriendsList(PlayerDataModule.Instance.data.user_id, (listData) =>
        {
            isFriendListChecking = false;

            if (listData?.data?.friends == null)
            {

                if (cachedFriendIds.Count > 0 && content_1.activeSelf)
                    RefreshFriendList();
                return;
            }

            var serverIds = new HashSet<int>();
            foreach (var fd in listData.data.friends)
                serverIds.Add(fd.user_id);

            bool changed = false;
            foreach (var id in cachedFriendIds)
            {
                if (!serverIds.Contains(id)) { changed = true; break; }
            }
            if (!changed && serverIds.Count != cachedFriendIds.Count)
                changed = true;

            if (changed && content_1.activeSelf)
                RefreshFriendList();

        },  1, 500);
    }

    private void OnDisable()
    {
        StopUnreadCheck();
        StopFriendReqAutoRefresh();
        isRequesting = false;
        isReqing = false;
        isRefreshingFriendReq = false;
        EventCenter.Instance.TriggerEvent(EventMessages.RefreshExpeditionFriendRedPoint, hasFriendRequest || hasUnreadMessage);
    }

    protected override void OnShow()
    {
        base.OnShow();
        StartUnreadCheck();
        RequestUnread();
        RequestNewFriend();
        RefreshRedPoints();
    }
}

public static class FriendPlayerDataParser
{
    public static PlayerData Parse(string userMore, int userId, string fallbackName = null, int fallbackHeadId = 0)
    {
        PlayerData playerData = null;
        bool hasHeadIdField = HasHeadIdField(userMore);

        if (!string.IsNullOrEmpty(userMore))
        {
            try
            {
                playerData = JsonConvert.DeserializeObject<PlayerData>(userMore);
            }
            catch (JsonException ex)
            {
                Debug.LogWarning($"[FriendPlayerDataParser] PlayerData deserialize failed, fallback to simple fields. userId={userId}, error={ex.Message}");
                playerData = ParseSimpleFields(userMore);
            }
        }

        if (playerData == null)
        {
            playerData = new PlayerData();
        }

        playerData.user_id = userId;

        if (string.IsNullOrEmpty(playerData.playerName))
        {
            playerData.playerName = string.IsNullOrEmpty(fallbackName) ? $"鐜╁{userId}" : fallbackName;
        }

        if (!hasHeadIdField && fallbackHeadId > 0)
        {
            playerData.headId = fallbackHeadId;
        }

        return playerData;
    }

    private static bool HasHeadIdField(string userMore)
    {
        if (string.IsNullOrEmpty(userMore))
        {
            return false;
        }

        try
        {
            JObject json = JObject.Parse(userMore);
            return json["headId"] != null || json["userHead"] != null;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static PlayerData ParseSimpleFields(string userMore)
    {
        try
        {
            JObject json = JObject.Parse(userMore);
            PlayerData playerData = new PlayerData();
            playerData.playerName = json.Value<string>("playerName") ?? json.Value<string>("userName") ?? json.Value<string>("user_rolename");
            playerData.user_id = json.Value<int?>("user_id") ?? json.Value<int?>("id") ?? 0;
            playerData.headId = json.Value<int?>("headId") ?? json.Value<int?>("userHead") ?? 0;
            playerData.accountLevel = json.Value<int?>("accountLevel") ?? json.Value<int?>("account_level") ?? 0;
            playerData.talentLevel = json.Value<int?>("talentLevel") ?? 0;
            playerData.tongbi = json.Value<int?>("tongbi") ?? 0;
            playerData.currentMapID = json.Value<int?>("currentMapID") ?? 1;
            return playerData;
        }
        catch (JsonException ex)
        {
            Debug.LogWarning($"[FriendPlayerDataParser] Simple field parse failed: {ex.Message}");
            return null;
        }
    }
}
