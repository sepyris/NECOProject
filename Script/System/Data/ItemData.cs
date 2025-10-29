using System;
using UnityEngine;

/// <summary>
/// ������ Ÿ��
/// </summary>
public enum ItemType
{
    Equipment,   // ���
    Consumable,  // �Һ� ������
    Material,    // ���
    QuestItem    // ����Ʈ ������
}

/// <summary>
/// ��� ���� Ÿ��
/// </summary>
public enum EquipmentSlot
{
    None,
    Weapon,
    Armor,
    Accessory
}

/// <summary>
/// ������ ������ (���� ������ Ŭ����)
/// </summary>
[Serializable]
public class ItemData
{
    // ���� ������
    public string itemID;           // ������ ���� ID
    public string itemName;         // ������ �̸�
    public ItemType itemType;       // ������ Ÿ��
    public string description;      // ����
    public int maxStack;            // �ִ� ���� �� (1 = ���� �Ұ�)
    public int buyPrice;            // ���� ����
    public int sellPrice;           // �Ǹ� ����
    public string iconPath;         // ������ ���

    // �Һ� ������ ���� ������
    public int healAmount;          // ȸ����

    // ��� ���� ������
    public EquipmentSlot equipSlot; // ��� ����
    public int attackBonus;         // ���ݷ� ���ʽ�
    public int defenseBonus;        // ���� ���ʽ�
    public int strBonus;            // �� ���ʽ�
    public int dexBonus;            // ��ø ���ʽ�
    public int intBonus;            // ���� ���ʽ�
}