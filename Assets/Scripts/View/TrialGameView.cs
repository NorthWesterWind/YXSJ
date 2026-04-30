using System;
using System.Collections;
using Controller;
using Module;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Utils;
using World.Controller;

public class TrialGameView : BaseView
{
    private const string TrialMonsterDeadEvent = "TrialMonsterDead";
    private const string TrialModeStopEvent = "TrialModeStop";
    private const string TrialSkillUsedEvent = "TrialSkillUsed";
    private const string DefaultSkillPrefabKey = "skillEffect";

    private static Sprite runtimeIndicatorSprite;

    public UIButton settingBtn;
    public Image fillImg;
    public TextMeshProUGUI counttimeText;
    public float countTime = 150f;
    public UIButton skillBtn;
    public Image skillMask;
    public TextMeshProUGUI skillText;
    public float skillCooldownTime = 10f;
    public float skillReadyButtonScale = 0.85f;

    [Header("Skill")]
    public Image indicator;
    public GameObject skillPrefab;
    public Transform skillRoot;
    public Camera worldCamera;
    public LayerMask skillTargetLayers = ~0;
    public string skillTargetTag = "Map";
    public bool requireSkillTargetTag = true;
    public string skillPrefabAssetKey = DefaultSkillPrefabKey;
    public string skillAnimationName = "animation";
    public bool skillAnimationLoop = true;
    public bool skillUseAnimationDuration = false;
    public float skillEffectLifetime = 1.5f;
    public float skillDestroyDelay = 0.1f;
    public float skillIndicatorConfirmDelay = 1f;

    [Header("Trial Progress")]
    public int targetMonsterCount = 100;

    public GameObject loadView;
    public Image fillImage;

    private int currentMonsterCount;
    private bool isLeavingTrial;
    private bool isTrialSettled;
    private bool isResultShown;
    private bool hasStoppedTrialRuntime;
    private float trialRemainingTime;
    private float skillRemainingTime;
    private bool isSkillCoolingDown;
    private bool isSkillAiming;
    private bool isSkillPendingRelease;
    private Vector2 currentSkillScreenPosition;
    private RectTransform runtimeIndicatorRect;
    private Image runtimeIndicatorImage;
    private Canvas cachedCanvas;
    private Coroutine pendingSkillReleaseCoroutine;
    private bool hasSkillInputFocus;
    private Vector3 skillButtonOriginalScale = Vector3.one;
    private bool hasCachedSkillButtonOriginalScale;

    protected override void Start()
    {
        base.Start();
        CacheSkillButtonOriginalScale();
    }


    protected override void AddEventListener()
    {
        base.AddEventListener();
        EventCenter.Instance.AddListener(TrialMonsterDeadEvent, HandleTrialMonsterDead);
        EventCenter.Instance.AddListener(EventMessages.CloseTrialView, HandleCloseTrialView);
        EventCenter.Instance.AddListener(EventMessages.UpdateLoadView, HandleUpdateLoadView);

        if (settingBtn != null)
        {
            settingBtn.onClick.RemoveAllListeners();
            settingBtn.onClick.AddListener(OnClickSettingBtn);
        }

        if (skillBtn != null)
        {
            skillBtn.onClick.RemoveListener(OnClickSkillBtn);
            skillBtn.onPointerDownEvent.RemoveListener(OnSkillPointerDown);
            skillBtn.onPointerUpEvent.RemoveListener(OnSkillPointerUp);
            skillBtn.onDragEvent.RemoveListener(OnSkillDrag);
            skillBtn.onClick.AddListener(OnClickSkillBtn);
        }
    }

