using System.Collections;
using Module.Data;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;
using Utils;

public class MonsterController2D : MonoBehaviour
{
    public float currentHp = 100;
    public MonsterData data;
    public SkeletonGraphic skeletonGraphic;
    public Image fillImg;
    public SkeletonGraphic specialEffect;
    [Header("Path")]
    public Transform[] pathPoints;   // 路径点
    private int currentIndex = 0;     // 当前目标点索引
    public float moveSpeed = 3f;     // 移动速度

    void Update()
    {
        var currentAnimation = skeletonGraphic.AnimationState.GetCurrent(0);
        if (currentAnimation == null || currentAnimation.Animation.Name != "walk")
        {
            skeletonGraphic.AnimationState.SetAnimation(0, "walk", true);
        }
        MoveAlongPath();
    }
    public void Init(Transform[] points)
    {
        pathPoints = points;
        specialEffect.AnimationState.SetAnimation(0, "animation", false);
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
                isDead = true;
                EventCenter.Instance.TriggerEvent(EventMessages.HasMonsterArrive);
                var state = skeletonGraphic.AnimationState;
                var current = state.GetCurrent(0);
                if (current == null || current.Animation.Name != "dead")
                {
                    state.SetAnimation(0, "dead", false);
                }
                StartCoroutine(DoDie());

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

    public void TakeDamage(int damage)
    {
        if (isDead) return;
        currentHp -= damage;
        fillImg.fillAmount = currentHp / data.hp;
        if (currentHp <= 0)
        {
            isDead = true;
            var state = skeletonGraphic.AnimationState;
            var current = state.GetCurrent(0);
            if (current == null || current.Animation.Name != "dead")
            {
                state.SetAnimation(0, "dead", false);
            }
            StartCoroutine(DoDie());
        }
    }
    public IEnumerator DoDie()
    {
        yield return new WaitForSeconds(1f);
        Destroy(gameObject);
    }
}
