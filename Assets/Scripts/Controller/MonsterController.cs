using System.Collections;
using Controller.Monster;
using DG.Tweening;
using Module.Data;
using PolyNav;
using Spine.Unity;
using UnityEngine;
using Utils;
using View;
using Random = UnityEngine.Random;

namespace Controller
{
    public enum MonsterState
    {
        Idle,
        Patrol,
        Flee,
        ChargeWait,     // 指示器阶段（巨型怪物专用）
        ChargeCooldown, // 冲撞后停顿
        Attack,
        ChargeMove
    }

    public class MonsterController : MonoBehaviour
    {
        public float currentHp = 100;
        private int currentSpeed;
        public MonsterData data;
        public SkeletonAnimation skeletonAnimation;
        [Header("巡逻范围设置")] public Vector2 patrolCenter;
        Vector2 patrolSize;

        [Header("行为参数")] public float idleTimeMin = 1.5f;
        public float idleTimeMax = 3f;
        public float fleeDistance = 4f;
        public float fleeDuration = 5f;

        private PolyNavAgent agent;
        private Coroutine stateRoutine;
        private Transform attacker;

        public MonsterState state;
        public MonsterType monsterType;
        public int factorID = -1;

        public Transform uiAnchor;
        private AssetHandle _assetHandle;
        private GameObject _hpInfo;
        private WorldSpaceUIFollow _worldSpaceUIFollow;

        private float lastDamageTime = -999f;
        private bool isRegenerating = false;
        private Coroutine regenCoroutine;
        private float regenDelay = 3f;
        private float regenSpeed = 10f;

        public DropItemType dropItemType;
        public MonsterBehavior behaviorType;
        private IMonsterBehaviorStrategy _behaviorStrategy;
        public GameObject indicatorParent;
        public Transform playerTransform;

        private bool isFleeing = false;
        private float fleeCooldown = 3f;


        private float _damageInterval = 0.2f;
        private float _lastHitTime = -999f;

        private float detectRadius = 8;
        private float lastHitTime;
        private float hitInterval = 3;
        private float deadtime;
        public MeshRenderer _meshRenderer;
        public CanvasGroup canvasGroup;
        public SkeletonAnimation specialEffect;
        public int atk; 

        void Awake()
        {
            agent = GetComponent<PolyNavAgent>();
            agent.map = GameObject.Find("Map").transform.GetComponent<PolyNavMap>();
            _assetHandle = GetComponent<AssetHandle>();

        }

        public void Init(MonsterData data, Vector2 center, MonsterBehavior behavior, int Id, Vector2 patrolSize)
        {
            this.data = data;
            currentHp = data.hp;
            currentSpeed = data.movespeed;
            patrolCenter = center;
            monsterType = data.type;
            behaviorType = behavior;
            factorID = Id;
            atk = data.atk;

            this.patrolSize = patrolSize;

            if (_hpInfo == null)
            {
                _hpInfo = Instantiate(_assetHandle.Get<GameObject>("HpInfo"), GameObject.Find("HpCanvas").transform,
                    false);
                _worldSpaceUIFollow = _hpInfo.GetComponent<WorldSpaceUIFollow>();
                _worldSpaceUIFollow.target = uiAnchor;
            }

            _worldSpaceUIFollow.UpdateFill(currentHp / data.hp);
            _hpInfo.SetActive(false);

            SetLayer();


            switch (behavior)
            {
                case MonsterBehavior.Normal:
                    _behaviorStrategy = new NormalMonsterStrategy();
                    break;
                case MonsterBehavior.Giant:
                    _behaviorStrategy = new GiantMonsterStrategy();
                    break;
                case MonsterBehavior.Golden:
                    _behaviorStrategy = new GoldenMonsterStrategy();
                    break;
            }

            playerTransform = GameObject.FindWithTag("Player").transform;

            deadtime = skeletonAnimation.Skeleton.Data.FindAnimation("dead").Duration;

            specialEffect.GetComponent<MeshRenderer>().sortingOrder = _meshRenderer.sortingOrder + 1;
            specialEffect.AnimationState.SetAnimation(0, "animation", false);
            indicatorParent.gameObject.SetActive(false);
        }

        void OnEnable()
        {
            agent.OnDestinationReached += OnReachDestination;
        }

