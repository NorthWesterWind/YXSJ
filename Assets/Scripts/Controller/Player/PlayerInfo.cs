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
        }

        public void HideHpInfo()
        {
            fillImage.gameObject.SetActive(false);
        }

        public void ShowHpInfo()
        {
            fillImage.gameObject.SetActive(true);
        }

        private void LateUpdate()
        {
            if (target == null) return;

            // 直接使用世界坐标
            transform.position = target.position + offset;
            SetLayer();
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
            text.text = $"{player.currentCarryNum}/{player.maxCarryNum}";
        }
    }
}
