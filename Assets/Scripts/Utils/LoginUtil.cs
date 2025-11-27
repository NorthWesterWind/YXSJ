using System;
using System.Collections;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace Utils
{
    [Serializable]
    public class ResponseLogin
    {
        public int state;
        public string msg;
        public int fcm;
        public int age;
        public int recharge;
        public string more;
        public int account_level;
    }

    public class ResponseRegister
    {
        public int state;
        public int code;
        public int fcm;
        public string msg;
    }

    public class ResponseRealName
    {
        public int state;
        public int age;
        public string msg;
    }

    public class ResponseClear
    {
        public int state;
        public string msg;
    }

    public class ResponseFindPassword
    {
        public int state;
        public string msg;
        public string pw;
        public string users;
    }

    public class ResponseSaveData
    {
        public int state;
        public string msg;
        public string timestamp;
        public string users;
    }
    [System.Serializable]
    public class AuthResponse
    {
        public int error_code;    // 错误码(0表示成功)
        public string reason;     // 状态说明
        public ResultData result; // 主要结果数据
        public string sn;         // 序列号
    }
    [System.Serializable]
    public class ResultData
    {
        public string realname;        // 脱敏姓名(如"史*")
        public string idcard;          // 脱敏身份证号
        public bool isok;              // 是否验证通过
        public IdCardInfo IdCardInfor; // 身份证详细信息
    }

    [System.Serializable]
    public class IdCardInfo
    {
        public string province; // 省份
        public string city;     // 城市
        public string district; // 区县
        public string area;     // 完整地区
        public string sex;      // 性别
        public string birthday; // 生日(yyyy-M-d格式)
    }
    public class LoginUtil : MonoSingleton<LoginUtil>
    {
        private string registerurl = "https://banhao2.dyhyyx.com/php/zhuce.php";
        private  string Loginurl = "https://banhao2.dyhyyx.com/php/denglu.php";
        private string realnameurl = "https://banhao2.dyhyyx.com/php/shiming.php";
        private string saveurl = "https://banhao2.dyhyyx.com/php/cunchu.php";
        private  string clearurl = "https://banhao2.dyhyyx.com/php/zhuxiao.php";

        public void RegisterCheck(string user, string password, Action<ResponseRegister> callback)
        {
            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(password))
            {
               // UIController.Instance.Show<TipView>("账号或密码不能为空");
                return;
            }

            StartCoroutine(GetRegisterDataCoroutine(user, password, callback));
        }

        private IEnumerator GetRegisterDataCoroutine(string user, string password, Action<ResponseRegister> callback)
        {
            WWWForm form = new WWWForm();
            form.AddField("user", user);
            form.AddField("password", password);
            form.AddField("app_name", GameName.App_name);

            using (UnityWebRequest webRequest = UnityWebRequest.Post(registerurl, form))
            {
                Debug.Log($"webRequest.url = {webRequest.url}");
                webRequest.timeout = 30;

                yield return webRequest.SendWebRequest();
                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log("注册请求成功：" + webRequest.downloadHandler.text);
                    ResponseRegister response = JsonUtility.FromJson<ResponseRegister>(webRequest.downloadHandler.text);
                    callback?.Invoke(response);
                }
                else
                {
                    Debug.LogError("注册请求失败：" + webRequest.error);
                }
            }
        }

        public void LoginCheck(string user, string password, Action<ResponseLogin> callback)
        {
            StartCoroutine(GetLoginDataCoroutine(user, password, callback));
        }

        private IEnumerator GetLoginDataCoroutine(string user, string password, Action<ResponseLogin> callback)
        {
            WWWForm form = new WWWForm();
            form.AddField("user", user);
            form.AddField("password", password);
            form.AddField("app_name", GameName.App_name);
            using (UnityWebRequest webRequest = UnityWebRequest.Post(Loginurl, form))
            {
                webRequest.timeout = 30;

                yield return webRequest.SendWebRequest();

                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    try
                    {
                        string responseText = webRequest.downloadHandler.text;
                        Debug.Log(
                            $"登录信息 ：user = {user} ,  password = {password} , webRequest.result = {webRequest.result} ");
                        ResponseLogin responseLogin = JsonConvert.DeserializeObject<ResponseLogin>(responseText);
                        Debug.Log($"responseLogin = {responseText}");
                        if (responseLogin != null)
                        {
                            callback?.Invoke(responseLogin);
                        }
                        else
                        {
                            callback?.Invoke(null);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"JSON解析错误: {ex.Message}");
                        callback?.Invoke(null);
                    }
                }
                else
                {
                    Debug.LogError($"登录失败: {webRequest.error}, URL: {Loginurl}");
                    callback?.Invoke(null);
                }
            }
        }


        // public void SaveToServer()
        // {
        //     StartCoroutine(UploadPlayerDataCoroutine());
        // }

        // private IEnumerator UploadPlayerDataCoroutine()
        // {
        //     WWWForm form = new WWWForm();
        //     form.AddField("user", ModuleMgr.Instance.GetModule<PlayerDataModule>().data.user);
        //     form.AddField("password", PlayerDataModule.password);
        //     form.AddField("user_more", JsonConvert.SerializeObject(PlayerDataModule.Instance._playerData));
        //     form.AddField("app_name", GameName.App_name);
        //     Debug.Log(
        //         $"JsonConvert.SerializeObject( PlayerDataModule.Instance._playerData) = {JsonConvert.SerializeObject(PlayerDataModule.Instance._playerData)}");
        //     Debug.Log(
        //         $"PlayerDataModule.Instance._playerData.playerChessProgressList.Count = {PlayerDataModule.Instance._playerData.playerChessProgressList.Count}");
        //     using (UnityWebRequest webRequest = UnityWebRequest.Post(saveurl, form))
        //     {
        //         webRequest.timeout = 30;
        //
        //         yield return webRequest.SendWebRequest();
        //         if (webRequest.result == UnityWebRequest.Result.Success)
        //         {
        //             Debug.Log("上传数据成功：" + webRequest.downloadHandler.text);
        //             ResponseSaveData response = JsonUtility.FromJson<ResponseSaveData>(webRequest.downloadHandler.text);
        //             if (response.state == 2)
        //             {
        //                 Debug.Log("更新数据成功");
        //             }
        //             else if (response.state == 3)
        //             {
        //                 Debug.Log("错误");
        //             }
        //             else if (response.state == 4)
        //             {
        //                 Debug.Log("用户不存在");
        //             }
        //         }
        //         else
        //         {
        //             Debug.LogError("上传数据失败：" + webRequest.error);
        //         }
        //
        //         PlayerDataModule.Instance.FixPlayerData();
        //     }
        // }
        //
        
        public void RealName(string user, string idnum, string chinese, string fcmLvl,
            Action<ResponseRealName> callback)
        {
            StartCoroutine(GetRealNameCoroutine(user, idnum, chinese, fcmLvl, callback));
        }

        private IEnumerator GetRealNameCoroutine(string user, string idnum, string chinese, string fcmLvl,
            Action<ResponseRealName> callback)
        {
            WWWForm form = new WWWForm();
            form.AddField("user", user);
            form.AddField("idnum", idnum);
            form.AddField("chinese", chinese);
            form.AddField("fcmLvl", fcmLvl);
            form.AddField("app_name", GameName.App_name);

            using (UnityWebRequest webRequest = UnityWebRequest.Post(realnameurl, form))
            {
                webRequest.timeout = 30;

                yield return webRequest.SendWebRequest();
                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log("实名请求成功：" + webRequest.downloadHandler.text);
                    ResponseRealName responseRealName =
                        JsonUtility.FromJson<ResponseRealName>(webRequest.downloadHandler.text);
                    callback(responseRealName);
                }
                else
                {
                    Debug.LogError("实名请求失败：" + webRequest.error);
                }
            }
        }

        public void ClearUser(string user, Action<ResponseClear> callback)
        {
            StartCoroutine(GetClearUserCoroutine(user, callback));
        }

        private IEnumerator GetClearUserCoroutine(string user, Action<ResponseClear> callback)
        {
            WWWForm form = new WWWForm();
            form.AddField("user", user);
            form.AddField("app_name", GameName.App_name);

            using (UnityWebRequest webRequest = UnityWebRequest.Post(clearurl, form))
            {
                webRequest.timeout = 30;

                yield return webRequest.SendWebRequest();
                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log("注销请求成功：" + webRequest.downloadHandler.text);
                    ResponseClear responseRealName =
                        JsonUtility.FromJson<ResponseClear>(webRequest.downloadHandler.text);
                    callback(responseRealName);
                }
                else
                {
                    Debug.LogError("注销请求失败：" + webRequest.error);
                }
            }
        }
    }

    public static class GameName
    {
        private static  string app_name = "Qhmx";

        public static  string App_name
        {
            get => app_name;
            set => app_name = value;
        }
    }
}