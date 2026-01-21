using Module;
using Module.Data;
using TMPro;
using UnityEngine.UI;
using Utils;

namespace View.PopUp
{
    public class CharacterDetailView : BaseView
    {
       public UIButton closeBtn;
       public TextMeshProUGUI atktxt;
       public TextMeshProUGUI hptxt;
       public TextMeshProUGUI bagtxt;
       public TextMeshProUGUI movetxt;
       public TextMeshProUGUI weaponsizetxt;
       public TextMeshProUGUI pickrangetxt;
       public TextMeshProUGUI atkhptxt;
    


       protected override void AddEventListener()
       {
           base.AddEventListener();
           closeBtn.onClick.AddListener((() =>
           {
               Hide();
           }));
       }

       public override void UpdateViewWithArgs(params object[] args)
       {
           base.UpdateViewWithArgs(args);
           PlayerData data = PlayerDataModule.Instance.data;
           atktxt.text = (data.atk + data.addAtk).ToString();
           hptxt.text = (data.hp + data.addHp).ToString();
           bagtxt.text = (data.bagCapacity + data.addBagCapacity).ToString();
           movetxt.text = ((1 + data.addMoveSpeed) * 100f).ToString("0") + "%";
           weaponsizetxt.text = ((1 + data.addweaponSize) * 100f).ToString("0") + "%";
           pickrangetxt.text = ((1 + data.addPickUpRange) * 100f).ToString("0") + "%";
           atkhptxt.text = (( data.addhpRecover) * 100f).ToString("0") + "%";
       }
    }
}
