using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerHUD : MonoBehaviour
{
    public static PlayerHUD Instance { get; private set; }

    private Canvas canvas;
    private Text sceneText;
    private Text posText;
    private Text healthText;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            CreateHud();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void CreateHud()
    {
        // ĵ���� ����
        GameObject canvasGO = new GameObject("PlayerHUD_Canvas");
        canvasGO.transform.SetParent(this.transform);
        canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;

        canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasGO.AddComponent<GraphicRaycaster>();

        // �г� (������ ���)
        GameObject panelGO = new GameObject("HUD_Panel");
        panelGO.transform.SetParent(canvasGO.transform);
        var img = panelGO.AddComponent<Image>();
        img.color = new Color(0f, 0f, 0f, 0.25f);
        RectTransform panelRT = panelGO.GetComponent<RectTransform>();
        panelRT.anchorMin = new Vector2(0.02f, 0.85f);
        panelRT.anchorMax = new Vector2(0.32f, 0.98f);
        panelRT.offsetMin = Vector2.zero;
        panelRT.offsetMax = Vector2.zero;

        // Scene Text
        sceneText = CreateText("HUD_SceneText", panelGO.transform, new Vector2(0.5f, 0.75f), 14);
        // Position Text
        posText = CreateText("HUD_PosText", panelGO.transform, new Vector2(0.5f, 0.5f), 14);
        // Health Text (placeholder, ������Ʈ API ����)
        healthText = CreateText("HUD_HealthText", panelGO.transform, new Vector2(0.5f, 0.25f), 14);

        sceneText.alignment = TextAnchor.MiddleLeft;
        posText.alignment = TextAnchor.MiddleLeft;
        healthText.alignment = TextAnchor.MiddleLeft;

        sceneText.text = "Scene: -";
        posText.text = "Pos: -";
        healthText.text = "Health: -";
    }

    private Text CreateText(string name, Transform parent, Vector2 anchor, int fontSize)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent);
        var txt = go.AddComponent<Text>();
        txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        txt.fontSize = fontSize;
        txt.color = Color.white;
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = new Vector2(0f, 0.5f);
        rt.sizeDelta = new Vector2(400f, 24f);
        rt.anchoredPosition = new Vector2(8f, 0f);
        return txt;
    }

    void LateUpdate()
    {
        // ��/�÷��̾� ��ġ ���� (���� ȭ�鿡 ǥ�õ����� ���� �ǽð� ����)
        sceneText.text = $"Scene: {SceneManager.GetActiveScene().name}";

        if (PlayerController.Instance != null)
        {
            Vector3 p = PlayerController.Instance.transform.position;
            posText.text = $"Pos: {p.x:F1}, {p.y:F1}";
        }
        else
        {
            posText.text = "Pos: -";
        }
    }

    // �ܺο��� �÷��̾� ü�� �� ������Ʈ �� API
    public void SetHealth(int hp)
    {
        if (healthText != null) healthText.text = $"Health: {hp}";
    }
}