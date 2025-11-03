using UnityEngine;
using System.Collections;
using static MonsterAI;

/// <summary>
/// 몬스터 이동 모듈
/// </summary>
public class MonsterMovement
{
    private Rigidbody2D rb;
    private Transform transform;

    public float moveSpeed = 2f;
    public float separationRadius = 1.0f;
    public float separationStrength = 1.0f;
    public LayerMask separationMask;

    public bool ignoreAreaLimit = false; // 도발 상태 등에서 true로

    private Collider2D spawnAreaCollider;

    public MonsterMovement(Rigidbody2D rb, Transform transform)
    {
        this.rb = rb;
        this.transform = transform;

        // 기본 레이어 마스크 설정
        separationMask = LayerMask.GetMask("Monster", "Player", "NPC");
    }

    /// <summary>
    /// 스폰 영역 설정
    /// </summary>
    public void SetSpawnArea(Collider2D areaCollider)
    {
        spawnAreaCollider = areaCollider;
    }

    /// <summary>
    /// 목표 위치로 이동
    /// </summary>
    public void MoveToPosition(Vector2 targetPosition, float speedMultiplier = 1f, bool ignoreArea = false)
    {
        if (rb == null) return;

        // 🔸 영역 제한 적용 (도발 상태나 ignoreArea 옵션이 false일 때만)
        if (!ignoreAreaLimit && spawnAreaCollider != null && !ignoreArea)
        {
            Vector2 clampedTarget = spawnAreaCollider.ClosestPoint(targetPosition);
            float distanceToOriginal = Vector2.Distance(targetPosition, clampedTarget);

            if (distanceToOriginal > 0.1f)
            {
                targetPosition = clampedTarget;
            }
        }

        Vector2 currentPos = rb.position;
        Vector2 toTarget = targetPosition - currentPos;
        float distance = toTarget.magnitude;

        // 🔸 너무 가까우면 즉시 정지 + 위치 보정
        if (distance <= 0.25f)
        {
            rb.velocity = Vector2.zero;
            rb.MovePosition(targetPosition); // 💫 스냅 위치
            return;
        }

        // 🔸 정상 이동
        Vector2 direction = toTarget.normalized;
        Vector2 desiredVel = direction * moveSpeed * speedMultiplier;
        Vector2 separation = ComputeSeparation() * separationStrength;

        Vector2 finalVel = desiredVel + separation;
        float maxVel = moveSpeed * speedMultiplier * 1.2f;

        if (finalVel.magnitude > maxVel)
            finalVel = finalVel.normalized * maxVel;

        // 🔸 영역 경계 보정 (비도발 상태에서만)
        if (!ignoreAreaLimit && spawnAreaCollider != null)
        {
            Vector2 nextPos = currentPos + finalVel * Time.fixedDeltaTime;
            Vector2 clampedNextPos = spawnAreaCollider.ClosestPoint(nextPos);

            if (Vector2.Distance(nextPos, clampedNextPos) > 0.05f)
            {
                Vector2 toInside = (clampedNextPos - currentPos).normalized;
                finalVel = toInside * finalVel.magnitude;
            }
        }

        rb.velocity = finalVel;
    }

    /// <summary>
    /// 이동 정지
    /// </summary>
    public void Stop()
    {
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }
    }

    /// <summary>
    /// 현재 위치 반환
    /// </summary>
    public Vector2 GetPosition()
    {
        return rb != null ? rb.position : Vector2.zero;
    }

    /// <summary>
    /// 목표까지의 거리 반환
    /// </summary>
    public float GetDistanceTo(Vector2 targetPosition)
    {
        return Vector2.Distance(GetPosition(), targetPosition);
    }

    /// <summary>
    /// 주변 대상과의 분리 벡터 계산 (겹침 방지)
    /// </summary>
    private Vector2 ComputeSeparation()
    {
        if (rb == null || separationRadius <= 0f) return Vector2.zero;

        Collider2D[] hits = Physics2D.OverlapCircleAll(rb.position, separationRadius, separationMask);
        Vector2 separation = Vector2.zero;
        int count = 0;

        foreach (var col in hits)
        {
            if (col == null) continue;
            Rigidbody2D otherRb = col.attachedRigidbody;
            if (otherRb == null || otherRb == rb) continue;

            Vector2 diff = rb.position - (Vector2)otherRb.position;
            float dist = diff.magnitude;
            if (dist > 0.0001f)
            {
                separation += diff.normalized / dist;
                count++;
            }
        }

        if (count > 0)
        {
            separation /= count;
            if (separation.magnitude > 1f)
                separation = separation.normalized;
        }

        return separation;
    }

    /// <summary>
    /// 위치를 영역 내로 제한
    /// </summary>
    public Vector2 ClampToArea(Vector2 position, Collider2D areaCollider)
    {
        if (areaCollider == null) return position;
        return areaCollider.ClosestPoint(position);
    }

    /// <summary>
    /// 속도 너무 느리면 멈춤 처리
    /// </summary>
    public void ClampVelocity()
    {
        if (rb == null) return;
        if (rb.velocity.magnitude < 0.05f)
            rb.velocity = Vector2.zero;
    }

    /// <summary>
    /// 부드럽게 위치 보정 (영역 복귀용)
    /// </summary>
    public void SmoothCorrection(Vector2 targetPosition, float lerpSpeed = 0.2f)
    {
        if (rb == null) return;

        Vector2 currentPos = rb.position;
        Vector2 newPos = Vector2.Lerp(currentPos, targetPosition, lerpSpeed);
        rb.MovePosition(newPos);
        rb.velocity = Vector2.zero;
    }
}
