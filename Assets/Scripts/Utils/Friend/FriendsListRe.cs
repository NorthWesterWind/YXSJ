using System;
using Newtonsoft.Json;
using UnityEngine;

public class FriendsListRe : RequestBase<FriendListData>
{
    public FriendsListRe(string url) : base(url) { }
    private FriendListData data = new();
    public void GetFriendsList(int user_id, Action<FriendListData> action, int page = 1, int limit = 20)
    {
        WWWForm form = new WWWForm();
        form.AddField("user_id", user_id);
        form.AddField("page", page);
        form.AddField("limit", limit);
        SentPost(form, action);
    }

    protected override FriendListData ParseResponse(string json)
    {
        data =  JsonConvert.DeserializeObject<FriendListData>(json);
        return data;
    }
}
public class FriendListData
{
    public bool success;
    public int code;
    public string message;
    public FriendListInternalData data;
}
public class FriendListInternalData
{
    public FriendData[] friends;
    public Pagination pagination;
}

public class FriendData
{
    public int user_id;
    public object remark_name;
    public string friend_since;
    public string user_name;
    public string display_name;
    public string user_more;
}

