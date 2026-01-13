using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingLayer : MonoBehaviour
{
 public SpriteRenderer spriteRenderer;
    void Start()
    {
        int  newOrder = 3000 - Mathf.RoundToInt(transform.position.y * 100);
        spriteRenderer.sortingOrder = newOrder;
    }


    void Update()
    {
        
    }
}
