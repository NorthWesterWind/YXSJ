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
        public Image fillImage;
        public Image fillBg;
        public Canvas canvas;
        public TextMeshProUGUI text;
        public PlayerController player;
        public Image bagIcon;
        public AssetHandle _assetHandle;
        private void Start()
        {


            _assetHandle = GetComponent<AssetHandle>();
            HideHpInfo();

            EventCenter.Instance.AddListener(EventMessages.UpdatePlayerEquimentInfo, UpdateBagInfo);
            EventCenter.Instance.AddListener(EventMessages.UpdatePlayerInfo, UpdateTxt);

        }
        void OnDestroy()
        {
            EventCenter.Instance.RemoveListener(EventMessages.UpdatePlayerEquimentInfo, UpdateBagInfo);
            EventCenter.Instance.RemoveListener(EventMessages.UpdatePlayerInfo, UpdateTxt);
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





        private void OnEnable()
        {

        }

        private void OnDisable()
        {

        }


        private void Update()
        {

        }

        public void UpdateFill(float value)
        {
            ShowHpInfo();
            fillImage.DOFillAmount(Mathf.Min(value, 1), 0.3f);
        }


        public void UpdateTxt(params object[] args)
        {
            int carryNum = 0;
            if (player.dropDic != null)
            {
                foreach (var kv in player.dropDic)
                {
                    if (kv.Value > 0)
                    {
                        carryNum += kv.Value;
                    }
                }
            }
            if (player.goodsDic != null)
            {
                foreach (var kv in player.goodsDic)
                {
                    if (kv.Value > 0)
                    {
                        carryNum += kv.Value;
                    }
                }
            }
            player.currentCarryNum = carryNum;

            if (carryNum >= player.maxCarryNum)
            {
                text.text = "储物袋已满";
            }
            else
            {
                text.text = $"{carryNum}/{player.maxCarryNum}";
            }
            EventCenter.Instance.TriggerEvent(EventMessages.UpdatePlayerCarryInfo);
        }
    }
}