        void OnDisable()
        {
            agent.OnDestinationReached -= OnReachDestination;
        }

        void Start()
        {
            ChangeState(MonsterState.Patrol);
            EventCenter.Instance.AddListener(EventMessages.NotifyToFlee, HandleNotifyToFlee);
        }

        public bool InAtking;
        void Update()
        {
            SetLayer();
            // 自动回血
            if (currentHp < data.hp && !isRegenerating)
            {
                if (Time.time - lastDamageTime >= regenDelay)
                    regenCoroutine = StartCoroutine(RegenerateHealth());
            }
            // 巨型怪物检测玩家
            if (behaviorType == MonsterBehavior.Giant)
            {
                // 只有在非冲撞准备/冲撞移动/冲撞停顿阶段才检测
                if (state != MonsterState.ChargeWait &&
                    state != MonsterState.ChargeMove &&
                    state != MonsterState.ChargeCooldown)
                {
                    CheckPlayer();
                }
            }

            // 检查当前动画槽是否为 null
            var currentAnimation = skeletonAnimation.AnimationState.GetCurrent(0);
            if (InAtking)
            {
                if (currentAnimation == null || currentAnimation.Animation.Name != "walk")
                {
                    skeletonAnimation.AnimationState.SetAnimation(0, "walk", true);
                }
            }
            else
            {
                if (agent.hasPath || agent.remainingDistance > 1)
                {
                    if (currentAnimation == null || currentAnimation.Animation.Name != "walk")
                    {
                        skeletonAnimation.AnimationState.SetAnimation(0, "walk", true);
                    }
                }
                else
                {
                    if (currentAnimation == null || currentAnimation.Animation.Name != "idle")
                    {
                        skeletonAnimation.AnimationState.SetAnimation(0, "idle", true);
                    }
                }

            }

        }
        private bool hasHitPlayer = false;
        private Coroutine chargeCoroutine;

        public void SetLayer()
        {
            int newOrder = 30000 - Mathf.FloorToInt(transform.localPosition.y);
            _meshRenderer.sortingOrder = newOrder;
        }

        public void ChangeState(MonsterState newState)
        {
            if (state == newState) return;

            if (stateRoutine != null)
            {
                StopCoroutine(stateRoutine);
                stateRoutine = null;
            }

            state = newState;

            switch (state)
            {
                case MonsterState.Idle:
                    stateRoutine = StartCoroutine(IdleState());
                    break;
                case MonsterState.Patrol:
                    stateRoutine = StartCoroutine(PatrolState());
                    break;
                case MonsterState.Flee:
                    stateRoutine = StartCoroutine(FleeState());
                    break;
            }
        }

        IEnumerator IdleState()
        {
            agent.Stop();
            float wait = Random.Range(idleTimeMin, idleTimeMax);
            yield return new WaitForSeconds(wait);
            ChangeState(MonsterState.Patrol);
        }

        IEnumerator PatrolState()
        {
            agent.SetDestination(GetRandomPointWithinPatrolArea());
            yield break;
        }

        IEnumerator FleeState()
        {
            isFleeing = true;
            lastDamageTime = Mathf.Max(lastDamageTime, Time.time);
            agent.OnDestinationReached += OnReachDestinationForFlee;

            SetNewFleeTarget();
            float startTime = Time.time;

            while (Time.time - lastDamageTime < fleeCooldown && Time.time - startTime < fleeDuration)
            {
                yield return null;
            }

            agent.OnDestinationReached -= OnReachDestinationForFlee;
            isFleeing = false;
            ChangeState(MonsterState.Patrol);
        }

        private void OnReachDestinationForFlee()
        {
            if (!isFleeing) return;
            StartCoroutine(DelayedSetNewFleeTarget());
        }

        private IEnumerator DelayedSetNewFleeTarget()
        {
            yield return new WaitForSeconds(0.05f);
            if (isFleeing)
                SetNewFleeTarget();
        }

