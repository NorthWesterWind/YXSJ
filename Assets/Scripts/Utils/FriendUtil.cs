using System;
using System.Collections;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using Utils;
using View;


public class GetFriendListResponse
{
    public bool success;
    public int code;
    public string message;

    public FriendListData data;
}
public class FriendListData
{
    public FriendData[] friendDataArr;
    public Pagination pagination;
}
public class FriendData
{
    public int user_id;
    public string remark_name;
    public string friend_since;
    public string user_name;
    public string display_name;
}
public class Pagination
{
    public int page;
    public int limit;
    public int total;
    public int total_pages;
}



public class FriendUtil : MonoSingleton<LoginUtil>
{
    private static string GameName = "Yxsj";
    private static string friendListUrl = $"http://game.zikunhh.com/php/api.php?s=Friend.getFriendList&app_name={GameName}";
    private static string confirmOrRefuseUrl = $"http://game.zikunhh.com/php/api.php?s=Friend.handleRequest&app_name={GameName}";
    private static string setRemarkUrl = $"http://game.zikunhh.com/php/api.php?s=Friend.setRemark&app_name={GameName}";
    private static string getRequestUrl = $"http://game.zikunhh.com/php/api.php?s=Friend.getRequestList&app_name={GameName}";
    private static string sendRequestUrl = $"http://game.zikunhh.com/php/api.php?s=Friend.sendRequest&app_name={GameName}";
    private static string deleteFriendUrl = $"http://game.zikunhh.com/php/api.php?s=Friend.deleteFriend&app_name={GameName}";
    private static string searchFriendUrl = $"http://game.zikunhh.com/php/api.php?s=Friend.searchFriends&app_name={GameName}";
    private static string searchStrangersUrl = $"http://game.zikunhh.com/php/api.php?s=Friend.searchStrangers&app_name={GameName}";


    public void GetFriendList(string user_id, string page, int limit, Action< GetFriendListResponse> callback)
    {
        StartCoroutine(GetFriendListCoroutine(user_id, page, limit, callback));
    }

    private IEnumerator GetFriendListCoroutine(string user_id, string page, int limit, Action< GetFriendListResponse> callback)
    {
        WWWForm form = new WWWForm();
        form.AddField("user_id", user_id);
        form.AddField("page", page);
        form.AddField("limit", limit);
        using (UnityWebRequest webRequest = UnityWebRequest.Post(friendListUrl, form))
        {
            webRequest.timeout = 30;

            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    string responseText = webRequest.downloadHandler.text;
                    GetFriendListResponse responseLogin = JsonConvert.DeserializeObject< GetFriendListResponse>(responseText);
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
                UIController.Instance.Show<TipView>("获取好友列表失败！");
                Debug.LogError($"登录失败: {webRequest.error}, URL: {friendListUrl}");
                callback?.Invoke(null);
            }
        }
    }
}
