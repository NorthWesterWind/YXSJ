using System.Collections;
using System.Collections.Generic;
using Utils;

namespace Controller.Pickups
{
    public class ScenePickupController : MonoSingleton<ScenePickupController>
    {
      public List<BasePickup> materials = new ();
      public List<BasePickup> products = new ();
   
    }
}
