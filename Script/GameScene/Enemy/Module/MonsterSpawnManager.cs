using UnityEngine;

/// <summary>
/// 몬스터 스폰 영역 관리 모듈 (수정 버전)
/// </summary>
public class MonsterSpawnManager
{
    private Transform transform;
    public Vector2 spawnPosition { get; private set; }
    public Collider2D spawnAreaCollider { get; private set; }
    public float movementRadius = 5f; // ⭐ 3f → 5f (더 넓은 배회 범위)

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

        Debug.Log($"[SpawnManager] 스폰 영역 설정: {areaCollider?.gameObject.name}");
    }

    /// <summary>
    /// 랜덤 이동 목표 생성 (스폰 영역 내로 제한)
    /// </summary>
    public Vector2 GetRandomMoveTarget()
    {
        Vector2 targetPosition;

        // ⭐ 스폰 영역이 설정되어 있으면 영역 내에서만 목표 생성 ⭐
        if (spawnAreaCollider != null)
        {
            Bounds bounds = spawnAreaCollider.bounds;

            // 영역 내에서 랜덤 위치 생성 (최대 10번 시도)
            int maxAttempts = 10;
            for (int i = 0; i < maxAttempts; i++)
            {
                // 영역 bounds 내에서 랜덤 선택
                float randomX = Random.Range(bounds.min.x, bounds.max.x);
                float randomY = Random.Range(bounds.min.y, bounds.max.y);
                Vector2 randomPos = new Vector2(randomX, randomY);

                // 콜라이더 내부에 있는지 확인
                Vector2 closestPoint = spawnAreaCollider.ClosestPoint(randomPos);
                float distance = Vector2.Distance(randomPos, closestPoint);

                // 충분히 내부에 있으면 해당 위치 반환
                if (distance < 0.1f)
                {
                    targetPosition = randomPos;
                    return targetPosition;
                }
            }

            // 실패하면 스폰 위치 근처로
            targetPosition = spawnPosition + (Random.insideUnitCircle * movementRadius);
            targetPosition = ClampToSpawnArea(targetPosition);
        }
        else
        {
            // 스폰 영역이 없으면 스폰 위치 기준으로 반경 내 랜덤
            Vector2 randomDirection = Random.insideUnitCircle * movementRadius;
            targetPosition = spawnPosition + randomDirection;
        }

        Debug.Log($"[SpawnManager] 배회 목표: {targetPosition}");
        return targetPosition;
    }

    /// <summary>
    /// 위치를 스폰 영역 내로 제한
    /// </summary>
    public Vector2 ClampToSpawnArea(Vector2 position)
    {
        if (spawnAreaCollider == null) return position;

        Vector2 clamped = spawnAreaCollider.ClosestPoint(position);

        // 디버그: 영역 밖으로 나가려는 시도 감지
        float distance = Vector2.Distance(position, clamped);
        if (distance > 0.1f)
        {
            Debug.LogWarning($"[SpawnManager] 영역 밖 위치 보정: {position} → {clamped} (거리: {distance:F2})");
        }

        return clamped;
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