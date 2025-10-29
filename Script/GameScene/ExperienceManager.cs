using UnityEngine;

/// <summary>
/// ����ġ ���� �̱��� �Ŵ���
/// �ٸ� ��ũ��Ʈ���� ���� ����ġ�� ������ �� �ֵ��� ��
/// </summary>
public class ExperienceManager : MonoBehaviour
{
    public static ExperienceManager Instance { get; private set; }

    [Header("����ġ ���� ����")]
    [SerializeField] private float expMultiplier = 1.0f; // ����ġ ���� (�̺�Ʈ � ���)

    [Header("��� ���� ����")]
    [SerializeField] private float goldMultiplier = 1.0f; // ��� ���� (�̺�Ʈ � ���)
    [SerializeField] private float goldRandomRange = 0.2f; // ��� ���� ���� (��20%)

    [Header("������ ����ġ ���̺� (�ɼ�)")]
    [SerializeField] private bool useCustomExpTable = false;
    [SerializeField] private int[] customExpTable; // ������ �ʿ� ����ġ

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// �÷��̾�� ����ġ ����
    /// </summary>
    public void GiveExperienceToPlayer(int baseAmount)
    {
        if (PlayerController.Instance == null)
        {
            Debug.LogWarning("[ExpManager] PlayerController�� ã�� �� �����ϴ�.");
            return;
        }

        var playerStats = PlayerController.Instance.GetComponent<PlayerStatsComponent>();
        if (playerStats != null)
        {
            int finalAmount = Mathf.RoundToInt(baseAmount * expMultiplier);
            playerStats.Stats.GainExperience(finalAmount);
        }
        else
        {
            Debug.LogWarning("[ExpManager] �÷��̾�� ���� ������Ʈ�� �����ϴ�.");
        }
    }

    /// <summary>
    /// Ư�� ĳ���Ϳ��� ����ġ ����
    /// </summary>
    public void GiveExperience(CharacterStats stats, int baseAmount)
    {
        if (stats == null)
        {
            Debug.LogWarning("[ExpManager] CharacterStats�� null�Դϴ�.");
            return;
        }

        int finalAmount = Mathf.RoundToInt(baseAmount * expMultiplier);
        stats.GainExperience(finalAmount);
    }

    /// <summary>
    /// ���� óġ �� ����ġ ����
    /// </summary>
    public void GiveExpForMonsterKill(int monsterLevel)
    {
        // ���� ������ ���� �⺻ ����ġ ���
        int baseExp = 10 + (monsterLevel * 5);
        GiveExperienceToPlayer(baseExp);
    }

    /// <summary>
    /// ���� óġ �� ���� ���� (����ġ + ���)
    /// </summary>
    public void GiveMonsterRewards(CharacterStats monsterStats)
    {
        if (monsterStats == null || PlayerController.Instance == null) return;

        var playerStats = PlayerController.Instance.GetComponent<PlayerStatsComponent>();
        if (playerStats == null)
        {
            Debug.LogWarning("[ExpManager] �÷��̾�� ���� ������Ʈ�� �����ϴ�.");
            return;
        }

        // ����ġ ����
        int rewardExp = monsterStats.rewardExp;
        if (rewardExp > 0)
        {
            int finalExp = Mathf.RoundToInt(rewardExp * expMultiplier);
            playerStats.Stats.GainExperience(finalExp);
            Debug.Log($"[Reward] ����ġ ȹ��: {finalExp}");
        }

        // ��� ���� (��20% ����)
        int rewardGold = monsterStats.rewardGold;
        if (rewardGold > 0)
        {
            int finalGold = CalculateRandomGold(rewardGold);
            playerStats.Stats.AddGold(finalGold);
            Debug.Log($"[Reward] ��� ȹ��: {finalGold}");
        }
    }

    /// <summary>
    /// ��� ���� ��� (��20%)
    /// </summary>
    private int CalculateRandomGold(int baseGold)
    {
        float randomFactor = Random.Range(1f - goldRandomRange, 1f + goldRandomRange);
        int finalGold = Mathf.RoundToInt(baseGold * randomFactor * goldMultiplier);
        return Mathf.Max(1, finalGold); // �ּ� 1���
    }

    /// <summary>
    /// �÷��̾�� ��� ����
    /// </summary>
    public void GiveGoldToPlayer(int baseAmount)
    {
        if (PlayerController.Instance == null)
        {
            Debug.LogWarning("[ExpManager] PlayerController�� ã�� �� �����ϴ�.");
            return;
        }

        var playerStats = PlayerController.Instance.GetComponent<PlayerStatsComponent>();
        if (playerStats != null)
        {
            int finalAmount = Mathf.RoundToInt(baseAmount * goldMultiplier);
            playerStats.Stats.AddGold(finalAmount);
        }
        else
        {
            Debug.LogWarning("[ExpManager] �÷��̾�� ���� ������Ʈ�� �����ϴ�.");
        }
    }

    /// <summary>
    /// ����Ʈ �Ϸ� �� ����ġ ����
    /// </summary>
    public void GiveQuestReward(int rewardExp)
    {
        GiveExperienceToPlayer(rewardExp);
        Debug.Log($"[ExpManager] ����Ʈ ���� ����ġ ����: {rewardExp}");
    }

    /// <summary>
    /// ����ġ ���� ���� (�̺�Ʈ, ���� � ���)
    /// </summary>
    public void SetExpMultiplier(float multiplier)
    {
        expMultiplier = Mathf.Max(0.1f, multiplier);
        Debug.Log($"[ExpManager] ����ġ ���� ����: x{expMultiplier}");
    }

    /// <summary>
    /// ��� ���� ���� (�̺�Ʈ, ���� � ���)
    /// </summary>
    public void SetGoldMultiplier(float multiplier)
    {
        goldMultiplier = Mathf.Max(0.1f, multiplier);
        Debug.Log($"[ExpManager] ��� ���� ����: x{goldMultiplier}");
    }

    /// <summary>
    /// ���� ����ġ ���� ��ȯ
    /// </summary>
    public float GetExpMultiplier()
    {
        return expMultiplier;
    }

    /// <summary>
    /// ���� ��� ���� ��ȯ
    /// </summary>
    public float GetGoldMultiplier()
    {
        return goldMultiplier;
    }

    /// <summary>
    /// ������ �ʿ� ����ġ ��� (Ŀ���� ���̺� ���)
    /// </summary>
    public int GetRequiredExpForLevel(int level)
    {
        if (useCustomExpTable && customExpTable != null && level <= customExpTable.Length)
        {
            return customExpTable[level - 1];
        }
        else
        {
            // �⺻ ����: 100 * 1.2^(level-1)
            return Mathf.RoundToInt(100 * Mathf.Pow(1.2f, level - 1));
        }
    }
}