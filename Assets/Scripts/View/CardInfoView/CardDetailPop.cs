using Module.Data;
using TMPro;
using UnityEngine.UI;
using Utils;

namespace View.CardInfoView
{
    public class CardDetailPop : BaseView
    {
       public UIButton closeBtn;
       public TextMeshProUGUI titletxt;
       public TextMeshProUGUI currenttxt;
       public TextMeshProUGUI nexttxt;
       public TextMeshProUGUI infotxt;
       public Image cardImg;
       public TextMeshProUGUI levelTxt;
       public Image fillImg;
       public TextMeshProUGUI filltxt;
       public CardLevelData  cardLevelData;
       public UIButton upgradeBtn;
       public TextMeshProUGUI cardprogresstxt;
       public TextMeshProUGUI goldneedtxt;
    }
}
