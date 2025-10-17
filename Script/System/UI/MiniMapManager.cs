using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Definitions;

public class MiniMapManager : MonoBehaviour
{
    public static MiniMapManager Instance { get; private set; }

    [Header("References")]
    public Camera minimapCamera;         // inspector�� �Ҵ� (orthographic, culling mask ����)
    public RawImage minimapDisplay;      // UI RawImage (RenderTexture�� ǥ��)
    public RenderTexture minimapTexture; // ���������� �����Ϳ��� �Ҵ�

    [Header("Settings")]
    public float padding = 2f;           // ���� �ٿ�� ����
    public float followSmooth = 8f;      // �÷��̾� ���� ������

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
            Debug.LogError("[MiniMapManager] minimapCamera�� �Ҵ���� �ʾҽ��ϴ�.");

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
        // �� �ε�� �÷��̾�/���� �ٿ�� �缳��
        ReinitializeForScene();
    }

    public void ReinitializeForScene()
    {
        // �÷��̾� ã��
        var playerObj = GameObject.FindGameObjectWithTag(Def_Name.PLAYER_TAG);
        player = playerObj != null ? playerObj.transform : null;

        // ���� �ٿ�� ��� �� ī�޶� ������/��ġ ������Ʈ
        Bounds bounds = CalculateWorldBounds();
        UpdateCameraSizeAndPosition(bounds);
    }

    void LateUpdate()
    {
        if (player == null || minimapCamera == null) return;

        // �÷��̾ �� ������ ������ ������
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
            // �ٿ�带 ��ã���� �÷��̾� ��ó �⺻ �ڽ� ��ȯ
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

        // ī�޶� ������ �߽����� �ű�� orthographicSize ���
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