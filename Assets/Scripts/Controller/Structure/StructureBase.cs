using Module;
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
        public bool isLock;
        public bool isCanUnlockState;
        protected virtual void Start()
        {
            int newOrder = 30000 - Mathf.RoundToInt(transform.position.y * 100);
            sprite.sortingOrder = newOrder;
            _assetHandle = GetComponent<AssetHandle>();
        }

        void OnEnable()
        {
            AddEvent();
        }

        void OnDisable()
        {
            RemoveEvent();
        }

        public void ShowLock(StructureLockData lockData)
        {
            content.SetActive(false);
            structureLock.gameObject.SetActive(true);
            structureLock.InitInfo(lockData);
        }
        public virtual void AddEvent()
        {
            
        }
        public virtual void RemoveEvent()
        {
            
        }
    }
}