using Module;
using Module.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace View.CharacterInfoView
{
    public class TalentItemView : MonoBehaviour
    {
        public TextMeshProUGUI leveltxt;
        public Image levelbg;
        public TextMeshProUGUI infotxt;
        public Image icon;
        public Image mask;
        public Image maskicon;
        public Image channelImg;
        private AssetHandle _assetHandle;
        public TalentData data;
        
        public void Init(TalentData talentData)
        { 
            data = talentData;
            if (_assetHandle == null)
            {
                _assetHandle = GetComponent<AssetHandle>();
            }
            leveltxt.text = data.id.ToString();
            infotxt.text = data.info;
            icon.sprite = _assetHandle.Get<Sprite>(data.resName);
            maskicon.sprite = _assetHandle.Get<Sprite>(data.resName + "灰");
            if (data.id != 1)
            {
                if (PlayerDataModule.Instance.data.talentLevel >= data.id && channelImg != null)
                {
                    channelImg.sprite = _assetHandle.Get<Sprite>("经验条1");
                }
                else
                {
                    channelImg.sprite = _assetHandle.Get<Sprite>("经验条3");
                }
            }
           if(PlayerDataModule.Instance.data.talentLevel >= data.id)
            {
                mask.gameObject.SetActive(false);
                levelbg.sprite = _assetHandle.Get<Sprite>("等级框1");
            }
            else
            {
                mask.gameObject.SetActive(true);
                 levelbg.sprite = _assetHandle.Get<Sprite>("等级框2");
            }
        }
    }
}
