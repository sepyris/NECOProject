using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 플레이어 인벤토리 관리 싱글톤
/// Collect 타입 퀘스트와 자동 연동
/// </summary>
public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [Header("인벤토리 설정")]
    [SerializeField] private int maxSlots = 50;

    private List<InventoryItem> items = new List<InventoryItem>();

    // 이벤트
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

    // ===== 아이템 추가 =====

    /// <summary>
    /// 아이템 추가
    /// </summary>
    public bool AddItem(string itemID, int quantity = 1)
    {
        ItemData data = ItemDataManager.Instance?.GetItemData(itemID);
        if (data == null)
        {
            Debug.LogError($"[Inventory] 존재하지 않는 아이템: {itemID}");
            return false;
        }

        int remainingQty = quantity;

        // 스택 가능한 아이템인 경우 기존 슬롯에 추가
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

                        // ⭐ 퀘스트 업데이트 (Collect & Gather) ⭐
                        UpdateQuestProgress(itemID, quantity);

                        Debug.Log($"[Inventory] {data.itemName} x{quantity} 추가됨 (기존 스택)");
                        return true;
                    }
                }
            }
        }

        // 새 슬롯에 추가
        while (remainingQty > 0)
        {
            if (items.Count >= maxSlots)
            {
                Debug.LogWarning("[Inventory] 인벤토리가 가득 찼습니다.");
                return false;
            }

            int stackSize = Mathf.Min(remainingQty, data.maxStack);
            InventoryItem newItem = new InventoryItem(itemID, stackSize);
            items.Add(newItem);
            remainingQty -= stackSize;

            OnItemAdded?.Invoke(newItem);
        }

        OnInventoryChanged?.Invoke();

        // ⭐ 퀘스트 업데이트 (Collect & Gather) ⭐
        UpdateQuestProgress(itemID, quantity);

        Debug.Log($"[Inventory] {data.itemName} x{quantity} 추가됨 (새 슬롯)");
        return true;
    }

    // ===== 아이템 제거 =====

    /// <summary>
    /// 아이템 제거
    /// </summary>
    public bool RemoveItem(string itemID, int quantity = 1)
    {
        int totalQty = GetItemQuantity(itemID);
        if (totalQty < quantity)
        {
            Debug.LogWarning($"[Inventory] 아이템 부족: {itemID} (보유: {totalQty}, 필요: {quantity})");
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

        // ⭐ Collect 타입 퀘스트 갱신 (아이템이 줄어들었으므로) ⭐
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.RefreshAllCollectObjectives();
        }

        Debug.Log($"[Inventory] {itemID} x{quantity} 제거됨");
        return true;
    }

    /// <summary>
    /// 퀘스트 진행 상황 업데이트
    /// </summary>
    private void UpdateQuestProgress(string itemID, int quantity)
    {
        if (QuestManager.Instance != null)
        {
            // Collect: 현재 인벤토리 개수로 업데이트
            // Gather: 획득한 개수만큼 증가
            QuestManager.Instance.UpdateItemProgress(itemID, quantity);
        }
    }

    // ===== 아이템 사용 =====

    /// <summary>
    /// 아이템 사용
    /// </summary>
    public bool UseItem(string itemID, CharacterStats stats = null)
    {
        InventoryItem item = items.FirstOrDefault(i => i.itemID == itemID);
        if (item == null)
        {
            Debug.LogWarning($"[Inventory] 아이템을 찾을 수 없음: {itemID}");
            return false;
        }

        ItemData data = item.GetItemData();
        if (data == null) return false;

        // 아이템 효과 적용
        bool used = ApplyItemEffect(data, stats);

        if (used)
        {
            OnItemUsed?.Invoke(item);

            // 소비 아이템은 개수 감소
            if (data.itemType == ItemType.Consumable)
            {
                RemoveItem(itemID, 1);
            }

            Debug.Log($"[Inventory] {data.itemName} 사용됨");
        }

        return used;
    }

    /// <summary>
    /// 아이템 효과 적용
    /// </summary>
    private bool ApplyItemEffect(ItemData data, CharacterStats stats)
    {
        switch (data.itemType)
        {
            case ItemType.Consumable:
                if (stats != null)
                {
                    // 체력 회복
                    if (data.healAmount > 0)
                    {
                        stats.Heal(data.healAmount);
                        Debug.Log($"[Inventory] HP +{data.healAmount}");
                    }

                    return true;
                }
                break;

            case ItemType.Equipment:
                // 장비 장착/해제 로직
                Debug.Log($"[Inventory] 장비 장착: {data.itemName}");
                return true;

            default:
                Debug.LogWarning($"[Inventory] 사용할 수 없는 아이템: {data.itemName}");
                return false;
        }

        return false;
    }

    // ===== 조회 =====

    /// <summary>
    /// 아이템 소유 개수 확인
    /// </summary>
    public int GetItemQuantity(string itemID)
    {
        return items.Where(i => i.itemID == itemID).Sum(i => i.quantity);
    }

    /// <summary>
    /// 아이템 소유 여부 확인
    /// </summary>
    public bool HasItem(string itemID, int quantity = 1)
    {
        return GetItemQuantity(itemID) >= quantity;
    }

    /// <summary>
    /// 모든 아이템 가져오기
    /// </summary>
    public List<InventoryItem> GetAllItems()
    {
        return new List<InventoryItem>(items);
    }

    /// <summary>
    /// 특정 타입의 아이템만 가져오기
    /// </summary>
    public List<InventoryItem> GetItemsByType(ItemType type)
    {
        return items.Where(i => i.GetItemData()?.itemType == type).ToList();
    }

    // ===== 저장/로드 =====

    /// <summary>
    /// 인벤토리 데이터 저장
    /// </summary>
    public InventorySaveData ToSaveData()
    {
        return new InventorySaveData
        {
            items = items.Select(i => i.ToSaveData()).ToList()
        };
    }

    /// <summary>
    /// 인벤토리 데이터 로드
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

        // ⭐ 로드 후 Collect 타입 퀘스트 갱신 ⭐
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.RefreshAllCollectObjectives();
        }

        Debug.Log($"[Inventory] 데이터 로드 완료 ({items.Count}개 아이템)");
    }

    /// <summary>
    /// 인벤토리 초기화 (새 게임)
    /// </summary>
    public void ClearInventory()
    {
        items.Clear();
        OnInventoryChanged?.Invoke();
        Debug.Log("[Inventory] 인벤토리 초기화됨");
    }

    // ===== 디버그 =====
    public void DebugPrintInventory()
    {
        Debug.Log($"===== 인벤토리 ({items.Count}/{maxSlots}) =====");
        foreach (var item in items)
        {
            ItemData data = item.GetItemData();
            string name = data != null ? data.itemName : item.itemID;
            Debug.Log($"- {name} x{item.quantity}");
        }
    }
}

/// <summary>
/// 인벤토리 저장 데이터
/// </summary>
[Serializable]
public class InventorySaveData
{
    public List<InventoryItemSaveData> items = new List<InventoryItemSaveData>();
}