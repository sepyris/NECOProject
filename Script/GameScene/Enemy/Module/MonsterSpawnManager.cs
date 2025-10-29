using UnityEngine;

/// <summary>
/// ���� ���� ���� ���� ���
/// </summary>
public class MonsterSpawnManager
{
    private Transform transform;

    public Vector2 spawnPosition { get; private set; }
    public Collider2D spawnAreaCollider { get; private set; }
    public float movementRadius = 3f;

    public MonsterSpawnManager(Transform transform)
    {
        this.transform = transform;
        spawnPosition = transform.position;
    }

    /// <summary>
    /// ���� ���� ����
    /// </summary>
    public void SetSpawnArea(Collider2D areaCollider)
    {
        spawnAreaCollider = areaCollider;
        spawnPosition = transform.position;
    }

    /// <summary>
    /// ���� �̵� ��ǥ ����
    /// </summary>
    public Vector2 GetRandomMoveTarget()
    {
        Vector2 randomDirection = Random.insideUnitCircle * movementRadius;
        Vector2 targetPosition = (Vector2)transform.position + randomDirection;

        // ���� ���� ���� ����
        if (spawnAreaCollider != null)
        {
            targetPosition = ClampToSpawnArea(targetPosition);
        }

        return targetPosition;
    }

    /// <summary>
    /// ��ġ�� ���� ���� ���� ����
    /// </summary>
    public Vector2 ClampToSpawnArea(Vector2 position)
    {
        if (spawnAreaCollider == null) return position;
        return spawnAreaCollider.ClosestPoint(position);
    }

    /// <summary>
    /// ���� ���� ���� �ִ��� Ȯ��
    /// </summary>
    public bool IsInsideSpawnArea(Vector2 position)
    {
        if (spawnAreaCollider == null) return true;

        Vector2 closest = spawnAreaCollider.ClosestPoint(position);
        float distance = Vector2.Distance(position, closest);

        return distance < 0.05f; // ��� ����
    }

    /// <summary>
    /// ���� ��ġ�� ���Ͱ� �ʿ����� Ȯ��
    /// </summary>
    public bool ShouldReturnToSpawn(Vector2 currentPosition)
    {
        if (spawnAreaCollider == null) return false;
        return !IsInsideSpawnArea(currentPosition);
    }

    /// <summary>
    /// ���� ��ġ������ �Ÿ� ��ȯ
    /// </summary>
    public float GetDistanceFromSpawn(Vector2 currentPosition)
    {
        return Vector2.Distance(currentPosition, spawnPosition);
    }

    /// <summary>
    /// ���� ������ ���� ����� ���� ��ȯ
    /// </summary>
    public Vector2 GetClosestPointInSpawnArea(Vector2 position)
    {
        if (spawnAreaCollider == null) return spawnPosition;
        return spawnAreaCollider.ClosestPoint(position);
    }
}