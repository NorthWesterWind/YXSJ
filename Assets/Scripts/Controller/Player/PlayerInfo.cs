using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace Controller.Player
{
    public class PlayerInfo : MonoBehaviour
    {
        public Transform target; // 角色头顶挂点
        public Vector3 offset;   // 屏幕偏移（比如往上抬一点）
        public Image fillImage;
        public Image fillBg;
        public Canvas canvas;
        public TextMeshProUGUI text;
        public PlayerController player;
        public Image bagIcon;
        public AssetHandle _assetHandle;
        private void Start()
        {
            if (player == null)
            {
                player = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
                target = player.infoTransform;
                player.playerInfo = this;
            }
            _assetHandle = GetComponent<AssetHandle>();
            HideHpInfo();

            EventCenter.Instance.AddListener(EventMessages.UpdatePlayerEquimentInfo, UpdateBagInfo);
          
        }
        void OnDestroy()
        {
            EventCenter.Instance.RemoveListener(EventMessages.UpdatePlayerEquimentInfo, UpdateBagInfo);
        }


        public void UpdateBagInfo(params object[] args)
        {
            StotageBagData stotageBagData = DataController.Instance.storageBagDataDic[player.dataModule.data.currentBag];
            bagIcon.sprite = _assetHandle.Get<Sprite>(stotageBagData.name);
        }

        public void HideHpInfo()
        {
            fillBg.gameObject.SetActive(false);
        }

        public void ShowHpInfo()
        {
            fillBg.gameObject.SetActive(true);
        }



        private Coroutine uiRoutine;

        private void OnEnable()
        {
            uiRoutine = StartCoroutine(UpdateUIRoutine());
        }

        private void OnDisable()
        {
            if (uiRoutine != null)
                StopCoroutine(uiRoutine);
        }

        private IEnumerator UpdateUIRoutine()
        {
            while (true)
            {
                yield return new WaitForEndOfFrame();

                Vector3 screenPos = Camera.main.WorldToScreenPoint(target.position + offset);
                transform.position = screenPos;

                SetLayer();
            }
        }

        private void Update()
        {

        }

        public void SetLayer()
        {
            int newOrder = 30000 - Mathf.FloorToInt(player.transform.localPosition.y);
            canvas.sortingOrder = newOrder;
        }

        public void UpdateFill(float value)
        {
            ShowHpInfo();
            fillImage.DOFillAmount(Mathf.Min(value, 1), 0.3f);
        }


        public void UpdateTxt()
        {
            if (player.currentCarryNum >= player.maxCarryNum)
            {
                text.text = "储物袋已满";
            }
            else
            {
                text.text = $"{player.currentCarryNum}/{player.maxCarryNum}";
            }
            Debug.LogError($"yj=> player.maxCarryNum = {player.maxCarryNum} ");
            EventCenter.Instance.TriggerEvent(EventMessages.UpdatePlayerCarryInfo);
        }
    }
}
