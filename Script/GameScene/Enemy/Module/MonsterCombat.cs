using UnityEngine;

/// <summary>
/// ���� ���� ���
/// </summary>
public class MonsterCombat
{
    private Transform transform;
    private MonsterStatsComponent statsComponent;

    public float attackRange = 1.5f;
    public float attackCooldown = 2f;
    public float preferredAttackDistance = 1.0f;

    private float lastAttackTime = -999f;

    public MonsterCombat(Transform transform, MonsterStatsComponent statsComponent)
    {
        this.transform = transform;
        this.statsComponent = statsComponent;
    }

    /// <summary>
    /// ���� ���� ���� Ȯ��
    /// </summary>
    public bool CanAttack()
    {
        return Time.time - lastAttackTime >= attackCooldown;
    }

    /// <summary>
    /// �÷��̾� ���� �õ�
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
    /// ���� ���� ����
    /// </summary>
    private void PerformAttack(Transform target)
    {
        lastAttackTime = Time.time;

        if (statsComponent == null)
        {
            Debug.LogWarning("[MonsterCombat] MonsterStatsComponent�� �����ϴ�!");
            return;
        }

        // �÷��̾�� ������ ����
        var playerStats = target.GetComponent<PlayerStatsComponent>();
        if (playerStats != null)
        {
            int damage = statsComponent.Stats.attackPower;
            playerStats.Stats.TakeDamage(damage);
            Debug.Log($"[Monster] �÷��̾� ����! ������: {damage}");
        }
    }

    /// <summary>
    /// ���� ���� ���� �ִ��� Ȯ��
    /// </summary>
    public bool IsInAttackRange(Transform target)
    {
        if (target == null) return false;

        float distance = Vector2.Distance(transform.position, target.position);
        return distance <= attackRange;
    }

    /// <summary>
    /// ��ȣ ���� �Ÿ��� �����ߴ��� Ȯ��
    /// </summary>
    public bool IsAtPreferredDistance(Transform target)
    {
        if (target == null) return false;

        float distance = Vector2.Distance(transform.position, target.position);
        return distance <= preferredAttackDistance;
    }

    /// <summary>
    /// ��ǥ������ �Ÿ� ��ȯ
    /// </summary>
    public float GetDistanceTo(Transform target)
    {
        if (target == null) return float.MaxValue;
        return Vector2.Distance(transform.position, target.position);
    }
}
