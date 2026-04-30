// using UnityEngine;

// public class TestAccountSetting
// {
//     private static string userName;
//     private static string NAME_PREFIX = "Rymxt";
//     private static string NAME_SUFFIX; //后缀有4位数，千位代表是否成年，百位代表等级，十位代表周岁阶段，个位代表同阶位数量
    
//     public static void Init()
//     {
//         Clear();
        
//         if (AccountManager.Instance?.CurrentAccount == null) return;
//         userName = AccountManager.Instance.CurrentAccount.username;
        
//         int charCount = NAME_PREFIX.Length;
//         if (!string.IsNullOrEmpty(userName) && userName.StartsWith(NAME_PREFIX) && userName.Length == charCount + 4)
//         {
//             string tempSuffix = userName.Substring(charCount, 4);
            
//             // --- 核心判断逻辑 ---
//             if (IsPureNumber(tempSuffix)) 
//             {
//                 NAME_SUFFIX = tempSuffix;
//                 Debug.Log($"测试账号解析成功：{NAME_SUFFIX}");
//             }
//             else
//             {
//                 Debug.LogError($"测试账号格式错误：后缀 '{tempSuffix}' 包含非数字字符！");
//                 NAME_SUFFIX = null; 
//             }
//         }
//     }
    
//     /// <summary>
//     /// 判断账号是否成年
//     /// 0表示本方法不判断，1表示成年，2表示未成年
//     /// </summary>
//     /// <returns></returns>
//     public static int GetAccountMaturity()
//     {
//         if (string.IsNullOrEmpty(NAME_SUFFIX)) return 0;
//         int val = NAME_SUFFIX[0] - '0';
//         return Mathf.Clamp(val, 0, 2); // 限制在注释定义的 0-2 范围内
//     }

//     /// <summary>
//     /// 设置账号等级
//     /// 0没等级，1表示初级账号，2表示中级账号，3表示高级账号
//     /// 用于跳过实名认证
//     /// </summary>
//     /// <returns></returns>
//     public static int GetAccountLevel()
//     {
//         if (string.IsNullOrEmpty(NAME_SUFFIX)) return 0;
//         int level = NAME_SUFFIX[1] - '0';
//         return Mathf.Clamp(level,0,3);
//     }

//     /// <summary>
//     /// 设置账号年龄
//     /// 0表示不在初设阶段内，1表示>=18，2表示未满8周岁，3表示8-16周岁，4表示16-18周岁
//     /// </summary>
//     /// <returns></returns>
//     public static int GetAccountAge()
//     {
//         if (string.IsNullOrEmpty(NAME_SUFFIX)) return 0;

//         return NAME_SUFFIX[2] switch
//         {
//             '1' => 18,
//             '3' => 12,//或8
//             '4' => 16,
//             _ => 0,
//         };
//     }

//     /// <summary>
//     /// 表示同阶段的数量，与其他测试账号区分开，一般不用
//     /// </summary>
//     /// <returns></returns>
//     public static int GetAccountCount()
//     {
//         if (string.IsNullOrEmpty(NAME_SUFFIX)) return 0;
        
//         return NAME_SUFFIX[3] - '0';
//     }
    

//     private static void Clear()
//     {
//         userName = null;
//         // NAME_PREFIX = null; 前缀保留
//         NAME_SUFFIX = null;
//     }
    
//     private static bool IsPureNumber(string str)
//     {
//         foreach (char c in str)
//         {
//             if (!char.IsDigit(c)) return false;
//         }
//         return true;
//     }
// }
