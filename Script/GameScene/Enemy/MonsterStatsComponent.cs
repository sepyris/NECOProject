using UnityEngine;

/// <summary>
/// 몬스터 스탯 컴포넌트
/// MonsterController에 추가하여 사용
/// </summary>
public class MonsterStatsComponent : MonoBehaviour
{
    [Header("몬스터 스탯")]
    public CharacterStats Stats = new CharacterStats();

    [Header("초기 설정")]
    [SerializeField] private string monsterName = "Goblin";
    [SerializeField] private int monsterLevel = 1;

    [Header("보상 설정")]
    [SerializeField] private int baseRewardExp = 50;   // 기본 경험치 보상
    [SerializeField] private int baseRewardGold = 10;  // 기본 골드 보상

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
    /// 몬스터 스탯 초기화
    /// </summary>
    private void InitializeMonsterStats()
    {
        // 기본 스탯 설정
        Stats.Initialize(monsterName, monsterLevel,true);

        // 보상 설정
        Stats.rewardExp = baseRewardExp;
        Stats.rewardGold = baseRewardGold;

        Debug.Log($"[Monster] '{monsterName}' Lv.{monsterLevel} 초기화 (보상: EXP {baseRewardExp}, Gold {baseRewardGold})");
    }

    /// <summary>
    /// 이벤트 구독
    /// </summary>
    private void SubscribeToEvents()
    {
        Stats.OnDeath += OnMonsterDeath;
        Stats.OnStatsChanged += OnStatsChanged;
    }

    /// <summary>
    /// 이벤트 구독 해제
    /// </summary>
    private void UnsubscribeFromEvents()
    {
        Stats.OnDeath -= OnMonsterDeath;
        Stats.OnStatsChanged -= OnStatsChanged;
    }

    /// <summary>
    /// 몬스터 사망 처리
    /// </summary>
    private void OnMonsterDeath()
    {
        Debug.Log($"[Monster] {Stats.characterName} 사망! 보상 지급 중...");

        // 플레이어에게 보상 지급
        if (ExperienceManager.Instance != null)
        {
            ExperienceManager.Instance.GiveMonsterRewards(Stats);
        }
        

        // 사망 애니메이션 및 오브젝트 제거
        HandleDeath();
    }

    /// <summary>
    /// 사망 처리 로직
    /// </summary>
    private void HandleDeath()
    {
        // 기존 MonsterController 로직과 통합
        // 애니메이션, 이펙트 등

        // 0.5초 후 오브젝트 삭제
        Destroy(gameObject, 0.5f);
    }

    /// <summary>
    /// 스탯 변경 시 (UI 업데이트 등)
    /// </summary>
    private void OnStatsChanged()
    {
        // HP 바 업데이트 등
    }

    /// <summary>
    /// 데미지 받기 (외부에서 호출)
    /// </summary>
    public void TakeDamage(int damage)
    {
        Stats.TakeDamage(damage);
    }

    /// <summary>
    /// 몬스터 스탯 설정 (스폰 시 사용)
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