using System.Collections;
using System.Collections.Generic;
using Module.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

public class OrderNeedItem : MonoBehaviour
{

    public Image icon;
    public TextMeshProUGUI num;

    public AssetHandle assetHandle;
    public void Init(object type, int ownNum, int needNum)
    {
        if(type is GoodsType)
        {
            icon.sprite = assetHandle.Get<Sprite>(Extensions.GetGoodsResNameByType((GoodsType)type));
        }
        else
        {
            icon.sprite = assetHandle.Get<Sprite>(Extensions.GetDropItemResNameByType((DropItemType)type));
        }

        int displayOwn = Mathf.Min(Mathf.Max(0, ownNum), Mathf.Max(0, needNum));
        num.text = displayOwn + "/" + Mathf.Max(0, needNum);
    }
}
