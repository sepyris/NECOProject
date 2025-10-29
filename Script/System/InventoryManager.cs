using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// �÷��̾� �κ��丮 ���� �̱���
/// </summary>
public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [Header("�κ��丮 ����")]
    [SerializeField] private int maxSlots = 50;

    private List<InventoryItem> items = new List<InventoryItem>();

    // �̺�Ʈ
    public event Action<InventoryItem> OnItemAdded;
    public event Action<InventoryItem> OnItemRemoved;
    public event Action<InventoryItem> OnItemUsed;
    public event Action OnInventoryChanged;

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

    // ===== ������ �߰� =====

    /// <summary>
    /// ������ �߰�
    /// </summary>
    public bool AddItem(string itemID, int quantity = 1)
    {
        ItemData data = ItemDataManager.Instance?.GetItemData(itemID);
        if (data == null)
        {
            Debug.LogError($"[Inventory] �������� �ʴ� ������: {itemID}");
            return false;
        }

        int remainingQty = quantity;

        // ���� ������ �������� ��� ���� ���Կ� �߰�
        if (data.maxStack > 1)
        {
            foreach (var item in items)
            {
                if (item.itemID == itemID && item.CanStack(remainingQty))
                {
                    int added = item.AddQuantity(remainingQty);
                    remainingQty -= added;

                    if (remainingQty <= 0)
                    {
                        OnItemAdded?.Invoke(item);
                        OnInventoryChanged?.Invoke();
                        Debug.Log($"[Inventory] {data.itemName} x{quantity} �߰��� (���� ����)");
                        return true;
                    }
                }
            }
        }

        // �� ���Կ� �߰�
        while (remainingQty > 0)
        {
            if (items.Count >= maxSlots)
            {
                Debug.LogWarning("[Inventory] �κ��丮�� ���� á���ϴ�.");
                return false;
            }

            int stackSize = Mathf.Min(remainingQty, data.maxStack);
            InventoryItem newItem = new InventoryItem(itemID, stackSize);
            items.Add(newItem);
            remainingQty -= stackSize;

            OnItemAdded?.Invoke(newItem);
        }

        OnInventoryChanged?.Invoke();
        Debug.Log($"[Inventory] {data.itemName} x{quantity} �߰��� (�� ����)");
        return true;
    }

    // ===== ������ ���� =====

    /// <summary>
    /// ������ ����
    /// </summary>
    public bool RemoveItem(string itemID, int quantity = 1)
    {
        int totalQty = GetItemQuantity(itemID);
        if (totalQty < quantity)
        {
            Debug.LogWarning($"[Inventory] ������ ����: {itemID} (����: {totalQty}, �ʿ�: {quantity})");
            return false;
        }

        int remainingQty = quantity;

        for (int i = items.Count - 1; i >= 0 && remainingQty > 0; i--)
        {
            if (items[i].itemID == itemID)
            {
                int removeQty = Mathf.Min(remainingQty, items[i].quantity);
                items[i].RemoveQuantity(removeQty);
                remainingQty -= removeQty;

                if (items[i].quantity <= 0)
                {
                    InventoryItem removedItem = items[i];
                    items.RemoveAt(i);
                    OnItemRemoved?.Invoke(removedItem);
                }
            }
        }

        OnInventoryChanged?.Invoke();
        Debug.Log($"[Inventory] {itemID} x{quantity} ���ŵ�");
        return true;
    }

    // ===== ������ ��� =====

    /// <summary>
    /// ������ ���
    /// </summary>
    public bool UseItem(string itemID, CharacterStats stats = null)
    {
        InventoryItem item = items.FirstOrDefault(i => i.itemID == itemID);
        if (item == null)
        {
            Debug.LogWarning($"[Inventory] �������� ã�� �� ����: {itemID}");
            return false;
        }

        ItemData data = item.GetItemData();
        if (data == null) return false;

        // ������ ȿ�� ����
        bool used = ApplyItemEffect(data, stats);

        if (used)
        {
            OnItemUsed?.Invoke(item);

            // �Һ� �������� ���� ����
            if (data.itemType == ItemType.Consumable)
            {
                RemoveItem(itemID, 1);
            }

            Debug.Log($"[Inventory] {data.itemName} ����");
        }

        return used;
    }

    /// <summary>
    /// ������ ȿ�� ����
    /// </summary>
    private bool ApplyItemEffect(ItemData data, CharacterStats stats)
    {
        switch (data.itemType)
        {
            case ItemType.Consumable:
                if (stats != null)
                {
                    // ü�� ȸ��
                    if (data.healAmount > 0)
                    {
                        stats.Heal(data.healAmount);
                        Debug.Log($"[Inventory] HP +{data.healAmount}");
                    }

                    return true;
                }
                break;

            case ItemType.Equipment:
                // ��� ����/���� ����
                Debug.Log($"[Inventory] ��� ����: {data.itemName}");
                return true;

            default:
                Debug.LogWarning($"[Inventory] ����� �� ���� ������: {data.itemName}");
                return false;
        }

        return false;
    }

    // ===== ��ȸ =====

    /// <summary>
    /// ������ ���� ���� Ȯ��
    /// </summary>
    public int GetItemQuantity(string itemID)
    {
        return items.Where(i => i.itemID == itemID).Sum(i => i.quantity);
    }

    /// <summary>
    /// ������ ���� ���� Ȯ��
    /// </summary>
    public bool HasItem(string itemID, int quantity = 1)
    {
        return GetItemQuantity(itemID) >= quantity;
    }

    /// <summary>
    /// ��� ������ ��������
    /// </summary>
    public List<InventoryItem> GetAllItems()
    {
        return new List<InventoryItem>(items);
    }

    /// <summary>
    /// Ư�� Ÿ���� �����۸� ��������
    /// </summary>
    public List<InventoryItem> GetItemsByType(ItemType type)
    {
        return items.Where(i => i.GetItemData()?.itemType == type).ToList();
    }

    // ===== ����/�ε� =====

    /// <summary>
    /// �κ��丮 ������ ����
    /// </summary>
    public InventorySaveData ToSaveData()
    {
        return new InventorySaveData
        {
            items = items.Select(i => i.ToSaveData()).ToList()
        };
    }

    /// <summary>
    /// �κ��丮 ������ �ε�
    /// </summary>
    public void LoadFromData(InventorySaveData data)
    {
        items.Clear();

        if (data != null && data.items != null)
        {
            foreach (var itemData in data.items)
            {
                items.Add(InventoryItem.FromSaveData(itemData));
            }
        }

        OnInventoryChanged?.Invoke();
        Debug.Log($"[Inventory] ������ �ε� �Ϸ� ({items.Count}�� ������)");
    }

    /// <summary>
    /// �κ��丮 �ʱ�ȭ (�� ����)
    /// </summary>
    public void ClearInventory()
    {
        items.Clear();
        OnInventoryChanged?.Invoke();
        Debug.Log("[Inventory] �κ��丮 �ʱ�ȭ��");
    }

    // ===== ����� =====
    public void DebugPrintInventory()
    {
        Debug.Log($"===== �κ��丮 ({items.Count}/{maxSlots}) =====");
        foreach (var item in items)
        {
            ItemData data = item.GetItemData();
            string name = data != null ? data.itemName : item.itemID;
            Debug.Log($"- {name} x{item.quantity}");
        }
    }
}

/// <summary>
/// �κ��丮 ���� ������
/// </summary>
[Serializable]
public class InventorySaveData
{
    public List<InventoryItemSaveData> items = new List<InventoryItemSaveData>();
}