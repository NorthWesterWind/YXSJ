using System.Collections;
using Module.Data;
using UnityEngine;
using Utils;
using Random = UnityEngine.Random;

namespace Controller
{
    public class CustomerFactory : MonoBehaviour
    {
       private AssetHandle _assetHandle;

       public CustomerData data;

        public Transform[] positions;
       private void Start()
       {
           _assetHandle = GetComponent<AssetHandle>();
           StartCoroutine(CreatCustomer());
       }

       public  IEnumerator CreatCustomer()
       {
           while(true)
           {
               GameObject obj = Instantiate(_assetHandle.Get<GameObject>("Npc") );
               obj.transform.position = GetRandomPosition();
               obj.GetComponent<CustomerController>().Init(data , positions);
               yield return new  WaitForSeconds(6);
           }
         
       }

       private Vector3 GetRandomPosition()
       {
           Vector3 position = transform.position;
           position.x += Random.Range(-5f, 5f);
           position.y += Random.Range(-5f, 5f);
           return position;
           
       }
    }
}
