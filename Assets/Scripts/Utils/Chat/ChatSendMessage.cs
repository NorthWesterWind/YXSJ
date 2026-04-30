using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChatSendMessage : RequestBase<string>
{
    public ChatSendMessage(string url) : base(url)
    {
        
    }

    public void SentMessage(int id, int dID, string msg, string message_type, Action<string> callback)
    {
        WWWForm form = new WWWForm();
        form.AddField("sender_id", id);
        form.AddField("receiver_id", dID);
        form.AddField("content", msg);
        form.AddField("message_type", message_type);

        SentPost(form, callback);
    }
}