    public override void RemoveEventListener()
    {
        EventCenter.Instance.RemoveListener(TrialMonsterDeadEvent, HandleTrialMonsterDead);
        EventCenter.Instance.RemoveListener(EventMessages.CloseTrialView, HandleCloseTrialView);
        EventCenter.Instance.RemoveListener(EventMessages.UpdateLoadView, HandleUpdateLoadView);

        if (skillBtn != null)
        {
            skillBtn.onClick.RemoveListener(OnClickSkillBtn);
            skillBtn.onPointerDownEvent.RemoveListener(OnSkillPointerDown);
            skillBtn.onPointerUpEvent.RemoveListener(OnSkillPointerUp);
            skillBtn.onDragEvent.RemoveListener(OnSkillDrag);
        }
    }

    public void HandleUpdateLoadView(params object[] args)
    {
        if (fillImage != null && args != null && args.Length > 0 && args[0] is float progress)
        {
            fillImage.fillAmount = progress;
        }
    }

    public override void UpdateViewWithArgs(params object[] args)
    {
        base.UpdateViewWithArgs(args);
        EnsureIndicator();
        SetIndicatorVisible(false);

        if (loadView != null)
        {
            loadView.SetActive(false);
        }

        if (fillImage != null)
        {
            fillImage.fillAmount = 0f;
        }

        isLeavingTrial = false;
        isTrialSettled = false;
        isResultShown = false;
        hasStoppedTrialRuntime = false;
        isSkillAiming = false;
        isSkillPendingRelease = false;
        SetSkillInputLocked(false);
        SetSkillButtonPreparedVisual(false);
        StopPendingSkillRelease();
        targetMonsterCount = Mathf.Max(1, targetMonsterCount);
        if (args != null && args.Length > 0 && args[0] != null)
        {
            if (args[0] is int targetCount)
            {
                targetMonsterCount = Mathf.Max(1, targetCount);
            }
            else if (args[0] is float targetCountFloat)
            {
                targetMonsterCount = Mathf.Max(1, Mathf.RoundToInt(targetCountFloat));
            }
        }

        currentMonsterCount = 0;
        trialRemainingTime = Mathf.Max(0f, countTime);
        skillRemainingTime = 0f;
        isSkillCoolingDown = false;
        RefreshTrialCountdownText();
        RefreshSkillCooldownUI();
        RefreshFill();
        EventCenter.Instance.TriggerEvent(EventMessages.MonsterBeginCreate);
    }

    private void Update()
    {
        if (!IsVisible || isLeavingTrial || isTrialSettled)
        {
            return;
        }

        UpdateSkillPlacementInput();
        UpdateTrialCountdown();
        UpdateSkillCooldown();
        EvaluateTrialResult();
    }

    protected override void OnHide()
    {
        base.OnHide();
        CancelSkillAim();
    }

    private void OnClickSettingBtn()
    {
        Time.timeScale = 0;
        Action action = () =>
        {
            EventCenter.Instance.TriggerEvent(EventMessages.CloseTrialView);
        };
        UIController.Instance.Show<PauseView>(action);
    }

    private void OnClickSkillBtn()
    {
        if (isSkillCoolingDown || isLeavingTrial || !IsVisible || isTrialSettled || isSkillPendingRelease)
        {
            return;
        }

        if (isSkillAiming)
        {
            CancelSkillAim();
            return;
        }

        EnterSkillAimState();
    }

    private void OnSkillPointerDown(PointerEventData eventData) { }

    private void OnSkillDrag(PointerEventData eventData)
    {
    }

    private void OnSkillPointerUp(PointerEventData eventData)
    {
    }

    public void HandleCloseTrialView(params object[] args)
    {
        if (isLeavingTrial)
        {
            return;
        }
        isLeavingTrial = true;
        CancelSkillAim();
        StopTrialRuntime();
        Time.timeScale = 1;
        if (loadView != null)
        {
            loadView.SetActive(true);
        }

        if (fillImage != null)
        {
            fillImage.fillAmount = 0f;
        }

        EventCenter.Instance.TriggerEvent(EventMessages.UpdateLoadView, 0f);

        ResetRuntimeState();
        StartCoroutine(LoadNextSceneCoroutine());
    }

