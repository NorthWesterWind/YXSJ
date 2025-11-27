using System.Collections;
using Controller.Monster;
using DG.Tweening;
using Module.Data;
using PolyNav;
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

        [Header("巡逻范围设置")] public Vector2 patrolCenter;
        public float patrolRadius = 5f;

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
        public SpriteRenderer spriteRenderer;

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

        public SpriteRenderer indicator;
        public GameObject indicatorParent;
        private Transform playerTransform;

        private bool isFleeing = false;
        private float fleeCooldown = 3f;
        private float originalSpeed;

        private float _damageInterval = 0.2f;
        private float _lastHitTime = -999f;

        private float detectRadius = 8;
        private float lastHitTime;
        private float hitInterval = 3;

        void Awake()
        {
            agent = GetComponent<PolyNavAgent>();
            agent.map = GameObject.Find("Map").transform.GetComponent<PolyNavMap>();
            _assetHandle = GetComponent<AssetHandle>();
        }

        public void Init(MonsterData data, Vector2 center, MonsterBehavior behavior, int Id)
        {
            this.data = data;
            currentHp = data.hp;
            currentSpeed = data.movespeed;
            patrolCenter = center;
            monsterType = data.type;
            behaviorType = behavior;
            factorID = Id;

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
                    transform.localScale = new Vector3(1.4f, 1.4f, 1.4f);
                    _behaviorStrategy = new GiantMonsterStrategy();
                    break;
                case MonsterBehavior.Golden:
                    _behaviorStrategy = new GoldenMonsterStrategy();
                    break;
            }

            playerTransform = GameObject.FindWithTag("Player").transform;
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
        }

        public void SetLayer()
        {
            int newOrder = 3000 - Mathf.FloorToInt(transform.localPosition.y);
            spriteRenderer.sortingOrder = newOrder;
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
            Vector2 fleeDir = (attacker != null)
                ? ((Vector2)transform.position - (Vector2)attacker.position).normalized
                : Random.insideUnitCircle.normalized;

            Vector2 targetPoint = (Vector2)transform.position + fleeDir * fleeDistance;
            Vector2 clamped = patrolCenter + Vector2.ClampMagnitude(targetPoint - patrolCenter, patrolRadius);

            if (Vector2.Distance(transform.position, clamped) < 0.5f)
                clamped += Random.insideUnitCircle.normalized * 1f;

            agent.SetDestination(clamped);
            spriteRenderer.flipX = fleeDir.x < 0;
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
                DoDie();
            }
        }

        // ---------------- 巨型怪物逻辑 ----------------
        public void CheckPlayer()
        {
            if (behaviorType != MonsterBehavior.Giant) return;

            if (state == MonsterState.ChargeWait || state == MonsterState.Flee || state == MonsterState.ChargeCooldown)
                return;

            float distance = Vector2.Distance(transform.position, playerTransform.position);
            if (distance < detectRadius)
            {
                if (stateRoutine != null) StopCoroutine(stateRoutine);
                stateRoutine = null;

                agent.Stop(); // 立即停止移动
                StartCoroutine(GiantChargeSequence(playerTransform.position));
            }
            else
            {
                ChangeState(MonsterState.Patrol);
            }
        }

        private IEnumerator GiantChargeSequence(Vector2 targetPos)
        {
            // 指示器阶段
            state = MonsterState.ChargeWait;
            agent.Stop();

            indicator.sortingOrder = 3000 - Mathf.FloorToInt(transform.localPosition.y);
            indicator.color = Color.white;
            indicator.transform.localScale = Vector3.zero;

            Vector2 dir = targetPos - (Vector2)indicatorParent.transform.position;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            indicatorParent.transform.rotation = Quaternion.Euler(0, 0, angle);

            // 冲撞阶段
            // --- 冲撞准备阶段 ---
            state = MonsterState.ChargeWait;
            agent.Stop();
            indicatorParent.gameObject.SetActive(true); // 指示器显示
            indicator.transform.localScale = Vector3.zero;
            indicator.DOColor(Color.red, 1f).SetLoops(2, LoopType.Yoyo);
            indicator.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
            yield return new WaitForSeconds(1.5f);       // 准备时间
            indicatorParent.gameObject.SetActive(false); // 准备完成隐藏

            // --- 冲撞移动阶段 ---
            state = MonsterState.ChargeMove;
            agent.maxSpeed = 8f;
            agent.SetDestination(targetPos);

            float startTime = Time.time;
            float maxChargeTime = 1f;
            while (Vector2.Distance(transform.position, targetPos) > 0.5f &&
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

            // 回到巡逻/闲置
            ChangeState(MonsterState.Patrol);
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

        public void DoDie()
        {
            EventCenter.Instance.TriggerEvent(EventMessages.CameraBeginShaking);
            EventCenter.Instance.TriggerEvent(EventMessages.MonsterDead, monsterType, gameObject , factorID);
            Destroy(_hpInfo);
            EventCenter.Instance.RemoveListener(EventMessages.NotifyToFlee, HandleNotifyToFlee);
            isDead = false;
            Debug.Log("yj == > 执行 DoDie");
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

        Vector2 GetRandomPointWithinPatrolArea()
        {
            Vector2 randomDir = Random.insideUnitCircle * patrolRadius;
            randomDir += patrolCenter;

            spriteRenderer.flipX = randomDir.x < transform.position.x;

            return randomDir;
        }

        private void OnReachDestination()
        {
            if (state == MonsterState.Patrol)
                ChangeState(MonsterState.Idle);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Vector2 center = (patrolCenter == Vector2.zero) ? transform.position : patrolCenter;
            Gizmos.DrawWireSphere(center, patrolRadius);
        }
    }
}