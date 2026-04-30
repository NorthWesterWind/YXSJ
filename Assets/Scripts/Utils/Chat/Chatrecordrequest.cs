using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

/// <summary>
/// 获取聊天记录
/// </summary>
public class Chatrecordrequest : RequestBase<ChatDatas>
{
    public Chatrecordrequest(string url) : base(url)
    {
    }
    private ChatDatas datas = new();
    public void ChatrecordCheck(int ID, int Did, Action<ChatDatas> action, int page = 1, int limt = 80)
    {
        WWWForm form = new WWWForm();
        form.AddField("user_id", ID);
        form.AddField("other_user_id", Did);
        form.AddField("page", page);
        form.AddField("limit", limt);
        SentPost(form, action);
    }
    protected override ChatDatas ParseResponse(string json)
    {
       datas = JsonConvert.DeserializeObject<ChatDatas>(json);
       Debug.LogError("yj => ChatDatas json = " + json);
       return datas;
    }




}
public class ChatDatas
{
    public  bool succes;
    public int code;
    public string message;
    public ChatInfoData data;
}
public  class ChatInfoData
{
    public MessageData[] messages;
    public int total;
    public int max_message_id;
    public Pagination pagination;
    internal IEnumerable<object> messaages;
}
public  class MessageData
{
    public int id;
    public int sender_id;
    public int receiver_id;
    public int message_type;
    public string content;
    public int is_read;
    public string created_at;
    public string time_formatted;
    public bool is_sent_by_me;
}
public class Pagination
{
    public int page;
    public int limit;
    public int total;
    public int total_pages; 
} 