    private void HandleTrialMonsterDead(params object[] args)
    {
        if (args == null || args.Length == 0)
        {
            return;
        }

        float delta = 1f;
        if (args.Length > 2 && args[2] != null)
        {
            if (args[2] is float floatDelta)
            {
                delta = Mathf.Max(0f, floatDelta);
            }
            else if (args[2] is int intDelta)
            {
                delta = Mathf.Max(0f, intDelta);
            }
        }

        currentMonsterCount += Mathf.Max(1, Mathf.RoundToInt(delta));
        RefreshFill();
        if (!isTrialSettled)
        {
            EvaluateTrialResult();
        }
    }

    private void UpdateTrialCountdown()
    {
        if (trialRemainingTime <= 0f)
        {
            trialRemainingTime = 0f;
            RefreshTrialCountdownText();
            return;
        }

        trialRemainingTime = Mathf.Max(0f, trialRemainingTime - Time.deltaTime);
        RefreshTrialCountdownText();
    }

    private void UpdateSkillCooldown()
    {
        if (!isSkillCoolingDown)
        {
            return;
        }

        skillRemainingTime = Mathf.Max(0f, skillRemainingTime - Time.deltaTime);
        RefreshSkillCooldownUI();

        if (skillRemainingTime <= 0f)
        {
            FinishSkillCooldown();
        }
    }

    private void StartSkillCooldown()
    {
        float cooldown = Mathf.Max(0f, skillCooldownTime);
        if (cooldown <= 0f)
        {
            return;
        }

        isSkillCoolingDown = true;
        skillRemainingTime = cooldown;
        RefreshSkillCooldownUI();
    }

    private void FinishSkillCooldown()
    {
        isSkillCoolingDown = false;
        skillRemainingTime = 0f;
        RefreshSkillCooldownUI();
    }

    private void RefreshTrialCountdownText()
    {
        if (counttimeText == null)
        {
            return;
        }

        counttimeText.text = FormatCountdown(trialRemainingTime);
    }

    private void RefreshSkillCooldownUI()
    {
        if (skillBtn != null)
        {
            skillBtn.interactable = !isSkillCoolingDown;
        }

        if (skillMask != null)
        {
            skillMask.gameObject.SetActive(isSkillCoolingDown);
            skillMask.fillAmount = isSkillCoolingDown
                ? Mathf.Clamp01(skillRemainingTime / Mathf.Max(0.01f, skillCooldownTime))
                : 0f;
        }

        if (skillText != null)
        {
            skillText.gameObject.SetActive(isSkillCoolingDown);
            skillText.text = isSkillCoolingDown ? FormatCountdown(skillRemainingTime) : string.Empty;
        }
    }

    private void ResetRuntimeState()
    {
        trialRemainingTime = Mathf.Max(0f, countTime);
        skillRemainingTime = 0f;
        isSkillCoolingDown = false;
        isTrialSettled = false;
        isResultShown = false;
        hasStoppedTrialRuntime = false;
        RefreshTrialCountdownText();
        RefreshSkillCooldownUI();
    }

    private string FormatCountdown(float seconds)
    {
        int totalSeconds = Mathf.Max(0, Mathf.CeilToInt(seconds));
        return $"{totalSeconds}";
    }

    private void RefreshFill()
    {
        if (fillImg == null)
        {
            return;
        }

        int safeTarget = Mathf.Max(1, targetMonsterCount);
        fillImg.fillAmount = Mathf.Clamp01(currentMonsterCount * 1f / safeTarget);
    }

    private void EvaluateTrialResult()
    {
        if (isTrialSettled)
        {
            return;
        }

        if (currentMonsterCount >= Mathf.Max(1, targetMonsterCount))
        {
            CompleteTrial(true);
            return;
        }

        if (trialRemainingTime <= 0f)
        {
            CompleteTrial(false);
        }
    }

