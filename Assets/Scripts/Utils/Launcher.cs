using Module;
using UnityEngine;

namespace Utils
{
    public class Launcher : MonoSingleton<Launcher>
    {
        public override void Awake()
        {
            base.Awake();
            HandleModule();
        }

        public void HandleModule()
        {
            ModuleMgr.Instance.RegisterModule<PlayerDataModule>();
        }
    }
}
