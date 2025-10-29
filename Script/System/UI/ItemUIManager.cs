using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// I키로 열리는 독립적인 인벤토리 UI 관리
/// </summary>
public class ItemUIManager : MonoBehaviour
{
    public static ItemUIManager Instance { get; private set; }

    [Header("메인 패널")]
    public GameObject itemUIPanel;
    public Button closeButton;

    [Header("탭 버튼")]
    public Button equipmentTabButton;
    public Button usingitemTabButton;
    public Button etcitemTabButton;
    public Button questitemTabButton;

    [Header("아이템 리스트")]
    public Transform itemListContainer;
    public GameObject itemListPrefab;

    [Header("아이템 상세 정보")]
    public GameObject itemDetailPanel;
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI itemDescriptionText;
    public TextMeshProUGUI itemStatsText;
    public Button useButton;
    public Button discardButton;

    // 처음 열릴 때만 초기화
    private bool isInitialized = false;

    private enum ItemTab
    {
        Equipment,
        Consumable,
        Material,
        QuestItem
    }

    private ItemTab currentTab = ItemTab.Equipment;
    private InventoryItem selectedItem;
    private bool isOpen = false;

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
            return;
        }

        if (itemUIPanel != null)
            itemUIPanel.SetActive(false);

        if (itemDetailPanel != null)
            itemDetailPanel.SetActive(false);

        SetupButtons();
        SubscribeToEvents();
    }

    void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    void Update()
    {
        // I키로 인벤토리 창 토글
        if (Input.GetKeyDown(KeyCode.I))
        {
            // 대화 중이면 인벤토리 창 열지 않음
            if (DialogueUIManager.Instance != null && DialogueUIManager.Instance.IsDialogueOpen)
                return;

            if (isOpen)
                CloseItemUI();
            else
                OpenItemUI();
        }

        // ESC키로 닫기
        if (isOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseItemUI();
        }
    }

    private void SetupButtons()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(CloseItemUI);

        if (equipmentTabButton != null)
            equipmentTabButton.onClick.AddListener(() => SwitchTab(ItemTab.Equipment));

        if (usingitemTabButton != null)
            usingitemTabButton.onClick.AddListener(() => SwitchTab(ItemTab.Consumable));

        if (etcitemTabButton != null)
            etcitemTabButton.onClick.AddListener(() => SwitchTab(ItemTab.Material));

        if (questitemTabButton != null)
            questitemTabButton.onClick.AddListener(() => SwitchTab(ItemTab.QuestItem));

        if (useButton != null)
            useButton.onClick.AddListener(OnUseButtonClicked);

        if (discardButton != null)
            discardButton.onClick.AddListener(OnDiscardButtonClicked);
    }

    private void SubscribeToEvents()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnInventoryChanged += OnInventoryChanged;
        }
    }

    private void UnsubscribeFromEvents()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnInventoryChanged -= OnInventoryChanged;
        }
    }
    // ==========================================
    // 인벤토리 UI 열기/닫기
    // ==========================================
    public void OpenItemUI()
    {
        if (isOpen) return;

        isOpen = true;
        itemUIPanel.SetActive(true);

        if (!isInitialized)
        {
            // 기본 탭으로 초기화
            SwitchTab(ItemTab.Equipment);
            isInitialized = true;
        }

        UpdateTabButtons();
        RefreshItemList();
        Debug.Log("[ItemUI] 인벤토리 창 열림");
    }

    public void CloseItemUI()
    {
        if (!isOpen) return;

        isOpen = false;
        itemUIPanel.SetActive(false);

        if (itemDetailPanel != null)
            itemDetailPanel.SetActive(false);

        Debug.Log("[ItemUI] 인벤토리 창 닫힘");
    }

    // ==========================================
    // 탭 전환
    // ==========================================
    private void SwitchTab(ItemTab tab)
    {
        currentTab = tab;
        UpdateTabButtons();
        RefreshItemList();

        // 탭 전환 시 상세정보 숨김
        if (itemDetailPanel != null)
            itemDetailPanel.SetActive(false);
    }

    private void UpdateTabButtons()
    {
        UpdateTabButtonColor(equipmentTabButton, currentTab == ItemTab.Equipment, new Color(1f, 0.8f, 0.5f));
        UpdateTabButtonColor(usingitemTabButton, currentTab == ItemTab.Consumable, new Color(0.5f, 1f, 0.5f));
        UpdateTabButtonColor(etcitemTabButton, currentTab == ItemTab.Material, new Color(0.7f, 0.7f, 1f));
        UpdateTabButtonColor(questitemTabButton, currentTab == ItemTab.QuestItem, new Color(1f, 0.5f, 0.8f));
    }

    private void UpdateTabButtonColor(Button button, bool isActive, Color activeColor)
    {
        if (button == null) return;

        var colors = button.colors;
        colors.normalColor = isActive ? activeColor : Color.white;
        button.colors = colors;
    }

    // ==========================================
    // 아이템 리스트 갱신
    // ==========================================
    private void RefreshItemList()
    {
        // 기존 리스트 아이템 삭제
        foreach (Transform child in itemListContainer)
            Destroy(child.gameObject);

        // 현재 탭에 맞는 아이템 가져오기
        List<InventoryItem> items = GetItemsForCurrentTab();

        // 아이템 리스트 아이템 생성
        foreach (var item in items)
        {
            CreateItemListItem(item);
        }

        Debug.Log($"[ItemUI] {currentTab} 탭: {items.Count}개 아이템 표시");
    }

    private List<InventoryItem> GetItemsForCurrentTab()
    {
        if (InventoryManager.Instance == null)
            return new List<InventoryItem>();

        switch (currentTab)
        {
            case ItemTab.Equipment:
                return InventoryManager.Instance.GetItemsByType(ItemType.Equipment);

            case ItemTab.Consumable:
                return InventoryManager.Instance.GetItemsByType(ItemType.Consumable);

            case ItemTab.Material:
                return InventoryManager.Instance.GetItemsByType(ItemType.Material);

            case ItemTab.QuestItem:
                return InventoryManager.Instance.GetItemsByType(ItemType.QuestItem);

            default:
                return new List<InventoryItem>();
        }
    }

    private void CreateItemListItem(InventoryItem item)
    {
        GameObject itemObj = Instantiate(itemListPrefab, itemListContainer);
        Button itemButton = itemObj.GetComponent<Button>();
        TextMeshProUGUI itemText = itemObj.GetComponentInChildren<TextMeshProUGUI>();
        // 1. 호버 핸들러 컴포넌트를 가져오거나 추가합니다.
        ItemDetailUiManager hoverHandler = itemObj.GetComponent<ItemDetailUiManager>();
        if (hoverHandler == null)
        {
            hoverHandler = itemObj.AddComponent<ItemDetailUiManager>();
        }

        // 2. 호버 핸들러를 초기화합니다.
        hoverHandler.Initialize(item, this);

        ItemData data = item.GetItemData();
        if (data == null) return;

        if (itemText != null)
        {
            string displayText = $"[{GetItemTypeIcon(data.itemType)}] {data.itemName}";

            if (item.quantity > 1)
                displayText += $" x{item.quantity}";

            if (item.isEquipped)
                displayText += " (E)";

            itemText.text = displayText;
        }
    }

    private string GetItemTypeIcon(ItemType type)
    {
        switch (type)
        {
            case ItemType.Equipment:
                return "⚔";
            case ItemType.Consumable:
                return "🍵";
            case ItemType.Material:
                return "📦";
            case ItemType.QuestItem:
                return "📜";
            default:
                return "?";
        }
    }

    public void ShowItemDetailOnHover(InventoryItem item, Transform buttonTransform)
    {
        selectedItem = item;
        ShowItemDetail(item, buttonTransform);
    }
    public void HideDetailPanelOnHoverExit()
    {
        if (itemDetailPanel == null) return;
        itemDetailPanel.SetActive(false);

    }

    private void ShowItemDetail(InventoryItem item, Transform buttonTransform = null)
    {
        if (itemDetailPanel == null) return;

        ItemData data = item.GetItemData();
        if (data == null) return;

        itemDetailPanel.SetActive(true);

        // 상세 패널 위치 조정 (호버 시)
        if (buttonTransform != null)
        {
            Vector3 newPosition = buttonTransform.position;
            RectTransform detailRect = itemDetailPanel.GetComponent<RectTransform>();
            if (detailRect != null)
            {
                newPosition.x += 10f;

                RectTransform buttonRect = buttonTransform.GetComponent<RectTransform>();
                if (buttonRect != null)
                {
                    float buttonRightEdgeX = buttonRect.position.x + buttonRect.rect.width * (1 - buttonRect.pivot.x);
                    float detailPanelPivotCompensation = detailRect.rect.width * detailRect.pivot.x;
                    newPosition.x = buttonRightEdgeX + 10f + detailPanelPivotCompensation;
                }
                newPosition.y -= 120f;
                detailRect.position = newPosition;
            }
        }

        // 이름
        if (itemNameText != null)
            itemNameText.text = data.itemName;

        // 설명
        if (itemDescriptionText != null)
            itemDescriptionText.text = data.description;

        // 스탯 정보
        if (itemStatsText != null)
        {
            string statsText = $"타입: {GetItemTypeName(data.itemType)}\n";
            statsText += $"개수: {item.quantity}\n";

            if (data.itemType == ItemType.Equipment)
            {
                statsText += $"슬롯: {GetEquipSlotName(data.equipSlot)}\n";
                statsText += "\n[보너스 스탯]\n";

                if (data.attackBonus > 0)
                    statsText += $"공격력: +{data.attackBonus}\n";
                if (data.defenseBonus > 0)
                    statsText += $"방어력: +{data.defenseBonus}\n";
                if (data.strBonus > 0)
                    statsText += $"힘: +{data.strBonus}\n";
                if (data.dexBonus > 0)
                    statsText += $"민첩: +{data.dexBonus}\n";
                if (data.intBonus > 0)
                    statsText += $"지능: +{data.intBonus}\n";
            }
            else if (data.itemType == ItemType.Consumable)
            {
                statsText += "\n[효과]\n";
                if (data.healAmount > 0)
                    statsText += $"HP 회복: +{data.healAmount}\n";
            }

            statsText += $"\n판매가: {data.sellPrice}G";

            itemStatsText.text = statsText;
        }

        // 사용/버리기 버튼
        if (useButton != null)
        {
            bool canUse = data.itemType == ItemType.Consumable || data.itemType == ItemType.Equipment;
            useButton.gameObject.SetActive(canUse);
        }

        if (discardButton != null)
        {
            bool canDiscard = data.itemType != ItemType.QuestItem;
            discardButton.gameObject.SetActive(canDiscard);
        }
    }

    private string GetItemTypeName(ItemType type)
    {
        switch (type)
        {
            case ItemType.Equipment: return "장비";
            case ItemType.Consumable: return "소비 아이템";
            case ItemType.Material: return "재료";
            case ItemType.QuestItem: return "퀘스트 아이템";
            default: return "알 수 없음";
        }
    }

    private string GetEquipSlotName(EquipmentSlot slot)
    {
        switch (slot)
        {
            case EquipmentSlot.Weapon: return "무기";
            case EquipmentSlot.Armor: return "방어구";
            case EquipmentSlot.Accessory: return "장신구";
            default: return "없음";
        }
    }

    // ==========================================
    // 버튼 클릭 이벤트
    // ==========================================
    private void OnUseButtonClicked()
    {
        if (selectedItem == null) return;

        ItemData data = selectedItem.GetItemData();
        if (data == null) return;

        // 스탯 시스템 가져오기
        CharacterStats stats = null;
        if (PlayerController.Instance != null)
        {
            var statsComp = PlayerController.Instance.GetComponent<PlayerStatsComponent>();
            if (statsComp != null)
                stats = statsComp.Stats;
        }

        // 아이템 사용
        bool used = InventoryManager.Instance.UseItem(selectedItem.itemID, stats);

        if (used)
        {
            Debug.Log($"[ItemUI] {data.itemName} 사용됨");

            // UI 갱신
            RefreshItemList();

            // 아이템이 전부 소진되었으면 상세정보 숨김
            if (!InventoryManager.Instance.HasItem(selectedItem.itemID))
            {
                if (itemDetailPanel != null)
                    itemDetailPanel.SetActive(false);
                selectedItem = null;
            }
            else
            {
                // 수량 갱신
                ShowItemDetail(selectedItem);
            }
        }
    }

    private void OnDiscardButtonClicked()
    {
        if (selectedItem == null) return;

        ItemData data = selectedItem.GetItemData();
        if (data == null) return;

        // 확인 다이얼로그 (간단 구현)
        Debug.Log($"[ItemUI] {data.itemName} 버림");

        InventoryManager.Instance.RemoveItem(selectedItem.itemID, 1);

        RefreshItemList();

        // 아이템이 전부 소진되었으면 상세정보 숨김
        if (!InventoryManager.Instance.HasItem(selectedItem.itemID))
        {
            if (itemDetailPanel != null)
                itemDetailPanel.SetActive(false);
            selectedItem = null;
        }
        else
        {
            // 수량 갱신
            ShowItemDetail(selectedItem);
        }
    }

    // ==========================================
    // 이벤트 핸들러
    // ==========================================
    private void OnInventoryChanged()
    {
        if (isOpen)
        {
            RefreshItemList();

            // 선택된 아이템이 있으면 상세정보 갱신
            if (selectedItem != null && InventoryManager.Instance.HasItem(selectedItem.itemID))
            {
                ShowItemDetail(selectedItem);
            }
        }
    }

    // ==========================================
    // 외부에서 호출 가능한 메서드
    // ==========================================
    public void RefreshUI()
    {
        if (isOpen)
            RefreshItemList();
    }

    public bool IsItemUIOpen()
    {
        return isOpen;
    }
}