    private void CompleteTrial(bool isSuccess)
    {
        if (isTrialSettled)
        {
            return;
        }

        isTrialSettled = true;
        CancelSkillAim();
        StopTrialRuntime();
        ShowTrialResult(isSuccess);
    }

    private void ShowTrialResult(bool isSuccess)
    {
        if (isResultShown)
        {
            return;
        }

        isResultShown = true;
        Action action = () =>
        {
            EventCenter.Instance.TriggerEvent(EventMessages.CloseTrialView);
        };

        UIController.Instance.Show<TrialResultView>(isSuccess, GetTrialReward(isSuccess),action);
    }

    private int GetTrialReward(bool isSuccess)
    {

        if (isSuccess)
        {
            return 200;
        }

        return 20;
    }

    private void StopTrialRuntime()
    {
        if (hasStoppedTrialRuntime)
        {
            return;
        }

        hasStoppedTrialRuntime = true;
        EventCenter.Instance.TriggerEvent(TrialModeStopEvent);
    }

    private void TryReleaseSkill()
    {
        if (isTrialSettled || isLeavingTrial || !IsVisible)
        {
            SetSkillInputLocked(false);
            SetSkillButtonPreparedVisual(false);
            return;
        }

        SetIndicatorVisible(false);

        if (!TryResolveSkillTarget(currentSkillScreenPosition, out Vector3 skillWorldPosition))
        {
            SetSkillInputLocked(false);
            SetSkillButtonPreparedVisual(false);
            return;
        }

        if (!SpawnSkill(skillWorldPosition))
        {
            SetSkillInputLocked(false);
            SetSkillButtonPreparedVisual(false);
            return;
        }

        SetSkillInputLocked(false);
        SetSkillButtonPreparedVisual(false);
        StartSkillCooldown();
        EventCenter.Instance.TriggerEvent(TrialSkillUsedEvent, skillWorldPosition);
    }

    private void CancelSkillAim()
    {
        StopPendingSkillRelease();
        isSkillAiming = false;
        isSkillPendingRelease = false;
        SetSkillInputLocked(false);
        SetSkillButtonPreparedVisual(false);
        SetIndicatorVisible(false);
    }

    private void EnterSkillAimState()
    {
        EnsureIndicator();
        StopPendingSkillRelease();
        isSkillAiming = true;
        isSkillPendingRelease = false;
        SetSkillInputLocked(true);
        SetSkillButtonPreparedVisual(true);
        SetIndicatorVisible(false);
    }

    private void UpdateSkillPlacementInput()
    {
        if (!isSkillAiming || isSkillPendingRelease)
        {
            return;
        }

        if (!TryGetSkillPlacementInput(out Vector2 screenPosition))
        {
            return;
        }

        ConfirmSkillPlacement(screenPosition);
    }

    private bool TryGetSkillPlacementInput(out Vector2 screenPosition)
    {
        screenPosition = Vector2.zero;

        if (Input.touchCount > 0)
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);
                if (touch.phase != TouchPhase.Began)
                {
                    continue;
                }

                if (IsPointerOverBlockingSkillUI(touch.position, touch.fingerId))
                {
                    continue;
                }

