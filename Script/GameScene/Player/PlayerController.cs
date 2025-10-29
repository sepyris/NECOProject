using Definitions;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Attack Settings")]
    [SerializeField] private PlayerAttack.AttackType attackType = PlayerAttack.AttackType.Melee;
    [SerializeField] private float attackDelay = 0.5f;
    [SerializeField] private int attackDamage = 10;
    [SerializeField] private float meleeRange = 2.0f;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float projectileSpeed = 8f;

    // 컴포넌트
    private Rigidbody2D rb;
    private Animator animator;

    // 모듈화된 클래스들
    private PlayerMovement movement;
    private PlayerAttack attack;
    private PlayerInteraction interaction; // ⭐ 채집 + NPC 통합
    private PlayerAnimationController animationController;
    private PlayerSaveLoad saveLoad;
    private PlayerBoundaryLimiter boundaryLimiter;

    // 조작 잠금
    private bool controlsLocked = false;
    public bool ControlsLocked => controlsLocked;

    // ⭐ 공격 상태 확인용 프로퍼티
    public bool IsAttacking => attack != null && attack.IsAttacking;

    // ⭐ 채집 상태 확인용 프로퍼티
    public bool IsGathering => interaction != null && interaction.IsGathering;

    public void SetControlsLocked(bool locked)
    {
        controlsLocked = locked;

        // 각 모듈에 전달
        if (movement != null) movement.ControlsLocked = locked;
        if (attack != null) attack.ControlsLocked = locked;
        if (interaction != null) interaction.ControlsLocked = locked;

        if (locked)
        {
            if (rb != null) rb.velocity = Vector2.zero;
        }
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);

            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            if (this.gameObject != Instance.gameObject)
            {
                SceneManager.sceneLoaded -= OnSceneLoaded;
                Destroy(this.gameObject);
            }
        }
    }

    void Start()
    {
        InitializeComponents();
        InitializeModules();

        // 현재 씬이 게임 씬이면 즉시 초기화
        Scene active = SceneManager.GetActiveScene();
        if (active.name.StartsWith(Def_Name.SCENE_NAME_START_MAP))
        {
            saveLoad.InitializePlayerState(active.name);
        }
    }

    private void InitializeComponents()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
            if (rb == null)
            {
                Debug.LogError("[Player] Rigidbody2D를 찾을 수 없습니다!");
            }
        }

        if (animator == null)
        {
            animator = GetComponent<Animator>();
            if (animator == null)
            {
                Debug.LogWarning("[Player] Animator를 찾을 수 없습니다. 애니메이션이 재생되지 않습니다.");
            }
        }
    }

    private void InitializeModules()
    {
        // 각 모듈 생성 및 초기화
        movement = new PlayerMovement(rb);
        movement.MoveSpeed = moveSpeed;

        animationController = new PlayerAnimationController(animator);

        attack = new PlayerAttack();
        attack.attackType = attackType;
        attack.attackDelay = attackDelay;
        attack.attackDamage = attackDamage;
        attack.meleeRange = meleeRange;
        attack.projectilePrefab = projectilePrefab;
        attack.projectileSpeed = projectileSpeed;
        attack.SetMovement(movement);
        attack.SetAnimationController(animationController);


        // ⭐ 통합된 상호작용 모듈 (채집 + NPC)
        interaction = new PlayerInteraction(transform, animationController);

        saveLoad = new PlayerSaveLoad(transform);
        BoxCollider2D playerCollider = GetComponent<BoxCollider2D>();

        // ⭐ 월드 경계 제한 모듈 초기화 (플레이어 콜라이더 포함)
        boundaryLimiter = new PlayerBoundaryLimiter(rb, playerCollider);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!scene.name.StartsWith(Def_Name.SCENE_NAME_START_MAP))
        {
            Debug.Log($"[Player] '{scene.name}'은 게임 컨텐츠 씬이 아님.");
            return;
        }

        Debug.Log($"[Player] 씬 '{scene.name}' 로드 완료 → 플레이어 초기화.");
        if (saveLoad != null)
        {
            saveLoad.InitializePlayerState(scene.name);
        }
            
    }

    void Update()
    {
        // F1 디버깅 키
        if (Input.GetKeyDown(KeyCode.F1))
        {
            if (LoadingScreenManager.Instance != null)
            {
                LoadingScreenManager.Instance.ForceStopLoading();
                Debug.Log("[Player] F1 키: 강제로 로딩 해제!");
            }
        }

        // 로딩 중이면 입력 무시
        if (LoadingScreenManager.Instance != null && LoadingScreenManager.Instance.IsLoading)
        {
            if (rb != null) rb.velocity = Vector2.zero;
            if (Time.frameCount % 60 == 0)
            {
                Debug.LogWarning("[Player] 로딩 중...");
            }
            return;
        }

        // 조작 잠금 상태면 입력 무시
        if (controlsLocked)
        {
            if (rb != null) rb.velocity = Vector2.zero;
            return;
        }

        // 상호작용 입력 처리 (공격 중이 아닐 때만)
        if (!attack.IsAttacking)
        {
            interaction.HandleInteractionInput();
        }

        // 공격 입력 처리
        attack.HandleAttackInput();

        // ⭐ 매 프레임 가장 가까운 상호작용 대상 감지 (채집물 또는 NPC)
        interaction.UpdateNearestInteractable();

        // 이동 입력 처리 (공격 중이 아닐 때만)
        if (!attack.IsAttacking)
        {
            float horizontal = Input.GetAxisRaw(Def_Name.HORIZONTAL);
            float vertical = Input.GetAxisRaw(Def_Name.VERTICAL);
            Vector2 input = new Vector2(horizontal, vertical);
            movement.UpdateMovement(input);
        }
        else
        {
            // 공격 중에는 이동 입력 무시
            movement.UpdateMovement(Vector2.zero);
        }
    }

    void FixedUpdate()
    {
        if (rb == null) return;

        // 로딩 중이거나 조작 잠금 상태면 이동 정지
        if ((LoadingScreenManager.Instance != null && LoadingScreenManager.Instance.IsLoading)
            || controlsLocked)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        // 이동 적용
        movement.ApplyMovement();


        boundaryLimiter?.ApplyBoundaryLimit();
    }

    // 외부에서 호출하는 메서드들
    public void SaveStateBeforeDeactivation()
    {
        saveLoad.SaveStateBeforeDeactivation();
    }

    public void RestoreSubSceneState(SubSceneData data)
    {
        //saveLoad.RestoreSubSceneState(data);
    }

    public void PlayAnimation(string triggerName)
    {
        animationController.PlayAnimation(triggerName);
    }

    // ⭐ 플레이어 방향 설정 (채집, 상호작용 등에서 사용)
    public void SetFacingDirection(Vector2 direction)
    {
        if (movement != null)
        {
            movement.SetLastDirection(direction);
        }

        // 스프라이트 반전 처리 (2D 게임용)
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null && Mathf.Abs(direction.x) > 0.1f)
        {
            spriteRenderer.flipX = direction.x < 0;
        }
    }
}