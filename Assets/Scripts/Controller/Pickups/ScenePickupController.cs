using System.Collections.Generic;
using UnityEngine;
using Utils;

namespace Controller.Pickups
{
    public class ScenePickupController : SingletonBase<ScenePickupController>
    {
      public List<BasePickup> pickups = new ();
    }
}
