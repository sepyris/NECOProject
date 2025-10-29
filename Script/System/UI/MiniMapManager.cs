using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Definitions;

public class MiniMapManager : MonoBehaviour
{
    public static MiniMapManager Instance { get; private set; }

    [Header("References")]
    public Camera minimapCamera;
    public RawImage minimapDisplay;
    public RenderTexture minimapTexture;

    [Header("Settings")]
    public float padding = 2f;
    public float followSmooth = 8f;
    public Color backgroundColor = Color.black;

    [Header("Game Scene Detection")]
    [Tooltip("게임 씬이 로드될 때까지 대기")]
    public bool waitForGameScene = true;
    public float updateInterval = 1f;  // 플레이어 재검색 간격

    private Transform player;
    private float nextUpdateTime = 0f;
    private bool isSetupComplete = false;

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
    }

    void Start()
    {
        SetupMinimapCamera();
        SetupRenderTextureIfNeeded();

        Debug.Log("[MiniMap] 초기 설정 완료. 게임 씬 대기 중...");
    }

    void Update()
    {
        // 주기적으로 게임씬 확인 및 플레이어 재검색
        if (!isSetupComplete || player == null)
        {
            if (Time.time >= nextUpdateTime)
            {
                nextUpdateTime = Time.time + updateInterval;
                TryFindGameScene();
            }
        }
    }

    public void ReInitialize()
    {
        SetupMinimapCamera();
        SetupRenderTextureIfNeeded();
        CenterCameraOnWorldBounds();
    }
    private void CenterCameraOnWorldBounds()
    {
        // WorldBorder 태그를 가진 오브젝트 모두 찾기
        GameObject[] borders = GameObject.FindGameObjectsWithTag(Def_Name.WORLD_BORDER_TAG);

        if (borders.Length == 0)
        {
            Debug.LogWarning("[MiniMap] WorldBorder 없음. (0,0)을 기본 중심으로 사용");
            minimapCamera.transform.position = new Vector3(0, 0, -20f);
            return;
        }

        // 첫 번째 border의 bounds를 기준으로 시작
        Collider2D first = borders[0].GetComponent<Collider2D>();
        if (first == null)
        {
            minimapCamera.transform.position = new Vector3(0, 0, -20f);
            return;
        }

        Bounds worldBounds = first.bounds;

        // 나머지 border들의 bounds를 합쳐서 전체 영역 계산
        for (int i = 1; i < borders.Length; i++)
        {
            Collider2D c = borders[i].GetComponent<Collider2D>();
            if (c != null)
                worldBounds.Encapsulate(c.bounds);
        }

        // 중심점 계산
        Vector3 worldCenter = worldBounds.center;
        worldCenter.z = -20f;

        minimapCamera.transform.position = worldCenter;

    }
    void LateUpdate()
    {
        if (!isSetupComplete || minimapCamera == null) return;

        // 전체맵 중심으로 고정
        CenterCameraOnWorldBounds();
    }
    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;

            // RenderTexture 정리
            if (minimapTexture != null)
            {
                if (minimapCamera != null && minimapCamera.targetTexture == minimapTexture)
                    minimapCamera.targetTexture = null;

                minimapTexture.Release();
                Destroy(minimapTexture);
                minimapTexture = null;
            }
        }
    }

    private void SetupMinimapCamera()
    {
        if (minimapCamera == null)
        {
            GameObject camObj = new GameObject("MiniMap_Camera");
            camObj.tag = "KeepCamera";
            minimapCamera = camObj.AddComponent<Camera>();
            DontDestroyOnLoad(camObj);

            Debug.Log("[MiniMap] 카메라 생성됨");
        }

        // 카메라 기본 설정
        minimapCamera.orthographic = true;
        minimapCamera.orthographicSize = 50f;  // 기본값 증가
        minimapCamera.cullingMask = (Def_Layer_Mask_Values.LAYER_MASK_DEFAULT) | (Def_Layer_Mask_Values.LAYER_MASK_MINIMAP_OBJECT);
        minimapCamera.clearFlags = CameraClearFlags.SolidColor;
        minimapCamera.backgroundColor = backgroundColor;
        minimapCamera.depth = -100;
        minimapCamera.transform.position = new Vector3(0, 0, -20f);
        minimapCamera.transform.rotation = Quaternion.identity;
    }

    private void SetupRenderTextureIfNeeded()
    {
        if (minimapCamera == null || minimapDisplay == null)
        {
            Debug.LogWarning("[MiniMap] Camera 또는 RawImage가 없습니다!");
            return;
        }

        // 기존 RenderTexture가 있으면 완전히 해제
        if (minimapTexture != null)
        {
            if (minimapCamera.targetTexture == minimapTexture)
                minimapCamera.targetTexture = null;

            if (minimapDisplay.texture == minimapTexture)
                minimapDisplay.texture = null;

            minimapTexture.Release();
            Destroy(minimapTexture);
            minimapTexture = null;

            Debug.Log("[MiniMap] 기존 RenderTexture 해제");
        }

        // 새 RenderTexture 생성 - 중요: Create() 명시적 호출
        minimapTexture = new RenderTexture(512, 512, 16, RenderTextureFormat.ARGB32);
        minimapTexture.name = "Minimap_RT";
        minimapTexture.filterMode = FilterMode.Bilinear;
        minimapTexture.anisoLevel = 0;
        minimapTexture.useMipMap = false;
        minimapTexture.autoGenerateMips = false;

        // 매우 중요: Create를 명시적으로 호출해야 실시간 렌더링됨
        minimapTexture.Create();

        Debug.Log("[MiniMap] RenderTexture 생성 및 Create() 호출");

        minimapCamera.targetTexture = minimapTexture;

        // RawImage에 할당
        minimapDisplay.texture = minimapTexture;
        minimapDisplay.enabled = true;
        minimapDisplay.gameObject.SetActive(true);

        // Canvas 갱신
        Canvas parentCanvas = minimapDisplay.GetComponentInParent<Canvas>();
        if (parentCanvas != null)
        {
            Debug.Log($"[MiniMap] Canvas: {parentCanvas.name}, RenderMode: {parentCanvas.renderMode}");
            Canvas.ForceUpdateCanvases();
        }

        Debug.Log($"[MiniMap] RenderTexture 연결 완료");
    }

    /// <summary>
    /// 게임 씬이 로드되었는지 확인하고 초기화
    /// </summary>
    private void TryFindGameScene()
    {
        // PlayerController.Instance 우선 시도 (DontDestroyOnLoad에 있으므로)
        if (PlayerController.Instance != null)
        {
            player = PlayerController.Instance.transform;

            // 월드 바운드 계산
            Bounds bounds = CalculateWorldBounds();
            UpdateCameraSizeAndPosition(bounds);

            isSetupComplete = true;
            return;
        }

        // FindObjectOfType로 모든 씬 검색 (느리지만 확실함)
        PlayerController pc = FindObjectOfType<PlayerController>();
        if (pc != null)
        {
            player = pc.transform;

            Bounds bounds = CalculateWorldBounds();
            UpdateCameraSizeAndPosition(bounds);

            isSetupComplete = true;
            return;
        }

        // 태그로 검색 (폴백)
        GameObject playerObj = GameObject.FindGameObjectWithTag(Def_Name.PLAYER_TAG);
        if (playerObj != null)
        {
            player = playerObj.transform;

            Bounds bounds = CalculateWorldBounds();
            UpdateCameraSizeAndPosition(bounds);

            isSetupComplete = true;
            return;
        }

        // 게임 씬을 찾지 못함
        if (!isSetupComplete)
        {
            Debug.LogWarning("[MiniMap] 플레이어를 찾을 수 없습니다. 대기 중...");
        }
    }

    private Bounds CalculateWorldBounds()
    {
        // 모든 씬에서 WorldBorder 검색
        GameObject[] borders = GameObject.FindGameObjectsWithTag(Def_Name.WORLD_BORDER_TAG);

        if (borders.Length == 0)
        {
            if (player != null)
                return new Bounds(player.position, Vector3.one * 30f);
            return new Bounds(Vector3.zero, Vector3.one * 30f);
        }

        Collider2D first = borders[0].GetComponent<Collider2D>();
        if (first == null)
        {
            return new Bounds(Vector3.zero, Vector3.one * 30f);
        }

        Bounds world = first.bounds;
        for (int i = 1; i < borders.Length; i++)
        {
            Collider2D c = borders[i].GetComponent<Collider2D>();
            if (c != null) world.Encapsulate(c.bounds);
        }

        Debug.Log($"[MiniMap] 월드 바운드: center={world.center}, size={world.size}");
        return world;
    }

    private void UpdateCameraSizeAndPosition(Bounds worldBounds)
    {
        if (minimapCamera == null) return;

        // 카메라 크기만 설정 (위치는 LateUpdate에서 플레이어 추적)
        float halfHeight = worldBounds.extents.y + padding;
        float halfWidth = worldBounds.extents.x + padding;
        float sizeByWidth = halfWidth / minimapCamera.aspect;
        float targetSize = Mathf.Max(halfHeight, sizeByWidth);

        minimapCamera.orthographicSize = targetSize;


        // 위치는 초기화 시에만 플레이어 위치로 설정
        if (player != null)
        {
            Vector3 initialPos = player.position;
            initialPos.z = -20f;
            minimapCamera.transform.position = initialPos;
        }
    }
}