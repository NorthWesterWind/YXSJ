using Module;
using Module.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace View
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
            data = talentData;
            if (_assetHandle == null)
            {
                _assetHandle = GetComponent<AssetHandle>();
            }
            leveltxt.text = data.id.ToString();
            infotxt.text = data.info;
            icon.sprite = _assetHandle.Get<Sprite>(data.resName);
            if (ModuleMgr.Instance.GetModule<PlayerDataModule>().data.talentLevel >= data.id)
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
