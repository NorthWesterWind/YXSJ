using UnityEngine;

namespace Controller
{
    public class StructureController : MonoBehaviour
    {
        public SpriteRenderer sprite;
        public Transform pointTransform;
        protected virtual void Start()
        {
            int newOrder = 3000 - Mathf.FloorToInt(transform.localPosition.y);
            sprite.sortingOrder = newOrder;
        }
        
    }
}