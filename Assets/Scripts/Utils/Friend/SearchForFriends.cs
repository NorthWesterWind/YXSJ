using System;
using Newtonsoft.Json;
using UnityEngine;


public class SearchForFriends : RequestBase<SearchFrsData>
{
    public SearchForFriends(string searchForFriendsUrl) : base(searchForFriendsUrl) { }
    private SearchFrsData data = new();
    public void SearchForFriendCheck(int id, string Key, Action<SearchFrsData> callback, int page = 1, int limit = 20)
    {
        WWWForm form = new WWWForm();
        form.AddField("user_id", id);
        form.AddField("keyword", Key);
        form.AddField("page", page);
        form.AddField("limit", limit);
        SentPost(form, callback);
    }

    protected override SearchFrsData ParseResponse(string json)
    {
       data = JsonConvert.DeserializeObject<SearchFrsData>(json);
       Debug.LogWarning($"yj ==> json = {json}");
        return data;
    }

}
public class SearchFrsData
{
    public bool success;
    public int code;
    public string message;
    public InterData data;

}
public class InterData
{
    public StrangerData[] strangers;
    public string keyword;
    public Pagination pagination;
}
public class StrangerData
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