        private void SetNewFleeTarget()
        {
            // 逃跑方向
            Vector2 fleeDir = (attacker != null)
                ? ((Vector2)transform.position - (Vector2)attacker.position).normalized
                : Random.insideUnitCircle.normalized;

            // 初步目标点
            Vector2 targetPoint = (Vector2)transform.position + fleeDir * fleeDistance;

            // 矩形限制
            float halfWidth = patrolSize.x / 2f;
            float halfHeight = patrolSize.y / 2f;

            Vector2 clamped;
            clamped.x = Mathf.Clamp(targetPoint.x, patrolCenter.x - halfWidth, patrolCenter.x + halfWidth);
            clamped.y = Mathf.Clamp(targetPoint.y, patrolCenter.y - halfHeight, patrolCenter.y + halfHeight);

            // 避免太靠近当前位置
            if (Vector2.Distance(transform.position, clamped) < 0.5f)
            {
                clamped += Random.insideUnitCircle.normalized * 1f;

                // 再次保证不出矩形
                clamped.x = Mathf.Clamp(clamped.x, patrolCenter.x - halfWidth, patrolCenter.x + halfWidth);
                clamped.y = Mathf.Clamp(clamped.y, patrolCenter.y - halfHeight, patrolCenter.y + halfHeight);
            }

            // 设置导航目标
            agent.SetDestination(clamped);

            // 翻转角色
            skeletonAnimation.skeleton.ScaleX = fleeDir.x < 0 ? -1 : 1;
        }


        private bool isDead = false;

        public void TakeDamage(int damage, Transform attacker)
        {
            if (isDead) return;
            if (Time.time - _lastHitTime < _damageInterval) return;

            _lastHitTime = Time.time;
            currentHp -= damage;
            this.attacker = attacker;
            lastDamageTime = Time.time;

            _behaviorStrategy.OnTakeDamage(this, damage);

            if (_behaviorStrategy.ShouldFlee(this) && state != MonsterState.Flee)
                ChangeState(MonsterState.Flee);

            if (regenCoroutine != null)
            {
                StopCoroutine(regenCoroutine);
                regenCoroutine = null;
                isRegenerating = false;
            }

            _hpInfo.SetActive(true);
            _worldSpaceUIFollow.UpdateFill(currentHp / data.hp);

            if (currentHp <= 0)
            {
                isDead = true;
                agent.Stop();
                var state = skeletonAnimation.AnimationState;
                var current = state.GetCurrent(0);
                if (current == null || current.Animation.Name != "dead")
                {
                    state.SetAnimation(1, "dead", false);
                }
                StartCoroutine(DoDie());
            }
        }

        // ---------------- 巨型怪物逻辑 ----------------
        public void CheckPlayer()
        {
            if (behaviorType != MonsterBehavior.Giant) return;

            if (state == MonsterState.ChargeWait || state == MonsterState.Flee || state == MonsterState.ChargeCooldown)
                return;

            if (playerTransform == null)
            {
                playerTransform = GameObject.Find("Player").transform;
            }
            if( chargeCoroutine  != null) return;
            float distance = Vector2.Distance(transform.position, playerTransform.position);
            if (distance < detectRadius)
            {
                if (stateRoutine != null) StopCoroutine(stateRoutine);
                stateRoutine = null;

                agent.Stop(); // 立即停止移动
                InAtking = true;
                chargeCoroutine = StartCoroutine(GiantChargeSequence(playerTransform.position));

            }
            else
            {
                InAtking = false;
                ChangeState(MonsterState.Patrol);
            }

        }
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (state != MonsterState.ChargeMove) return;
            if (hasHitPlayer) return;

            if (other.CompareTag("Player"))
            {
                hasHitPlayer = true;
                EventCenter.Instance.TriggerEvent(EventMessages.PlayerTakeDamage, atk);
            }
        }

