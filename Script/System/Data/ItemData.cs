using System;
using UnityEngine;

/// <summary>
/// 아이템 타입
/// </summary>
public enum ItemType
{
    Equipment,   // 장비
    Consumable,  // 소비 아이템
    Material,    // 재료
    QuestItem    // 퀘스트 아이템
}

/// <summary>
/// 장비 슬롯 타입
/// </summary>
public enum EquipmentSlot
{
    None,
    Weapon,
    Armor,
    Accessory
}

/// <summary>
/// 아이템 데이터 (순수 데이터 클래스)
/// </summary>
[Serializable]
public class ItemData
{
    // 공통 데이터
    public string itemID;           // 아이템 고유 ID
    public string itemName;         // 아이템 이름
    public ItemType itemType;       // 아이템 타입
    public string description;      // 설명
    public int maxStack;            // 최대 스택 수 (1 = 스택 불가)
    public int buyPrice;            // 구매 가격
    public int sellPrice;           // 판매 가격
    public string iconPath;         // 아이콘 경로

    // 소비 아이템 전용 데이터
    public int healAmount;          // 회복량

    // 장비 전용 데이터
    public EquipmentSlot equipSlot; // 장비 슬롯
    public int attackBonus;         // 공격력 보너스
    public int defenseBonus;        // 방어력 보너스
    public int strBonus;            // 힘 보너스
    public int dexBonus;            // 민첩 보너스
    public int intBonus;            // 지능 보너스
}