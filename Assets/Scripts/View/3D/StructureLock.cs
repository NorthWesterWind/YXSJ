using Controller;
using Module.Data;
using TMPro;
using UnityEngine;
using Utils;

public class StructureLock : MonoBehaviour
{
    public SpriteRenderer structureSprite;
    public SpriteRenderer fill;
    public TextMeshPro moneyTxt;
    public SpriteRenderer lockSprite;
    
    public BuildingType buildType;
    private AssetHandle _assetHandle;

    public void InitInfo( BuildingType type, int money)
    {
        buildType = type;
        structureSprite.sprite = GetComponent<AssetHandle>().Get<Sprite>(Extensions.GetStructureResNameByType(type));
        //float fillWidth = Mathf.Clamp01((float)money / DataController.Instance.GetBuildingDataByType(type).unlockCost);
       // fill.size = new Vector2(fillWidth * 100, fill.size.y);
        moneyTxt.text = money.ToString();
        lockSprite.gameObject.SetActive(false);
    }
  
}
