using UnityEngine;

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

    public MonsterMovement(Rigidbody2D rb, Transform transform)
    {
        this.rb = rb;
        this.transform = transform;

        // 기본 레이어 마스크 설정
        separationMask = LayerMask.GetMask("Monster", "Player", "NPC");
    }

    /// <summary>
    /// 목표 위치로 이동
    /// </summary>
    public void MoveToPosition(Vector2 targetPosition, float speedMultiplier = 1f)
    {
        if (rb == null) return;

        Vector2 currentPos = rb.position;
        Vector2 direction = (targetPosition - currentPos).normalized;
        float distance = Vector2.Distance(currentPos, targetPosition);

        if (distance > 0.2f)
        {
            Vector2 desiredVel = direction * moveSpeed * speedMultiplier;
            Vector2 separation = ComputeSeparation() * separationStrength;
            Vector2 finalVel = desiredVel + separation;

            float maxVel = moveSpeed * speedMultiplier * 1.2f;
            if (finalVel.magnitude > maxVel)
            {
                finalVel = finalVel.normalized * maxVel;
            }

            rb.velocity = finalVel;
        }
        else
        {
            rb.velocity = Vector2.zero;
        }
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
                // 거리 기반 가중치 (가까울수록 강하게)
                separation += diff.normalized / dist;
                count++;
            }
        }

        if (count > 0)
        {
            separation /= count;
            if (separation.magnitude > 1f)
            {
                separation = separation.normalized;
            }
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