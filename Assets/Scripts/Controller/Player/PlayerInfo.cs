using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Controller.Player
{
    public class PlayerInfo : MonoBehaviour
    {
        public Transform target; // 角色头顶挂点
        public Vector3 offset;   // 屏幕偏移（比如往上抬一点）
        public Image fillImage;
        public Image fillBg;
        public Canvas  canvas;
        public TextMeshProUGUI text;
        public CharacterController  player;
        private void Start()
        {
            if ( player == null)
            {
                 player = GameObject.FindWithTag("Player").GetComponent<CharacterController>();
                 target =  player.infoTransform;
                 player.playerInfo = this;
            }
            HideHpInfo();
        }

        public void HideHpInfo()
        {
            fillBg.gameObject.SetActive(false);
        }

        public void ShowHpInfo()
        {
            fillBg.gameObject.SetActive(true);
        }

      

       private void LateUpdate()
       {
           StartCoroutine(UpdateUIPosition());
       }

       private IEnumerator UpdateUIPosition()
       {
           yield return new WaitForEndOfFrame(); // 等摄像机完全更新完

           Vector3 screenPos = Camera.main.WorldToScreenPoint(target.position + offset);
           transform.position = screenPos;

           SetLayer();
       }

        private void Update()
        {
            
        }

        public void SetLayer()
        {
            int newOrder = 3000 - Mathf.FloorToInt(player.transform.localPosition.y);
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
        }
    }
}
