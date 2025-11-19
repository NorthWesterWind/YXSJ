using UnityEngine;

namespace Controller.Pickups
{
    public interface IPickable 
    {
       void OnPicked(GameObject picker);
    }
}
