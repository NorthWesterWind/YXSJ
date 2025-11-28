using System.Collections.Generic;
using Controller;
using Module;
using Module.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using World.View.UI;

namespace View.Task
{
    public class TaskPop : BaseView
    {
        private Animation _animation;
        public TextMeshProUGUI mapNameTxt;
        public TextMeshProUGUI mapprogressTxt;
        public UIButton rewardBtn;
        public Image sliderFill;
        public TextMeshProUGUI sliderText;
        private MapData _mapData;
        public Transform taskContent;

        public override void UpdateViewWithArgs(params object[] args)
        {
            base.UpdateViewWithArgs(args);
            PlayerData tempdata = ModuleMgr.Instance.GetModule<PlayerDataModule>().data;
            _mapData = DataController.Instance.mapDataDic[tempdata.currentMapID];
            int count  = tempdata.mapTaskRecordDic[_mapData.id].Count;
            mapNameTxt.text = _mapData.name;
           
            int tempvalue = count % _mapData.taskGroupSize;
            int tempvalue1 = count / _mapData.taskGroupSize;
            mapprogressTxt.text = tempvalue1 + "/" + _mapData.taskGroupNum;
            sliderText.text = tempvalue + "/" +  _mapData.taskGroupSize;
            sliderFill.fillAmount = tempvalue * 1f / _mapData.taskGroupSize;
        }

        protected override void OnShowComplete()
        {
            base.OnShowComplete();
            _animation.Play(); 
        }

        public void UpdateTaskContent()
        {
            Extensions.ClearChildren(taskContent);
            PlayerData tempdata = ModuleMgr.Instance.GetModule<PlayerDataModule>().data;
          //  List<TaskData> dataList = DataController.Instance.GetTaskGroupIds();
            int count  = tempdata.mapTaskRecordDic[_mapData.id].Count;
            
            
            for (int i = 0; i < _mapData.taskGroupSize; i++)
            {
                
            }
        }
        void Update()
        {
        }
    }
}