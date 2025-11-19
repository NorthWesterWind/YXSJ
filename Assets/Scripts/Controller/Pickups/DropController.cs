using Module.Data;
using UnityEngine;
using CharacterController = Controller.Player.CharacterController;

namespace Controller.Pickups
{
    /// <summary>
    /// 怪物死亡后的材料控制脚本
    /// </summary>
    public class DropController : BasePickup ,IPickable
    {
        
        private CharacterController _characterController;
        public DropItemType itemType;
        public SpriteRenderer spriteRenderer;
        
        public void Init(DropItemType type)
        {
            itemType = type;
            //可以用于加载图片
            
            isAttracted = true;
            ScenePickupController.Instance.pickups.Add(this);
        }
        
        public void OnPicked(GameObject picker)
        {
            picker.GetComponent<CharacterController>().AddDropItem(itemType);
        }
    }
}
