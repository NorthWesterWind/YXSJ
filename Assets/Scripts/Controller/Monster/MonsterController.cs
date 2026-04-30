using System;
using System.Collections;
using Cinemachine;
using Controller.Player;
using Controller.Monster;
using DG.Tweening;
using Module.Data;
using PolyNav;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using View;
using World.Controller;
using Random = UnityEngine.Random;
using UnityEngine.SceneManagement;

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
        public CinemachineImpulseSource impulseSource;
        public float currentHp = 100;
        private int currentSpeed;
        public MonsterData data;
        public SkeletonAnimation skeletonAnimation;
        [Header("巡逻范围设置")] public Vector2 patrolCenter;
        Vector2 patrolSize;
        private bool hasPatrolCenter;

        [Header("行为参数")] public float idleTimeMin = 1.5f;
        public float idleTimeMax = 3f;
        public float fleeDistance = 4f;
        public float fleeDuration = 5f;

        private PolyNavAgent agent;
        private PolyNavMap cachedMap;
        private Coroutine stateRoutine;
        private Transform attacker;

        public MonsterState state;
        public MonsterType monsterType;
        public int factorID = -1;

        public Transform uiAnchor;
        private AssetHandle _assetHandle;
        // private WorldSpaceUIFollow _worldSpaceUIFollow;
        public GameObject fillBg;
        public Image fillImg;

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
        private const float PatrolInnerInset = 0.6f;


        private float _damageInterval = 0.2f;
        private float _lastHitTime = -999f;

        private float detectRadius = 5;
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
            cachedMap = agent.map;
            _assetHandle = GetComponent<AssetHandle>();
            impulseSource = GetComponent<CinemachineImpulseSource>();
        }
        public void Init(MonsterData data, Vector2 center, MonsterBehavior behavior, int Id, Vector2 patrolSize)
        {
            this.data = data;
            currentHp = data.hp;
            currentSpeed = data.movespeed;
            patrolCenter = center;
            hasPatrolCenter = true;
            monsterType = data.type;
            behaviorType = behavior;
            factorID = Id;
            atk = data.atk;
            agent.enabled = true;
            this.patrolSize = patrolSize;
            agent.maxSpeed = currentSpeed;
            fillImg.fillAmount = currentHp / data.hp;
            fillBg.gameObject.SetActive(false);

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
            if (indicatorParent != null)
            {
                indicatorParent.gameObject.SetActive(false);
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
        public float slowtime = 1f;
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
            if (!isDead)
            {
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

            if (currentSpeed < data.movespeed)
            {
                slowtime -= Time.deltaTime;
                if (slowtime <= 0)
                {
                    slowtime = 1f;
                    currentSpeed = data.movespeed;
                    agent.maxSpeed = currentSpeed;
                }
            }

            if (state == MonsterState.Patrol &&
                agent != null &&
                !agent.pathPending &&
                !agent.hasPath &&
                IsNearPatrolBoundary(transform.position))
            {
                ForceState(MonsterState.Patrol);
                return;
            }

            EnforcePatrolBounds();
        }
        private bool hasHitPlayer = false;
        private Coroutine chargeCoroutine;
        private Canvas canvas;

        public void SetLayer()
        {
            int newOrder = 30000 - Mathf.FloorToInt(transform.localPosition.y * 100);
            if (canvas == null)
                canvas = GetComponent<Canvas>();
            canvas.sortingOrder = newOrder + 1;
            _meshRenderer.sortingOrder = newOrder;
        }

        public void ChangeState(MonsterState newState)
        {
            if (state == newState) return;

            ApplyState(newState);
        }

        private void ForceState(MonsterState newState)
        {
            ApplyState(newState);
        }

        private void ApplyState(MonsterState newState)
        {
            if (state == MonsterState.Flee)
            {
                isFleeing = false;
                if (agent != null)
                {
                    agent.OnDestinationReached -= OnReachDestinationForFlee;
                }
            }

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
            if (IsNearPatrolBoundary(transform.position))
            {
                ForceState(MonsterState.Patrol);
                yield break;
            }

            agent.Stop();
            float wait = Random.Range(idleTimeMin, idleTimeMax);
            yield return new WaitForSeconds(wait);
            ChangeState(MonsterState.Patrol);
        }

        IEnumerator PatrolState()
        {
            if (TryGetPatrolDestination(out var destination))
            {
                SetFacingByTarget(destination);
                agent.SetDestination(destination);
            }
            else
            {
                ChangeState(MonsterState.Idle);
            }
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
            Vector2 clamped = GetSafePatrolPoint(targetPoint);

            // 避免太靠近当前位置
            if (Vector2.Distance(transform.position, clamped) < 0.5f)
            {
                clamped = GetSafePatrolPoint(clamped + Random.insideUnitCircle.normalized * 1f);

                // 再次保证不出矩形
            }

            // 设置导航目标
            SetFacingByTarget(clamped);
            agent.SetDestination(clamped);
        }




        private bool isDead = false;

        public bool TakeDamage(float damage, Transform attacker, float slowDownValue = 0f, bool isPlayer = false)
        {
            if (isDead) return false;
            if (Time.time - _lastHitTime < _damageInterval) return false;
            if (slowDownValue > 0 && behaviorType != MonsterBehavior.Giant)
            {
                currentSpeed = Mathf.Max(1, data.movespeed - Convert.ToInt32(slowDownValue));
                agent.maxSpeed = currentSpeed;
                slowtime = 1f;
            }
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
            if (!fillBg.activeSelf)
            {
                fillBg.SetActive(true);
            }
            fillImg.fillAmount = currentHp / data.hp;

            if (currentHp <= 0)
            {
                isDead = true;
                agent.Stop();
                var state = skeletonAnimation.AnimationState;
                var current = state.GetCurrent(0);
                if (current == null || current.Animation.Name != "dead")
                {
                    state.SetAnimation(0, "dead", false);
                }
                StartCoroutine(DoDie(isPlayer));
            }

            return true;
        }

        // ---------------- 巨型怪物逻辑 ----------------
        public bool KillImmediately(Transform attacker, bool isPlayer = true)
        {
            if (isDead) return false;

            this.attacker = attacker;
            currentHp = 0f;
            lastDamageTime = Time.time;

            if (regenCoroutine != null)
            {
                StopCoroutine(regenCoroutine);
                regenCoroutine = null;
                isRegenerating = false;
            }

            if (!fillBg.activeSelf)
            {
                fillBg.SetActive(true);
            }

            fillImg.fillAmount = 0f;
            isDead = true;
            agent.Stop();
            var state = skeletonAnimation.AnimationState;
            var current = state.GetCurrent(0);
            if (current == null || current.Animation.Name != "dead")
            {
                state.SetAnimation(0, "dead", false);
            }

            StartCoroutine(DoDie(isPlayer));
            return true;
        }

        public void CheckPlayer()
        {
            if (behaviorType != MonsterBehavior.Giant) return;

            if (state == MonsterState.ChargeWait || state == MonsterState.Flee || state == MonsterState.ChargeCooldown)
                return;

            if (playerTransform == null)
            {
                playerTransform = GameObject.Find("Player").transform;
            }
            if (chargeCoroutine != null) return;
            float distance = Vector2.Distance(transform.position, playerTransform.position);
            if (distance < detectRadius)
            {
                if (stateRoutine != null) StopCoroutine(stateRoutine);
                stateRoutine = null;

                agent.Stop(); // 立即停止移动
                InAtking = true;

                // 初步目标点
                Vector2 targetPoint = (Vector2)playerTransform.position;

                // 矩形限制
                Vector2 clamped = GetValidPointWithinPatrolArea(targetPoint);
                // 设置导航目标
                SetFacingByTarget(clamped);
                agent.SetDestination(clamped);
                chargeCoroutine = StartCoroutine(GiantChargeSequence(clamped));
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
                float totalDamage = atk;
                PlayerController playerController = other.GetComponent<PlayerController>();
                if (playerController == null)
                {
                    playerController = other.GetComponentInParent<PlayerController>();
                }

                if (playerController != null)
                {
                    totalDamage += playerController.maxHp * 0.1f;
                }

                EventCenter.Instance.TriggerEvent(EventMessages.PlayerTakeDamage, totalDamage);
                return;
            }

            if (other.TryGetComponent(out CollectorController collector))
            {
                hasHitPlayer = true;
                collector.TakeDamage(atk);
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
            skeletonAnimation.skeleton.ScaleX = dir.x < 0 ? -1 : 1;
            // 冲撞阶段
            // --- 冲撞准备阶段 ---
            state = MonsterState.ChargeWait;
            agent.Stop();
            indicatorParent.gameObject.SetActive(true); // 指示器显示
            yield return new WaitForSeconds(0.5f);       // 准备时间
            indicatorParent.gameObject.SetActive(false); // 准备完成隐藏

            // --- 冲撞移动阶段 ---
            state = MonsterState.ChargeMove;
            agent.maxSpeed = 10f;
            agent.avoidRadius = 0f; // 禁用避障
            agent.SetDestination(targetPos);

            float startTime = Time.time;
            float maxChargeTime = 1f;
            while (!hasHitPlayer &&
                   Time.time - startTime < maxChargeTime)
            {
                yield return null;
            }

            // 冲撞完成
            agent.maxSpeed = currentSpeed;

            // --- 冲撞停顿阶段 ---
            state = MonsterState.ChargeCooldown;
            agent.Stop();
            yield return new WaitForSeconds(1.5f);
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
                if (!fillBg.activeSelf)
                {
                    fillBg.SetActive(true);
                }
                fillImg.fillAmount = (currentHp / data.hp);
                yield return null;

                if (Time.time - lastDamageTime < regenDelay)
                {
                    isRegenerating = false;
                    yield break;
                }
            }

            fillBg.SetActive(false);
            isRegenerating = false;
        }

        public IEnumerator DoDie(bool isPlayer)
        {
            agent.Stop();
            agent.enabled = false;
            EventCenter.Instance.RemoveListener(EventMessages.NotifyToFlee, HandleNotifyToFlee);
            if (monsterType == MonsterType.JingYuanBao)
            {
                EventCenter.Instance.TriggerEvent(EventMessages.JingYuanBaoDead, isPlayer);
            }
            yield return new WaitForSeconds(deadtime * 0.85f);
            bool isTrialScene = SceneManager.GetActiveScene().name == "Game_Trial";
            if (isPlayer && !isTrialScene)
            {
                impulseSource.GenerateImpulse(0.7f * AudioSourceController.Instance.soundVolume);
            }
            if (!isTrialScene)
            {
                EventCenter.Instance.TriggerEvent(EventMessages.CameraBeginShaking);
            }
            EventCenter.Instance.TriggerEvent(EventMessages.MonsterDead, monsterType, gameObject, factorID, isPlayer);
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
            {
                if (IsNearPatrolBoundary(transform.position))
                {
                    ForceState(MonsterState.Patrol);
                }
                else
                {
                    ChangeState(MonsterState.Idle);
                }
            }
        }

        private void EnforcePatrolBounds()
        {
            if (isDead || patrolSize.x <= 0f || patrolSize.y <= 0f)
            {
                return;
            }

            Vector2 currentPos = transform.position;
            if (IsInsidePatrolArea(currentPos, 0.05f))
            {
                return;
            }

            Vector2 correctedPos = GetValidPointWithinPatrolArea(currentPos);
            if (Vector2.Distance(currentPos, correctedPos) <= 0.01f)
            {
                return;
            }

            if (chargeCoroutine != null)
            {
                StopCoroutine(chargeCoroutine);
                chargeCoroutine = null;
            }

            if (stateRoutine != null)
            {
                StopCoroutine(stateRoutine);
                stateRoutine = null;
            }

            isFleeing = false;
            agent.Stop();
            transform.position = new Vector3(correctedPos.x, correctedPos.y, transform.position.z);
            InAtking = false;
            ForceState(MonsterState.Patrol);
        }

        private Vector2 GetPatrolCenter()
        {
            return hasPatrolCenter ? patrolCenter : (Vector2)transform.position;
        }

        private bool IsInsidePatrolArea(Vector2 point, float padding = 0f)
        {
            Vector2 center = GetPatrolCenter();
            float halfWidth = patrolSize.x * 0.5f + padding;
            float halfHeight = patrolSize.y * 0.5f + padding;
            return point.x >= center.x - halfWidth &&
                   point.x <= center.x + halfWidth &&
                   point.y >= center.y - halfHeight &&
                   point.y <= center.y + halfHeight;
        }

        private float GetPatrolInset()
        {
            float maxInset = Mathf.Min(patrolSize.x, patrolSize.y) * 0.25f;
            return Mathf.Min(PatrolInnerInset, maxInset);
        }

        private Vector2 ClampToPatrolArea(Vector2 point)
        {
            return ClampToPatrolArea(point, 0f);
        }

        private Vector2 ClampToPatrolArea(Vector2 point, float inset)
        {
            Vector2 center = GetPatrolCenter();
            float halfWidth = Mathf.Max(0.05f, patrolSize.x * 0.5f - inset);
            float halfHeight = Mathf.Max(0.05f, patrolSize.y * 0.5f - inset);
            return new Vector2(
                Mathf.Clamp(point.x, center.x - halfWidth, center.x + halfWidth),
                Mathf.Clamp(point.y, center.y - halfHeight, center.y + halfHeight));
        }

        private Vector2 GetValidPointWithinPatrolArea(Vector2 desiredPoint)
        {
            Vector2 clamped = ClampToPatrolArea(desiredPoint);
            if (!TryGetPolyNavMap(out var map))
            {
                return clamped;
            }

            if (map.PointIsValid(clamped))
            {
                return clamped;
            }

            Vector2 snapped = map.GetCloserEdgePoint(clamped);
            snapped = ClampToPatrolArea(snapped);
            if (map.PointIsValid(snapped))
            {
                return snapped;
            }

            float desiredProbeRadius = Mathf.Max(0.5f, Mathf.Min(patrolSize.x, patrolSize.y) * 0.2f);
            for (int i = 0; i < 8; i++)
            {
                Vector2 probe = ClampToPatrolArea(clamped + Random.insideUnitCircle * desiredProbeRadius);
                if (map.PointIsValid(probe))
                {
                    return probe;
                }
            }

            Vector2 center = ClampToPatrolArea(GetPatrolCenter());
            if (map.PointIsValid(center))
            {
                return center;
            }

            float probeRadius = Mathf.Max(0.5f, Mathf.Min(patrolSize.x, patrolSize.y) * 0.25f);
            for (int i = 0; i < 8; i++)
            {
                Vector2 probe = ClampToPatrolArea(center + Random.insideUnitCircle * probeRadius);
                if (map.PointIsValid(probe))
                {
                    return probe;
                }
            }

            return clamped;
        }

        private bool TryGetPolyNavMap(out PolyNavMap map)
        {
            map = cachedMap;
            if (map == null)
            {
                map = agent != null ? agent.map : null;
            }

            if (map == null)
            {
                var mapObj = GameObject.FindWithTag("Map");
                if (mapObj != null)
                {
                    map = mapObj.GetComponent<PolyNavMap>();
                }
            }

            if (map == null)
            {
                var mapObjByName = GameObject.Find("Map");
                if (mapObjByName != null)
                {
                    map = mapObjByName.GetComponent<PolyNavMap>();
                }
            }

            if (map != null && map.nodesCount == 0)
            {
                map.GenerateMap();
            }

            cachedMap = map;
            return map != null && map.nodesCount > 0;
        }

        private bool TryGetPatrolDestination(out Vector2 destination)
        {
            Vector2 currentPos = transform.position;
            if (IsNearPatrolBoundary(currentPos))
            {
                for (int i = 0; i < 4; i++)
                {
                    destination = GetBoundaryRecoveryTarget(currentPos);
                    if (Vector2.Distance(currentPos, destination) > 0.35f)
                    {
                        return true;
                    }
                }
            }

            for (int i = 0; i < 8; i++)
            {
                destination = GetRandomPointWithinPatrolArea();
                if (Vector2.Distance(currentPos, destination) > 0.35f)
                {
                    return true;
                }
            }

            Vector2 center = GetSafePatrolPoint(GetPatrolCenter());
            if (Vector2.Distance(currentPos, center) > 0.35f)
            {
                destination = center;
                return true;
            }

            destination = currentPos;
            return false;
        }

        private bool IsNearPatrolBoundary(Vector2 point)
        {
            Vector2 center = GetPatrolCenter();
            float halfWidth = patrolSize.x * 0.5f;
            float halfHeight = patrolSize.y * 0.5f;
            float margin = Mathf.Max(0.35f, Mathf.Min(halfWidth, halfHeight) * 0.18f);

            return point.x <= center.x - halfWidth + margin ||
                   point.x >= center.x + halfWidth - margin ||
                   point.y <= center.y - halfHeight + margin ||
                   point.y >= center.y + halfHeight - margin;
        }

        private Vector2 GetBoundaryRecoveryTarget(Vector2 currentPos)
        {
            Vector2 center = GetPatrolCenter();
            Vector2 inwardDir = center - currentPos;
            if (inwardDir.sqrMagnitude <= 0.0001f)
            {
                inwardDir = Random.insideUnitCircle;
            }

            inwardDir.Normalize();
            Vector2 tangent = new Vector2(-inwardDir.y, inwardDir.x);

            float inwardDistance = Mathf.Max(0.8f, Mathf.Min(patrolSize.x, patrolSize.y) * 0.18f);
            float tangentDistance = Mathf.Max(0.4f, Mathf.Min(patrolSize.x, patrolSize.y) * 0.12f);
            float tangentOffset = Random.Range(-tangentDistance, tangentDistance);

            Vector2 rawTarget = currentPos + inwardDir * inwardDistance + tangent * tangentOffset;
            return GetSafePatrolPoint(rawTarget);
        }

        private Vector2 GetSafePatrolPoint(Vector2 desiredPoint)
        {
            float inset = GetPatrolInset();
            Vector2 safePoint = ClampToPatrolArea(desiredPoint, inset);
            Vector2 validPoint = GetValidPointWithinPatrolArea(safePoint);
            return ClampToPatrolArea(validPoint, inset * 0.5f);
        }

        private void SetFacingByTarget(Vector2 target)
        {
            if (skeletonAnimation == null || skeletonAnimation.skeleton == null)
            {
                return;
            }

            float deltaX = target.x - transform.position.x;
            if (Mathf.Abs(deltaX) <= 0.01f)
            {
                return;
            }

            skeletonAnimation.skeleton.ScaleX = deltaX < 0f ? -1f : 1f;
        }

        Vector2 GetRandomPointWithinPatrolArea()
        {
            // 生成矩形内随机点
            float inset = GetPatrolInset();
            float halfWidth = Mathf.Max(0.05f, patrolSize.x * 0.5f - inset);
            float halfHeight = Mathf.Max(0.05f, patrolSize.y * 0.5f - inset);

            Vector2 center = GetPatrolCenter();
            Vector2 randomPoint = center;

            for (int i = 0; i < 10; i++)
            {
                float randomX = Random.Range(center.x - halfWidth, center.x + halfWidth);
                float randomY = Random.Range(center.y - halfHeight, center.y + halfHeight);
                randomPoint = GetSafePatrolPoint(new Vector2(randomX, randomY));
                if (IsInsidePatrolArea(randomPoint))
                {
                    break;
                }
            }

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
