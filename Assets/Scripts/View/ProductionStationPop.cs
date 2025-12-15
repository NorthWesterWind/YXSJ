using Module;
using Module.Data;
using TMPro;
using UnityEditor.AddressableAssets.Build.Layout;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace View
{
    public class ProductionStationPop :BaseView
    {
        public TextMeshProUGUI cardleveltxt;
        public TextMeshProUGUI earningtxt;
        public TextMeshProUGUI cardprogresstxt;
        public TextMeshProUGUI pricetxt;
        public TextMeshProUGUI workingtimetxt;
        public TextMeshProUGUI cardnametxt;
        public TextMeshProUGUI nametxt;
        public TextMeshProUGUI productiontxt;
        public Image cardIcon;
        public Image priceIcon;
        public Image cardprogressfill;
        public Image bottomleveltxt1;
        public Image bottomleveltxt2;
        public Image bottompreviewtxt1;
        public Image bottompreviewtxt2;
        public Image bottomprogressfill;
        public TextMeshProUGUI bottomprogresstxt;
        public UIButton bootomBtn1;
        public TextMeshProUGUI bootomBtntxt1;
        public UIButton bootomBtn2;
        public TextMeshProUGUI bootomBtntxt2;

        public override void UpdateViewWithArgs(params object[] args)
        {
            base.UpdateViewWithArgs(args);
            BuildingType type = (BuildingType)args[0];
            PlayerData player =  ModuleMgr.Instance.GetModule<PlayerDataModule>().data;
            ProductStationData data =  player.ProductStationDataList.Find(x => x.buildingType == type);
            CardUpProgress cardData;
           switch (type)
           {
               case BuildingType.YuShaHu_1:
                   cardData = player.cardUpProgressesList.Find(x => x.developType == CardDevelopType.UpgradeYuShaHu_1);
                   break;
               case BuildingType.YuShaHu_2:
                   cardData = player.cardUpProgressesList.Find(x => x.developType == CardDevelopType.UpgradeYuShaHu_2);
                   break;
               case BuildingType.YuShaHu_3:
                   cardData = player.cardUpProgressesList.Find(x => x.developType == CardDevelopType.UpgradeYuShaHu_3);
                   break;
               case BuildingType.YuShaHu_4:
                   cardData = player.cardUpProgressesList.Find(x => x.developType == CardDevelopType.UpgradeYuShaHu_4);
                   break;
               case BuildingType.LianQiLu_1:
                   cardData = player.cardUpProgressesList.Find(x => x.developType == CardDevelopType.UpgradeLianQiLu_1);
                   break;
               case BuildingType.LianQiLu_2:
                   cardData = player.cardUpProgressesList.Find(x => x.developType == CardDevelopType.UpgradeLianQiLu_2);
                   break;
               case BuildingType.LianQiLu_3:
                   cardData = player.cardUpProgressesList.Find(x => x.developType == CardDevelopType.UpgradeLianQiLu_3);
                   break;
           }
            if (data == null)
            {
                
            }
        }
    }
}
