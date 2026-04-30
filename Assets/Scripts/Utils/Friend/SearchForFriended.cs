using System;
using Newtonsoft.Json;
using UnityEngine;

public class SearchForFriended : RequestBase<SearchFredData>
{
    public SearchForFriended(string url) : base(url)
    {
        
    }    
    public SearchFredData data = new();
    public void SearchForFriendCheck(int id, string Key, Action<SearchFredData> callback, int page = 1, int limit = 20)
    {
        WWWForm form = new WWWForm();
        form.AddField("user_id", id);
        form.AddField("keyword", Key);
        form.AddField("page", page);
        form.AddField("limit", limit);
        SentPost(form, callback);
    }

    protected override SearchFredData ParseResponse(string json)
    {
        data = JsonConvert.DeserializeObject<SearchFredData>(json);
        Debug.LogWarning($"yj ==> SearchForFriended json = {json}");
        return data;
    }
}

public class SearchFredData
{
    public bool success;
    public int code;
    public string message;
    public InteredData data;

}
public class InteredData
{
    public FriendedData[] friends;
    public string keyword;
    public Pagination pagination;
}
public class FriendedData
{
    public int user_id;
    public string remark_name;
    public string friend_since;
    public string user_name;
    public string user_rolename;
    public string user_more;
    public string user_item;
    public string user_uuid;
    public int user_fcm;
}
