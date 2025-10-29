using System.Collections.Generic;

/// <summary>
/// 몬스터 타입
/// </summary>
public enum MonsterType
{
    Normal,    // 일반
    Elite,     // 정예
    Boss       // 보스
}

/// <summary>
/// 드롭 아이템 정보
/// </summary>
[System.Serializable]
public class DropItem
{
    public string itemID;      // 아이템 ID
    public float dropRate;     // 드롭 확률 (0~100)
    public int quantity;       // 드롭 개수
}

/// <summary>
/// 몬스터 데이터
/// </summary>
[System.Serializable]
public class MonsterData
{
    public string monsterID;           // ID
    public string monsterName;         // 이름
    public string description;         // 설명
    public int level;                  // 레벨
    public MonsterType monsterType;    // 몬스터 타입
    public bool isAggressive;          // 선공 여부
    public bool isRanged;              // 원거리 여부
    public float attackSpeed;          // 공격 속도
    public float moveSpeed;            // 이동 속도
    public float detectionRange;       // 감지 거리 (선공일 시)

    // 스탯
    public int strength;               // 힘
    public int dexterity;              // 민첩
    public int intelligence;           // 지력
    public int maxHealth;              // 최대 체력
    public int attackPower;            // 공격력
    public int defense;                // 방어력
    public float criticalRate;         // 크리티컬 확률
    public float criticalDamage;       // 크리티컬 데미지
    public float evasionRate;          // 회피 확률
    public float accuracy;             // 명중률

    // 드롭
    public int dropExp;                // 드롭 경험치
    public int dropGold;               // 드롭 골드
    public List<DropItem> dropItems = new List<DropItem>(); // 드롭 아이템 테이블
}