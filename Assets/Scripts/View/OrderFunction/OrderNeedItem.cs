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

    AssetHandle assetHandle;
    public void Init(object type , string info)
    {
        if(type is GoodsType)
        {
            icon.sprite = assetHandle.Get<Sprite>(Extensions.GetGoodsResNameByType((GoodsType)type));
        }
        else
        {
            icon.sprite = assetHandle.Get<Sprite>(Extensions.GetDropItemResNameByType((DropItemType)type));
        }
        num.text = info;
    }
}
