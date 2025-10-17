// MonsterController.cs (수정됨: 공격 전환 조건에 isAggressive/isProvoked 적용)
using UnityEngine;
using System.Collections;

public class MonsterController : MonoBehaviour
{       
    [Header("Monster Stats")]   
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int currentHealth;
    [SerializeField] private int attackDamage = 10;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float attackCooldown = 2f;

    [Header("AI Settings")]
    [SerializeField] private bool isAggressive = true; // true: 선공, false: 후공
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float detectionRange = 5f;
    [SerializeField] private float movementRadius = 3f;

    [Header("Movement Settings")]
    [SerializeField] private float minWaitTime = 3f;
    [SerializeField] private float maxWaitTime = 10f;
    [SerializeField] private float idleProbability = 0.2f; // 20% 확률로 대기

    [Header("Combat Behavior")]
    [Tooltip("플레이어를 공격할 때 유지할 최소 거리.")]
    [SerializeField] private float preferredAttackDistance = 1.0f;

    // 영역 복귀 보정 속도 (0..1, 1이면 즉시 스냅)
    [SerializeField] private float returnLerpSpeed = 0.2f;

    [Header("Provocation")]
    [Tooltip("몬스터가 자극(provoked)되어 스폰 지역을 무시할 최대 시간(초)")]
    [SerializeField] private float provokedDuration = 8f;
    private float provokedTimeRemaining = 0f;
    private bool isProvoked = false;

    // 부드러운 귀환 제어
    [Header("Return Behavior")]
    [Tooltip("귀환 시 이동속도 배수 (1 = 평소 속도, <1이면 느리게)")]
    [SerializeField] private float returnSpeedMultiplier = 0.8f;
    [Tooltip("귀환이 완료된 것으로 간주할 거리")]
    [SerializeField] private float returnStopDistance = 0.15f;
    private bool isReturning = false;

    // 원래 공격 성향 저장
    private bool originalIsAggressive;

    [Header("Separation (avoid overlap)")]
    [Tooltip("이 반경 내 다른 오브젝트들과 겹침 보정을 수행합니다.")]
    [SerializeField] private float separationRadius = 1.0f;
    [Tooltip("분리 벡터에 곱해지는 세기")]
    [SerializeField] private float separationStrength = 1.0f;
    [Tooltip("겹침 회피 대상 레이어 (예: Monster, Player, NPC)")]
    [SerializeField] private LayerMask separationMask = 0;

    private Vector2 spawnPosition;
    private Vector2 moveTargetPosition;
    private Rigidbody2D rb;
    private Collider2D spawnAreaCollider;

    private Transform playerTransform;
    private float lastAttackTime;