                screenPosition = touch.position;
                return true;
            }

            return false;
        }

        if (!Input.GetMouseButtonDown(0))
        {
            return false;
        }

        Vector2 mousePosition = Input.mousePosition;
        if (IsPointerOverBlockingSkillUI(mousePosition))
        {
            return false;
        }

        screenPosition = mousePosition;
        return true;
    }

    private void ConfirmSkillPlacement(Vector2 screenPosition)
    {
        isSkillAiming = false;
        isSkillPendingRelease = true;
        SetSkillButtonPreparedVisual(false);
        UpdateSkillAimPosition(screenPosition);
        SetIndicatorVisible(true);

        StopPendingSkillRelease();
        pendingSkillReleaseCoroutine = StartCoroutine(ReleaseSkillAfterDelay());
    }

    private IEnumerator ReleaseSkillAfterDelay()
    {
        yield return new WaitForSeconds(Mathf.Max(0f, skillIndicatorConfirmDelay));
        pendingSkillReleaseCoroutine = null;
        isSkillPendingRelease = false;
        TryReleaseSkill();
    }

    private void StopPendingSkillRelease()
    {
        if (pendingSkillReleaseCoroutine == null)
        {
            return;
        }

        StopCoroutine(pendingSkillReleaseCoroutine);
        pendingSkillReleaseCoroutine = null;
    }

    private void SetSkillInputLocked(bool locked)
    {
        if (hasSkillInputFocus == locked)
        {
            return;
        }

        hasSkillInputFocus = locked;
        EventCenter.Instance.TriggerEvent(locked ? EventMessages.LockJoystickInput : EventMessages.UnlockJoystickInput);
    }

    private void CacheSkillButtonOriginalScale()
    {
        if (skillBtn != null && !hasCachedSkillButtonOriginalScale)
        {
            skillButtonOriginalScale = skillBtn.transform.localScale;
            hasCachedSkillButtonOriginalScale = true;
        }
    }

    private void SetSkillButtonPreparedVisual(bool prepared)
    {
        if (skillBtn == null)
        {
            return;
        }

        CacheSkillButtonOriginalScale();
        float scale = prepared ? Mathf.Max(0.1f, skillReadyButtonScale) : 1f;
        skillBtn.transform.localScale = skillButtonOriginalScale * scale;
    }

    private bool IsPointerOverBlockingSkillUI(Vector2 screenPosition, int pointerId = -1)
    {
        if (EventSystem.current == null)
        {
            return false;
        }

        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = screenPosition,
            pointerId = pointerId
        };

        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        for (int i = 0; i < results.Count; i++)
        {
            GameObject hitObject = results[i].gameObject;
            if (hitObject == null)
            {
                continue;
            }

            if (indicator != null && (hitObject == indicator.gameObject || hitObject.transform.IsChildOf(indicator.transform)))
            {
                continue;
            }

            if (hitObject.GetComponentInParent<Selectable>() != null)
            {
                return true;
            }
        }

        return false;
    }

    private bool TryResolveSkillTarget(Vector2 screenPosition, out Vector3 skillWorldPosition)
    {
        skillWorldPosition = Vector3.zero;
        Camera cameraToUse = GetWorldCamera();
        if (cameraToUse == null)
        {
            return false;
        }

        Ray ray = cameraToUse.ScreenPointToRay(screenPosition);
        RaycastHit2D[] hits = Physics2D.GetRayIntersectionAll(ray, float.PositiveInfinity, skillTargetLayers.value);
        if (hits != null && hits.Length > 0)
        {
            for (int i = 0; i < hits.Length; i++)
            {
                Collider2D collider = hits[i].collider;
                if (collider == null)
                {
                    continue;
                }

                if (requireSkillTargetTag && !collider.CompareTag(skillTargetTag))
                {
                    continue;
                }

                skillWorldPosition = hits[i].point;
                return true;
            }
        }

        Vector3 worldPosition = cameraToUse.ScreenToWorldPoint(new Vector3(
            screenPosition.x,
            screenPosition.y,
            -cameraToUse.transform.position.z));
        worldPosition.z = 0f;
        skillWorldPosition = worldPosition;
        return true;
    }

    private bool SpawnSkill(Vector3 skillWorldPosition)
    {
        GameObject prefab = ResolveSkillPrefab();
        if (prefab == null)
        {
            Debug.LogWarning("TrialGameView: skill effect prefab is not assigned.");
            return false;
        }

        GameObject spawnedSkill = skillRoot != null
            ? Instantiate(prefab, skillWorldPosition, Quaternion.identity, skillRoot)
            : Instantiate(prefab, skillWorldPosition, Quaternion.identity);

        if (spawnedSkill == null)
        {
            return false;
        }

        TrialSkillArea2D skillArea = spawnedSkill.GetComponent<TrialSkillArea2D>();
        if (skillArea == null)
        {
            skillArea = spawnedSkill.AddComponent<TrialSkillArea2D>();
        }

        skillArea.Configure(
            transform,
            skillAnimationName,
            skillAnimationLoop,
            skillUseAnimationDuration,
            skillEffectLifetime,
            skillDestroyDelay);
        return true;
    }

    private GameObject ResolveSkillPrefab()
    {
        if (skillPrefab != null)
        {
            return skillPrefab;
        }

        if (_assetHandle != null)
        {
            string key = string.IsNullOrWhiteSpace(skillPrefabAssetKey) ? DefaultSkillPrefabKey : skillPrefabAssetKey;
            return _assetHandle.Get<GameObject>(key);
        }

        return null;
    }

    private void EnsureIndicator()
    {
        if (indicator != null)
        {
            return;
        }

        if (runtimeIndicatorImage != null)
        {
            indicator = runtimeIndicatorImage;
            return;
        }

        Canvas canvas = GetCachedCanvas();
        if (canvas == null)
        {
            return;
        }

        GameObject indicatorObject = new GameObject("SkillAimIndicator", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        indicatorObject.transform.SetParent(canvas.transform, false);
        runtimeIndicatorRect = indicatorObject.GetComponent<RectTransform>();
        runtimeIndicatorRect.sizeDelta = new Vector2(200f, 200f);
        runtimeIndicatorRect.anchorMin = new Vector2(0.5f, 0.5f);
        runtimeIndicatorRect.anchorMax = new Vector2(0.5f, 0.5f);
        runtimeIndicatorRect.pivot = new Vector2(0.5f, 0.5f);

        runtimeIndicatorImage = indicatorObject.GetComponent<Image>();
        runtimeIndicatorImage.sprite = GetRuntimeIndicatorSprite();
        runtimeIndicatorImage.type = Image.Type.Sliced;
        runtimeIndicatorImage.color = new Color(0.25f, 0.85f, 1f, 0.35f);
        runtimeIndicatorImage.raycastTarget = false;
        runtimeIndicatorImage.gameObject.SetActive(false);
        indicator = runtimeIndicatorImage;
    }

    private void SetIndicatorVisible(bool visible)
    {
        EnsureIndicator();
        if (indicator == null)
        {
            return;
        }

        indicator.gameObject.SetActive(visible);
        if (visible)
        {
            indicator.rectTransform.SetAsLastSibling();
        }
    }

    private void UpdateIndicatorScreenPosition(Vector2 screenPosition)
    {
        EnsureIndicator();
        if (indicator == null)
        {
            return;
        }

        RectTransform indicatorRect = indicator.rectTransform;
        RectTransform parentRect = indicatorRect.parent as RectTransform;
        if (parentRect == null)
        {
            indicatorRect.position = screenPosition;
            return;
        }

        Camera eventCamera = GetCanvasEventCamera(parentRect.GetComponentInParent<Canvas>());
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, screenPosition, eventCamera, out Vector2 localPosition))
        {
            indicatorRect.anchoredPosition = localPosition;
        }
    }

    private void UpdateSkillAimPosition(Vector2 screenPosition)
    {
        currentSkillScreenPosition = ClampScreenPosition(screenPosition);
        UpdateIndicatorScreenPosition(currentSkillScreenPosition);
    }

    private Vector2 ClampScreenPosition(Vector2 screenPosition)
    {
        float clampedX = Mathf.Clamp(screenPosition.x, 0f, Screen.width);
        float clampedY = Mathf.Clamp(screenPosition.y, 0f, Screen.height);
        return new Vector2(clampedX, clampedY);
    }

    private Camera GetWorldCamera()
    {
        if (worldCamera != null)
        {
            return worldCamera;
        }

        if (Camera.main != null)
        {
            worldCamera = Camera.main;
        }

        return worldCamera;
    }

    private Canvas GetCachedCanvas()
    {
        if (cachedCanvas == null)
        {
            cachedCanvas = GetComponentInParent<Canvas>();
        }

        return cachedCanvas;
    }

    private Camera GetCanvasEventCamera(Canvas canvas)
    {
        if (canvas == null || canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            return null;
        }

        if (canvas.worldCamera != null)
        {
            return canvas.worldCamera;
        }

        return GetWorldCamera();
    }

    private Sprite GetRuntimeIndicatorSprite()
    {
        if (runtimeIndicatorSprite == null)
        {
            runtimeIndicatorSprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f));
        }

        return runtimeIndicatorSprite;
    }

    private IEnumerator LoadNextSceneCoroutine()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync($"Game_{PlayerDataModule.Instance.data.currentMapID}");
        asyncLoad.allowSceneActivation = false;
        float displayProgress = 0f;

        if (fillImage != null)
        {
            fillImage.fillAmount = 0f;
        }

        EventCenter.Instance.TriggerEvent(EventMessages.UpdateLoadView, 0f);

        while (!asyncLoad.isDone)
        {
            float targetProgress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
            displayProgress = Mathf.MoveTowards(displayProgress, targetProgress, Time.unscaledDeltaTime * 0.3f);
            EventCenter.Instance.TriggerEvent(EventMessages.UpdateLoadView, displayProgress);

            if (asyncLoad.progress >= 0.9f && displayProgress >= 0.99f)
            {
                displayProgress = Mathf.MoveTowards(displayProgress, 1f, Time.unscaledDeltaTime * 0.3f);
                EventCenter.Instance.TriggerEvent(EventMessages.UpdateLoadView, displayProgress);
                if (displayProgress >= 1f)
                {
                    yield return new WaitForSecondsRealtime(0.5f);
                    asyncLoad.allowSceneActivation = true;
                }
            }

            yield return null;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == $"Game_{PlayerDataModule.Instance.data.currentMapID}")
        {
            PlayerDataModule.Instance.BeginAutoSave();

            if (GameController.Instance != null)
            {
                GameController.Instance.currentMapID = PlayerDataModule.Instance.data.currentMapID;
                GameController.Instance.RefreshStructureCaches();
            }

            if (PlayerDataModule.Instance.data.currentMapID == 1 &&
                PlayerDataModule.Instance.data.guideStep != GuideStep.Over)
            {
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
            AudioSourceController.Instance.PlaySound();
            UIController.Instance.Show<DungeonLevelView>();
        }

        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}

