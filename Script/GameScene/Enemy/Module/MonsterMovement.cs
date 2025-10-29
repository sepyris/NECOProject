using UnityEngine;

/// <summary>
/// ���� �̵� ���
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

        // �⺻ ���̾� ����ũ ����
        separationMask = LayerMask.GetMask("Monster", "Player", "NPC");
    }

    /// <summary>
    /// ��ǥ ��ġ�� �̵�
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
    /// �̵� ����
    /// </summary>
    public void Stop()
    {
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }
    }

    /// <summary>
    /// ���� ��ġ ��ȯ
    /// </summary>
    public Vector2 GetPosition()
    {
        return rb != null ? rb.position : Vector2.zero;
    }

    /// <summary>
    /// ��ǥ������ �Ÿ� ��ȯ
    /// </summary>
    public float GetDistanceTo(Vector2 targetPosition)
    {
        return Vector2.Distance(GetPosition(), targetPosition);
    }

    /// <summary>
    /// �ֺ� ������ �и� ���� ��� (��ħ ����)
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
                // �Ÿ� ��� ����ġ (�������� ���ϰ�)
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
    /// ��ġ�� ���� ���� ����
    /// </summary>
    public Vector2 ClampToArea(Vector2 position, Collider2D areaCollider)
    {
        if (areaCollider == null) return position;
        return areaCollider.ClosestPoint(position);
    }

    /// <summary>
    /// �ε巴�� ��ġ ���� (���� ���Ϳ�)
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