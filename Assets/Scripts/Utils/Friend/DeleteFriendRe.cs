using System;
using Newtonsoft.Json;
using UnityEngine;


public class DeleteFriendRe : RequestBase<DeleteData>
{
    public DeleteData data = new();
    public DeleteFriendRe(string url) : base(url)
    {
    }
    private string msg = "删除成功";
    public void DeleteCheck(int ID, int Did, Action<DeleteData> action)
    {
        WWWForm form = new WWWForm();
        form.AddField("user_id", ID);
        form.AddField("friend_id", Did);
        SentPost(form, action);
    }

    protected override DeleteData ParseResponse(string json)
    {
        data = JsonConvert.DeserializeObject<DeleteData>(json);
        return data;
    }

}
public class DeleteData
{
    public bool success;
    public int code;
    public string message;
    public DeleteInternalData data;

}
public class DeleteInternalData
{
    public int deletede_records;
}