[DisallowMultipleComponent]
public class TrialSkillArea2D : MonoBehaviour
{
    [SerializeField] private SkeletonAnimation skeletonAnimation;
    [SerializeField] private string animationName = "animation";
    [SerializeField] private bool loopAnimation = true;
    [SerializeField] private bool useAnimationDuration = false;
    [SerializeField] private float effectLifetime = 1.5f;
    [SerializeField] private float destroyDelay = 0.1f;
    [SerializeField] private Collider2D[] hitColliders;
    [SerializeField] private Renderer[] effectRenderers;
    [SerializeField] private int sortingOrderOffset = 20;

    private readonly System.Collections.Generic.HashSet<int> hitMonsterIds = new();
    private Coroutine destroyCoroutine;
    private Transform owner;

    private void Awake()
    {
        if (skeletonAnimation == null)
        {
            skeletonAnimation = GetComponentInChildren<SkeletonAnimation>(true);
        }

        if (effectRenderers == null || effectRenderers.Length == 0)
        {
            effectRenderers = GetComponentsInChildren<Renderer>(true);
        }

        EnsurePhysicsSetup();
        UpdateSortingOrder();
    }

    private void OnEnable()
    {
        PlaySkillAnimation();
    }

    private void OnDisable()
    {
        if (destroyCoroutine != null)
        {
            StopCoroutine(destroyCoroutine);
            destroyCoroutine = null;
        }
    }

