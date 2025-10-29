using UnityEngine;

/// <summary>
/// 플레이어 스탯 컴포넌트
/// PlayerController에 붙여서 사용
/// </summary>
public class PlayerStatsComponent : MonoBehaviour
{
    [Header("스탯 시스템")]
    public CharacterStats Stats = new CharacterStats();

    [Header("초기 설정")]
    [SerializeField] private string playerName = "Hero";
    [SerializeField] private int startLevel = 1;

    void Awake()
    {
        InitializeSystems();
    }

    void Start()
    {
        // 이벤트 구독
        SubscribeToEvents();
    }

    void OnDestroy()
    {
        // 이벤트 구독 해제
        UnsubscribeFromEvents();
    }

    /// <summary>
    /// 시스템 초기화
    /// </summary>
    private void InitializeSystems()
    {
        // 스탯 초기화
        Stats.Initialize(playerName, startLevel);

        Debug.Log($"[PlayerStats] 플레이어 '{playerName}' 초기화 완료 (Lv.{startLevel})");
    }

    /// <summary>
    /// 이벤트 구독
    /// </summary>
    private void SubscribeToEvents()
    {
        // 스탯 이벤트
        Stats.OnStatsChanged += OnStatsChanged;
        Stats.OnLevelUp += OnLevelUp;
        Stats.OnDeath += OnDeath;
        Stats.OnExpGained += OnExpGained;

        // 인벤토리 이벤트 (InventoryManager 싱글톤 사용)
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnItemAdded += OnItemAdded;
            InventoryManager.Instance.OnItemRemoved += OnItemRemoved;
            InventoryManager.Instance.OnItemUsed += OnItemUsed;
        }
    }

    /// <summary>
    /// 이벤트 구독 해제
    /// </summary>
    private void UnsubscribeFromEvents()
    {
        Stats.OnStatsChanged -= OnStatsChanged;
        Stats.OnLevelUp -= OnLevelUp;
        Stats.OnDeath -= OnDeath;
        Stats.OnExpGained -= OnExpGained;

        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnItemAdded -= OnItemAdded;
            InventoryManager.Instance.OnItemRemoved -= OnItemRemoved;
            InventoryManager.Instance.OnItemUsed -= OnItemUsed;
        }
    }

    // ===== 이벤트 핸들러 =====

    private void OnStatsChanged()
    {
        // UI 업데이트 등
        Debug.Log($"[PlayerStats] 스탯 변경됨 (HP: {Stats.currentHP}/{Stats.maxHP})");
    }

    private void OnLevelUp()
    {
        Debug.Log($"[PlayerStats] 레벨업! Lv.{Stats.level}");
        // 레벨업 이펙트, 사운드 재생 등
    }

    private void OnDeath()
    {
        Debug.Log($"[PlayerStats] 플레이어 사망!");
        // 사망 처리 로직 (리스폰, 게임오버 등)
        HandlePlayerDeath();
    }

    private void OnExpGained(int amount)
    {
        Debug.Log($"[PlayerStats] 경험치 획득 +{amount}");
        // 경험치 획득 이펙트 등
    }

    private void OnItemAdded(InventoryItem item)
    {
        Debug.Log($"[PlayerStats] 아이템 획득: {item.itemName} x{item.quantity}");
        // 아이템 획득 알림 UI 표시 등
    }

    private void OnItemRemoved(InventoryItem item)
    {
        Debug.Log($"[PlayerStats] 아이템 사용/제거: {item.itemName}");
    }

    private void OnItemUsed(InventoryItem item)
    {
        Debug.Log($"[PlayerStats] 아이템 사용: {item.itemName}");
        // 아이템 사용 이펙트 등
    }

    // ===== 게임플레이 메서드 =====

    /// <summary>
    /// 플레이어 사망 처리
    /// </summary>
    private void HandlePlayerDeath()
    {
        // 조작 잠금
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.SetControlsLocked(true);
        }

        // 사망 애니메이션 재생
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.PlayAnimation("Death");
        }

        // 3초 후 리스폰 (예시)
        Invoke(nameof(Respawn), 3f);
    }

    /// <summary>
    /// 플레이어 리스폰
    /// </summary>
    private void Respawn()
    {
        // HP 50% 회복
        Stats.currentHP = Stats.maxHP / 2;
        Stats.RecalculateStats();

        // 조작 잠금 해제
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.SetControlsLocked(false);
            PlayerController.Instance.PlayAnimation("Idle");
        }

        Debug.Log("[PlayerStats] 리스폰 완료!");
    }

    /// <summary>
    /// 아이템 사용 (단축키 등에서 호출)
    /// </summary>
    public void UseItemByID(string itemID)
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.UseItem(itemID, Stats);
        }
    }

    /// <summary>
    /// 포션 사용 예시
    /// </summary>
    public void UseHealthPotion()
    {
        UseItemByID("potion_health");
    }

    // ===== 저장/로드 =====

    /// <summary>
    /// 플레이어 데이터 저장
    /// </summary>
    public PlayerDataSave GetSaveData()
    {
        return new PlayerDataSave
        {
            statsData = Stats.ToSaveData(),
            inventoryData = InventoryManager.Instance?.ToSaveData()
        };
    }

    /// <summary>
    /// 플레이어 데이터 로드
    /// </summary>
    public void LoadData(PlayerDataSave data)
    {
        if (data == null)
        {
            Debug.LogWarning("[PlayerStats] 저장 데이터가 없습니다. 기본값 사용.");
            return;
        }

        // 스탯 로드
        if (data.statsData != null)
        {
            Stats.LoadFromData(data.statsData);
        }

        // 인벤토리 로드
        if (data.inventoryData != null && InventoryManager.Instance != null)
        {
            InventoryManager.Instance.LoadFromData(data.inventoryData);
        }

        Debug.Log("[PlayerStats] 데이터 로드 완료!");
    }

    // ===== 디버그 명령어 (테스트용) =====
    void Update()
    {
        // 테스트용 단축키
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            UseHealthPotion();
        }

        // 디버그: HP 감소
        if (Input.GetKeyDown(KeyCode.Minus))
        {
            Stats.TakeDamage(10);
        }

        // 디버그: 경험치 추가
        if (Input.GetKeyDown(KeyCode.Equals))
        {
            Stats.GainExperience(50);
        }
    }
}

/// <summary>
/// 플레이어 전체 데이터 저장 구조체
/// </summary>
[System.Serializable]
public class PlayerDataSave
{
    public CharacterStatsData statsData;
    public InventorySaveData inventoryData;
}