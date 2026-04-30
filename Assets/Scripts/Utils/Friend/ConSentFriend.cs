using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

public class ConSentFriend : RequestBase<ConSentFriendData>
{
    public ConSentFriendData data;

    public ConSentFriend(string url) : base(url)
    {
        
    }

    public void ConSentFriendCheck(int mid, int did, string Msg, Action<ConSentFriendData> action)
    {
        WWWForm form = new WWWForm();
        form.AddField("request_id", did.ToString());
        form.AddField("action", Msg);
        form.AddField("user_id", mid.ToString());
       
        SentPost(form, action);
    }

    protected override ConSentFriendData ParseResponse(string json)
    {
        data = JsonConvert.DeserializeObject<ConSentFriendData>(json);
        return data;
    }

}
public class ConSentFriendData
{
    public bool success;
    public int code;
    public string message;
    public object data;
}
