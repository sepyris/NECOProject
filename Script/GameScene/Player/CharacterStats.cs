using System;
using UnityEngine;

/// <summary>
/// ĳ���� ���� �ý��� (�÷��̾�, ����, NPC ����)
/// </summary>
[System.Serializable]
public class CharacterStats
{
    [Header("�⺻ ����")]
    public string characterName = "Unknown";
    public int level = 1;
    public int currentExp = 0;
    public int expToNextLevel = 100;
    public int gold = 0;  // ���� ��� (�÷��̾��)

    [Header("�⺻ ����")]
    public int strength = 10;      // �� (���ݷ� ����)
    public int dexterity = 10;     // ��ø (���߷�, ȸ���� ����)
    public int intelligence = 10;  // ���� (���� ���ݷ� ����)

    [Header("���� ����")]
    public int maxHP = 100;
    public int currentHP = 100;
    public int attackPower = 10;
    public int defense = 5;
    public float criticalChance = 5f;      // % ����
    public float criticalDamage = 150f;    // % ���� (�⺻ 150%)
    public float evasionRate = 5f;         // % ����
    public float accuracy = 95f;           // % ����

    [Header("���� ���� (���Ϳ�)")]
    public int rewardExp = 0;     // óġ �� �ִ� ����ġ
    public int rewardGold = 0;    // óġ �� �ִ� ���

    // ���� ���� �̺�Ʈ
    public event Action OnStatsChanged;
    public event Action OnLevelUp;
    public event Action OnDeath;
    public event Action<int> OnExpGained;  // ȹ���� ����ġ��
    public event Action<int> OnGoldChanged; // ��� ����

    private bool is_monster_stat = false;


    /// <summary>
    /// ���� �ʱ�ȭ (�⺻�� ����)
    /// </summary>
    public void Initialize(string name = "Character", int startLevel = 1,bool is_monster = false)
    {
        characterName = name;
        level = startLevel;
        currentExp = 0;
        is_monster_stat = is_monster;

        // ������ ���� �⺻ ���� ����
        strength = 10 + (level - 1) * 2;
        dexterity = 10 + (level - 1) * 2;
        intelligence = 10 + (level - 1) * 2;

        RecalculateStats();
        currentHP = maxHP;
    }

    /// <summary>
    /// �⺻ ������ ������� ���� ���� ����
    /// </summary>
    public void RecalculateStats()
    {
        //�����ϰ�� �⺻ ���Ȼ��
        if(is_monster_stat)
        {
            return;
        }
        // �� -> ���ݷ� (�� 1�� ���ݷ� 2)
        attackPower = 10 + (strength * 2);

        // ���� -> �ִ� HP (������ 20 HP)
        maxHP = 100 + (level - 1) * 20;

        // �� -> ���� (�� 2�� ���� 1)
        defense = 5 + (strength / 2);

        // ��ø -> ���߷� (��ø 2�� 1%)
        accuracy = 95f + (dexterity * 0.5f);

        // ��ø -> ȸ���� (��ø 3�� 1%)
        evasionRate = 5f + (dexterity / 3f);

        // ��ø -> ũ��Ƽ�� Ȯ�� (��ø 4�� 1%)
        criticalChance = 5f + (dexterity / 4f);

        // ���� -> ũ��Ƽ�� ������ (���� 5�� 5%)
        criticalDamage = 150f + (intelligence);

        OnStatsChanged?.Invoke();
    }

    /// <summary>
    /// ����ġ ȹ��
    /// </summary>
    public void GainExperience(int amount)
    {
        if (amount <= 0) return;

        currentExp += amount;
        OnExpGained?.Invoke(amount);

        Debug.Log($"[{characterName}] ����ġ +{amount} (����: {currentExp}/{expToNextLevel})");

        // ������ üũ
        while (currentExp >= expToNextLevel)
        {
            LevelUp();
        }
    }

    /// <summary>
    /// ������ ó��
    /// </summary>
    private void LevelUp()
    {
        currentExp -= expToNextLevel;
        level++;

        // ���� ���� �ʿ� ����ġ ��� (���� ����)
        expToNextLevel = Mathf.RoundToInt(100 * Mathf.Pow(1.2f, level - 1));

        // ���� ����
        strength += 2;
        dexterity += 2;
        intelligence += 2;

        // ���� ���� ����
        int oldMaxHP = maxHP;
        RecalculateStats();

        // HP ȸ�� (������ �ִ� HP��ŭ)
        int hpIncrease = maxHP - oldMaxHP;
        currentHP += hpIncrease;

        Debug.Log($"[{characterName}] ���� ��! Lv.{level} (��:{strength}, ��:{dexterity}, ��:{intelligence})");

        OnLevelUp?.Invoke();
    }

    /// <summary>
    /// ������ �ޱ�
    /// </summary>
    public int TakeDamage(int damage)
    {
        // ���� ���� (���¸�ŭ ������ ����, �ּ� 1)
        int actualDamage = Mathf.Max(1, damage - defense);

        currentHP -= actualDamage;
        currentHP = Mathf.Max(0, currentHP);

        Debug.Log($"[{characterName}] ������ -{actualDamage} (HP: {currentHP}/{maxHP})");

        OnStatsChanged?.Invoke();

        if (currentHP <= 0)
        {
            Die();
        }

        return actualDamage;
    }

    /// <summary>
    /// ü�� ȸ��
    /// </summary>
    public void Heal(int amount)
    {
        if (currentHP <= 0) return; // ��� ���¸� ȸ�� �Ұ�

        currentHP = Mathf.Min(maxHP, currentHP + amount);
        Debug.Log($"[{characterName}] ü�� ȸ�� +{amount} (HP: {currentHP}/{maxHP})");

        OnStatsChanged?.Invoke();
    }

    /// <summary>
    /// ��� �߰�
    /// </summary>
    public void AddGold(int amount)
    {
        if (amount <= 0) return;

        gold += amount;
        Debug.Log($"[{characterName}] ��� +{amount} (����: {gold})");
        OnGoldChanged?.Invoke(gold);
        OnStatsChanged?.Invoke();
    }

    /// <summary>
    /// ��� ��� (���� ��)
    /// </summary>
    public bool SpendGold(int amount)
    {
        if (amount <= 0) return false;

        if (gold < amount)
        {
            Debug.LogWarning($"[{characterName}] ��� ����! (����: {gold}, �ʿ�: {amount})");
            return false;
        }

        gold -= amount;
        Debug.Log($"[{characterName}] ��� -{amount} (����: {gold})");
        OnGoldChanged?.Invoke(gold);
        OnStatsChanged?.Invoke();
        return true;
    }

    /// <summary>
    /// ��� Ȯ��
    /// </summary>
    public bool HasGold(int amount)
    {
        return gold >= amount;
    }

    /// <summary>
    /// ��� ó��
    /// </summary>
    private void Die()
    {
        Debug.Log($"[{characterName}] ���!");
        OnDeath?.Invoke();
    }

    /// <summary>
    /// ���� ȸ��
    /// </summary>
    public void FullRecover()
    {
        currentHP = maxHP;
        OnStatsChanged?.Invoke();
    }

    /// <summary>
    /// ���� ���� (���� ���� � ���)
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
    /// ������ ����� ����ȭ
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
    /// ���� �����Ϳ��� ����
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

        // ����� �ִ� HP�� ����� ���� �ٸ��� ���尪 ���
        if (data.maxHP != maxHP)
        {
            maxHP = data.maxHP;
        }
    }
}

/// <summary>
/// ���� ���� ������ ����ü
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