using UnityEngine;

/// <summary>
/// 몬스터 전투 모듈 (CSV 데이터 기반)
/// </summary>
public class MonsterCombat
{
    private Transform transform;
    private MonsterController controller;

    public float attackRange = 1.5f;
    public float attackCooldown = 2f;
    public float preferredAttackDistance = 1.0f;

    private float lastAttackTime = -999f;

    public MonsterCombat(Transform transform, MonsterController controller)
    {
        this.transform = transform;
        this.controller = controller;
    }

    /// <summary>
    /// 공격 가능 여부 확인
    /// </summary>
    public bool CanAttack()
    {
        return Time.time - lastAttackTime >= attackCooldown;
    }

    /// <summary>
    /// 플레이어 공격 시도
    /// </summary>
    public bool TryAttackPlayer(Transform playerTransform)
    {
        if (playerTransform == null || !CanAttack()) return false;

        float distance = Vector2.Distance(transform.position, playerTransform.position);
        if (distance <= attackRange)
        {
            PerformAttack(playerTransform);
            return true;
        }

        return false;
    }

    /// <summary>
    /// 실제 공격 수행
    /// </summary>
    private void PerformAttack(Transform target)
    {
        lastAttackTime = Time.time;

        if (controller == null)
        {
            Debug.LogWarning("[MonsterCombat] MonsterController가 없습니다!");
            return;
        }

        // 플레이어에게 데미지 적용
        var playerStats = target.GetComponent<PlayerStatsComponent>();
        if (playerStats != null)
        {
            int damage = controller.GetAttackPower();
            playerStats.Stats.TakeDamage(damage);
            Debug.Log($"[Monster] 플레이어 공격! 데미지: {damage}");
        }
    }

    /// <summary>
    /// 공격 범위 내에 있는지 확인
    /// </summary>
    public bool IsInAttackRange(Transform target)
    {
        if (target == null) return false;
        float distance = Vector2.Distance(transform.position, target.position);
        return distance <= attackRange;
    }

    /// <summary>
    /// 선호 공격 거리에 도달했는지 확인
    /// </summary>
    public bool IsAtPreferredDistance(Transform target)
    {
        if (target == null) return false;
        float distance = Vector2.Distance(transform.position, target.position);
        return distance <= preferredAttackDistance;
    }

    /// <summary>
    /// 목표까지의 거리 반환
    /// </summary>
    public float GetDistanceTo(Transform target)
    {
        if (target == null) return float.MaxValue;
        return Vector2.Distance(transform.position, target.position);
    }
}