using Definitions;
using UnityEngine;
using static UnityEditor.Progress;

/// <summary>
/// 몬스터 메인 컨트롤러 (모듈화 버전)
/// 각 기능을 모듈로 분리하여 관리
/// </summary>
public class MonsterController : MonoBehaviour
{
    [Header("몬스터 정보")]
    public string monsterID = "Monster_Slime"; // 퀘스트용 고유 ID

    [Header("AI 설정")]
    [SerializeField] private bool isAggressive = true; // true: 선공, false: 후공
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float detectionRange = 5f;

    [Header("공격 설정")]
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private float preferredAttackDistance = 1.0f;

    [Header("아이템 드롭")]
    [SerializeField] private string dropItemID = "leather";
    [SerializeField][Range(0f, 1f)] private float dropChance = 0.5f;

    // 컴포넌트
    private Rigidbody2D rb;
    private MonsterStatsComponent statsComponent;

    // 모듈화된 클래스들
    private MonsterMovement movement;
    private MonsterAI ai;
    private MonsterCombat combat;
    private MonsterSpawnManager spawnManager;

    // 플레이어 참조
    private Transform playerTransform;

    void Awake()
    {
        InitializeComponents();
    }

    void Start()
    {
        InitializeModules();
        FindPlayer();
        SetupIgnorePlayerCollision();
    }

    void Update()
    {
        if (statsComponent != null && statsComponent.Stats.currentHP <= 0)
            return;

        // AI 업데이트
        ai?.UpdateAI(playerTransform);
    }

    void FixedUpdate()
    {
        if (statsComponent != null && statsComponent.Stats.currentHP <= 0)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        // AI 상태에 따른 행동 실행
        ai?.ExecuteCurrentState();
    }

    /// <summary>
    /// 컴포넌트 초기화
    /// </summary>
    private void InitializeComponents()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError($"[Monster] {monsterID}: Rigidbody2D를 찾을 수 없습니다!");
        }

        statsComponent = GetComponent<MonsterStatsComponent>();
        if (statsComponent == null)
        {
            Debug.LogError($"[Monster] {monsterID}: MonsterStatsComponent를 찾을 수 없습니다!");
        }
    }

    /// <summary>
    /// 모듈 초기화
    /// </summary>
    private void InitializeModules()
    {
        // 이동 모듈
        movement = new MonsterMovement(rb, transform);
        movement.moveSpeed = moveSpeed;

        // 스폰 관리 모듈
        spawnManager = new MonsterSpawnManager(transform);

        // 전투 모듈
        combat = new MonsterCombat(transform, statsComponent);
        combat.attackRange = attackRange;
        combat.attackCooldown = attackCooldown;
        combat.preferredAttackDistance = preferredAttackDistance;

        // AI 모듈
        ai = new MonsterAI(transform, movement, combat, spawnManager);
        ai.isAggressive = isAggressive;
        ai.detectionRange = detectionRange;

        Debug.Log($"[Monster] {monsterID} 모듈 초기화 완료");
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
        if (statsComponent == null) return;

        statsComponent.TakeDamage(damage);

        // AI에게 도발(Provoked) 상태 알림
        ai?.SetProvoked();
    }

    /// <summary>
    /// 스폰 영역 설정
    /// </summary>
    public void SetSpawnArea(Collider2D areaCollider)
    {
        spawnManager?.SetSpawnArea(areaCollider);
    }

    /// <summary>
    /// 아이템 드롭 처리
    /// </summary>
    public void DropItem()
    {
        if (string.IsNullOrEmpty(dropItemID)) return;

        float roll = Random.Range(0f, 1f);
        if (roll <= dropChance)
        {
            Debug.Log($"[Monster] {monsterID}가 {dropItemID} 드롭!");

            // 퀘스트 매니저에 알림
            if (QuestManager.Instance != null)
            {
                QuestManager.Instance.UpdateItemProgress(dropItemID, 1);
            }

            InventoryManager.Instance.AddItem(dropItemID, 1);
        }
    }

    /// <summary>
    /// 몬스터 사망 알림 (퀘스트용)
    /// </summary>
    public void NotifyDeath()
    {
        // 퀘스트 매니저에 처치 알림
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.UpdateKillProgress(monsterID);
        }

        // 아이템 드롭
        DropItem();

        // 스폰 매니저에 알림
        MonsterSpawnArea spawnArea = GetComponentInParent<MonsterSpawnArea>();
        if (spawnArea == null && spawnManager?.spawnAreaCollider != null)
        {
            spawnArea = spawnManager.spawnAreaCollider.GetComponent<MonsterSpawnArea>();
        }

        if (spawnArea != null)
        {
            spawnArea.OnMonsterDied(this.gameObject);
        }
    }

    void OnDrawGizmosSelected()
    {
        // 감지 범위
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // 공격 범위
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // 분리 범위 (movement 모듈)
        if (Application.isPlaying && movement != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, movement.separationRadius);
        }
    }
}