        private IEnumerator GiantChargeSequence(Vector2 targetPos)
        {
            hasHitPlayer = false;
            // 指示器阶段
            state = MonsterState.ChargeWait;
            agent.Stop();
            Vector2 dir = targetPos - (Vector2)indicatorParent.transform.position;
            indicatorParent.transform.right = dir;

            // 冲撞阶段
            // --- 冲撞准备阶段 ---
            state = MonsterState.ChargeWait;
            agent.Stop();
            indicatorParent.gameObject.SetActive(true); // 指示器显示
            yield return new WaitForSeconds(0.5f);       // 准备时间
            indicatorParent.gameObject.SetActive(false); // 准备完成隐藏

            // --- 冲撞移动阶段 ---
            state = MonsterState.ChargeMove;
            agent.maxSpeed = 8f;
            agent.avoidRadius = 0f; // 禁用避障
            agent.SetDestination(targetPos);

            float startTime = Time.time;
            float maxChargeTime = 1f;
            while (!hasHitPlayer && Vector2.Distance(transform.position, targetPos) > 0.5f &&
                   Time.time - startTime < maxChargeTime)
            {
                yield return null;
            }

            // 冲撞完成
            agent.maxSpeed = currentSpeed;

            // --- 冲撞停顿阶段 ---
            state = MonsterState.ChargeCooldown;
            agent.Stop();
            yield return new WaitForSeconds(1f);
            agent.avoidRadius = 0.6f; // 禁用避障
            // 回到巡逻/闲置
            ChangeState(MonsterState.Patrol);
             chargeCoroutine = null;
        }

        private IEnumerator RegenerateHealth()
        {
            isRegenerating = true;

            while (currentHp < data.hp)
            {
                currentHp += regenSpeed * Time.deltaTime;
                currentHp = Mathf.Min(currentHp, data.hp);
                _worldSpaceUIFollow.UpdateFill(currentHp / data.hp);
                yield return null;

                if (Time.time - lastDamageTime < regenDelay)
                {
                    isRegenerating = false;
                    yield break;
                }
            }

            _hpInfo.SetActive(false);
            isRegenerating = false;
        }

        public IEnumerator DoDie()
        {

            Destroy(_hpInfo);
            EventCenter.Instance.RemoveListener(EventMessages.NotifyToFlee, HandleNotifyToFlee);
            if (monsterType == MonsterType.JingYuanBao)
            {
                EventCenter.Instance.TriggerEvent(EventMessages.JingYuanBaoDead);
            }
            isDead = false;
            yield return new WaitForSeconds(deadtime);
            EventCenter.Instance.TriggerEvent(EventMessages.CameraBeginShaking);
            EventCenter.Instance.TriggerEvent(EventMessages.MonsterDead, monsterType, gameObject, factorID);
        }

        private void OnDestroy()
        {
            EventCenter.Instance.RemoveListener(EventMessages.NotifyToFlee, HandleNotifyToFlee);
        }

        private void HandleNotifyToFlee(params object[] args)
        {
            if (behaviorType == MonsterBehavior.Giant) return;
            int id = (int)args[0];
            if (id != factorID) return;
            if (state != MonsterState.Flee) ChangeState(MonsterState.Flee);
        }
        private void OnReachDestination()
        {
            if (state == MonsterState.Patrol)
                ChangeState(MonsterState.Idle);
        }
        Vector2 GetRandomPointWithinPatrolArea()
        {
            // 生成矩形内随机点
            float halfWidth = patrolSize.x / 2f;
            float halfHeight = patrolSize.y / 2f;

            Vector2 center = (patrolCenter == Vector2.zero) ? (Vector2)transform.position : patrolCenter;

            float randomX = Random.Range(center.x - halfWidth, center.x + halfWidth);
            float randomY = Random.Range(center.y - halfHeight, center.y + halfHeight);

            Vector2 randomPoint = new Vector2(randomX, randomY);

            // 设置角色翻转
            skeletonAnimation.skeleton.ScaleX = (randomPoint.x < transform.position.x) ? -1 : 1;

            return randomPoint;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Vector2 center = (patrolCenter == Vector2.zero) ? (Vector2)transform.position : patrolCenter;

            float halfWidth = patrolSize.x / 2f;
            float halfHeight = patrolSize.y / 2f;

            // 画矩形框
            Vector3 topLeft = new Vector3(center.x - halfWidth, center.y + halfHeight, 0);
            Vector3 topRight = new Vector3(center.x + halfWidth, center.y + halfHeight, 0);
            Vector3 bottomRight = new Vector3(center.x + halfWidth, center.y - halfHeight, 0);
            Vector3 bottomLeft = new Vector3(center.x - halfWidth, center.y - halfHeight, 0);

            Gizmos.DrawLine(topLeft, topRight);
            Gizmos.DrawLine(topRight, bottomRight);
            Gizmos.DrawLine(bottomRight, bottomLeft);
            Gizmos.DrawLine(bottomLeft, topLeft);
        }

    }
}