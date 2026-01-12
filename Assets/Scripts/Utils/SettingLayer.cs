using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingLayer : MonoBehaviour
{
 public SpriteRenderer spriteRenderer;
    void Start()
    {
        int  newOrder = 3000 - Mathf.FloorToInt(transform.localPosition.y);
        spriteRenderer.sortingOrder = newOrder;
    }


    void Update()
    {
        
    }
}
