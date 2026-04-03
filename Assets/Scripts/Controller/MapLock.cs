using System.Collections;
using System.Collections.Generic;
using Controller;
using Controller.Player;
using Module;
using Module.Data;
using TMPro;
using UnityEngine;
using Utils;
using View;

public class MapLock : MonoBehaviour
{
    public MonsterType monsterType;
    public bool isLocked = true;
    public GameObject lockObject;

    public GameObject bg;
    public GameObject fill;
    public TextMeshPro needText;

    public MapLockData mapLockData;

    // private ILockInteractStrategy interactStrategy;

    public bool playerInRange;
    private PlayerController player;

    public Transform receiveTransform;
    private Coroutine autoThrowCoroutine;
    private const float AutoThrowInterval = 0.25f;

    void OnEnable()
    {
        EventCenter.Instance.AddListener(EventMessages.ThrowOutTongBi, OnPlayerThrowTongBi);
        EventCenter.Instance.AddListener(EventMessages.UpdateMapLockState, HandleUpdateState);

    }
    void OnDisable()
    {
        EventCenter.Instance.RemoveListener(EventMessages.ThrowOutTongBi, OnPlayerThrowTongBi);
        EventCenter.Instance.RemoveListener(EventMessages.UpdateMapLockState, HandleUpdateState);
        StopAutoThrow();
    }

    private void OnPlayerThrowTongBi(params object[] args)
    {
        Transform t = (Transform)args[0];
        if (t != receiveTransform)
            return;
        var progress = GetProgressData();
        if (progress == null)
            return;
        progress.currentOwnMoney += 100;
        UpdateProgress(progress.currentOwnMoney);
    }

    void Update()
    {
        // if (!playerInRange || interactStrategy == null || !isLocked)
        //     return;
    }
    public void HandleUpdateState(params object[] args)
    {
        if ((MonsterType)args[0] != monsterType)
            return;
        var progress = GetProgressData();
        if (progress != null)
        {
            isLocked = !progress.isUnlock;
            if (!isLocked)
            {
                lockObject.SetActive(false);
                bg.SetActive(false);
            }
            else
            {
                lockObject.SetActive(true);
                bg.SetActive(progress.canShowBg);
                UpdateProgress(progress.currentOwnMoney);
            }
        }
        else
        {
            lockObject.SetActive(true);
            bg.SetActive(false);
        }
    }

    public void Init(MapLockData data)
    {
        mapLockData = data;
        monsterType = data.monsterType;
        var progress = GetProgressData();
        if (progress != null)
        {
            isLocked = !progress.isUnlock;
            if (!isLocked)
            {
                lockObject.SetActive(false);
                bg.SetActive(false);
            }
            else
            {
                lockObject.SetActive(true);
                bg.SetActive(progress.canShowBg);
                UpdateProgress(progress.currentOwnMoney);
            }
        }
        else
        {
            lockObject.SetActive(true);
            bg.SetActive(false);
        }
    }

    #region Progress

    public float LoadProgress()
    {
        var data = GetProgressData();
        return data?.currentOwnMoney ?? 0f;
    }

    public void SaveProgress(float value)
    {
        var data = GetProgressData();
        if (data != null)
            data.currentOwnMoney = value;
    }

    public void UpdateProgress(float value)
    {
        float percent = value / mapLockData.needMoney;
        fill.transform.localScale = new Vector3(percent, 1, 1);
        needText.text = $"{mapLockData.needMoney - Mathf.CeilToInt(value)}";
        if (percent >= 1f && isLocked)
        {
            Unlock();
        }
    }

    private MapLockDataProgress GetProgressData()
    {
        var playerData = PlayerDataModule.Instance.data;

        return playerData.mapLockDataProgressList.Find(
            x => x.lockId == mapLockData.lockId &&
                 x.mapId == playerData.currentMapID);
    }

    #endregion

    #region Trigger

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!isLocked) return;
        if (!other.CompareTag("Player")) return;

        playerInRange = true;

        player = other.GetComponent<PlayerController>();
        player.InRange = true;
        Debug.Log("yj = > 进入区域解锁范围");
        StartAutoThrow();


    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!isLocked) return;
        if (!other.CompareTag("Player")) return;
        player.InRange = false;
        playerInRange = false;
        StopAutoThrow();
        player = null;
    }

    #endregion

    private void Unlock()
    {
        isLocked = false;
        StopAutoThrow();
        lockObject.SetActive(false);
        bg.SetActive(false);

        var data = GetProgressData();
        if (data != null)
        {
            data.isUnlock = true;
            data.currentOwnMoney = mapLockData.needMoney;
        }
        PlayerDataModule.Instance.data.goldIngot += DataController.Instance.giftpackDataDic[4].JinYuanBao;
        var dic = PlayerDataModule.Instance.LotteryCard(DataController.Instance.giftpackDataDic[4]);
        UIController.Instance.Show<RewardConfirmView>(dic, new Dictionary<CurrencyType, int> { { CurrencyType.JingYuanBao, DataController.Instance.giftpackDataDic[4].JinYuanBao } });
        EventCenter.Instance.TriggerEvent(EventMessages.UpdatePlayerMoneyInfo);
        EventCenter.Instance.TriggerEvent(EventMessages.MapLockUnlocked, mapLockData);
        EventCenter.Instance.TriggerEvent(EventMessages.UnLockMapTask, monsterType);
        UIController.Instance.Show<TipView>( Extensions.GetSMonsterRegionNameByType(monsterType) +"区域解锁成功！");
    }

    private void StartAutoThrow()
    {
        if (autoThrowCoroutine != null)
            return;
        autoThrowCoroutine = StartCoroutine(AutoThrowCoinCoroutine());
    }

    private void StopAutoThrow()
    {
        if (autoThrowCoroutine == null)
            return;
        StopCoroutine(autoThrowCoroutine);
        autoThrowCoroutine = null;
    }

    private IEnumerator AutoThrowCoinCoroutine()
    {
        while (CanAutoThrow())
        {
            if (player != null && !player.isMoving)
            {
                player.ThrowOutTongBi(receiveTransform);
            }
            yield return new WaitForSeconds(AutoThrowInterval);
        }

        autoThrowCoroutine = null;
    }

    private bool CanAutoThrow()
    {
        return isLocked &&
               playerInRange &&
               player != null &&
               receiveTransform != null &&
               lockObject != null &&
               lockObject.activeSelf &&
               GetProgressData() != null;
    }
}
