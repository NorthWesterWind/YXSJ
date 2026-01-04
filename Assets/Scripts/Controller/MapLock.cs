using Controller.Player;
using Module;
using Module.Data;
using TMPro;
using UnityEngine;
using Utils;

public class MapLock : MonoBehaviour
{
    public MonsterType monsterType;
    public bool isLocked = true;
    public GameObject lockObject;

    public GameObject bg;
    public GameObject fill;
    public TextMeshProUGUI needText;

    public MapLockData mapLockData;

    private ILockInteractStrategy interactStrategy;

    private bool playerInRange;
    private PlayerController player;

    public Transform receiveTransform;


    void Update()
    {
        if (!playerInRange || interactStrategy == null || !isLocked)
            return;

        if (interactStrategy.IsFinished)
        {
            Unlock();
        }
    }

    public void Init(MapLockData data)
    {
        mapLockData = data;
        monsterType = data.monsterType;

        PlayerData playerData = ModuleMgr.Instance
            .GetModule<PlayerDataModule>().data;

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
        needText.text = $"{Mathf.CeilToInt(value)}/{mapLockData.needMoney}";
    }

    private MapLockDataProgress GetProgressData()
    {
        var playerData = ModuleMgr.Instance
            .GetModule<PlayerDataModule>().data;

        return playerData.mapLockDataProgressList.Find(
            x => x.lockId == mapLockData.lockId &&
                 x.mapId == playerData.currentMapID);
    }

    #endregion

    #region Trigger

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = true;
        player = other.GetComponent<PlayerController>();
        interactStrategy?.OnEnter(this, player);
       
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = false;
        player = null;
        interactStrategy?.OnExit();
       
    }

    #endregion

    private void Unlock()
    {
        isLocked = false;
        lockObject.SetActive(false);
        bg.SetActive(false);

        var data = GetProgressData();
        if (data != null)
        {
            data.isUnlock = true;
            data.currentOwnMoney = mapLockData.needMoney;
        }
    }
}