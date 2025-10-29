using System.Collections.Generic;

/// <summary>
/// ���� Ÿ��
/// </summary>
public enum MonsterType
{
    Normal,    // �Ϲ�
    Elite,     // ����
    Boss       // ����
}

/// <summary>
/// ��� ������ ����
/// </summary>
[System.Serializable]
public class DropItem
{
    public string itemID;      // ������ ID
    public float dropRate;     // ��� Ȯ�� (0~100)
    public int quantity;       // ��� ����
}

/// <summary>
/// ���� ������
/// </summary>
[System.Serializable]
public class MonsterData
{
    public string monsterID;           // ID
    public string monsterName;         // �̸�
    public string description;         // ����
    public int level;                  // ����
    public MonsterType monsterType;    // ���� Ÿ��
    public bool isAggressive;          // ���� ����
    public bool isRanged;              // ���Ÿ� ����
    public float attackSpeed;          // ���� �ӵ�
    public float moveSpeed;            // �̵� �ӵ�
    public float detectionRange;       // ���� �Ÿ� (������ ��)

    // ����
    public int strength;               // ��
    public int dexterity;              // ��ø
    public int intelligence;           // ����
    public int maxHealth;              // �ִ� ü��
    public int attackPower;            // ���ݷ�
    public int defense;                // ����
    public float criticalRate;         // ũ��Ƽ�� Ȯ��
    public float criticalDamage;       // ũ��Ƽ�� ������
    public float evasionRate;          // ȸ�� Ȯ��
    public float accuracy;             // ���߷�

    // ���
    public int dropExp;                // ��� ����ġ
    public int dropGold;               // ��� ���
    public List<DropItem> dropItems = new List<DropItem>(); // ��� ������ ���̺�
}