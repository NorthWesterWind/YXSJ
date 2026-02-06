using System.Collections;
using System.Collections.Generic;
using Module;
using Module.Data;
using UnityEngine;
using Utils;

public class RewardConfirmView : BaseView
{
    public UIButton closeBtn;
    public Transform content;
    public Dictionary<CurrencyType, int> currencyDic = new Dictionary<CurrencyType, int>();
    public Dictionary<int, int> cardlevelIdDic = new Dictionary<int, int>();

    public override void UpdateViewWithArgs(params object[] args)
    {
        base.UpdateViewWithArgs(args);
        Extensions.ClearChildren(content);
        content.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
        if (args[0] is Dictionary<int, int>)
        {
            cardlevelIdDic = args[0] as Dictionary<int, int>;
            foreach (var pair1 in cardlevelIdDic)
            {
                GameObject obj = GameObject.Instantiate(_assetHandle.Get<GameObject>("RewardInfoItem"), content.transform, false);
                var data = PlayerDataModule.Instance.data.cardUpProgressesList.Find(x => x.id == pair1.Key);
                obj.GetComponent<RewardInfoItem>().Init(1, data, pair1.Value);
            }
            if (args.Length > 1)
            {
                currencyDic = args[1] as Dictionary<CurrencyType, int>;
                foreach (var pair in currencyDic)
                {
                    GameObject obj = GameObject.Instantiate(_assetHandle.Get<GameObject>("RewardInfoItem"), content.transform, false);
                    long num = pair.Value;
                    obj.GetComponent<RewardInfoItem>().Init(2, pair.Key, num);
                }
            }
        }
        else
        {
            currencyDic = args[0] as Dictionary<CurrencyType, int>;
            foreach (var pair in currencyDic)
            {
                GameObject obj = GameObject.Instantiate(_assetHandle.Get<GameObject>("RewardInfoItem"), content.transform, false);
                long num = pair.Value;
                obj.GetComponent<RewardInfoItem>().Init(2, pair.Key, num);
            }
        }

    }
    protected override void AddEventListener()
    {
        base.AddEventListener();
        closeBtn.onClick.RemoveAllListeners();
        closeBtn.onClick.AddListener((() =>
        {
            Hide();
        }));
    }
}
