using Module.Data;
using UnityEngine;
using Utils;

namespace Controller.Structure
{
    public class StructureBase : MonoBehaviour
    {
        public BuildingType structureType;
        public SpriteRenderer sprite;
        protected AssetHandle _assetHandle;
        protected virtual void Start()
        {
            int newOrder = 3000 - Mathf.FloorToInt(transform.localPosition.y);
            sprite.sortingOrder = newOrder;
            _assetHandle = GetComponent<AssetHandle>();
        }

      
        
    }
}