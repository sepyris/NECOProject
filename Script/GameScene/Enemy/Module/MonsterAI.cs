using UnityEngine;
using System.Collections;

/// <summary>
/// ���� AI ���
/// </summary>
public class MonsterAI
{
    private Transform transform;
    private MonsterMovement movement;
    private MonsterCombat combat;
    private MonsterSpawnManager spawnManager;

    // AI ����
    public bool isAggressive = true;  // ���� ����
    public float detectionRange = 5f;

    // ��ȸ ����
    public float minWaitTime = 3f;
    public float maxWaitTime = 10f;
    public float idleProbability = 0.2f;

    // ���� ����
    public float provokedDuration = 8f;
    private float provokedTimeRemaining = 0f;
    private bool isProvoked = false;
    private bool originalIsAggressive;

    // ��ȯ ����
    public float returnSpeedMultiplier = 0.8f;
    public float returnStopDistance = 0.15f;
    public float returnLerpSpeed = 0.2f;
    private bool isReturning = false;

    // ����
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

        // �ڷ�ƾ ����� MonoBehaviour ã��
        coroutineRunner = transform.GetComponent<MonsterController>();
        if (coroutineRunner != null)
        {
            coroutineRunner.StartCoroutine(AIRoutine());
        }
    }

    /// <summary>
    /// AI ������Ʈ (�� ������)
    /// </summary>
    public void UpdateAI(Transform playerTransform)
    {
        // ���� Ÿ�̸� ����
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

        // �÷��̾� ���� �� ���� ��ȯ
        if (playerTransform != null)
        {
            UpdateStateByPlayer(playerTransform);
        }
    }

    /// <summary>
    /// ���� ���� ����
    /// </summary>
    public void ExecuteCurrentState()
    {
        // ��ȯ �켱 ó��
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
                // ��ǥ ���� �� ���� ����
                if (movement.GetDistanceTo(moveTargetPosition) <= 0.2f)
                {
                    currentState = MonsterState.Idle;
                }
                break;

            case MonsterState.Chasing:
                // ChasePlayer�� UpdateStateByPlayer���� ó����
                break;

            case MonsterState.Attacking:
                movement.Stop();
                break;
        }

        // ���� ���� ���� üũ (����/��ȯ ���� �ƴ� ��)
        if (!isProvoked && !isReturning)
        {
            EnsureInsideSpawnArea();
        }
    }

    /// <summary>
    /// �÷��̾ ���� ���� ������Ʈ
    /// </summary>
    private void UpdateStateByPlayer(Transform playerTransform)
    {
        float distanceToPlayer = combat.GetDistanceTo(playerTransform);

        // �����̰ų� ���� ������ ���� ����/����
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
            // �񼱰�: ���� ���̾��ٰ� �־����� ��ȸ�� ��ȯ
            if (currentState == MonsterState.Attacking && !combat.IsInAttackRange(playerTransform))
            {
                currentState = MonsterState.Wandering;
            }
        }
    }

    /// <summary>
    /// �÷��̾� ����
    /// </summary>
    private void ChasePlayer(Transform playerTransform)
    {
        Vector2 targetPosition = playerTransform.position;

        // ���� ���°� �ƴϸ� ���� �������� ����
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
            // �����̰ų� ���� ���¸� �������� ��ȯ
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
    /// ���� ���� ���� �ε巴�� ����
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
    /// ���� ��ġ�� ��ȯ
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
            // ��ȯ �Ϸ�
            isReturning = false;
            movement.Stop();
            isAggressive = originalIsAggressive;
            currentState = MonsterState.Idle;
            return;
        }

        // �ε巴�� �̵�
        movement.MoveToPosition(target, returnSpeedMultiplier);
    }

    /// <summary>
    /// ���� ��ȸ ��ǥ ����
    /// </summary>
    private void SetRandomMoveTarget()
    {
        moveTargetPosition = spawnManager.GetRandomMoveTarget();
    }

    /// <summary>
    /// ���� ���� ���� (�÷��̾� ���� ����)
    /// </summary>
    public void SetProvoked()
    {
        isProvoked = true;
        provokedTimeRemaining = provokedDuration;

        if (isReturning)
        {
            isReturning = false;
        }

        // ������ ���� ��� (��ȯ �Ϸ� �� ����)
        isAggressive = true;
    }

    /// <summary>
    /// ���� �������� ��ȯ ����
    /// </summary>
    private void StartReturnToSpawn()
    {
        isReturning = true;
        currentState = MonsterState.Returning;
    }

    /// <summary>
    /// AI ��ƾ (�ڷ�ƾ)
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