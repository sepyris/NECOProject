using System;
using UnityEngine;

/// <summary>
/// �÷��̾ ������ ������ ������ (�κ��丮 ����)
/// </summary>
[Serializable]
public class InventoryItem
{
    public string itemID;       // ������ ID (ItemData ����)
    public int quantity;        // ���� ����
    public bool isEquipped;     // ���� ���� (��� �����۸� �ش�)

    [NonSerialized]
    private ItemData cachedData; // ĳ�õ� ������ ������

    // ���� �Ӽ�: ������ �̸�
    public string itemName
    {
        get
        {
            ItemData data = GetItemData();
            return data != null ? data.itemName : itemID;
        }
    }

    public InventoryItem(string id, int qty = 1)
    {
        itemID = id;
        quantity = qty;
        isEquipped = false;
    }
    /// <summary>
    /// ������ ������ �������� (ĳ��)
    /// </summary>
    public ItemData GetItemData()
    {
        if (cachedData == null)
        {
            if (ItemDataManager.Instance != null)
            {
                cachedData = ItemDataManager.Instance.GetItemData(itemID);
            }
        }
        return cachedData;
    }

    /// <summary>
    /// ������ �߰� ���� ���� (����)
    /// </summary>
    public bool CanStack(int amount = 1)
    {
        ItemData data = GetItemData();
        if (data == null) return false;

        return quantity + amount <= data.maxStack;
    }

    /// <summary>
    /// ������ ���� �߰�
    /// </summary>
    public int AddQuantity(int amount)
    {
        ItemData data = GetItemData();
        if (data == null) return 0;

        int maxAdd = data.maxStack - quantity;
        int actualAdd = Mathf.Min(amount, maxAdd);

        quantity += actualAdd;
        return actualAdd; // ������ �߰��� ���� ��ȯ
    }

    /// <summary>
    /// ������ ���� ����
    /// </summary>
    public bool RemoveQuantity(int amount)
    {
        if (quantity >= amount)
        {
            quantity -= amount;
            return true;
        }
        return false;
    }

    /// <summary>
    /// ����� �����ͷ� ��ȯ
    /// </summary>
    public InventoryItemSaveData ToSaveData()
    {
        return new InventoryItemSaveData
        {
            itemID = this.itemID,
            quantity = this.quantity,
            isEquipped = this.isEquipped
        };
    }

    /// <summary>
    /// ���� �����Ϳ��� ����
    /// </summary>
    public static InventoryItem FromSaveData(InventoryItemSaveData data)
    {
        return new InventoryItem(data.itemID, data.quantity)
        {
            isEquipped = data.isEquipped
        };
    }
}

/// <summary>
/// �κ��丮 ������ ���� ������
/// </summary>
[Serializable]
public class InventoryItemSaveData
{
    public string itemID;
    public int quantity;
    public bool isEquipped;
}