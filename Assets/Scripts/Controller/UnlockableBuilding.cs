using Controller.Structure;
using Module;
using Module.Data;
using UnityEngine;
using Utils;

namespace Controller
{
    public class UnlockableBuilding : StructureBase
    {
        public BuildingData data; // 判断条件（等级等）

        public GameObject lockedVisual;
        public GameObject unlockedVisual;

        private bool isUnlocked = false;
        PlayerData playerData;
        void Start()
        {
            playerData = ModuleMgr.Instance.GetModule<PlayerDataModule>().data;
            CheckUnlock();
        }

        public void CheckUnlock()
        {
            int playerLevel = playerData.accountLevel ;

            if (playerLevel >= data.requiredLevel)
                Unlock();
            else
                Lock();
        }

        public void Unlock()
        {
            if (isUnlocked) return;

            isUnlocked = true;
            lockedVisual.SetActive(false);
            unlockedVisual.SetActive(true);

            PlayUnlockAnimation();
        }

        public void Lock()
        {
            isUnlocked = false;
            lockedVisual.SetActive(true);
            unlockedVisual.SetActive(false);
        }

        void PlayUnlockAnimation()
        {
            // 播放相机动画 / 特效 / 音效
        }
    }
}
