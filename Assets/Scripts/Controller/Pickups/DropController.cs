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
            canPickup = false;
            ScenePickupController.Instance.materials.Add(this);
            itemName = "DropObj";
        }
        
        public void OnPicked(GameObject picker)
        {
            if (picker.GetComponent<CharacterController>() != null)
            {
                picker.GetComponent<CharacterController>().AddDropItem(itemType);
            }else if (picker.GetComponent<CollectorController>() != null)
            {
                picker.GetComponent<CollectorController>().AddDropItem(itemType);
            }
           
        }
    }
}
