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
        public StructureLock structureLock;
        public GameObject content;
        protected virtual void Start()
        {
            int newOrder = 3000 -  Mathf.RoundToInt(transform.position.y * 100);
            sprite.sortingOrder = newOrder;
            _assetHandle = GetComponent<AssetHandle>();
        }

      
        
    }
}