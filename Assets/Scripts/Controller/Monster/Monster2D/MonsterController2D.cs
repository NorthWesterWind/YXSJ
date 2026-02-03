using System;
using System.Collections;
using Module;
using Module.Data;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;
using Utils;

public class MonsterController2D : MonoBehaviour
{
    public float currentHp;
    // public MonsterData data;
    public SkeletonGraphic skeletonGraphic;
    public GameObject fillContent;
    public Image fillImg;
    public SkeletonGraphic specialEffect;
    [Header("Path")]
    public Transform[] pathPoints;   // 路径点
    private int currentIndex = 0;     // 当前目标点索引
    private float moveSpeed = 300f;     // 移动速度
    public bool canWalk = true;
    public float maxHp;
    private Canvas canvas;
    void OnEnable()
    {
       
    }
    void OnDisable()
    {
        
    }

    void Update()
    {
        if (!canWalk) return;
        var currentAnimation = skeletonGraphic.AnimationState.GetCurrent(0);
        if (currentAnimation == null || currentAnimation.Animation.Name != "walk")
        {
            skeletonGraphic.AnimationState.SetAnimation(0, "walk", true);
        }
        MoveAlongPath();
        int newOrder = 32000 - Mathf.RoundToInt(transform.position.y );
        canvas.sortingOrder = newOrder;
    }
    public void Init(Transform[] points, float hp)
    {
        canWalk = true;
        pathPoints = points;
        specialEffect.AnimationState.SetAnimation(0, "animation", false);
        fillContent.SetActive(true);
        fillImg.fillAmount = 1f;
        maxHp = hp;
        currentHp = maxHp;
        canvas = GetComponent<Canvas>();
    }

    void MoveAlongPath()
    {
        if (pathPoints == null || pathPoints.Length == 0 || isDead)
            return;

        Transform target = pathPoints[currentIndex];
        UpdateFacing(target.position);
        // 移动
        transform.position = Vector3.MoveTowards(
            transform.position,
            target.position,
            moveSpeed * Time.deltaTime
        );

        // 到达当前点 → 切换下一个
        if (Vector3.Distance(transform.position, target.position) < 0.05f)
        {
            currentIndex++;

            if (currentIndex >= pathPoints.Length)
            {
                EventCenter.Instance.TriggerEvent(EventMessages.StopCreat2DMonster);
                canWalk = false;
                isDead = true;
                fillContent.SetActive(false);
                EventCenter.Instance.TriggerEvent(EventMessages.HasMonsterArrive);
                var state = skeletonGraphic.AnimationState;
                var current = state.GetCurrent(0);
                if (current == null || current.Animation.Name != "dead")
                {
                    state.SetAnimation(0, "dead", false);
                }
                StartCoroutine(DoDie());
                if(PlayerDataModule.Instance.data.playTrialCurrencyType == CurrencyType.JingYuanBao)
                {
                    UIController.Instance.Show<TrialResultView>(false,100);
                }
                else
                {
                    UIController.Instance.Show<TrialResultView>(false,50);
                }
               

            }
        }
    }
    void UpdateFacing(Vector3 targetPos)
    {
        if (skeletonGraphic == null || skeletonGraphic.Skeleton == null)
            return;

        float dir = targetPos.x - transform.position.x;
        skeletonGraphic.gameObject.transform.localScale = dir >= 0 ? new Vector3(0.8f, 0.8f, 1) : new Vector3(-0.8f, 0.8f, 1);
    }
    public bool isDead = false;

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHp -= damage;
        currentHp = Mathf.Max(currentHp, 0);

        if (fillImg != null && currentHp > 0)
        {
            fillImg.fillAmount = Mathf.Clamp01((float)currentHp / maxHp);
        }

        if (currentHp <= 0)
        {
            isDead = true;
            canWalk = false;
            fillContent.SetActive(false);
            var state = skeletonGraphic.AnimationState;
            var current = state.GetCurrent(0);

            if (current == null || current.Animation.Name != "dead")
            {
                state.SetAnimation(0, "dead", false);
            }
            StartCoroutine(DoDie());
            EventCenter.Instance.TriggerEvent(EventMessages.MonsterDead2D, gameObject);
        }
    }

    public IEnumerator DoDie()
    {
        yield return new WaitForSeconds(1f);
        Destroy(gameObject);
    }
}
