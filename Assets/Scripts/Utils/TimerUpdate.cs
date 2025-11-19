using UnityEngine;

namespace Utils
{
    public class TimerUpdate : MonoSingleton<TimerUpdate>
    {
        private void Update()
        {
            TimerSystem.Update(Time.deltaTime);
        }
    }
}