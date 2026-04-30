using System;
using Newtonsoft.Json;
using UnityEngine;

public class GetUnreadCount : RequestBase<UnreadMessageData>
{
    public GetUnreadCount(string url) : base(url)
    {
    }

    public void LoadMsg(int id, Action<UnreadMessageData> action)
    {
        WWWForm form = new WWWForm();
        form.AddField("user_id", id);
        SentPost(form, action);
    }

    protected override UnreadMessageData ParseResponse(string json)
    {
        return JsonConvert.DeserializeObject<UnreadMessageData>(json);
    }
}
public class UnreadMessageData
{
    public bool success;
    public int code;
    public string message;
    public UnreadInternalData data;
}
public class UnreadInternalData
{
    public int total_unread;
    public UnreadSender[] unread_by_sender;
}
public class UnreadSender
{
    public int sender_id;
    public int count;
}
