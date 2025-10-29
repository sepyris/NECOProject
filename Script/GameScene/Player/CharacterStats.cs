using System;
using UnityEngine;

/// <summary>
/// 캐릭터 스탯 시스템 (플레이어, 몬스터, NPC 공용)
/// </summary>
[System.Serializable]
public class CharacterStats
{
    [Header("기본 정보")]
    public string characterName = "Unknown";
    public int level = 1;
    public int currentExp = 0;
    public int expToNextLevel = 100;
    public int gold = 0;  // 보유 골드 (플레이어용)

    [Header("기본 스탯")]
    public int strength = 10;      // 힘 (공격력 증가)
    public int dexterity = 10;     // 민첩 (명중률, 회피율 증가)
    public int intelligence = 10;  // 지능 (마법 공격력 증가)

    [Header("전투 스탯")]
    public int maxHP = 100;
    public int currentHP = 100;
    public int attackPower = 10;
    public int defense = 5;
    public float criticalChance = 5f;      // % 단위
    public float criticalDamage = 150f;    // % 단위 (기본 150%)
    public float evasionRate = 5f;         // % 단위
    public float accuracy = 95f;           // % 단위

    [Header("보상 정보 (몬스터용)")]
    public int rewardExp = 0;     // 처치 시 주는 경험치
    public int rewardGold = 0;    // 처치 시 주는 골드

    // 스탯 변경 이벤트
    public event Action OnStatsChanged;
    public event Action OnLevelUp;
    public event Action OnDeath;
    public event Action<int> OnExpGained;  // 획득한 경험치량
    public event Action<int> OnGoldChanged; // 골드 변경

    private bool is_monster_stat = false;


    /// <summary>
    /// 스탯 초기화 (기본값 설정)
    /// </summary>
    public void Initialize(string name = "Character", int startLevel = 1,bool is_monster = false)
    {
        characterName = name;
        level = startLevel;
        currentExp = 0;
        is_monster_stat = is_monster;

        // 레벨에 따른 기본 스탯 설정
        strength = 10 + (level - 1) * 2;
        dexterity = 10 + (level - 1) * 2;
        intelligence = 10 + (level - 1) * 2;

        RecalculateStats();
        currentHP = maxHP;
    }

    /// <summary>
    /// 기본 스탯을 기반으로 전투 스탯 재계산
    /// </summary>
    public void RecalculateStats()
    {
        //몬스터일경우 기본 스탯사용
        if(is_monster_stat)
        {
            return;
        }
        // 힘 -> 공격력 (힘 1당 공격력 2)
        attackPower = 10 + (strength * 2);

        // 레벨 -> 최대 HP (레벨당 20 HP)
        maxHP = 100 + (level - 1) * 20;

        // 힘 -> 방어력 (힘 2당 방어력 1)
        defense = 5 + (strength / 2);

        // 민첩 -> 명중률 (민첩 2당 1%)
        accuracy = 95f + (dexterity * 0.5f);

        // 민첩 -> 회피율 (민첩 3당 1%)
        evasionRate = 5f + (dexterity / 3f);

        // 민첩 -> 크리티컬 확률 (민첩 4당 1%)
        criticalChance = 5f + (dexterity / 4f);

        // 지능 -> 크리티컬 데미지 (지능 5당 5%)
        criticalDamage = 150f + (intelligence);

        OnStatsChanged?.Invoke();
    }

    /// <summary>
    /// 경험치 획득
    /// </summary>
    public void GainExperience(int amount)
    {
        if (amount <= 0) return;

        currentExp += amount;
        OnExpGained?.Invoke(amount);

        Debug.Log($"[{characterName}] 경험치 +{amount} (현재: {currentExp}/{expToNextLevel})");

        // 레벨업 체크
        while (currentExp >= expToNextLevel)
        {
            LevelUp();
        }
    }

    /// <summary>
    /// 레벨업 처리
    /// </summary>
    private void LevelUp()
    {
        currentExp -= expToNextLevel;
        level++;

        // 다음 레벨 필요 경험치 계산 (지수 증가)
        expToNextLevel = Mathf.RoundToInt(100 * Mathf.Pow(1.2f, level - 1));

        // 스탯 증가
        strength += 2;
        dexterity += 2;
        intelligence += 2;

        // 전투 스탯 재계산
        int oldMaxHP = maxHP;
        RecalculateStats();

        // HP 회복 (증가한 최대 HP만큼)
        int hpIncrease = maxHP - oldMaxHP;
        currentHP += hpIncrease;

        Debug.Log($"[{characterName}] 레벨 업! Lv.{level} (힘:{strength}, 민:{dexterity}, 지:{intelligence})");

        OnLevelUp?.Invoke();
    }

