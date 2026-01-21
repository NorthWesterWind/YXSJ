using Module;
using Module.Data;
using UnityEngine;

namespace Utils
{
    public class Launcher : MonoSingleton<Launcher>
    {
        PlayerData playerData;
        public override void Awake()
        {
            base.Awake();
            HandleModule();
        }

        public void HandleModule()
        {
          
        }
    }
}
