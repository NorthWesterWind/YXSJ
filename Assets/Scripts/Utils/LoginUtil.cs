using System;
using System.Collections;
using System.ComponentModel;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Module;
using Module.Data;
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
        public int user_age;
        public int user_fcm;

        [JsonProperty("id", DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue(null)]
        public int? id;
        public string more;
        public string msg;
        public string users;
        public string password;
        public string user_login;
        public string user_pass;
        public int recharge;
        public int state;
        public string user_uuid;
        public string timestamp;

        public int GetResolvedAge()
        {
            return age > 0 ? age : user_age;
        }

        public int GetResolvedFcm()
        {
            return fcm > 0 ? fcm : user_fcm;
        }

        public string GetResolvedAccount()
        {
            return !string.IsNullOrEmpty(users) ? users : user_login;
        }

        public string GetResolvedPassword()
        {
            return !string.IsNullOrEmpty(password) ? password : user_pass;
        }

        public int GetResolvedId()
        {
            return id ?? 0;
        }
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
        public int error_code;    // 错误码（0表示成功）
        public string reason;     // 状态说明
        public ResultData result; // 主要结果数据
        public string sn;         // 序列号
    }
    [System.Serializable]
    public class ResultData
    {
        public string realname;        // 脱敏姓名（如：张*）
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
        public string birthday; // 生日（yyyy-M-d格式）
    }
    public class LoginUtil : MonoSingleton<LoginUtil>
    {
        private static readonly JsonSerializerSettings CloudSaveJsonSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Ignore
        };
        private const bool EnableVerboseCloudLogs = false;
        private sealed class SavePayload
        {
            public string User;
            public string Password;
            public string UserRoleName;
            public PlayerData Snapshot;
        }

        private string registerurl = "http://game.zikunhh.com/php/zhuce.php?app_name=Yjsj";
        private string Loginurl = "http://game.zikunhh.com/php/denglu.php?app_name=Yjsj";
        private string realnameurl = "http://game.zikunhh.com/php/shiming.php?app_name=Yjsj";
        private string saveurl = "http://game.zikunhh.com/php/cunchu.php?app_name=Yjsj";
        private static string clearurl = "http://game.zikunhh.com/php/zhuxiao.php?app_name=Yjsj";
        private string blockedwordsurl = "http://game.zikunhh.com/php/blocked.php?action=check";
        private bool _isUploadingPlayerData;
        private SavePayload _pendingPlayerDataUpload;



        public void ClearUser(string user, string password, Action<ResponseClear> callback)
        {
            StartCoroutine(GetClearUserCoroutine(user, password, callback));
        }

        private IEnumerator GetClearUserCoroutine(string user, string password, Action<ResponseClear> callback)
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
                    // Debug.Log("注销请求成功：" + webRequest.downloadHandler.text);
                    // ResponseClear responseRealName =
                    //     JsonUtility.FromJson<ResponseClear>(webRequest.downloadHandler.text);
                    // callback(responseRealName);

                    ResponseLogin responseLogin = JsonConvert.DeserializeObject<ResponseLogin>(webRequest.downloadHandler.text);
                    if (responseLogin != null)
                    {
                        if (responseLogin.state == 1)
                        {
                            WWWForm form_1 = new WWWForm();
                            form_1.AddField("user", user);
                            form_1.AddField("app_name", GameName.App_name);
                            using (UnityWebRequest webRequest_1 = UnityWebRequest.Post(clearurl, form_1))
                            {
                                webRequest_1.timeout = 30;
                                yield return webRequest_1.SendWebRequest();
                                if (webRequest_1.result == UnityWebRequest.Result.Success)
                                {
                                    Debug.Log("注销请求成功：" + webRequest_1.downloadHandler.text);
                                    ResponseClear responseRealName =
                                        JsonUtility.FromJson<ResponseClear>(webRequest_1.downloadHandler.text);
                                    callback(responseRealName);
                                }
                                else
                                {
                                    Debug.LogError("注销请求失败：" + webRequest_1.error);
                                    UIController.Instance.Show<TipView>("注销失败，请重试！");
                                }
                            }
                        }
                        else
                        {
                            UIController.Instance.Show<TipView>("账号或密码错误！");
                        }
                    }
                    else
                    {
                        Debug.LogError("注销请求失败：" + webRequest.error);
                        UIController.Instance.Show<TipView>("账号或密码错误！");
                    }
                }
            }
        }


        public void RegisterCheck(string user, string password, Action<ResponseRegister> callback)
        {
            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(password))
            {
                UIController.Instance.Show<TipView>("账号或密码不能为空！");
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
                        Debug.Log($"登录信息：user = {user} ,  password = {password} , webRequest.result = {webRequest.result} ");
                        ResponseLogin responseLogin = JsonConvert.DeserializeObject<ResponseLogin>(responseText);
                        if (EnableVerboseCloudLogs)
                        {
                            Debug.Log($"responseLogin = {responseText}");
                            Debug.Log($"[CloudLogin] user={user} raw={BuildJsonDigest(responseText)} more={BuildJsonDigest(responseLogin?.more)} state={(responseLogin != null ? responseLogin.state.ToString() : "null")}");
                        }
                        if (responseLogin != null)
                        {
                            callback?.Invoke(responseLogin);
                        }
                        else
                        {
                            UIController.Instance.Show<TipView>("登录失败！");
                        }
                        int resolvedId = responseLogin != null ? responseLogin.GetResolvedId() : 0;
                        if (resolvedId > 0)
                        {
                            PlayerDataModule.Instance.data.user_id = resolvedId;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[Login] Login flow failed: {ex}");
                        UIController.Instance.Show<TipView>("登录失败！");
                    }
                }
                else
                {
                    Debug.LogError($"登录失败: {webRequest.error}, URL: {Loginurl}");
                    UIController.Instance.Show<TipView>("登录失败！");
                }
            }
        }


        public void SaveToServer(PlayerData playerDataSnapshot)
        {
            if (playerDataSnapshot == null ||
                string.IsNullOrWhiteSpace(playerDataSnapshot.userAccount) ||
                string.IsNullOrWhiteSpace(playerDataSnapshot.userPassword))
            {
                Debug.LogWarning("[SaveToServer] Skip upload because account credentials are missing.");
                return;
            }

            SavePayload payload = new SavePayload
            {
                User = playerDataSnapshot.userAccount,
                Password = playerDataSnapshot.userPassword,
                UserRoleName = playerDataSnapshot.playerName,
                Snapshot = playerDataSnapshot
            };

            if (_isUploadingPlayerData)
            {
                _pendingPlayerDataUpload = payload;
                return;
            }

            StartCoroutine(UploadPlayerDataCoroutine(payload));
        }

        private IEnumerator UploadPlayerDataCoroutine(SavePayload payload)
        {
            _isUploadingPlayerData = true;
            WWWForm form = new WWWForm();
            form.AddField("user", payload.User);
            form.AddField("password", payload.Password);
            var serializeTask = Task.Run(() => JsonConvert.SerializeObject(payload.Snapshot, Formatting.None, CloudSaveJsonSettings));
            while (!serializeTask.IsCompleted)
            {
                yield return null;
            }

            if (serializeTask.IsFaulted)
            {
                _isUploadingPlayerData = false;
                if (_pendingPlayerDataUpload != null)
                {
                    SavePayload pendingPayload = _pendingPlayerDataUpload;
                    _pendingPlayerDataUpload = null;
                    SaveToServer(pendingPayload.Snapshot);
                }
                yield break;
            }
            string playerDataJson = serializeTask.Result;
            if (EnableVerboseCloudLogs)
            {
                Debug.Log($"[CloudSaveUpload] user={payload.User} role={payload.UserRoleName ?? string.Empty} raw={BuildJsonDigest(playerDataJson)}");
            }
            form.AddField("user_more", playerDataJson);
            form.AddField("user_rolename", payload.UserRoleName ?? string.Empty);
            using (UnityWebRequest webRequest = UnityWebRequest.Post(saveurl, form))
            {
                webRequest.timeout = 30;

                yield return webRequest.SendWebRequest();
                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log("上传数据成功：" + webRequest.downloadHandler.text);
                    ResponseSaveData response = JsonUtility.FromJson<ResponseSaveData>(webRequest.downloadHandler.text);
                    Debug.Log($"[CloudSaveUploadResponse] user={payload.User} raw={BuildJsonDigest(webRequest.downloadHandler.text)} state={(response != null ? response.state.ToString() : "null")}");
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

            _isUploadingPlayerData = false;
            if (_pendingPlayerDataUpload != null)
            {
                SavePayload pendingPayload = _pendingPlayerDataUpload;
                _pendingPlayerDataUpload = null;
                SaveToServer(pendingPayload.Snapshot);
            }
        }

        private static string BuildJsonDigest(string text)
        {
            if (text == null)
            {
                return "null";
            }

            if (text.Length == 0)
            {
                return "empty";
            }

            int bytes = Encoding.UTF8.GetByteCount(text);
            int openBraceCount = CountChar(text, '{');
            int closeBraceCount = CountChar(text, '}');
            int quoteCount = CountChar(text, '"');
            return $"chars={text.Length} bytes={bytes} sha256={ComputeSha256Prefix(text)} braces={openBraceCount}/{closeBraceCount} quotes={quoteCount} head=\"{GetSnippet(text, true)}\" tail=\"{GetSnippet(text, false)}\"";
        }

        private static int CountChar(string text, char value)
        {
            int count = 0;
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == value)
                {
                    count++;
                }
            }

            return count;
        }

        private static string ComputeSha256Prefix(string text)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            byte[] hash;
            using (SHA256 sha256 = SHA256.Create())
            {
                hash = sha256.ComputeHash(bytes);
            }
            StringBuilder sb = new StringBuilder(16);
            for (int i = 0; i < 8 && i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("x2"));
            }

            return sb.ToString();
        }

        private static string GetSnippet(string text, bool fromStart, int maxLength = 80)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            string snippet = fromStart
                ? text.Substring(0, Math.Min(maxLength, text.Length))
                : text.Substring(Math.Max(0, text.Length - maxLength));
            return snippet.Replace("\r", "\\r").Replace("\n", "\\n");
        }


        public void RealName(string idnum, string chinese, string fcmLvl,
            Action<ResponseRealName> callback)
        {
            StartCoroutine(GetRealNameCoroutine(idnum, chinese, fcmLvl, callback));
        }

        private IEnumerator GetRealNameCoroutine(string idnum, string chinese, string fcmLvl,
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
                        JsonConvert.DeserializeObject<ResponseRealName>(webRequest.downloadHandler.text);
                    callback(responseRealName);
                }
                else
                {
                    Debug.LogError("实名请求失败：" + webRequest.error);
                }
            }
        }

        public void CheckBlockedWords(string str, Action<BlockedWordData> callback)
        {
            StartCoroutine(GetBlockedWordsCoroutine(str, callback));
        }
        private IEnumerator GetBlockedWordsCoroutine(string str, Action<BlockedWordData> callback)
        {
            WWWForm form = new WWWForm();
            form.AddField("test", str);
            using (UnityWebRequest webRequest = UnityWebRequest.Post(blockedwordsurl, form))
            {
                webRequest.timeout = 30;

                yield return webRequest.SendWebRequest();
                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log("敏感词请求成功：" + webRequest.downloadHandler.text);
                    BlockedWordData response =
                        JsonConvert.DeserializeObject<BlockedWordData>(webRequest.downloadHandler.text);
                    callback(response);
                }
                else
                {
                    Debug.LogError("敏感词请求失败：" + webRequest.error);
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
        private static string app_name = "Yjsj";

        public static string App_name
        {
            get => app_name;
            set => app_name = value;
        }
    }
    public class BlockedWordData
    {
        public bool success;
        public int code;
        public string message;
        public BlockedWordInternalData data;
    }
    public class BlockedWordInternalData
    {
        public string reason;
        public string hit_word;
        public string reason_type;
        public bool has_sensitive;
    }


}

