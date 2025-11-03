using Definitions;
using UnityEngine;

/// <summary>
/// 몬스터 컨트롤러 (CSV 데이터 기반)
/// </summary>
public class MonsterController : MonoBehaviour
{
    [Header("Monster ID")]
    [SerializeField] private string monsterID; // CSV에서 로드할 몬스터 ID

    [Header("Visual")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator animator;

    // CSV에서 로드한 데이터
    private MonsterData monsterData;

    // 현재 스탯
    private int currentHP;
    private bool isDead = false;

    // 컴포넌트
    private Rigidbody2D rb;

    // 모듈
    private MonsterMovement movement;
    private MonsterAI ai;
    private MonsterCombat combat;
    private MonsterSpawnManager spawnManager;

    // 플레이어 참조
    private Transform playerTransform;

    // 스폰 영역 참조
    private MonsterSpawnArea parentSpawnArea;
    private Collider2D cachedSpawnAreaCollider; // ⭐ 캐시 추가

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (animator == null)
            animator = GetComponent<Animator>();
    }

    void Start()
    {
        LoadMonsterData();
        InitializeModules();

        // ⭐ 모듈 초기화 후 캐시된 스폰 영역 설정 ⭐
        if (cachedSpawnAreaCollider != null)
        {
            ApplySpawnArea(cachedSpawnAreaCollider);
        }

        FindPlayer();
        SetupIgnorePlayerCollision();
    }

    void Update()
    {
        if (isDead) return;

        // AI 업데이트
        ai?.UpdateAI(playerTransform);
    }

    void FixedUpdate()
    {
        if (isDead)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        // AI 상태에 따른 행동 실행
        ai?.ExecuteCurrentState();
    }

    /// <summary>
    /// 몬스터 ID 설정 (SpawnArea에서 호출)
    /// </summary>
    public void SetMonsterID(string id)
    {
        monsterID = id;
        LoadMonsterData();
        InitializeModules();
    }

    /// <summary>
    /// CSV에서 몬스터 데이터 로드
    /// </summary>
    private void LoadMonsterData()
    {
        if (string.IsNullOrEmpty(monsterID))
        {
            Debug.LogWarning("[MonsterController] 몬스터 ID가 설정되지 않았습니다.");
            return;
        }

        if (MonsterDataManager.Instance == null)
        {
            Debug.LogError("[MonsterController] MonsterDataManager가 없습니다!");
            return;
        }

        monsterData = MonsterDataManager.Instance.GetMonsterData(monsterID);

        if (monsterData == null)
        {
            Debug.LogError($"[MonsterController] 몬스터 데이터를 찾을 수 없음: {monsterID}");
            return;
        }

        // 현재 HP 초기화
        currentHP = monsterData.maxHealth;

        Debug.Log($"[MonsterController] 몬스터 데이터 로드 완료: {monsterData.monsterName} (HP: {currentHP})");
    }

    /// <summary>
    /// 모듈 초기화
    /// </summary>
    private void InitializeModules()
    {
        if (monsterData == null || rb == null) return;

        // 이동 모듈
        movement = new MonsterMovement(rb, transform);
        movement.moveSpeed = monsterData.moveSpeed;

        // 스폰 관리 모듈
        spawnManager = new MonsterSpawnManager(transform);

        // 전투 모듈 (CSV 데이터 기반)
        combat = new MonsterCombat(transform, this);
        combat.attackRange = 1.5f; // 기본값, 필요시 CSV에 추가
        combat.attackCooldown = 1f / monsterData.attackSpeed; // 공격속도 → 쿨다운
        combat.preferredAttackDistance = 1.0f;

        // AI 모듈
        ai = new MonsterAI(transform, movement, combat, spawnManager, monsterData.isAggressive);
        ai.originalIsAggressive = monsterData.isAggressive;
        ai.detectionRange = monsterData.detectionRange;

        Debug.Log($"[MonsterController] {monsterData.monsterName} 모듈 초기화 완료");
    }

