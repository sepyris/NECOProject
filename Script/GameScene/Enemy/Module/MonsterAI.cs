using UnityEngine;
using System.Collections;

/// <summary>
/// 몬스터 AI 모듈
/// </summary>
public class MonsterAI
{
    private Transform transform;
    private MonsterMovement movement;
    private MonsterCombat combat;
    private MonsterSpawnManager spawnManager;

    // AI 설정
    public bool isAggressive = true;  // 선공 여부
    public float detectionRange = 5f;

    // 배회 설정
    public float minWaitTime = 3f;
    public float maxWaitTime = 10f;
    public float idleProbability = 0.2f;

    // 도발 상태
    public float provokedDuration = 8f;
    private float provokedTimeRemaining = 0f;
    private bool isProvoked = false;
    private bool originalIsAggressive;

    // 귀환 설정
    public float returnSpeedMultiplier = 0.8f;
    public float returnStopDistance = 0.15f;
    public float returnLerpSpeed = 0.2f;
    private bool isReturning = false;

    // 상태
    private MonsterState currentState = MonsterState.Idle;
    private Vector2 moveTargetPosition;
    private MonoBehaviour coroutineRunner;

    public enum MonsterState { Idle, Wandering, Chasing, Attacking, Returning }

    public MonsterAI(Transform transform, MonsterMovement movement, MonsterCombat combat, MonsterSpawnManager spawnManager)
    {
        this.transform = transform;
        this.movement = movement;
        this.combat = combat;
        this.spawnManager = spawnManager;
        this.originalIsAggressive = isAggressive;

        // 코루틴 실행용 MonoBehaviour 찾기
        coroutineRunner = transform.GetComponent<MonsterController>();
        if (coroutineRunner != null)
        {
            coroutineRunner.StartCoroutine(AIRoutine());
        }
    }

    /// <summary>
    /// AI 업데이트 (매 프레임)
    /// </summary>
    public void UpdateAI(Transform playerTransform)
    {
        // 도발 타이머 감소
        if (isProvoked)
        {
            provokedTimeRemaining -= Time.deltaTime;
            if (provokedTimeRemaining <= 0f)
            {
                isProvoked = false;
                provokedTimeRemaining = 0f;
                StartReturnToSpawn();
            }
        }

        // 플레이어 감지 및 상태 전환
        if (playerTransform != null)
        {
            UpdateStateByPlayer(playerTransform);
        }
    }

    /// <summary>
    /// 현재 상태 실행
    /// </summary>
    public void ExecuteCurrentState()
    {
        // 귀환 우선 처리
        if (isReturning)
        {
            PerformReturn();
            return;
        }

        switch (currentState)
        {
            case MonsterState.Idle:
                movement.Stop();
                break;

            case MonsterState.Wandering:
                movement.MoveToPosition(moveTargetPosition, 1f);
                // 목표 도착 시 상태 변경
                if (movement.GetDistanceTo(moveTargetPosition) <= 0.2f)
                {
                    currentState = MonsterState.Idle;
                }
                break;

            case MonsterState.Chasing:
                // ChasePlayer는 UpdateStateByPlayer에서 처리됨
                break;

            case MonsterState.Attacking:
                movement.Stop();
                break;
        }

        // 스폰 영역 복귀 체크 (도발/귀환 중이 아닐 때)
        if (!isProvoked && !isReturning)
        {
            EnsureInsideSpawnArea();
        }
    }

