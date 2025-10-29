using UnityEngine;

/// <summary>
/// ���� ���� ������Ʈ
/// MonsterController�� �߰��Ͽ� ���
/// </summary>
public class MonsterStatsComponent : MonoBehaviour
{
    [Header("���� ����")]
    public CharacterStats Stats = new CharacterStats();

    [Header("�ʱ� ����")]
    [SerializeField] private string monsterName = "Goblin";
    [SerializeField] private int monsterLevel = 1;

    [Header("���� ����")]
    [SerializeField] private int baseRewardExp = 50;   // �⺻ ����ġ ����
    [SerializeField] private int baseRewardGold = 10;  // �⺻ ��� ����

    void Awake()
    {
        InitializeMonsterStats();
        SubscribeToEvents();
    }

    void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    /// <summary>
    /// ���� ���� �ʱ�ȭ
    /// </summary>
    private void InitializeMonsterStats()
    {
        // �⺻ ���� ����
        Stats.Initialize(monsterName, monsterLevel,true);

        // ���� ����
        Stats.rewardExp = baseRewardExp;
        Stats.rewardGold = baseRewardGold;

        Debug.Log($"[Monster] '{monsterName}' Lv.{monsterLevel} �ʱ�ȭ (����: EXP {baseRewardExp}, Gold {baseRewardGold})");
    }

    /// <summary>
    /// �̺�Ʈ ����
    /// </summary>
    private void SubscribeToEvents()
    {
        Stats.OnDeath += OnMonsterDeath;
        Stats.OnStatsChanged += OnStatsChanged;
    }

    /// <summary>
    /// �̺�Ʈ ���� ����
    /// </summary>
    private void UnsubscribeFromEvents()
    {
        Stats.OnDeath -= OnMonsterDeath;
        Stats.OnStatsChanged -= OnStatsChanged;
    }

    /// <summary>
    /// ���� ��� ó��
    /// </summary>
    private void OnMonsterDeath()
    {
        Debug.Log($"[Monster] {Stats.characterName} ���! ���� ���� ��...");

        // �÷��̾�� ���� ����
        if (ExperienceManager.Instance != null)
        {
            ExperienceManager.Instance.GiveMonsterRewards(Stats);
        }
        

        // ��� �ִϸ��̼� �� ������Ʈ ����
        HandleDeath();
    }

    /// <summary>
    /// ��� ó�� ����
    /// </summary>
    private void HandleDeath()
    {
        // ���� MonsterController ������ ����
        // �ִϸ��̼�, ����Ʈ ��

        // 0.5�� �� ������Ʈ ����
        Destroy(gameObject, 0.5f);
    }

    /// <summary>
    /// ���� ���� �� (UI ������Ʈ ��)
    /// </summary>
    private void OnStatsChanged()
    {
        // HP �� ������Ʈ ��
    }

    /// <summary>
    /// ������ �ޱ� (�ܺο��� ȣ��)
    /// </summary>
    public void TakeDamage(int damage)
    {
        Stats.TakeDamage(damage);
    }

    /// <summary>
    /// ���� ���� ���� (���� �� ���)
    /// </summary>
    public void SetMonsterStats(string name, int level, int rewardExp, int rewardGold)
    {
        monsterName = name;
        monsterLevel = level;
        baseRewardExp = rewardExp;
        baseRewardGold = rewardGold;

        InitializeMonsterStats();
    }
}