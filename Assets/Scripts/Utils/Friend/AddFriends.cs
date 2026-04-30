using System;
using UnityEngine;

public class AddFriends : RequestBase<AddFriendsData>
{
    public AddFriends(string Url) : base(Url) { }
    public void AddFriendsCheck(int mid, int did, Action<AddFriendsData> action)
    {
        WWWForm form = new WWWForm();
        form.AddField("from_user_id", mid);
        form.AddField("to_user_id", did);
        form.AddField("message", "交个朋友。");
        SentPost(form, action);
    }
}
public class AddFriendsData
{
    public bool success;
    public int code;
    public string message;
    public object data;
}
