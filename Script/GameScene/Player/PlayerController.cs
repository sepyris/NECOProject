// PlayerController.cs (최종 버전 - 단순화 + 공격 시스템)
using Definitions;
using GameSave;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    [Header("Movement")]
    private Rigidbody2D rb;
    public float moveSpeed = 5f;
    private bool isMoving = false;

    // 새로 추가: 외부에서 플레이어 입력을 잠글 수 있는 플래그
    private bool controlsLocked = false;
    public bool ControlsLocked => controlsLocked;

    public void SetControlsLocked(bool locked)
    {
        controlsLocked = locked;
        if (locked)
        {
            isMoving = false;
            if (rb != null) rb.velocity = Vector2.zero;
        }
    }

    public enum AttackType { Melee, Ranged }
    [Header("Attack")]
    [SerializeField] private AttackType attackType = AttackType.Melee;
    [SerializeField] private float attackDelay = 0.5f;
    [SerializeField] private int attackDamage = 10;
    [SerializeField] private float meleeRange = 2.0f;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float projectileSpeed = 8f;

    private bool isAttacking = false;
    private float lastAttackTime = -999f;
    private Vector2 lastMoveDirection = Vector2.right;

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

    // Ensure rb is initialized even if the scene was already active when this object was created.
    void Start()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();

        // If current active scene is a game content scene, initialize player state immediately.
        Scene active = SceneManager.GetActiveScene();
        if (active.name.StartsWith(Def_Name.SCENE_NAME_START_GAME))
        {
            InitializePlayerState(active.name);
        }
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
        if (!scene.name.StartsWith(Def_Name.SCENE_NAME_START_GAME))
        {
            Debug.Log($"[Player] '{scene.name}'은 게임 컨텐츠 씬이 아님.");
            return;
        }

        Debug.Log($"[Player] 씬 '{scene.name}' 로드 완료 → 플레이어 초기화.");
        InitializePlayerState(scene.name);
    }

    private void InitializePlayerState(string currentSceneName)
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();

        if (GameDataManager.Instance != null)
        {
            string targetSpawnID = GameDataManager.Instance.nextSceneSpawnPointID;
            SubSceneData loadedData = GameDataManager.Instance.LoadSubSceneState();

            loadedData.currentSceneName = currentSceneName;
            Debug.Log($"[Player] 현재 게임씬 이름 저장: {currentSceneName}");

            bool isLoadedPositionValid =
                (loadedData.positionX != 0f || loadedData.positionY != 0f || loadedData.positionZ != 0f);

            // 맵 이동 스폰 위치
            if (!string.IsNullOrEmpty(targetSpawnID))
            {
                Vector3 spawnPos = Vector3.zero;

                MapSpawnPoint[] allPoints = FindObjectsOfType<MapSpawnPoint>();
                foreach (var point in allPoints)
                {
                    if (point.spawnPointID == targetSpawnID)
                    {
                        spawnPos = point.transform.position;
                        break;
                    }
                }

                if (spawnPos != Vector3.zero)
                {
                    Debug.Log($"맵 이동 스폰 ID '{targetSpawnID}' 위치 적용: {spawnPos}");
                    transform.position = spawnPos;

                    GameDataManager.Instance.nextSceneSpawnPointID = "";

                    loadedData.positionX = spawnPos.x;
                    loadedData.positionY = spawnPos.y;
                    loadedData.positionZ = spawnPos.z;
                }
                else
                {
                    Debug.LogWarning($"맵 이동 스폰 ID '{targetSpawnID}'를 찾을 수 없습니다!");
                }
            }
            else if (isLoadedPositionValid)
            {
                Debug.Log("저장된 SubSceneData 위치를 복원합니다.");
            }
            else
            {
                Debug.Log("저장된 위치가 없어 씬 초기 위치로 설정합니다.");
                loadedData.positionX = transform.position.x;
                loadedData.positionY = transform.position.y;
                loadedData.positionZ = transform.position.z;

                if (loadedData.health == 0) loadedData.health = 100;
            }

            RestoreSubSceneState(loadedData);
        }
    }

    void Update()
    {
        // F1은 항상 동작시켜 디버깅 가능하도록 유지
        if (Input.GetKeyDown(KeyCode.F1))
        {
            if (LoadingScreenManager.Instance != null)
            {
                LoadingScreenManager.Instance.ForceStopLoading();
                Debug.Log("[Player] F1 키: 강제로 로딩 해제!");
            }
        }

        // 로딩 중에는 입력 무시
        if (LoadingScreenManager.Instance != null && LoadingScreenManager.Instance.IsLoading)
        {
            isMoving = false;
            if (rb != null) rb.velocity = Vector2.zero;

            if (Time.frameCount % 60 == 0)
            {
                Debug.LogWarning("[Player] 로딩 중... 이동 불가. (F1 키로 강제 해제 가능)");
            }
            return;
        }

        // if controls locked by interaction (dialogue/shop), skip input handling
        if (controlsLocked)
        {
            isMoving = false;
            if (rb != null) rb.velocity = Vector2.zero;
            return;
        }

        // 이동 입력 처리
        float horizontalInput = Input.GetAxisRaw(Def_Name.HORIZONTAL);
        float verticalInput = Input.GetAxisRaw(Def_Name.VERTICAL);
        Vector2 movementInput = new Vector2(horizontalInput, verticalInput).normalized;

        if (isAttacking)
        {
            isMoving = false;
        }
        else
        {
            isMoving = movementInput.magnitude > 0.01f;
            if (isMoving)
                lastMoveDirection = movementInput;
        }

        // 공격 입력 처리
        if (!isAttacking && Input.GetKeyDown(KeyCode.Space) && Time.time - lastAttackTime >= attackDelay)
        {
            StartCoroutine(AttackRoutine());
        }
    }

    void FixedUpdate()
    {
        if (rb == null) return;

        // 로딩 중에는 이동 정지
        if (LoadingScreenManager.Instance != null && LoadingScreenManager.Instance.IsLoading)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        // controls locked 체크
        if (controlsLocked)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        // 공격 중에는 이동 정지
        if (isAttacking)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        // 이동 처리
        if (isMoving)
        {
            float horizontalInput = Input.GetAxisRaw(Def_Name.HORIZONTAL);
            float verticalInput = Input.GetAxisRaw(Def_Name.VERTICAL);
            Vector2 direction = new Vector2(horizontalInput, verticalInput).normalized;
            rb.velocity = direction * moveSpeed;
        }
        else
        {
            rb.velocity = Vector2.zero;
        }
    }

    // ===== 공격 시스템 =====

    private IEnumerator AttackRoutine()
    {
        isAttacking = true;
        lastAttackTime = Time.time;

        Attack();

        yield return new WaitForSeconds(attackDelay);

        isAttacking = false;
    }

    private void Attack()
    {
        if (attackType == AttackType.Melee)
            MeleeAttack();
        else if (attackType == AttackType.Ranged)
            RangedAttack();
    }

    private void MeleeAttack()
    {
        Debug.Log("공격");
        Vector2 attackDirection = lastMoveDirection.normalized;
        Vector2 attackOrigin = (Vector2)transform.position + attackDirection * 0.5f;

        // Monster 레이어만 감지
        int monsterLayerMask = LayerMask.GetMask("Monster");
        RaycastHit2D hit = Physics2D.Raycast(attackOrigin, attackDirection, meleeRange, monsterLayerMask);

        if (hit.collider != null)
        {
            MonsterController monster = hit.collider.GetComponent<MonsterController>();
            if (monster != null)
            {
                monster.TakeDamage(attackDamage);
                Debug.Log($"[Player] 근거리 공격 성공! 대상: {hit.collider.gameObject.name}");
            }
        }

        // 디버그용 레이 표시
        Debug.DrawRay(attackOrigin, attackDirection * meleeRange, Color.red, 0.2f);
    }

    private void RangedAttack()
    {
        if (projectilePrefab == null)
        {
            Debug.LogWarning("[Player] 투사체 프리팹이 설정되지 않았습니다!");
            return;
        }

        Vector2 spawnPos = (Vector2)transform.position + lastMoveDirection * 0.5f;
        GameObject projectile = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);

        Rigidbody2D projRb = projectile.GetComponent<Rigidbody2D>();
        if (projRb != null)
        {
            projRb.velocity = lastMoveDirection * projectileSpeed;
        }

        Destroy(projectile, 3f); // 3초 후 자동 삭제
    }

    // ===== 저장/로드 =====

    public void SaveStateBeforeDeactivation()
    {
        string gameSceneName = GetCurrentGameSceneNameForSave();

        if (string.IsNullOrEmpty(gameSceneName))
        {
            Debug.Log("[Player] 저장할 게임씬 이름이 유효하지 않음 → 저장 스킵.");
            return;
        }

        if (GameDataManager.Instance == null) return;

        var dataToSave = new SubSceneData
        {
            currentSceneName = gameSceneName,
            positionX = transform.position.x,
            positionY = transform.position.y,
            positionZ = transform.position.z,
            health = 99,
            inventoryItems = new List<string>()
        };

        GameDataManager.Instance.SaveSubSceneState(dataToSave);
        Debug.Log($"[Player] 게임씬 '{gameSceneName}' 상태 저장 완료.");
    }

    private string GetCurrentGameSceneNameForSave()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (scene.name.StartsWith(Def_Name.SCENE_NAME_START_GAME))
            {
                return scene.name;
            }
        }
        return string.Empty;
    }

    public void RestoreSubSceneState(SubSceneData data)
    {
        transform.position = new Vector3(data.positionX, data.positionY, data.positionZ);
        Debug.Log($"플레이어 상태 복원: Scene={data.currentSceneName}, Pos=({data.positionX:F2},{data.positionY:F2})");
    }
}