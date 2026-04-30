using System;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

public class BlockedWordRe : RequestBase<BlockedWordData>
{
    public BlockedWordRe(string url) : base(url)
    {
        this.Url = url;
    }
    private BlockedWordData datas = new();
    public void JugmentBlockedWord(string str, Action<BlockedWordData> action)
    {
        WWWForm form = new WWWForm();
        // string encoded = UnityWebRequest.EscapeURL(str);
        form.AddField("test", str);
        SentPost(form, action);
    }
    protected override BlockedWordData ParseResponse(string json)
    {
        datas = JsonConvert.DeserializeObject<BlockedWordData>(json);
        Debug.Log("BlockedWordRe response: " + json);
        return datas;
    }
}

public class BlockedWordData
{
    public bool success;
    public int code;
    public string message;
    public BlockedWordInternalData data;
}
public class BlockedWordInternalData
{
    public string reason;
    public string hit_word;
    public string reason_type;
    public bool has_sensitive;
}
