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
    private PlayerInteraction interaction;
    private PlayerAnimationController animationController;
    private PlayerSaveLoad saveLoad;
    private PlayerBoundaryLimiter boundaryLimiter;

    // 조작 잠금
    private bool controlsLocked = false;
    public bool ControlsLocked => controlsLocked;

    // 공격 상태 확인용 프로퍼티
    public bool IsAttacking => attack != null && attack.IsAttacking;

    // 채집 상태 확인용 프로퍼티
    public bool IsGathering => interaction != null && interaction.IsGathering;

    public void SetControlsLocked(bool locked)
    {
        controlsLocked = locked;

        if (movement != null) movement.ControlsLocked = locked;
        if (attack != null) attack.ControlsLocked = locked;
        if (interaction != null) interaction.ControlsLocked = locked;

        if (locked && rb != null) rb.velocity = Vector2.zero;
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            if (this.gameObject != Instance.gameObject)
            {
                SceneManager.sceneLoaded -= OnSceneLoaded;
                Destroy(gameObject);
            }
        }
    }

    void Start()
    {
        InitializeComponents();
        InitializeModules();

        Scene active = SceneManager.GetActiveScene();
        if (active.name.StartsWith(Def_Name.SCENE_NAME_START_MAP))
        {
            saveLoad.InitializePlayerState(active.name);
        }
    }

    private void InitializeComponents()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (animator == null) animator = GetComponent<Animator>();
    }

    private void InitializeModules()
    {
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

        interaction = new PlayerInteraction(transform, animationController);

        saveLoad = new PlayerSaveLoad(transform);
        BoxCollider2D playerCollider = GetComponent<BoxCollider2D>();
        boundaryLimiter = new PlayerBoundaryLimiter(rb, playerCollider);
    }

    private void OnDestroy()
    {
        if (Instance == this) SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.LogWarning($"[PlayerController] OnSceneLoaded 호출! scene.name={scene.name}, SCENE_NAME_START_MAP={Def_Name.SCENE_NAME_START_MAP}");

        if (!scene.name.StartsWith(Def_Name.SCENE_NAME_START_MAP))
            return;

        rb = GetComponent<Rigidbody2D>();
        Vector2 lastDirBeforeRegen = movement != null ? movement.LastMoveDirection : Vector2.right;

        if (movement != null)
        {
            movement.rb = rb;
            movement.MoveSpeed = moveSpeed;
            movement.SetLastDirection(lastDirBeforeRegen);
        }

        if (saveLoad != null)
            saveLoad.InitializePlayerState(scene.name);

        if (movement != null)
        {
            movement.SetLoadGuard();

            Vector2 restoredInput = InputManager.GetSavedInputForSceneTransition();

            Debug.LogWarning($"[OnSceneLoaded] 복원된 입력: {restoredInput}");
            Debug.LogWarning($"[OnSceneLoaded] 현재 키 상태 - Right:{Input.GetKey(KeyCode.RightArrow)}, Left:{Input.GetKey(KeyCode.LeftArrow)}, Up:{Input.GetKey(KeyCode.UpArrow)}, Down:{Input.GetKey(KeyCode.DownArrow)}");

            movement.currentInput = restoredInput;
            movement.SetLastDirection(restoredInput);

            if (rb != null)
                rb.velocity = movement.currentInput * movement.MoveSpeed;

            Debug.LogWarning($"[PlayerMovement Fix] 씬 로드 후 velocity 설정: {rb.velocity}");

            // 클리어는 나중에 호출 - 주석 처리하거나 삭제
            // InputManager.ClearSavedInput();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            if (LoadingScreenManager.Instance != null)
            {
                LoadingScreenManager.Instance.ForceStopLoading();
            }
        }

        bool skipLoadingCheck = movement != null && movement.IsInLoadGuard;

        if (!skipLoadingCheck && LoadingScreenManager.Instance != null && LoadingScreenManager.Instance.IsLoading)
        {
            if (rb != null) rb.velocity = Vector2.zero;
            return;
        }

        if (controlsLocked)
        {
            if (rb != null) rb.velocity = Vector2.zero;
            return;
        }

        if (!attack.IsAttacking) interaction.HandleInteractionInput();
        attack.HandleAttackInput();
        interaction.UpdateNearestInteractable();

        if (!attack.IsAttacking)
        {
            if (movement != null && movement.IsInLoadGuard)
            {
                float h = 0f;
                float v = 0f;

                if (Input.GetKey(KeyCode.LeftArrow)) h -= 1f;
                if (Input.GetKey(KeyCode.RightArrow)) h += 1f;
                if (Input.GetKey(KeyCode.UpArrow)) v += 1f;
                if (Input.GetKey(KeyCode.DownArrow)) v -= 1f;

                Vector2 realTimeInput = new Vector2(h, v);

                if (realTimeInput.magnitude > 0.01f)
                {
                    movement.currentInput = realTimeInput.normalized;
                    Debug.Log($"[Update] LoadGuard - 실시간 키 감지: {movement.currentInput}");
                }
                else
                {
                    Debug.Log($"[Update] LoadGuard - 저장된 입력 유지: {movement.currentInput}");
                }

                return;
            }

            if (movement != null && movement.IsInPostLoadWait)
            {
                Debug.Log($"[Update] PostLoadWait - 입력 유지: {movement.currentInput}");
                InputManager.currentInput = movement.currentInput;
                return;
            }

            // **추가: 일반 입력 갱신 전에 체크**
            InputManager.UpdateInput();

            // Input 시스템이 제대로 작동하지 않으면 마지막 입력 유지
            if (InputManager.currentInput.magnitude < 0.01f && movement.currentInput.magnitude > 0.01f)
            {
                // 실제로 키가 눌려있는지 직접 확인
                float h = 0f;
                float v = 0f;

                if (Input.GetKey(KeyCode.LeftArrow)) h -= 1f;
                if (Input.GetKey(KeyCode.RightArrow)) h += 1f;
                if (Input.GetKey(KeyCode.UpArrow)) v += 1f;
                if (Input.GetKey(KeyCode.DownArrow)) v -= 1f;

                Vector2 realTimeInput = new Vector2(h, v);

                if (realTimeInput.magnitude > 0.01f)
                {
                    // 실제로 키가 눌려있으면 사용
                    movement.currentInput = realTimeInput.normalized;
                    InputManager.currentInput = movement.currentInput;
                    Debug.LogWarning($"[Update] Input 시스템 복구 - 직접 체크: {movement.currentInput}");
                }
                else
                {
                    // 실제로도 키가 안 눌려있으면 정상적으로 멈춤
                    movement.UpdateMovement();
                    Debug.Log($"[Update] 일반 입력 처리 - 정상 멈춤");
                }
            }
            else
            {
                movement.UpdateMovement();
                Debug.Log($"[Update] 일반 입력 처리 - movement.currentInput: {movement.currentInput}, InputManager.currentInput: {InputManager.currentInput}");
            }
        }
    }

    void FixedUpdate()
    {
        if (rb == null) return;

        bool isLockedOrLoading = (LoadingScreenManager.Instance != null && LoadingScreenManager.Instance.IsLoading) || controlsLocked;

        if (movement != null && movement.IsInLoadGuard)
        {
            rb.velocity = movement.currentInput * movement.MoveSpeed;
            movement.DecrementLoadGuard();

            Debug.Log($"[FixedUpdate] LoadGuard 중 - currentInput: {movement.currentInput}, velocity: {rb.velocity}");
            return;
        }

        // **추가: PostLoadWait 기간**
        if (movement != null && movement.IsInPostLoadWait)
        {
            rb.velocity = movement.currentInput * movement.MoveSpeed;
            movement.DecrementPostLoadWait();

            Debug.Log($"[FixedUpdate] PostLoadWait 중 - currentInput: {movement.currentInput}, velocity: {rb.velocity}");
            return;
        }

        if (isLockedOrLoading)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        Debug.Log($"[FixedUpdate] 일반 이동 - movement.currentInput: {movement.currentInput}, velocity 적용 전");

        movement.ApplyMovement();
        boundaryLimiter?.ApplyBoundaryLimit();

        Debug.Log($"[FixedUpdate] 일반 이동 - velocity 적용 후: {rb.velocity}");
    }


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

    public void SetFacingDirection(Vector2 direction)
    {
        if (movement != null) movement.SetLastDirection(direction);

        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null && Mathf.Abs(direction.x) > 0.1f)
            spriteRenderer.flipX = direction.x < 0;
    }
    public void RestoreInputAfterSceneLoad(Vector2 input)
    {
        Debug.LogWarning($"[PlayerController] RestoreInputAfterSceneLoad 호출: {input}");

        if (movement != null)
        {
            movement.SetLoadGuard();
            movement.currentInput = input;
            movement.SetLastDirection(input);

            if (rb != null)
                rb.velocity = input * movement.MoveSpeed;

            Debug.LogWarning($"[PlayerController] 입력 복원 완료: currentInput={movement.currentInput}, velocity={rb.velocity}");
        }
    }
}
