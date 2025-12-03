using System;
using System.Collections;
using System.Collections.Generic;
using Controller;
using DG.Tweening;
using Module;
using Module.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Object = System.Object;

namespace View.Task
{
    public class TaskPop : BaseView
    {
        public TextMeshProUGUI mapNameTxt;
        public TextMeshProUGUI mapprogressTxt;
        public UIButton rewardBtn;
        public Image sliderFill;
        public TextMeshProUGUI sliderText;
        private MapData _mapData;
        public Transform taskContent;
        public UIButton closeBtn;
        public RectTransform content;

        private void OnEnable()
        {
            content.anchoredPosition = new Vector2(0, -910);
        }

        public override void UpdateViewWithArgs(params object[] args)
        {
            base.UpdateViewWithArgs(args);
            StopAllCoroutines();
            PlayerData tempdata = ModuleMgr.Instance.GetModule<PlayerDataModule>().data;
            _mapData = DataController.Instance.mapDataDic[tempdata.currentMapID];
            int count  = tempdata.mapTaskRecordDic[_mapData.id].Count;
            mapNameTxt.text = _mapData.name;
           
            int tempvalue = count % _mapData.taskGroupSize;
            int tempvalue1 = count / _mapData.taskGroupSize;
            mapprogressTxt.text = tempvalue1 + "/" + _mapData.taskGroupNum;
            sliderText.text = tempvalue + "/" +  _mapData.taskGroupSize;
            sliderFill.fillAmount = tempvalue * 1f / _mapData.taskGroupSize;
            content.DOAnchorPos(new Vector2(0, 0), 0.5f)
                .SetEase(Ease.OutBack);
            UpdateTaskContent();
        }

        protected override void AddEventListener()
        {
            base.AddEventListener();
            closeBtn.onClick.AddListener(OnClickClose);
        }

     
        
        public void UpdateTaskContent()
        {
            Extensions.ClearChildren(taskContent);
            PlayerData tempdata = ModuleMgr.Instance.GetModule<PlayerDataModule>().data;
            List<TaskData> dataList = DataController.Instance.GetTaskGroupIds();
            List<TaskData> list1 = new List<TaskData>();
            List<TaskData> list2 = new List<TaskData>();
            foreach (TaskData data in dataList)
            {
                if (tempdata.completedTaskIdList.Contains(data.taskId))
                {
                   list2.Add(data);
                }
                else
                {
                    list1.Add(data);
                }
            }
            
            for (int i = 0; i < list1.Count; i++)
            {
                GameObject obj = Instantiate( _assetHandle.Get<GameObject>("taskViewItem") , taskContent , false );
                obj.GetComponent<TaskViewItem>().Init(list1[i]);
            }
            for (int i = 0; i < list2.Count; i++)
            {
                GameObject obj = Instantiate( _assetHandle.Get<GameObject>("taskViewItem") , taskContent , false );
                obj.GetComponent<TaskViewItem>().Init(list2[i]);
            }
        }
        void Update()
        {
        }

        private void OnClickClose()
        {
            StartCoroutine(ShowAnimation());
        }

        private IEnumerator ShowAnimation()
        {
            content.DOAnchorPos(new Vector2(0, -910), 0.4f)
                .SetEase(Ease.InBack);
            yield return new WaitForSeconds(0.4f);
            Hide();
        }
    }
}