    /// <summary>
    /// 데미지 받기
    /// </summary>
    public int TakeDamage(int damage)
    {
        // 방어력 적용 (방어력만큼 데미지 감소, 최소 1)
        int actualDamage = Mathf.Max(1, damage - defense);

        currentHP -= actualDamage;
        currentHP = Mathf.Max(0, currentHP);

        Debug.Log($"[{characterName}] 데미지 -{actualDamage} (HP: {currentHP}/{maxHP})");

        OnStatsChanged?.Invoke();

        if (currentHP <= 0)
        {
            Die();
        }

        return actualDamage;
    }

    /// <summary>
    /// 체력 회복
    /// </summary>
    public void Heal(int amount)
    {
        if (currentHP <= 0) return; // 사망 상태면 회복 불가

        currentHP = Mathf.Min(maxHP, currentHP + amount);
        Debug.Log($"[{characterName}] 체력 회복 +{amount} (HP: {currentHP}/{maxHP})");

        OnStatsChanged?.Invoke();
    }

    /// <summary>
    /// 골드 추가
    /// </summary>
    public void AddGold(int amount)
    {
        if (amount <= 0) return;

        gold += amount;
        Debug.Log($"[{characterName}] 골드 +{amount} (현재: {gold})");
        OnGoldChanged?.Invoke(gold);
        OnStatsChanged?.Invoke();
    }

    /// <summary>
    /// 골드 사용 (구매 등)
    /// </summary>
    public bool SpendGold(int amount)
    {
        if (amount <= 0) return false;

        if (gold < amount)
        {
            Debug.LogWarning($"[{characterName}] 골드 부족! (보유: {gold}, 필요: {amount})");
            return false;
        }

        gold -= amount;
        Debug.Log($"[{characterName}] 골드 -{amount} (현재: {gold})");
        OnGoldChanged?.Invoke(gold);
        OnStatsChanged?.Invoke();
        return true;
    }

    /// <summary>
    /// 골드 확인
    /// </summary>
    public bool HasGold(int amount)
    {
        return gold >= amount;
    }

    /// <summary>
    /// 사망 처리
    /// </summary>
    private void Die()
    {
        Debug.Log($"[{characterName}] 사망!");
        OnDeath?.Invoke();
    }

    /// <summary>
    /// 완전 회복
    /// </summary>
    public void FullRecover()
    {
        currentHP = maxHP;
        OnStatsChanged?.Invoke();
    }

    /// <summary>
    /// 스탯 복사 (몬스터 생성 등에 사용)
    /// </summary>
    public CharacterStats Clone()
    {
        CharacterStats clone = new CharacterStats();
        clone.characterName = this.characterName;
        clone.level = this.level;
        clone.currentExp = this.currentExp;
        clone.expToNextLevel = this.expToNextLevel;
        clone.gold = this.gold;
        clone.strength = this.strength;
        clone.dexterity = this.dexterity;
        clone.intelligence = this.intelligence;
        clone.maxHP = this.maxHP;
        clone.currentHP = this.currentHP;
        clone.attackPower = this.attackPower;
        clone.defense = this.defense;
        clone.criticalChance = this.criticalChance;
        clone.criticalDamage = this.criticalDamage;
        clone.evasionRate = this.evasionRate;
        clone.accuracy = this.accuracy;
        clone.rewardExp = this.rewardExp;
        clone.rewardGold = this.rewardGold;
        return clone;
    }

    /// <summary>
    /// 데이터 저장용 직렬화
    /// </summary>
    public CharacterStatsData ToSaveData()
    {
        return new CharacterStatsData
        {
            characterName = this.characterName,
            level = this.level,
            currentExp = this.currentExp,
            expToNextLevel = this.expToNextLevel,
            gold = this.gold,
            strength = this.strength,
            dexterity = this.dexterity,
            intelligence = this.intelligence,
            currentHP = this.currentHP,
            maxHP = this.maxHP
        };
    }

    /// <summary>
    /// 저장 데이터에서 복원
    /// </summary>
    public void LoadFromData(CharacterStatsData data)
    {
        characterName = data.characterName;
        level = data.level;
        currentExp = data.currentExp;
        expToNextLevel = data.expToNextLevel;
        gold = data.gold;
        strength = data.strength;
        dexterity = data.dexterity;
        intelligence = data.intelligence;
        currentHP = data.currentHP;

        RecalculateStats();

        // 저장된 최대 HP가 재계산된 값과 다르면 저장값 사용
        if (data.maxHP != maxHP)
        {
            maxHP = data.maxHP;
        }
    }
}

/// <summary>
/// 스탯 저장 데이터 구조체
/// </summary>
[System.Serializable]
public class CharacterStatsData
{
    public string characterName;
    public int level;
    public int currentExp;
    public int expToNextLevel;
    public int gold;
    public int strength;
    public int dexterity;
    public int intelligence;
    public int currentHP;
    public int maxHP;
}