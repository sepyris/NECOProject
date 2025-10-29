using UnityEngine;

/// <summary>
/// 경험치 관리 싱글톤 매니저
/// 다른 스크립트에서 쉽게 경험치를 지급할 수 있도록 함
/// </summary>
public class ExperienceManager : MonoBehaviour
{
    public static ExperienceManager Instance { get; private set; }

    [Header("경험치 배율 설정")]
    [SerializeField] private float expMultiplier = 1.0f; // 경험치 배율 (이벤트 등에 사용)

    [Header("골드 배율 설정")]
    [SerializeField] private float goldMultiplier = 1.0f; // 골드 배율 (이벤트 등에 사용)
    [SerializeField] private float goldRandomRange = 0.2f; // 골드 랜덤 범위 (±20%)

    [Header("레벨별 경험치 테이블 (옵션)")]
    [SerializeField] private bool useCustomExpTable = false;
    [SerializeField] private int[] customExpTable; // 레벨별 필요 경험치

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
    /// 플레이어에게 경험치 지급
    /// </summary>
    public void GiveExperienceToPlayer(int baseAmount)
    {
        if (PlayerController.Instance == null)
        {
            Debug.LogWarning("[ExpManager] PlayerController를 찾을 수 없습니다.");
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
            Debug.LogWarning("[ExpManager] 플레이어에게 스탯 컴포넌트가 없습니다.");
        }
    }

    /// <summary>
    /// 특정 캐릭터에게 경험치 지급
    /// </summary>
    public void GiveExperience(CharacterStats stats, int baseAmount)
    {
        if (stats == null)
        {
            Debug.LogWarning("[ExpManager] CharacterStats가 null입니다.");
            return;
        }

        int finalAmount = Mathf.RoundToInt(baseAmount * expMultiplier);
        stats.GainExperience(finalAmount);
    }

    /// <summary>
    /// 몬스터 처치 시 경험치 지급
    /// </summary>
    public void GiveExpForMonsterKill(int monsterLevel)
    {
        // 몬스터 레벨에 따른 기본 경험치 계산
        int baseExp = 10 + (monsterLevel * 5);
        GiveExperienceToPlayer(baseExp);
    }

    /// <summary>
    /// 몬스터 처치 시 보상 지급 (경험치 + 골드)
    /// </summary>
    public void GiveMonsterRewards(CharacterStats monsterStats)
    {
        if (monsterStats == null || PlayerController.Instance == null) return;

        var playerStats = PlayerController.Instance.GetComponent<PlayerStatsComponent>();
        if (playerStats == null)
        {
            Debug.LogWarning("[ExpManager] 플레이어에게 스탯 컴포넌트가 없습니다.");
            return;
        }

        // 경험치 보상
        int rewardExp = monsterStats.rewardExp;
        if (rewardExp > 0)
        {
            int finalExp = Mathf.RoundToInt(rewardExp * expMultiplier);
            playerStats.Stats.GainExperience(finalExp);
            Debug.Log($"[Reward] 경험치 획득: {finalExp}");
        }

        // 골드 보상 (±20% 랜덤)
        int rewardGold = monsterStats.rewardGold;
        if (rewardGold > 0)
        {
            int finalGold = CalculateRandomGold(rewardGold);
            playerStats.Stats.AddGold(finalGold);
            Debug.Log($"[Reward] 골드 획득: {finalGold}");
        }
    }

    /// <summary>
    /// 골드 랜덤 계산 (±20%)
    /// </summary>
    private int CalculateRandomGold(int baseGold)
    {
        float randomFactor = Random.Range(1f - goldRandomRange, 1f + goldRandomRange);
        int finalGold = Mathf.RoundToInt(baseGold * randomFactor * goldMultiplier);
        return Mathf.Max(1, finalGold); // 최소 1골드
    }

    /// <summary>
    /// 플레이어에게 골드 지급
    /// </summary>
    public void GiveGoldToPlayer(int baseAmount)
    {
        if (PlayerController.Instance == null)
        {
            Debug.LogWarning("[ExpManager] PlayerController를 찾을 수 없습니다.");
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
            Debug.LogWarning("[ExpManager] 플레이어에게 스탯 컴포넌트가 없습니다.");
        }
    }

    /// <summary>
    /// 퀘스트 완료 시 경험치 지급
    /// </summary>
    public void GiveQuestReward(int rewardExp)
    {
        GiveExperienceToPlayer(rewardExp);
        Debug.Log($"[ExpManager] 퀘스트 보상 경험치 지급: {rewardExp}");
    }

    /// <summary>
    /// 경험치 배율 설정 (이벤트, 버프 등에 사용)
    /// </summary>
    public void SetExpMultiplier(float multiplier)
    {
        expMultiplier = Mathf.Max(0.1f, multiplier);
        Debug.Log($"[ExpManager] 경험치 배율 변경: x{expMultiplier}");
    }

    /// <summary>
    /// 골드 배율 설정 (이벤트, 버프 등에 사용)
    /// </summary>
    public void SetGoldMultiplier(float multiplier)
    {
        goldMultiplier = Mathf.Max(0.1f, multiplier);
        Debug.Log($"[ExpManager] 골드 배율 변경: x{goldMultiplier}");
    }

    /// <summary>
    /// 현재 경험치 배율 반환
    /// </summary>
    public float GetExpMultiplier()
    {
        return expMultiplier;
    }

    /// <summary>
    /// 현재 골드 배율 반환
    /// </summary>
    public float GetGoldMultiplier()
    {
        return goldMultiplier;
    }

    /// <summary>
    /// 레벨별 필요 경험치 계산 (커스텀 테이블 사용)
    /// </summary>
    public int GetRequiredExpForLevel(int level)
    {
        if (useCustomExpTable && customExpTable != null && level <= customExpTable.Length)
        {
            return customExpTable[level - 1];
        }
        else
        {
            // 기본 공식: 100 * 1.2^(level-1)
            return Mathf.RoundToInt(100 * Mathf.Pow(1.2f, level - 1));
        }
    }
}