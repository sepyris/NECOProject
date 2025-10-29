using UnityEngine;

/// <summary>
/// �÷��̾� ���� ������Ʈ
/// PlayerController�� �ٿ��� ���
/// </summary>
public class PlayerStatsComponent : MonoBehaviour
{
    [Header("���� �ý���")]
    public CharacterStats Stats = new CharacterStats();

    [Header("�ʱ� ����")]
    [SerializeField] private string playerName = "Hero";
    [SerializeField] private int startLevel = 1;

    void Awake()
    {
        InitializeSystems();
    }

    void Start()
    {
        // �̺�Ʈ ����
        SubscribeToEvents();
    }

    void OnDestroy()
    {
        // �̺�Ʈ ���� ����
        UnsubscribeFromEvents();
    }

    /// <summary>
    /// �ý��� �ʱ�ȭ
    /// </summary>
    private void InitializeSystems()
    {
        // ���� �ʱ�ȭ
        Stats.Initialize(playerName, startLevel);

        Debug.Log($"[PlayerStats] �÷��̾� '{playerName}' �ʱ�ȭ �Ϸ� (Lv.{startLevel})");
    }

    /// <summary>
    /// �̺�Ʈ ����
    /// </summary>
    private void SubscribeToEvents()
    {
        // ���� �̺�Ʈ
        Stats.OnStatsChanged += OnStatsChanged;
        Stats.OnLevelUp += OnLevelUp;
        Stats.OnDeath += OnDeath;
        Stats.OnExpGained += OnExpGained;

        // �κ��丮 �̺�Ʈ (InventoryManager �̱��� ���)
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnItemAdded += OnItemAdded;
            InventoryManager.Instance.OnItemRemoved += OnItemRemoved;
            InventoryManager.Instance.OnItemUsed += OnItemUsed;
        }
    }

    /// <summary>
    /// �̺�Ʈ ���� ����
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

    // ===== �̺�Ʈ �ڵ鷯 =====

    private void OnStatsChanged()
    {
        // UI ������Ʈ ��
        Debug.Log($"[PlayerStats] ���� ����� (HP: {Stats.currentHP}/{Stats.maxHP})");
    }

    private void OnLevelUp()
    {
        Debug.Log($"[PlayerStats] ������! Lv.{Stats.level}");
        // ������ ����Ʈ, ���� ��� ��
    }

    private void OnDeath()
    {
        Debug.Log($"[PlayerStats] �÷��̾� ���!");
        // ��� ó�� ���� (������, ���ӿ��� ��)
        HandlePlayerDeath();
    }

    private void OnExpGained(int amount)
    {
        Debug.Log($"[PlayerStats] ����ġ ȹ�� +{amount}");
        // ����ġ ȹ�� ����Ʈ ��
    }

    private void OnItemAdded(InventoryItem item)
    {
        Debug.Log($"[PlayerStats] ������ ȹ��: {item.itemName} x{item.quantity}");
        // ������ ȹ�� �˸� UI ǥ�� ��
    }

    private void OnItemRemoved(InventoryItem item)
    {
        Debug.Log($"[PlayerStats] ������ ���/����: {item.itemName}");
    }

    private void OnItemUsed(InventoryItem item)
    {
        Debug.Log($"[PlayerStats] ������ ���: {item.itemName}");
        // ������ ��� ����Ʈ ��
    }

    // ===== �����÷��� �޼��� =====

    /// <summary>
    /// �÷��̾� ��� ó��
    /// </summary>
    private void HandlePlayerDeath()
    {
        // ���� ���
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.SetControlsLocked(true);
        }

        // ��� �ִϸ��̼� ���
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.PlayAnimation("Death");
        }

        // 3�� �� ������ (����)
        Invoke(nameof(Respawn), 3f);
    }

    /// <summary>
    /// �÷��̾� ������
    /// </summary>
    private void Respawn()
    {
        // HP 50% ȸ��
        Stats.currentHP = Stats.maxHP / 2;
        Stats.RecalculateStats();

        // ���� ��� ����
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.SetControlsLocked(false);
            PlayerController.Instance.PlayAnimation("Idle");
        }

        Debug.Log("[PlayerStats] ������ �Ϸ�!");
    }

    /// <summary>
    /// ������ ��� (����Ű ��� ȣ��)
    /// </summary>
    public void UseItemByID(string itemID)
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.UseItem(itemID, Stats);
        }
    }

    /// <summary>
    /// ���� ��� ����
    /// </summary>
    public void UseHealthPotion()
    {
        UseItemByID("potion_health");
    }

    // ===== ����/�ε� =====

    /// <summary>
    /// �÷��̾� ������ ����
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
    /// �÷��̾� ������ �ε�
    /// </summary>
    public void LoadData(PlayerDataSave data)
    {
        if (data == null)
        {
            Debug.LogWarning("[PlayerStats] ���� �����Ͱ� �����ϴ�. �⺻�� ���.");
            return;
        }

        // ���� �ε�
        if (data.statsData != null)
        {
            Stats.LoadFromData(data.statsData);
        }

        // �κ��丮 �ε�
        if (data.inventoryData != null && InventoryManager.Instance != null)
        {
            InventoryManager.Instance.LoadFromData(data.inventoryData);
        }

        Debug.Log("[PlayerStats] ������ �ε� �Ϸ�!");
    }

    // ===== ����� ��ɾ� (�׽�Ʈ��) =====
    void Update()
    {
        // �׽�Ʈ�� ����Ű
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            UseHealthPotion();
        }

        // �����: HP ����
        if (Input.GetKeyDown(KeyCode.Minus))
        {
            Stats.TakeDamage(10);
        }

        // �����: ����ġ �߰�
        if (Input.GetKeyDown(KeyCode.Equals))
        {
            Stats.GainExperience(50);
        }
    }
}

/// <summary>
/// �÷��̾� ��ü ������ ���� ����ü
/// </summary>
[System.Serializable]
public class PlayerDataSave
{
    public CharacterStatsData statsData;
    public InventorySaveData inventoryData;
}