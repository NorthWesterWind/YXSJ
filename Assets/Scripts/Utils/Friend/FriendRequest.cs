using System;
using Newtonsoft.Json;
using UnityEngine;

public class FriendRequest : RequestBase<FriendRequestData>
{
    public FriendRequest(string url) : base(url) { }

    private FriendRequestData data = new();
    public void FriendRequestCheck(int id, string Type, Action<FriendRequestData> action)
    {
        WWWForm form = new WWWForm();
        form.AddField("user_id", id);
        form.AddField("type", Type);
        SentPost(form, action);
    }

    protected override FriendRequestData ParseResponse(string json)
    {
        data = JsonConvert.DeserializeObject<FriendRequestData>(json);
        Debug.LogWarning($"FriendRequestData json = {json}");
        return data;
    }
}
public class FriendRequestData
{
    public bool success;
    public int code;

    public string message;
    public FriendRequestInternalData data;

}
public class FriendRequestInternalData
{
    public RequestData[] requests;
    public string type;
}
[Serializable]
public class RequestData
{
    public int id;
    public int from_user_id;
    public int to_user_id;
    public string message;
    public int status;
    public string created_at;
    public string updated_at;
    public string expired_at;
    public string from_username;
    public string is_expired;
    public string user_more;
}