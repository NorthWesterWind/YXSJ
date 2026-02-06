using System;
using System.Collections;
using System.Collections.Generic;
using Module;
using Module.Data;
using UnityEngine;
using Utils;

public class TrialView : BaseView
{
    [Header("Path")]
    public Transform[] pathPoints;

    [Header("Monster")]
    public MonsterController2D monsterPrefab;
    public Transform monsterParent;
    public GameObject[] lowLevelMonsters;
    public GameObject[] midLevelMonsters;
    public GameObject[] highLevelMonsters;

    public GameObject[] lowLevelMonsters_1;
    public GameObject[] midLevelMonsters_2;
    public GameObject[] highLevelMonsters_3;

    [Header("Spawn Settings")]
    public float spawnInterval = 0.5f;   // 可以在外部动态调整
    public int lowCount = 10;            // 每层生成数量
    public int midCount = 10;
    public int highCount = 6;
    public int lowTypes = 2;             // 每层随机挑几种怪物
    public int midTypes = 2;
    public int highTypes = 1;

    private Coroutine spawnCoroutine;
    public List<GameObject> spawnList = new List<GameObject>();
    public bool isCreatOver = false;
    public GameObject img_1;
    public GameObject img_2;
    private bool isShowUI = false;

    void OnEnable()
    {
        EventCenter.Instance.AddListener(EventMessages.StopCreat2DMonster, StopSpawn);
        EventCenter.Instance.AddListener(EventMessages.CloseTrialView, HandleCloseView);
        EventCenter.Instance.AddListener(EventMessages.MonsterDead2D, HandleMonsterDead2D);
        EventCenter.Instance.AddListener(EventMessages.HasMonsterArrive, StopSpawn);
    }

    void OnDisable()
    {
        EventCenter.Instance.RemoveListener(EventMessages.StopCreat2DMonster, StopSpawn);
        EventCenter.Instance.RemoveListener(EventMessages.CloseTrialView, HandleCloseView);
        EventCenter.Instance.RemoveListener(EventMessages.MonsterDead2D, HandleMonsterDead2D);
        EventCenter.Instance.RemoveListener(EventMessages.HasMonsterArrive, StopSpawn);
    }
    void Start()
    {

    }
    public void HandleMonsterDead2D(params object[] args)
    {
        GameObject monster = args[0] as GameObject;
        if (spawnList.Contains(monster))
        {
            spawnList.Remove(monster);
        }
    }
    void Update()
    {
        if (spawnList.Count == 0 && isCreatOver && !isShowUI)
        {
            if (PlayerDataModule.Instance.data.playTrialCurrencyType == CurrencyType.JingYuanBao)
            {
                UIController.Instance.Show<TrialResultView>(true, 500);
            }
            else
            {
                UIController.Instance.Show<TrialResultView>(true, 200);
            }
            isShowUI = true;
            Hide();

        }
    }

    public void HandleCloseView(params object[] args)
    {
        Hide();
    }
    public override void UpdateViewWithArgs(params object[] args)
    {
        base.UpdateViewWithArgs(args);
        isShowUI = false;
        spawnList.Clear();
        spawnCoroutine = null;
        Extensions.ClearChildren(monsterParent);
        if (PlayerDataModule.Instance.data.playTrialCurrencyType == CurrencyType.JingYuanBao)
        {
            img_1.SetActive(true);
            img_2.SetActive(false);
            StartSpawn(true);
        }
        else
        {
            img_1.SetActive(false);
            img_2.SetActive(true);
            StartSpawn(false);
        }
        isCreatOver = false;
    }
    public void StartSpawn(params object[] args)
    {
        if (spawnCoroutine != null) return;
        if ((bool)args[0])
        {
            spawnCoroutine = StartCoroutine(SpawnLoop());
        }
        else
        {
            spawnCoroutine = StartCoroutine(SpawnLoop_1());
        }

    }

    public void StopSpawn(params object[] args)
    {

        if (spawnCoroutine == null) return;
        StopCoroutine(spawnCoroutine);
        spawnCoroutine = null;
        Hide();
    }

    IEnumerator SpawnLoop()
    {

        yield return StartCoroutine(SpawnLayer(lowLevelMonsters, lowCount, lowTypes, 100));
        yield return StartCoroutine(SpawnLayer(midLevelMonsters, midCount, midTypes,120));
        yield return StartCoroutine(SpawnLayer(highLevelMonsters, highCount, highTypes, 160));
        isCreatOver = true;
    }
    IEnumerator SpawnLoop_1()
    {
        yield return StartCoroutine(SpawnLayer(lowLevelMonsters_1, lowCount, lowTypes, 100));
        yield return StartCoroutine(SpawnLayer(midLevelMonsters_2, midCount, midTypes, 120));
        yield return StartCoroutine(SpawnLayer(highLevelMonsters_3, highCount, highTypes, 160));
        isCreatOver = true;
    }

    IEnumerator SpawnLayer(GameObject[] monsterPool, int totalCount, int typesToSelect, int value)
    {
        if (monsterPool == null || monsterPool.Length == 0) yield break;

        if (typesToSelect > monsterPool.Length) typesToSelect = monsterPool.Length;

        List<GameObject> selectedMonsters = new List<GameObject>();
        List<int> indices = new List<int>();
        for (int i = 0; i < monsterPool.Length; i++) indices.Add(i);

        for (int i = 0; i < typesToSelect; i++)
        {
            int randIndex = UnityEngine.Random.Range(0, indices.Count);
            selectedMonsters.Add(monsterPool[indices[randIndex]]);
            indices.RemoveAt(randIndex);
        }
        int baseCount = totalCount / typesToSelect;
        int remainder = totalCount % typesToSelect;

        for (int i = 0; i < selectedMonsters.Count; i++)
        {
            int spawnCount = baseCount;
            if (i == selectedMonsters.Count - 1) spawnCount += remainder; // 最后一个补余数

            for (int j = 0; j < spawnCount; j++)
            {
                GameObject go = Instantiate(selectedMonsters[i], monsterParent);
                spawnList.Add(go);
                go.transform.position = pathPoints[0].position;
                MonsterController2D monsterCtr = go.GetComponent<MonsterController2D>();
                if (monsterCtr != null)
                {
                    monsterCtr.Init(pathPoints, value);
                }
                yield return new WaitForSeconds(spawnInterval);
            }
        }
    }
}
