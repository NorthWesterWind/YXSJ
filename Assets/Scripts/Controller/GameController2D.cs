using System.Collections;
using UnityEngine;
using Utils;

public class GameController2D : MonoBehaviour
{
    public Transform[] pathPoints;
    public float spawnInterval = 1.5f;

    private Coroutine spawnCoroutine;

    [Header("Monster")]
    public MonsterController2D monsterPrefab;
    public Transform monsterParent;


    void OnEnable()
    {
        EventCenter.Instance.AddListener(EventMessages.BeginCreat2DMonster, StartSpawn);
        EventCenter.Instance.AddListener(EventMessages.StopCreat2DMonster, StopSpawn);
    }

    void OnDisable()
    {
        EventCenter.Instance.RemoveListener(EventMessages.BeginCreat2DMonster, StartSpawn);
        EventCenter.Instance.RemoveListener(EventMessages.StopCreat2DMonster, StopSpawn);
    }


    public void StartSpawn(params object[] args)
    {
        if (spawnCoroutine != null) return;

        spawnCoroutine = StartCoroutine(SpawnLoop());
    }

    public void StopSpawn(params object[] args)
    {
        if (spawnCoroutine == null) return;

        StopCoroutine(spawnCoroutine);
        spawnCoroutine = null;
    }

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            SpawnOne();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void SpawnOne()
    {
        MonsterController2D monster = Instantiate(
            monsterPrefab,
            pathPoints[0].position,
            Quaternion.identity,
            monsterParent
        );

        // monster.Init(pathPoints);
    }
}

