using Controller;
using Controller.Player;
using Module;
using Module.Data;
using TMPro;
using UnityEngine;
using Utils;

public class StructureLock : MonoBehaviour
{
    public SpriteRenderer structureSprite;
    public GameObject fill;
    public TextMeshPro needText;
    public GameObject lockSprite;

    public BuildingType buildType;
    private AssetHandle _assetHandle;

    public Transform receiveTransform;
    private StructureLockData _data;

    public GameObject bg;
    private bool playerInRange;
    private PlayerController player;
    public bool isLocked = true;

    public void InitInfo(StructureLockData data)
    {
        _data = data;
        buildType = _data.buildingType;
        structureSprite.sprite = _assetHandle.Get<Sprite>(Extensions.GetStructureResNameByType(buildType));
        PlayerData playerData = ModuleMgr.Instance.GetModule<PlayerDataModule>().data;
        StructureLockProgressData progressData = playerData.structureLockDataList.Find(s => s.buildType == buildType && s.lockId == data.lockId && s.mapId == playerData.currentMapID);
        if (progressData != null)
        {
            float fillWidth = Mathf.Clamp01(progressData.currentOwnMoney / _data.needMoney);
            fill.transform.localScale = new Vector3(fillWidth, 1, 1);
            needText.text = $"{_data.needMoney - (int)progressData.currentOwnMoney}";
            if (progressData.isUnlock)
            {
                lockSprite.gameObject.SetActive(false);
                bg.SetActive(false);
            }
            else
            {
                lockSprite.gameObject.SetActive(true);
                bg.SetActive(true);
            }
        }
        else
        {
            lockSprite.gameObject.SetActive(true);
            bg.SetActive(false);
        }

    }

    #region Trigger

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = true;
        player = other.GetComponent<PlayerController>();
        OnEnter();

    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = false;
        player = null;
        OnExit();

    }

    #endregion



    void OnEnable()
    {
        EventCenter.Instance.AddListener(EventMessages.ThrowOutTongBi, OnPlayerThrowTongBi);
    }
    void OnDisable()
    {
        EventCenter.Instance.RemoveListener(EventMessages.ThrowOutTongBi, OnPlayerThrowTongBi);
    }

    private void OnPlayerThrowTongBi(params object[] args)
    {
        Transform t = (Transform)args[0];
        if (t != receiveTransform)
            return;
        var progress = GetProgressData();
        progress.currentOwnMoney += 100;
        UpdateProgress(progress.currentOwnMoney);
    }

    public void OnExit()
    {
        player.InteractionTriggerInRange = false;
        player.InteractionTriggerTransform = null;
        PlayerData playerData = ModuleMgr.Instance.GetModule<PlayerDataModule>().data;
        StructureLockProgressData progressData = playerData.structureLockDataList.Find(s => s.buildType == buildType && s.mapId == playerData.currentMapID);
        float fillWidth = Mathf.Clamp01(progressData.currentOwnMoney / _data.needMoney);
        fill.transform.localScale = new Vector3(fillWidth, 1, 1);
    }

    public void OnEnter()
    {
        player.InteractionTriggerInRange = true;
        player.InteractionTriggerTransform = receiveTransform;
    }


    public void UpdateProgress(float value)
    {
        float percent = value / _data.needMoney;
        fill.transform.localScale = new Vector3(percent, 1, 1);
        needText.text = $"{_data.needMoney - Mathf.CeilToInt(value)}";
        if (percent >= 1f && isLocked)
        {
            Unlock();
        }
    }

    private StructureLockProgressData GetProgressData()
    {
        var playerData = ModuleMgr.Instance
            .GetModule<PlayerDataModule>().data;

        return playerData.structureLockDataList.Find(
            x => x.buildType == _data.buildingType &&
                 x.mapId == playerData.currentMapID);
    }

    private void Unlock()
    {
        isLocked = false;
        bg.SetActive(false);
        var data = GetProgressData();
        if (data != null)
        {
            data.isUnlock = true;
            data.currentOwnMoney = _data.needMoney;
        }
        if (player.InteractionTriggerTransform == receiveTransform)
        {
            player.InteractionTriggerInRange = false;
            player.InteractionTriggerTransform = null;
        }
        EventCenter.Instance.TriggerEvent(EventMessages.StructureLockUnlocked, _data);
    }

}
