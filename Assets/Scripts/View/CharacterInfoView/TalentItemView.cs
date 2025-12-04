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
        public TextMeshProUGUI infotxt;
        public Image icon;
        public Image channelImg;
        private AssetHandle _assetHandle;
        public TalentData data;
        
        public void Init(TalentData talentData)
        {
            Debug.Log("yj => TalentData 数据初始化");
            if (leveltxt == null)
            {
                leveltxt = transform.Find("Image/leveltxt").GetComponent<TextMeshProUGUI>();
            }

            if (infotxt == null)
            {
                infotxt = transform.Find("info").GetComponent<TextMeshProUGUI>();
            }

            if (icon == null)
            {
                icon = transform.Find("Image/Icon").GetComponent<Image>();
            }

            if (channelImg == null)
            {
                channelImg = transform.Find("Image").GetComponent<Image>();
            }
            
            data = talentData;
            if (_assetHandle == null)
            {
                _assetHandle = GetComponent<AssetHandle>();
            }
            leveltxt.text = data.id.ToString();
            infotxt.text = data.info;
            //icon.sprite = _assetHandle.Get<Sprite>(data.resName);
            if (data.id != 1)
            {
                if (ModuleMgr.Instance.GetModule<PlayerDataModule>().data.talentLevel >= data.id && channelImg != null)
                {
                    channelImg.color = Color.green;
                }
                else
                {
                    channelImg.color = Color.white;
                }
            }
           
        }
    }
}
