using System;
using Newtonsoft.Json;
using UnityEngine;

public class GetStrangerList : RequestBase<GetStrangerListData>
{
    public GetStrangerListData getStrangerListData;
    public GetStrangerList(string url) : base(url)
    {
    
    }

    public void GetStrangerListCheck(int id, Action<GetStrangerListData> action)
    {
        WWWForm form = new WWWForm();
        form.AddField("u_id", id);
        SentPost(form, action);
    }

    protected override GetStrangerListData ParseResponse(string json)
    {
        getStrangerListData = JsonConvert.DeserializeObject<GetStrangerListData>(json);
        Debug.LogError("yj => GetStrangerListData json = " + json);
        return getStrangerListData;
    }
}

public class GetStrangerListData
{
    public string msg;
    public int state;
    public object my_info;
    public RankList[] ranking_list;
    public int total_players;
}


public class RankList
{
    public int id;
    public string user_item;
    public string userName;
    public int userHead;
    public int userHeadKuang;
    public int user_fcm;
    public string user_more;
}
