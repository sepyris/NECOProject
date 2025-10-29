using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI.Table;

/// <summary>
/// CSV 파일에서 아이템 데이터를 로드하고 관리하는 싱글톤 매니저
/// </summary>
public class ItemDataManager : MonoBehaviour
{
    public static ItemDataManager Instance { get; private set; }

    [Header("CSV 파일")]
    public TextAsset csvFile; // CSV 파일을 Inspector에서 할당

    private Dictionary<string, ItemData> itemDatabase = new Dictionary<string, ItemData>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (csvFile != null)
            {
                ParseCSV(csvFile.text);
            }
            else
            {
                Debug.LogWarning("[ItemDataManager] CSV 파일이 할당되지 않았습니다. 기본 아이템 생성.");
                CreateDefaultItems();
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// CSV 내용 파싱
    /// </summary>
    void ParseCSV(string csvText)
    {
        itemDatabase.Clear();

        var lines = CSVUtility.GetLinesFromCSV(csvText);
        bool skipHeader = true;

        foreach (var raw in lines)
        {
            if (skipHeader) { skipHeader = false; continue; }
            if (string.IsNullOrWhiteSpace(raw)) continue;

            string trimmed = raw.TrimStart();
            if (trimmed.StartsWith("#")) continue;

            var parts = CSVUtility.SplitCSVLine(raw);
            // CSV 구조: itemID, itemName, itemType, description, maxStack, buyPrice, sellPrice, iconPath
            if (parts.Count < 8) continue;

            // 모든 파트는 SplitCSVLine에서 따옴표를 고려하여 분리되었으므로,
            // 이제 각 파트의 앞뒤 공백만 제거합니다.
            string itemID = parts[0].Trim();
            string itemName = parts[1].Trim();
            string itemTypeStr = parts[2].Trim();
            string description = parts[3].Trim();
            string maxStackStr = parts[4].Trim();
            string buyPriceStr = parts[5].Trim();
            string sellPriceStr = parts[6].Trim();
            string iconPath = parts[7].Trim();

            ItemData item = new ItemData
            {
                itemID = itemID,
                itemName = itemName,
                itemType = ParseItemType(itemTypeStr),
                description = description,
                // int.Parse 대신 안전한 TryParse를 사용하거나, 값이 비어있을 경우 0으로 처리하는 것이 좋습니다.
                // 여기서는 기존 코드를 따라 int.Parse를 유지하되, TryParse를 권장합니다.
                maxStack = int.Parse(maxStackStr),
                buyPrice = int.Parse(buyPriceStr),
                sellPrice = int.Parse(sellPriceStr),
                iconPath = iconPath
            };

            // 소비 아이템 데이터 (8번째 인덱스: healAmount)
            if (parts.Count > 8)
            {
                int.TryParse(parts[8].Trim(), out item.healAmount);
            }

            // 장비 데이터 (9번째 이후)
            if (parts.Count > 9 && !string.IsNullOrEmpty(parts[9].Trim()))
            {
                item.equipSlot = ParseEquipSlot(parts[9].Trim());
            }
            if (parts.Count > 10)
            {
                int.TryParse(parts[10].Trim(), out item.attackBonus);
            }
            if (parts.Count > 11)
            {
                int.TryParse(parts[11].Trim(), out item.defenseBonus);
            }
            if (parts.Count > 12)
            {
                int.TryParse(parts[12].Trim(), out item.strBonus);
            }
            if (parts.Count > 13)
            {
                int.TryParse(parts[13].Trim(), out item.dexBonus);
            }
            if (parts.Count > 14)
            {
                int.TryParse(parts[14].Trim(), out item.intBonus);
            }

            // 데이터베이스에 추가
            if (!itemDatabase.ContainsKey(item.itemID))
            {
                itemDatabase.Add(item.itemID, item);
            }
            else
            {
                Debug.LogWarning($"[ItemDataManager] 중복 아이템 ID: {item.itemID}");
            }
        }

        Debug.Log($"[ItemDataManager] CSV에서 {itemDatabase.Count}개의 아이템 로드 완료");
    }

    /// <summary>
    /// 아이템 타입 파싱
    /// </summary>
    private ItemType ParseItemType(string typeStr)
    {
        switch (typeStr.ToLower())
        {
            case "equipment": return ItemType.Equipment;
            case "consumable": return ItemType.Consumable;
            case "material": return ItemType.Material;
            case "questitem": return ItemType.QuestItem;
            default:
                Debug.LogWarning($"[ItemDataManager] 알 수 없는 아이템 타입: {typeStr}");
                return ItemType.Material;
        }
    }

    /// <summary>
    /// 장비 슬롯 파싱
    /// </summary>
    private EquipmentSlot ParseEquipSlot(string slotStr)
    {
        switch (slotStr.ToLower())
        {
            case "weapon": return EquipmentSlot.Weapon;
            case "armor": return EquipmentSlot.Armor;
            case "accessory": return EquipmentSlot.Accessory;
            default: return EquipmentSlot.None;
        }
    }

    // ==========================================
    // 조회 메서드
    // ==========================================

    /// <summary>
    /// 아이템 ID로 데이터 가져오기
    /// </summary>
    public ItemData GetItemData(string itemID)
    {
        if (itemDatabase.TryGetValue(itemID, out ItemData data))
        {
            return data;
        }

        Debug.LogWarning($"[ItemDataManager] 아이템을 찾을 수 없음: {itemID}");
        return null;
    }

    /// <summary>
    /// 모든 아이템 데이터 가져오기
    /// </summary>
    public Dictionary<string, ItemData> GetAllItems()
    {
        return itemDatabase;
    }

    /// <summary>
    /// 특정 타입의 아이템만 가져오기
    /// </summary>
    public List<ItemData> GetItemsByType(ItemType type)
    {
        List<ItemData> result = new List<ItemData>();

        foreach (var item in itemDatabase.Values)
        {
            if (item.itemType == type)
            {
                result.Add(item);
            }
        }

        return result;
    }

    // ==========================================
    // 테스트용 기본 아이템 (CSV 없을 때)
    // ==========================================

    /// <summary>
    /// 테스트용 기본 아이템 생성
    /// </summary>
    private void CreateDefaultItems()
    {
        Debug.Log("[ItemDataManager] 기본 아이템 생성 (테스트용)");

        // 소비 아이템
        AddDefaultItem(new ItemData
        {
            itemID = "potion_health",
            itemName = "체력 포션",
            itemType = ItemType.Consumable,
            description = "HP를 50 회복합니다.",
            maxStack = 99,
            buyPrice = 50,
            sellPrice = 25,
            iconPath = "Icons/potion_health",
            healAmount = 50
        });

        AddDefaultItem(new ItemData
        {
            itemID = "potion_health_large",
            itemName = "대형 체력 포션",
            itemType = ItemType.Consumable,
            description = "HP를 150 회복합니다.",
            maxStack = 50,
            buyPrice = 150,
            sellPrice = 75,
            iconPath = "Icons/potion_health_large",
            healAmount = 150
        });

        // 무기
        AddDefaultItem(new ItemData
        {
            itemID = "sword_iron",
            itemName = "철 검",
            itemType = ItemType.Equipment,
            description = "기본적인 철제 검입니다.",
            maxStack = 1,
            buyPrice = 500,
            sellPrice = 250,
            iconPath = "Icons/sword_iron",
            equipSlot = EquipmentSlot.Weapon,
            attackBonus = 10,
            strBonus = 2
        });

        AddDefaultItem(new ItemData
        {
            itemID = "sword_steel",
            itemName = "강철 검",
            itemType = ItemType.Equipment,
            description = "날카로운 강철 검입니다.",
            maxStack = 1,
            buyPrice = 1500,
            sellPrice = 750,
            iconPath = "Icons/sword_steel",
            equipSlot = EquipmentSlot.Weapon,
            attackBonus = 25,
            strBonus = 5
        });

        // 방어구
        AddDefaultItem(new ItemData
        {
            itemID = "armor_leather",
            itemName = "가죽 갑옷",
            itemType = ItemType.Equipment,
            description = "기본적인 가죽 방어구입니다.",
            maxStack = 1,
            buyPrice = 400,
            sellPrice = 200,
            iconPath = "Icons/armor_leather",
            equipSlot = EquipmentSlot.Armor,
            defenseBonus = 8,
            dexBonus = 1
        });

        AddDefaultItem(new ItemData
        {
            itemID = "armor_iron",
            itemName = "철 갑옷",
            itemType = ItemType.Equipment,
            description = "튼튼한 철제 갑옷입니다.",
            maxStack = 1,
            buyPrice = 1200,
            sellPrice = 600,
            iconPath = "Icons/armor_iron",
            equipSlot = EquipmentSlot.Armor,
            defenseBonus = 20,
            strBonus = 3
        });

        // 장신구
        AddDefaultItem(new ItemData
        {
            itemID = "ring_power",
            itemName = "힘의 반지",
            itemType = ItemType.Equipment,
            description = "공격력을 높여주는 마법 반지입니다.",
            maxStack = 1,
            buyPrice = 2000,
            sellPrice = 1000,
            iconPath = "Icons/ring_power",
            equipSlot = EquipmentSlot.Accessory,
            attackBonus = 15,
            strBonus = 3,
            dexBonus = 2
        });

        // 재료
        AddDefaultItem(new ItemData
        {
            itemID = "herb_basic",
            itemName = "약초",
            itemType = ItemType.Material,
            description = "포션 제작에 사용되는 기본 약초입니다.",
            maxStack = 99,
            buyPrice = 10,
            sellPrice = 5,
            iconPath = "Icons/herb"
        });

        AddDefaultItem(new ItemData
        {
            itemID = "ore_iron",
            itemName = "철광석",
            itemType = ItemType.Material,
            description = "철을 제련하는 데 사용되는 광석입니다.",
            maxStack = 99,
            buyPrice = 20,
            sellPrice = 10,
            iconPath = "Icons/ore_iron"
        });

        // 퀘스트 아이템
        AddDefaultItem(new ItemData
        {
            itemID = "quest_letter",
            itemName = "의뢰서",
            itemType = ItemType.QuestItem,
            description = "마을 촌장의 의뢰서입니다.",
            maxStack = 1,
            buyPrice = 0,
            sellPrice = 0,
            iconPath = "Icons/letter"
        });
    }

    private void AddDefaultItem(ItemData item)
    {
        if (!itemDatabase.ContainsKey(item.itemID))
        {
            itemDatabase.Add(item.itemID, item);
        }
    }
}