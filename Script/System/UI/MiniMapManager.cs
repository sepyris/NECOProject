using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Definitions;

public class MiniMapManager : MonoBehaviour
{
    public static MiniMapManager Instance { get; private set; }

    [Header("References")]
    public Camera minimapCamera;         // inspector에 할당 (orthographic, culling mask 설정)
    public RawImage minimapDisplay;      // UI RawImage (RenderTexture를 표시)
    public RenderTexture minimapTexture; // 선택적으로 에디터에서 할당

    [Header("Settings")]
    public float padding = 2f;           // 월드 바운드 여유
    public float followSmooth = 8f;      // 플레이어 추적 스무딩

    private Transform player;
    private Vector3 targetPos;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (minimapCamera == null)
            Debug.LogError("[MiniMapManager] minimapCamera가 할당되지 않았습니다.");

        SetupRenderTextureIfNeeded();
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Instance = null;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 씬 로드시 플레이어/월드 바운드 재설정
        ReinitializeForScene();
    }

    public void ReinitializeForScene()
    {
        // 플레이어 찾기
        var playerObj = GameObject.FindGameObjectWithTag(Def_Name.PLAYER_TAG);
        player = playerObj != null ? playerObj.transform : null;

        // 월드 바운드 계산 및 카메라 사이즈/위치 업데이트
        Bounds bounds = CalculateWorldBounds();
        UpdateCameraSizeAndPosition(bounds);
    }

    void LateUpdate()
    {
        if (player == null || minimapCamera == null) return;

        // 플레이어를 매 프레임 따르되 스무딩
        Vector3 desired = new Vector3(player.position.x, player.position.y, minimapCamera.transform.position.z);
        minimapCamera.transform.position = Vector3.Lerp(minimapCamera.transform.position, desired, Time.deltaTime * followSmooth);
    }

    private void SetupRenderTextureIfNeeded()
    {
        if (minimapCamera == null || minimapDisplay == null) return;

        if (minimapTexture == null)
        {
            minimapTexture = new RenderTexture(512, 512, 16);
            minimapTexture.name = "MiniMap_RT";
        }

        minimapCamera.targetTexture = minimapTexture;
        minimapDisplay.texture = minimapTexture;
    }

    private Bounds CalculateWorldBounds()
    {
        var borders = GameObject.FindGameObjectsWithTag(Def_Name.WORLD_BORDER_TAG);
        if (borders.Length == 0)
        {
            // 바운드를 못찾으면 플레이어 근처 기본 박스 반환
            if (player != null)
                return new Bounds(player.position, Vector3.one * 10f);
            return new Bounds(Vector3.zero, Vector3.one * 10f);
        }

        Collider2D first = borders[0].GetComponent<Collider2D>();
        if (first == null) return new Bounds(Vector3.zero, Vector3.one * 10f);

        Bounds world = first.bounds;
        for (int i = 1; i < borders.Length; i++)
        {
            var c = borders[i].GetComponent<Collider2D>();
            if (c != null) world.Encapsulate(c.bounds);
        }
        return world;
    }

    private void UpdateCameraSizeAndPosition(Bounds worldBounds)
    {
        if (minimapCamera == null) return;

        // 카메라를 월드의 중심으로 옮기고 orthographicSize 계산
        Vector3 center = worldBounds.center;
        float halfHeight = worldBounds.extents.y + padding;
        float halfWidth = worldBounds.extents.x + padding;
        float sizeByWidth = halfWidth / minimapCamera.aspect;
        float targetSize = Mathf.Max(halfHeight, sizeByWidth);

        minimapCamera.orthographic = true;
        minimapCamera.orthographicSize = targetSize;
        minimapCamera.transform.position = new Vector3(center.x, center.y, minimapCamera.transform.position.z);

        Debug.Log($"[MiniMapManager] Minimap reinit: center={center}, size={targetSize}");
    }
}