    private void LateUpdate()
    {
        UpdateSortingOrder();
    }

    public void Configure(
        Transform ownerTransform,
        string animationNameValue,
        bool loopAnimationValue,
        bool useAnimationDurationValue,
        float effectLifetimeValue,
        float destroyDelayValue)
    {
        owner = ownerTransform;
        animationName = animationNameValue;
        loopAnimation = loopAnimationValue;
        useAnimationDuration = useAnimationDurationValue;
        effectLifetime = Mathf.Max(0.05f, effectLifetimeValue);
        destroyDelay = Mathf.Max(0f, destroyDelayValue);
        EnsurePhysicsSetup();

        if (isActiveAndEnabled)
        {
            PlaySkillAnimation();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryKillMonster(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryKillMonster(other);
    }

    private void EnsurePhysicsSetup()
    {
        Rigidbody2D rigidbody2D = GetComponent<Rigidbody2D>();
        if (rigidbody2D == null)
        {
            rigidbody2D = gameObject.AddComponent<Rigidbody2D>();
        }

        rigidbody2D.bodyType = RigidbodyType2D.Kinematic;
        rigidbody2D.gravityScale = 0f;
        rigidbody2D.simulated = true;

        if (hitColliders == null || hitColliders.Length == 0)
        {
            hitColliders = GetComponentsInChildren<Collider2D>(true);
        }

        if (hitColliders == null || hitColliders.Length == 0)
        {
            hitColliders = new Collider2D[] { gameObject.AddComponent<BoxCollider2D>() };
        }

        for (int i = 0; i < hitColliders.Length; i++)
        {
            if (hitColliders[i] != null)
            {
                hitColliders[i].isTrigger = true;
            }
        }
    }

    private void PlaySkillAnimation()
    {
        if (destroyCoroutine != null)
        {
            StopCoroutine(destroyCoroutine);
        }

        float lifetime = Mathf.Max(0.05f, effectLifetime);
        if (skeletonAnimation != null && skeletonAnimation.AnimationState != null && !string.IsNullOrEmpty(animationName))
        {
            var trackEntry = skeletonAnimation.AnimationState.SetAnimation(0, animationName, loopAnimation);
            if (useAnimationDuration && trackEntry != null && trackEntry.Animation != null)
            {
                lifetime = Mathf.Max(lifetime, trackEntry.Animation.Duration);
            }
        }

        destroyCoroutine = StartCoroutine(DestroyAfterLifetime(lifetime + destroyDelay));
    }

    private IEnumerator DestroyAfterLifetime(float lifetime)
    {
        yield return new WaitForSeconds(lifetime);
        Destroy(gameObject);
    }

    private void TryKillMonster(Collider2D other)
    {
        if (other == null || !other.CompareTag("Monster"))
        {
            return;
        }

        MonsterController monster = other.GetComponent<MonsterController>();
        if (monster == null)
        {
            monster = other.GetComponentInParent<MonsterController>();
        }

        if (monster == null)
        {
            return;
        }

        int instanceId = monster.GetInstanceID();
        if (!hitMonsterIds.Add(instanceId))
        {
            return;
        }

        monster.KillImmediately(owner, true);
    }

    private void UpdateSortingOrder()
    {
        if (effectRenderers == null || effectRenderers.Length == 0)
        {
            return;
        }

        int order = 30000 - Mathf.FloorToInt(transform.position.y * 100f) + sortingOrderOffset;
        for (int i = 0; i < effectRenderers.Length; i++)
        {
            if (effectRenderers[i] != null)
            {
                effectRenderers[i].sortingOrder = order;
            }
        }
    }
}
