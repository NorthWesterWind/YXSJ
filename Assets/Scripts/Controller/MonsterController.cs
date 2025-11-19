using System;
using System.Collections;
using Controller.Monster;
using Module.Data;
using PolyNav;
using Unity.VisualScripting;
using UnityEngine;
using Utils;
using View;
using Random = UnityEngine.Random;

namespace Controller
{
    public class MonsterController : MonoBehaviour
    {
        public float currentHp = 100;
        private int currentSpeed;
        public MonsterData data;

        [Header("巡逻范围设置")] public Vector2 patrolCenter; // 活动中心点
        public float patrolRadius = 5f;                 // 活动半径

        [Header("行为参数")] public float idleTimeMin = 1.5f;
        public float idleTimeMax = 3f;
        public float fleeDistance = 4f; // 每次逃跑距离
        public float fleeDuration = 5f; // 逃跑状态最大持续时间（防止异常卡死）

        private PolyNavAgent agent;
        private Coroutine stateRoutine;
        private Transform attacker;
        public Animator animator;
        public MonsterState state;
        public MonsterType monsterType;
        public SpriteRenderer spriteRenderer;

        public Transform uiAnchor;
        private AssetHandle _assetHandle;
        private GameObject _hpInfo;
        private WorldSpaceUIFollow _worldSpaceUIFollow;

        // ---------------- 血量恢复相关 ----------------
        private float lastDamageTime = -999f; // 上次受伤时间
        private bool isRegenerating = false;
        private Coroutine regenCoroutine;
        private float regenDelay = 3f;
        private float regenSpeed = 10f;

        // ---------------- 逃跑状态相关 ----------------
        private bool isFleeing = false;
        private float fleeCooldown = 3f; // 停止攻击后多久结束逃跑

        public DropItemType dropItemType;
        public MonsterBehavior behaviorType;

        private IMonsterBehaviorStrategy _behaviorStrategy;

        void Awake()
        {
            agent = GetComponent<PolyNavAgent>();
            agent.map = GameObject.Find("Map").transform.GetComponent<PolyNavMap>();
            _assetHandle = GetComponent<AssetHandle>();
        }
        
        public void Init(MonsterData data, Vector2 center, MonsterBehavior behavior)
        {
            this.data = data;
            currentHp = data.hp;
            currentSpeed = data.movespeed;
            patrolCenter = center;
            monsterType = data.type;
            behaviorType = behavior;
            if (_hpInfo == null)
            {
                _hpInfo = Instantiate(_assetHandle.Get<GameObject>("HpInfo"), GameObject.Find("HpCanvas").transform,
                    false);
                _worldSpaceUIFollow = _hpInfo.GetComponent<WorldSpaceUIFollow>();
                _worldSpaceUIFollow.target = uiAnchor;
            }
            _worldSpaceUIFollow.UpdateFill(currentHp / data.hp);
            _hpInfo.SetActive(false); // 默认隐藏
            SetLayer();
            ObjectPoolManager.Instance.WarmPool("DropObj", _assetHandle.Get<GameObject>("DropObj"), 30);
         
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
            // 动画控制
            animator.SetBool("move", state != MonsterState.Idle);

            // 动态层级（按Y轴）
            SetLayer();

            // 自动回血检测
            if (currentHp < data.hp && !isRegenerating)
            {
                if (Time.time - lastDamageTime >= regenDelay)
                {
                    regenCoroutine = StartCoroutine(RegenerateHealth());
                }
            }
        }

        public void SetLayer()
        {
            int newOrder = 3000 - Mathf.FloorToInt(transform.localPosition.y);
            spriteRenderer.sortingOrder = newOrder;
        }

        // ---------------- 状态切换管理 ----------------
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

        // ---------------- FLEE 逃跑状态 ----------------
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

        Vector2 fleeDir;
        Vector2 fleeTarget;
        Vector2 clamped;

        private void SetNewFleeTarget()
        {
            if (attacker != null)
                fleeDir = ((Vector2)transform.position - (Vector2)attacker.position).normalized;
            else
                fleeDir = Random.insideUnitCircle.normalized;

            fleeTarget = (Vector2)transform.position + fleeDir * fleeDistance;
            clamped = patrolCenter + Vector2.ClampMagnitude(fleeTarget - patrolCenter, patrolRadius);

            if (Vector2.Distance(transform.position, clamped) < 0.5f)
                clamped += Random.insideUnitCircle.normalized * 1f;

            agent.SetDestination(clamped);
            transform.localScale = new Vector3(fleeDir.x < 0 ? -1 : 1, 1, 1);
        }

        void OnReachDestination()
        {
            if (state == MonsterState.Patrol)
                ChangeState(MonsterState.Idle);
        }

        private float _damageInterval = 0.2f; // 受伤间隔（可暴露到 Inspector）

        private float _lastHitTime = -999f;

        // ---------------- 受击与回血 ----------------
        public void TakeDamage(int damage, Transform attacker)
        {
            if (currentHp <= 0) return;

            if (Time.time - _lastHitTime < _damageInterval)
                return;
            
            _lastHitTime = Time.time;
            currentHp -= damage;
            this.attacker = attacker;
            lastDamageTime = Time.time;
            
            // 策略处理受击逻辑
            _behaviorStrategy.OnTakeDamage(this, damage);

            // 受伤后是否逃跑？
            if (_behaviorStrategy.ShouldFlee(this))
            {
                if (state != MonsterState.Flee)
                    ChangeState(MonsterState.Flee);
            }
            
            
            // 打断回血
            if (regenCoroutine != null)
            {
                StopCoroutine(regenCoroutine);
                regenCoroutine = null;
                isRegenerating = false;
            }

            // 显示血条
            _hpInfo.SetActive(true);
            _worldSpaceUIFollow.UpdateFill(currentHp / data.hp);

         

          

            if (currentHp <= 0)
            {
                DoDie();
            }
        }
        
        //巨型怪物行为
        public void CheckPlayer()
        {
            if(behaviorType != MonsterBehavior.Giant)
                return;
            
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

        // ---------------- 死亡逻辑 ----------------
        public void DoDie()
        {
            EventCenter.Instance.TriggerEvent(EventMessages.CameraBeginShaking);
            EventCenter.Instance.TriggerEvent(EventMessages.MonsterDead, monsterType, gameObject);
            Destroy(_hpInfo);
            EventCenter.Instance.RemoveListener(EventMessages.NotifyToFlee, HandleNotifyToFlee);
        }

        private void OnDestroy()
        {
            EventCenter.Instance.RemoveListener(EventMessages.NotifyToFlee, HandleNotifyToFlee);
        }

        // ---------------- 事件响应 ----------------
        private void HandleNotifyToFlee(params object[] args)
        {
            if(behaviorType == MonsterBehavior.Giant)
                return;
          
            if (state != MonsterState.Flee)
                ChangeState(MonsterState.Flee);
        }

        // ---------------- 工具函数 ----------------
        Vector2 GetRandomPointWithinPatrolArea()
        {
            Vector2 randomDir = Random.insideUnitCircle * patrolRadius;
            randomDir += patrolCenter;

            transform.localScale = new Vector3(randomDir.x - transform.position.x < 0 ? -1 : 1, 1, 1);
            return randomDir;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Vector2 center = (patrolCenter == Vector2.zero) ? transform.position : patrolCenter;
            Gizmos.DrawWireSphere(center, patrolRadius);
        }
    }
}