    /// <summary>
    /// 플레이어에 따른 상태 업데이트
    /// </summary>
    private void UpdateStateByPlayer(Transform playerTransform)
    {
        float distanceToPlayer = combat.GetDistanceTo(playerTransform);

        // 선공이거나 도발 상태일 때만 추적/공격
        if (isAggressive || isProvoked)
        {
            if (combat.IsInAttackRange(playerTransform))
            {
                currentState = MonsterState.Attacking;
                combat.TryAttackPlayer(playerTransform);
            }
            else if (distanceToPlayer <= detectionRange)
            {
                currentState = MonsterState.Chasing;
                ChasePlayer(playerTransform);
            }
            else if (currentState == MonsterState.Chasing && distanceToPlayer > detectionRange * 1.2f)
            {
                currentState = MonsterState.Wandering;
            }
        }
        else
        {
            // 비선공: 공격 중이었다가 멀어지면 배회로 전환
            if (currentState == MonsterState.Attacking && !combat.IsInAttackRange(playerTransform))
            {
                currentState = MonsterState.Wandering;
            }
        }
    }

    /// <summary>
    /// 플레이어 추적
    /// </summary>
    private void ChasePlayer(Transform playerTransform)
    {
        Vector2 targetPosition = playerTransform.position;

        // 도발 상태가 아니면 스폰 영역으로 제한
        if (!isProvoked && spawnManager.spawnAreaCollider != null)
        {
            targetPosition = spawnManager.ClampToSpawnArea(targetPosition);
        }

        float distanceToTarget = movement.GetDistanceTo(targetPosition);
        float stopDistance = Mathf.Max(combat.preferredAttackDistance, 0.1f);

        if (distanceToTarget > stopDistance)
        {
            movement.MoveToPosition(targetPosition, 1.5f);
        }
        else
        {
            // 선공이거나 도발 상태면 공격으로 전환
            if (isAggressive || isProvoked)
            {
                movement.Stop();
                currentState = MonsterState.Attacking;
            }
            else
            {
                movement.Stop();
                currentState = MonsterState.Wandering;
            }
        }
    }

    /// <summary>
    /// 스폰 영역 내로 부드럽게 복귀
    /// </summary>
    private void EnsureInsideSpawnArea()
    {
        if (spawnManager.spawnAreaCollider == null) return;

        Vector2 currentPos = movement.GetPosition();
        Vector2 closest = spawnManager.GetClosestPointInSpawnArea(currentPos);
        float dist = Vector2.Distance(currentPos, closest);

        if (dist > 0.05f)
        {
            movement.SmoothCorrection(closest, returnLerpSpeed);
        }
    }

    /// <summary>
    /// 스폰 위치로 귀환
    /// </summary>
    private void PerformReturn()
    {
        Vector2 currentPos = movement.GetPosition();
        Vector2 target = spawnManager.spawnPosition;

        if (spawnManager.spawnAreaCollider != null)
        {
            target = spawnManager.ClampToSpawnArea(target);
        }

        float dist = Vector2.Distance(currentPos, target);

        if (dist <= returnStopDistance)
        {
            // 귀환 완료
            isReturning = false;
            movement.Stop();
            isAggressive = originalIsAggressive;
            currentState = MonsterState.Idle;
            return;
        }

        // 부드럽게 이동
        movement.MoveToPosition(target, returnSpeedMultiplier);
    }

    /// <summary>
    /// 랜덤 배회 목표 설정
    /// </summary>
    private void SetRandomMoveTarget()
    {
        moveTargetPosition = spawnManager.GetRandomMoveTarget();
    }

    /// <summary>
    /// 도발 상태 설정 (플레이어 공격 받음)
    /// </summary>
    public void SetProvoked()
    {
        isProvoked = true;
        provokedTimeRemaining = provokedDuration;

        if (isReturning)
        {
            isReturning = false;
        }

        // 강제로 선공 모드 (귀환 완료 시 복원)
        isAggressive = true;
    }

    /// <summary>
    /// 스폰 영역으로 귀환 시작
    /// </summary>
    private void StartReturnToSpawn()
    {
        isReturning = true;
        currentState = MonsterState.Returning;
    }

    /// <summary>
    /// AI 루틴 (코루틴)
    /// </summary>
    private IEnumerator AIRoutine()
    {
        while (true)
        {
            if (currentState != MonsterState.Chasing &&
                currentState != MonsterState.Attacking &&
                currentState != MonsterState.Returning)
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
}