    private enum MonsterState { Idle, Wandering, Chasing, Attacking }
    private MonsterState currentState = MonsterState.Idle;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;
        originalIsAggressive = isAggressive; // 원래 값 저장
    }

    void Start()
    {
        spawnPosition = transform.position;
        FindPlayer(); // playerTransform 설정
        SetupIgnorePlayerCollision(); // 플레이어와의 충돌 무시 시도

        // separationMask가 비어있으면 기본 레이어 시도
        if (separationMask == 0)
        {
            separationMask = LayerMask.GetMask("Monster", "Player", "NPC");
        }

        StartCoroutine(AIRoutine());
    }

    void Update()
    {
        // provoked 타이머 감소
        if (isProvoked)
        {
            provokedTimeRemaining -= Time.deltaTime;
            if (provokedTimeRemaining <= 0f)
            {
                isProvoked = false;
                provokedTimeRemaining = 0f;
                // provoked가 끝나면 부드럽게 스폰 지역으로 귀환 시작
                StartReturnToSpawnArea();
            }
        }

        if (currentHealth <= 0) return;

        if (playerTransform != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

            // 선공이거나 provoked 상태일 때만 추적/공격으로 전환
            if (isAggressive || isProvoked)
            {
                if (distanceToPlayer <= attackRange)
                {
                    currentState = MonsterState.Attacking;
                }
                else if (distanceToPlayer <= detectionRange)
                {
                    currentState = MonsterState.Chasing;
                }
                else if (currentState == MonsterState.Chasing && distanceToPlayer > detectionRange * 1.2f)
                {
                    currentState = MonsterState.Wandering;
                }
            }
            else
            {
                // 비선공: 이미 공격 상태였고 이제 플레이어가 멀어지면 다시 배회로 전환
                if (currentState == MonsterState.Attacking && distanceToPlayer > attackRange)
                {
                    currentState = MonsterState.Wandering;
                }
            }
        }
    }

    void FixedUpdate()
    {
        if (currentHealth <= 0)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        // 귀환 우선 처리: isReturning이면 귀환 로직 실행
        if (isReturning)
        {
            PerformReturnMovement();
            return;
        }

        switch (currentState)
        {
            case MonsterState.Chasing:
                ChasePlayer();
                break;
            case MonsterState.Wandering:
                MoveToTarget();
                break;
            case MonsterState.Attacking:
                AttackPlayer();
                break;
            case MonsterState.Idle:
                rb.velocity = Vector2.zero;
                break;
        }

        // 영역 외부일 경우: provoked 상태면 허용, 아닐 경우 부드럽게 복귀
        if (!isProvoked && !isReturning)
        {
            EnsureInsideSpawnArea();
        }
    }

    private IEnumerator AIRoutine()
    {
        while (currentHealth > 0)
        {
            if (currentState != MonsterState.Chasing && currentState != MonsterState.Attacking)
            {
                float randomDuration = Random.Range(minWaitTime, maxWaitTime);
                if (Random.value < idleProbability)
                {
                    currentState = MonsterState.Idle;
                }
                else
                {
                    SetRandomMoveTarget();
                    currentState = MonsterState.Wandering;
                }
                yield return new WaitForSeconds(randomDuration);
            }
            else
            {
                yield return new WaitForSeconds(0.5f);
            }
        }
    }

    private void SetRandomMoveTarget()
    {
        Vector2 randomDirection = Random.insideUnitCircle * movementRadius;
        moveTargetPosition = (Vector2)transform.position + randomDirection;
        if (spawnAreaCollider != null && !isProvoked)
        {
            moveTargetPosition = ClampPositionToSpawnArea(moveTargetPosition);
        }
    }

    private void MoveToTarget()
    {
        Vector2 direction = (moveTargetPosition - (Vector2)transform.position).normalized;
        float distance = Vector2.Distance(transform.position, moveTargetPosition);

        if (distance > 0.2f)
        {
            Vector2 desiredVel = direction * moveSpeed;
            Vector2 sep = ComputeSeparation() * separationStrength;
            Vector2 finalVel = desiredVel + sep;
            float maxVel = moveSpeed * 1.2f;
            if (finalVel.magnitude > maxVel) finalVel = finalVel.normalized * maxVel;
            rb.velocity = finalVel;
        }
        else
        {
            rb.velocity = Vector2.zero;
            currentState = MonsterState.Idle;
        }
    }

    private void ChasePlayer()
    {
        if (playerTransform == null) return;

        Vector2 targetPosition = playerTransform.position;

        // provoked 상태가 아니면 플레이어 위치를 스폰 영역으로 제한
        if (spawnAreaCollider != null && !isProvoked)
        {
            targetPosition = ClampPositionToSpawnArea(targetPosition);
        }

        float distanceToTarget = Vector2.Distance(transform.position, targetPosition);
        float stopDistance = Mathf.Max(preferredAttackDistance, 0.1f);

        if (distanceToTarget > stopDistance)
        {
            Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
            Vector2 desiredVel = direction * moveSpeed * 1.5f;
            Vector2 sep = ComputeSeparation() * separationStrength;
            Vector2 finalVel = desiredVel + sep;
            float maxVel = moveSpeed * 2.0f;
            if (finalVel.magnitude > maxVel) finalVel = finalVel.normalized * maxVel;
            rb.velocity = finalVel;
        }
        else
        {
            // 근접 상태 진입 시에도 실제 공격으로 전환하려면 isAggressive 또는 isProvoked여야 함
            if (isAggressive || isProvoked)
            {
                rb.velocity = Vector2.zero;
                currentState = MonsterState.Attacking;
            }
            else
            {
                // 비선공이면 근접 상태라도 공격으로 전환하지 않고 대기/배회로 둠
                rb.velocity = Vector2.zero;
                currentState = MonsterState.Wandering;
            }
        }
    }

    private void AttackPlayer()
    {
        rb.velocity = Vector2.zero;

        if (Time.time - lastAttackTime >= attackCooldown)
        {
            lastAttackTime = Time.time;

            // 공격은 '접촉' 기반이 아니라 타이밍으로 처리 (데미지 로직은 추후 연결)
            if (playerTransform != null)
            {
                // 공격이 발생하면 provoked 상태 갱신(플레이어와의 교전 유지)
                SetProvoked();
                Debug.Log($"[Monster] 플레이어 공격! 데미지: {attackDamage}");
            }
        }

        if (playerTransform != null)
        {
            float dist = Vector2.Distance(transform.position, playerTransform.position);
            if (dist > detectionRange)
            {
                currentState = MonsterState.Wandering;
            }
        }
    }

    // 스폰 영역 밖이면 가장 가까운 지점으로 부드럽게 복귀
    private void EnsureInsideSpawnArea()
    {
        if (spawnAreaCollider == null || rb == null) return;

        Vector2 currentPos = rb.position;
        Vector2 closest = spawnAreaCollider.ClosestPoint(currentPos);
        float dist = Vector2.Distance(currentPos, closest);

        // 허용 오차보다 크면 서서히 보정
        if (dist > 0.05f)
        {
            Vector2 newPos = Vector2.Lerp(currentPos, closest, returnLerpSpeed);
            rb.MovePosition(newPos);
            rb.velocity = Vector2.zero;
        }
    }

    // 귀환 이동 처리 (부드럽게 스폰 위치로 복귀)
    private void PerformReturnMovement()
    {
        if (rb == null) return;

        Vector2 currentPos = rb.position;
        Vector2 target = spawnPosition;
        if (spawnAreaCollider != null)
        {
            target = ClampPositionToSpawnArea(target);
        }

        float dist = Vector2.Distance(currentPos, target);

        if (dist <= returnStopDistance)
        {
            // 도착: 귀환 완료
            isReturning = false;
            rb.velocity = Vector2.zero;
            // 귀환 완료 후 원래의 공격 성향으로 복원
            isAggressive = originalIsAggressive;
            currentState = MonsterState.Idle;
            return;
        }

        // 부드럽게 이동 (returnSpeedMultiplier로 속도 조절)
        Vector2 dir = (target - currentPos).normalized;
        Vector2 desiredVel = dir * moveSpeed * returnSpeedMultiplier;
        Vector2 sep = ComputeSeparation() * separationStrength;
        Vector2 finalVel = desiredVel + sep;
        float maxVel = moveSpeed * 1.5f;
        if (finalVel.magnitude > maxVel) finalVel = finalVel.normalized * maxVel;
        rb.velocity = finalVel;
    }

    private Vector2 ClampPositionToSpawnArea(Vector2 targetPos)
    {
        if (spawnAreaCollider == null) return targetPos;
        return spawnAreaCollider.ClosestPoint(targetPos);
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log($"[Monster] 데미지 받음: {damage}, 남은 체력: {currentHealth}");

        // 플레이어로부터 공격 받으면 provoked 상태 활성화
        SetProvoked();

        // provoked 상태에서는 공격 성향을 강제로 선공으로 바꿀 수 있지만
        // originalIsAggressive는 보존되어 귀환 시 복원됩니다.
        isAggressive = true;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("[Monster] 사망");
        MonsterSpawnArea spawnArea = GetComponentInParent<MonsterSpawnArea>();
        if (spawnArea == null && spawnAreaCollider != null)
        {
            spawnArea = spawnAreaCollider.GetComponent<MonsterSpawnArea>();
        }

        if (spawnArea != null)
        {
            spawnArea.OnMonsterDied(this.gameObject);
        }

        Destroy(gameObject, 0.5f);
    }

    public void SetSpawnArea(Collider2D areaCollider)
    {
        spawnAreaCollider = areaCollider;
        spawnPosition = transform.position;
    }

    private void FindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
    }

    private void SetProvoked()
    {
        // provoked 재진입 시 귀환 중이면 취소하고 다시 추적 허용
        isProvoked = true;
        provokedTimeRemaining = provokedDuration;

        if (isReturning)
        {
            isReturning = false;
        }

        // 선공 모드로 전환 (귀환 완료 시 originalIsAggressive로 복원됨)
        isAggressive = true;
    }

    private void StartReturnToSpawnArea()
    {
        // provoked 끝났을 때 부드럽게 귀환 시작
        // 귀환 시작 시 원래 공격 성향 복원(귀환 중에는 추적 금지)
        isReturning = true;
        // 복원은 귀환 완료 시 수행
    }

    // 플레이어와의 물리 충돌로 인한 데미지 발생을 방지하기 위해
    // 몬스터의 Collider들과 플레이어 Collider 간 IgnoreCollision을 설정
    private void SetupIgnorePlayerCollision()
    {
        // playerTransform이 아직 없을 수 있으므로 안전하게 시도
        if (playerTransform == null) FindPlayer();
        if (playerTransform == null) return;

        Collider2D playerCol = playerTransform.GetComponent<Collider2D>();
        if (playerCol == null) return;

        Collider2D[] myCols = GetComponentsInChildren<Collider2D>();
        foreach (var c in myCols)
        {
            if (c == null) continue;
            Physics2D.IgnoreCollision(c, playerCol, true);
        }
    }

    // 주변 대상(레이어마스크)에 대해 분리 벡터 계산
    private Vector2 ComputeSeparation()
    {
        if (rb == null) return Vector2.zero;
        if (separationRadius <= 0f) return Vector2.zero;

        Collider2D[] hits = Physics2D.OverlapCircleAll(rb.position, separationRadius, separationMask);
        Vector2 sep = Vector2.zero;
        int count = 0;
        foreach (var col in hits)
        {
            if (col == null) continue;
            Rigidbody2D otherRb = col.attachedRigidbody;
            if (otherRb == null) continue;
            if (otherRb == rb) continue; // 자기 자신 무시

            Vector2 diff = (Vector2)rb.position - (Vector2)otherRb.position;
            float dist = diff.magnitude;
            if (dist <= 0.0001f) continue;
            // 가까울수록 더 강하게 밀어냄 (역거리 가중)
            sep += diff.normalized / dist;
            count++;
        }

        if (count > 0)
        {
            sep /= count;
            // 정규화하여 방향만 취하거나, 약간 거리 기반 크기 유지
            if (sep.magnitude > 1f) sep = sep.normalized;
        }

        return sep;
    }

    // 디버그용 기즈모: 분리 반경 표시
    void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, separationRadius);
    }
}