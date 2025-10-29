using UnityEngine;

/// <summary>
/// 몬스터 스폰 영역 관리 모듈
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
    /// 스폰 영역 설정
    /// </summary>
    public void SetSpawnArea(Collider2D areaCollider)
    {
        spawnAreaCollider = areaCollider;
        spawnPosition = transform.position;
    }

    /// <summary>
    /// 랜덤 이동 목표 생성
    /// </summary>
    public Vector2 GetRandomMoveTarget()
    {
        Vector2 randomDirection = Random.insideUnitCircle * movementRadius;
        Vector2 targetPosition = (Vector2)transform.position + randomDirection;

        // 스폰 영역 내로 제한
        if (spawnAreaCollider != null)
        {
            targetPosition = ClampToSpawnArea(targetPosition);
        }

        return targetPosition;
    }

    /// <summary>
    /// 위치를 스폰 영역 내로 제한
    /// </summary>
    public Vector2 ClampToSpawnArea(Vector2 position)
    {
        if (spawnAreaCollider == null) return position;
        return spawnAreaCollider.ClosestPoint(position);
    }

    /// <summary>
    /// 스폰 영역 내에 있는지 확인
    /// </summary>
    public bool IsInsideSpawnArea(Vector2 position)
    {
        if (spawnAreaCollider == null) return true;

        Vector2 closest = spawnAreaCollider.ClosestPoint(position);
        float distance = Vector2.Distance(position, closest);

        return distance < 0.05f; // 허용 오차
    }

    /// <summary>
    /// 스폰 위치로 복귀가 필요한지 확인
    /// </summary>
    public bool ShouldReturnToSpawn(Vector2 currentPosition)
    {
        if (spawnAreaCollider == null) return false;
        return !IsInsideSpawnArea(currentPosition);
    }

    /// <summary>
    /// 스폰 위치까지의 거리 반환
    /// </summary>
    public float GetDistanceFromSpawn(Vector2 currentPosition)
    {
        return Vector2.Distance(currentPosition, spawnPosition);
    }

    /// <summary>
    /// 스폰 영역의 가장 가까운 지점 반환
    /// </summary>
    public Vector2 GetClosestPointInSpawnArea(Vector2 position)
    {
        if (spawnAreaCollider == null) return spawnPosition;
        return spawnAreaCollider.ClosestPoint(position);
    }
}