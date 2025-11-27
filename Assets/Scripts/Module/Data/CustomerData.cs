using UnityEngine;
using Utils;

namespace Module.Data
{
    //购买的商品类型
 
    [System.Serializable]
    public class CustomerData 
    {
        public CustomerType type;  // 顾客类型
        public int carryNum;  // 可携带数量
        public int waitTime;  // 等待时间
        public int movespeed; // 移动速度
    }
}