    /// <summary>
    /// 플레이어 찾기
    /// </summary>
    private void FindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag(Def_Name.PLAYER_TAG);
        if (player != null)
        {
            playerTransform = player.transform;
        }
    }

    /// <summary>
    /// 플레이어와의 충돌 무시 설정
    /// </summary>
    private void SetupIgnorePlayerCollision()
    {
        if (playerTransform == null) return;

        Collider2D playerCol = playerTransform.GetComponent<Collider2D>();
        if (playerCol == null) return;

        Collider2D[] myCols = GetComponentsInChildren<Collider2D>();
        foreach (var c in myCols)
        {
            if (c != null)
            {
                Physics2D.IgnoreCollision(c, playerCol, true);
            }
        }
    }

    /// <summary>
    /// 데미지 받기 (외부에서 호출)
    /// </summary>
    public void TakeDamage(int damage)
    {
        if (isDead || monsterData == null) return;

        currentHP -= damage;
        Debug.Log($"[Monster] {monsterData.monsterName} 데미지: {damage}, 남은 HP: {currentHP}/{monsterData.maxHealth}");

        // ⭐ AI에게 즉시 도발 상태 알림 (추적 시작) ⭐
        if (ai != null)
        {
            ai.SetProvoked();
            Debug.Log($"[Monster] {monsterData.monsterName} 도발됨! 플레이어 추적 시작");
        }

        if (currentHP <= 0)
        {
            currentHP = 0;
            Die();
        }
    }

    /// <summary>
    /// 체력 회복 (귀환 완료 시 호출)
    /// </summary>
    public void RegenerateHealth()
    {
        if (monsterData == null || isDead) return;

        int oldHP = currentHP;
        currentHP = monsterData.maxHealth;

        Debug.Log($"[Monster] {monsterData.monsterName} 체력 회복: {oldHP} → {currentHP}");
    }

    /// <summary>
    /// 공격력 가져오기 (Combat 모듈에서 사용)
    /// </summary>
    public int GetAttackPower()
    {
        return monsterData != null ? monsterData.attackPower : 10;
    }

    /// <summary>
    /// 몬스터 사망 처리
    /// </summary>
    private void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log($"[Monster] {monsterData.monsterName} 사망!");

        // 퀘스트 처치 알림
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.UpdateKillProgress(monsterID);
        }

        // 보상 지급
        GiveRewards();

        // 스폰 영역에 알림
        if (parentSpawnArea != null)
        {
            parentSpawnArea.OnMonsterDied(this.gameObject);
        }

        // 사망 애니메이션 후 제거
        if (animator != null)
        {
            animator.SetTrigger("Die");
        }

        Destroy(gameObject, 0.5f);
    }

    /// <summary>
    /// 보상 지급 (경험치, 골드, 아이템 드롭)
    /// </summary>
    private void GiveRewards()
    {
        if (monsterData == null) return;

        // 경험치 지급
        if (monsterData.dropExp > 0)
        {
            Debug.Log($"[Monster] 경험치 보상: {monsterData.dropExp}");
            // ExperienceManager.Instance?.AddExp(monsterData.dropExp);
        }

        // 골드 지급
        if (monsterData.dropGold > 0)
        {
            Debug.Log($"[Monster] 골드 보상: {monsterData.dropGold}");
            // PlayerStats.Instance?.AddGold(monsterData.dropGold);
        }

        // 아이템 드롭
        ProcessDropTable();
    }

    /// <summary>
    /// 드롭 테이블 처리
    /// </summary>
    private void ProcessDropTable()
    {
        if (monsterData.dropItems == null || monsterData.dropItems.Count == 0)
        {
            return;
        }

        foreach (var dropItem in monsterData.dropItems)
        {
            // ItemReward의 RollDrop() 메서드 사용
            if (dropItem.RollDrop())
            {
                // 아이템 획득
                if (InventoryManager.Instance != null)
                {
                    InventoryManager.Instance.AddItem(dropItem.itemId, dropItem.quantity);
                    Debug.Log($"[Monster] 아이템 드롭: {dropItem.itemId} x{dropItem.quantity} (확률: {dropItem.dropRate}%)");
                }

                // 퀘스트 매니저에 아이템 획득 알림
                if (QuestManager.Instance != null)
                {
                    QuestManager.Instance.UpdateItemProgress(dropItem.itemId, dropItem.quantity);
                }
            }
        }
    }

    /// <summary>
    /// 스폰 영역 설정 (MonsterSpawnArea에서 호출)
    /// Start() 이전에 호출될 수 있으므로 캐시만 함
    /// </summary>
    public void SetSpawnArea(MonsterSpawnArea spawnArea, Collider2D areaCollider)
    {
        parentSpawnArea = spawnArea;
        cachedSpawnAreaCollider = areaCollider;

        // ⭐ 모듈이 이미 초기화되어 있으면 즉시 적용 ⭐
        if (spawnManager != null && movement != null)
        {
            ApplySpawnArea(areaCollider);
        }
        else
        {
            Debug.Log($"[MonsterController] 스폰 영역 캐시됨 (Start()에서 적용 예정): {areaCollider?.gameObject.name}");
        }
    }

    /// <summary>
    /// 스폰 영역을 실제로 적용
    /// </summary>
    private void ApplySpawnArea(Collider2D areaCollider)
    {
        spawnManager?.SetSpawnArea(areaCollider);
        movement?.SetSpawnArea(areaCollider);

        Debug.Log($"[MonsterController] 스폰 영역 적용 완료: {areaCollider?.gameObject.name}");
    }

    /// <summary>
    /// 몬스터 이름 가져오기
    /// </summary>
    public string GetMonsterName()
    {
        return monsterData != null ? monsterData.monsterName : monsterID;
    }

    void OnDrawGizmosSelected()
    {
        if (monsterData == null) return;

        // 감지 범위
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, monsterData.detectionRange);

        // 공격 범위
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 1.5f);
    }
}