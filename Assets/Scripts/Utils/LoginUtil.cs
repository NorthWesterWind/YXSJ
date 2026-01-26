using System;
using System.Collections;
using System.ComponentModel;
using Module;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using View;

namespace Utils
{
    [Serializable]
    public class ResponseLogin
    {
        public int account_level;
        public int age;
        public int fcm;

        [JsonProperty("id", DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue(0)]
        public int id;
        public string more;
        public string msg;
        public int users;
        public int password;
        public int recharge;
        public int state;
        public string user_uuid;
        public string timestamp;

    }

    public class ResponseRegister
    {
        public int state;
        public string msg;
        public int fcm;
        public int code;
        public ResData res;
        public string timestamp;

    }
    public class ResData
    {
        public int id;
        public string user_login;
        public string user_pass;
        public string user_idnum;
        public int user_fcm;
        public int user_recharge;
        public int user_vip;
        public int user_age;
        public string user_name;
        public string user_item;
        public string user_app_name;
        public int account_level;
        public string user_more;
        public int user_zhanli;
        public int user_level;
        public string user_uuid;
        public int increase_power;
        public int decrease_power;
        public int user_currentLv;
    }

    public class ResponseRealName
    {
        public int state;
        public int age;
        public string msg;
        public int fcm;
        public string timestamp;
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
        public SaveUser user;
    }
    public class SaveUser
    {
        public int id;
        public int user_age;
        public int user_fcm;
        public int user_vip;
        public string user_item;
        public string user_more;
        public string user_name;
        public string user_pass;
        public string user_uuid;
        public string user_idnum;
        public int user_level;
        public string user_login;
        public int user_zhanli;

        public int account_level;
        public string user_app_name;
        public int user_recharge;
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
        private string registerurl = "http://game.zikunhh.com/php/zhuce.php?app_name=Yjsj";
        private string Loginurl = "http://game.zikunhh.com/php/denglu.php?app_name=Yjsj";
        private string realnameurl = "http://game.zikunhh.com/php/shiming.php?app_name=Yjsj";
        private string saveurl = "http://game.zikunhh.com/php/cunchu.php?app_name=Yjsj";

        public void RegisterCheck(string user, string password, Action<ResponseRegister> callback)
        {
            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(password))
            {
                 UIController.Instance.Show<TipView>("账号或密码不能为空");
                return;
            }

            StartCoroutine(GetRegisterDataCoroutine(user, password, callback));
        }

        private IEnumerator GetRegisterDataCoroutine(string user, string password, Action<ResponseRegister> callback)
        {
            WWWForm form = new WWWForm();
            form.AddField("user", user);
            form.AddField("password", password);


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
                    // if(response.state == 1)
                    // {
                    //    PlayerDataModule.Instance.data.user_id = response.res.id;
                    //    SaveToServer();
                    // }
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
                            UIController.Instance.Show<TipView>("登录失败!");
                        }
                         PlayerDataModule.Instance.data.user_id = responseLogin.id;
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"JSON解析错误: {ex.Message}");
                       UIController.Instance.Show<TipView>("登录失败!");
                    }
                }
                else
                {
                    Debug.LogError($"登录失败: {webRequest.error}, URL: {Loginurl}");
                    UIController.Instance.Show<TipView>("登录失败!");
                }
            }
        }


        public void SaveToServer()
        {
            StartCoroutine(UploadPlayerDataCoroutine());
        }

        private IEnumerator UploadPlayerDataCoroutine()
        {
            WWWForm form = new WWWForm();
            form.AddField("user", PlayerDataModule.Instance.data.userAccount);
            form.AddField("password", PlayerDataModule.Instance.data.userPassword);
            form.AddField("user_more", JsonConvert.SerializeObject(PlayerDataModule.Instance.data));
            form.AddField("user_rolename", PlayerDataModule.Instance.data.userName);
            Debug.Log(
                $"JsonConvert.SerializeObject( PlayerData) = {JsonConvert.SerializeObject(PlayerDataModule.Instance.data)}");
            using (UnityWebRequest webRequest = UnityWebRequest.Post(saveurl, form))
            {
                webRequest.timeout = 30;

                yield return webRequest.SendWebRequest();
                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log("上传数据成功：" + webRequest.downloadHandler.text);
                    ResponseSaveData response = JsonUtility.FromJson<ResponseSaveData>(webRequest.downloadHandler.text);
                    if (response.state == 2)
                    {
                        Debug.Log("更新数据成功");
                    }
                    else if (response.state == 3)
                    {
                        Debug.Log("错误");
                    }
                    else if (response.state == 4)
                    {
                        Debug.Log("用户不存在");
                    }
                }
                else
                {
                    Debug.LogError("上传数据失败：" + webRequest.error);
                }

            }
        }


        public void RealName( string idnum, string chinese, string fcmLvl,
            Action<ResponseRealName> callback)
        {
            StartCoroutine(GetRealNameCoroutine(idnum, chinese, fcmLvl, callback));
        }

        private IEnumerator GetRealNameCoroutine( string idnum, string chinese, string fcmLvl,
            Action<ResponseRealName> callback)
        {
            WWWForm form = new WWWForm();
            form.AddField("user", PlayerDataModule.Instance.data.userAccount);
            form.AddField("idnum", idnum);
            form.AddField("chinese", chinese);
            form.AddField("fcmLvl", fcmLvl);

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

        // public void ClearUser(string user, Action<ResponseClear> callback)
        // {
        //     StartCoroutine(GetClearUserCoroutine(user, callback));
        // }

        // private IEnumerator GetClearUserCoroutine(string user, Action<ResponseClear> callback)
        // {
        //     WWWForm form = new WWWForm();
        //     form.AddField("user", user);
        //     form.AddField("app_name", GameName.App_name);

        //     using (UnityWebRequest webRequest = UnityWebRequest.Post(clearurl, form))
        //     {
        //         webRequest.timeout = 30;

        //         yield return webRequest.SendWebRequest();
        //         if (webRequest.result == UnityWebRequest.Result.Success)
        //         {
        //             Debug.Log("注销请求成功：" + webRequest.downloadHandler.text);
        //             ResponseClear responseRealName =
        //                 JsonUtility.FromJson<ResponseClear>(webRequest.downloadHandler.text);
        //             callback(responseRealName);
        //         }
        //         else
        //         {
        //             Debug.LogError("注销请求失败：" + webRequest.error);
        //         }
        //     }
        // }
    }

    public static class GameName
    {
        private static string app_name = "Yxsj";

        public static string App_name
        {
            get => app_name;
            set => app_name = value;
        }
    }
}