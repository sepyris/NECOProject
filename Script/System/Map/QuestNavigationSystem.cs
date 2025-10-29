using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// ����Ʈ �׺���̼� �ý���
/// ����� �⺻ ������ - ���߿� �̴ϸ�/ȭ��ǥ ���� �߰�
/// </summary>
public class QuestNavigationSystem : MonoBehaviour
{
    public static QuestNavigationSystem Instance { get; private set; }

    [Header("UI ���")]
    public GameObject navigationPanel;
    public TextMeshProUGUI targetNameText;
    public TextMeshProUGUI distanceText;
    public Image directionArrow;

    [Header("���� ���")]
    private string currentTargetNPCId;
    private NPCController currentTargetNPC;

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
        }

        if (navigationPanel != null)
            navigationPanel.SetActive(false);
    }

    void Update()
    {
        if (currentTargetNPC != null)
        {
            UpdateNavigation();
        }
    }

    /// <summary>
    /// Ư�� NPC�� ���� ������� ����
    /// </summary>
    public void SetNavigationTarget(string npcId)
    {
        if (NPCInfoManager.Instance == null) return;

        NPCInfo npcInfo = NPCInfoManager.Instance.GetNPCInfo(npcId);
        if (npcInfo == null)
        {
            Debug.LogWarning($"[Navigation] NPC ������ ã�� �� ����: {npcId}");
            return;
        }

        // ���� �ʿ� �ִ��� Ȯ��
        if (MapInfoManager.Instance != null)
        {
            if (npcInfo.mapId != MapInfoManager.Instance.currentMapId)
            {
                // �ٸ� �ʿ� ���� - ��� �ȳ�
                ShowMapPathToTarget(npcInfo);
                return;
            }
        }

        // ���� �ʿ� ���� - ���� NPC ã��
        NPCController[] npcs = FindObjectsOfType<NPCController>();
        foreach (var npc in npcs)
        {
            if (npc.npcId == npcId)
            {
                currentTargetNPC = npc;
                currentTargetNPCId = npcId;

                if (navigationPanel != null)
                    navigationPanel.SetActive(true);

                if (targetNameText != null)
                    targetNameText.text = $"��ǥ: {npcInfo.npcName}";

                Debug.Log($"[Navigation] ���� ����: {npcInfo.npcName}");
                return;
            }
        }

        Debug.LogWarning($"[Navigation] ������ NPC�� ã�� �� ����: {npcId}");
    }

    /// <summary>
    /// �ٸ� �ʿ� �ִ� NPC�� ���� ��� ǥ��
    /// </summary>
    private void ShowMapPathToTarget(NPCInfo npcInfo)
    {
        if (MapInfoManager.Instance == null) return;

        string currentMap = MapInfoManager.Instance.currentMapId;
        string targetMap = npcInfo.mapId;

        var path = MapInfoManager.Instance.FindPathBetweenMaps(currentMap, targetMap);

        if (path.Count > 1)
        {
            // ������ ���� �� �� ǥ��
            string nextMapId = path[1];
            string nextMapName = MapInfoManager.Instance.GetMapName(nextMapId);

            if (navigationPanel != null)
                navigationPanel.SetActive(true);

            if (targetNameText != null)
                targetNameText.text = $"��ǥ: {npcInfo.npcName}";

            if (distanceText != null)
                distanceText.text = $"{nextMapName}(��)�� �̵��ϼ���";

            Debug.Log($"[Navigation] ���: {string.Join(" �� ", path)}");
        }
    }

    /// <summary>
    /// �׺���̼� ������Ʈ (�Ÿ�, ���� ���)
    /// </summary>
    private void UpdateNavigation()
    {
        if (PlayerController.Instance == null || currentTargetNPC == null)
            return;

        Vector3 playerPos = PlayerController.Instance.transform.position;
        Vector3 targetPos = currentTargetNPC.transform.position;

        // �Ÿ� ���
        float distance = Vector3.Distance(playerPos, targetPos);

        if (distanceText != null)
        {
            if (distance < 2f)
            {
                distanceText.text = "����!";
                distanceText.color = Color.green;
            }
            else
            {
                distanceText.text = $"{distance:F1}m";
                distanceText.color = Color.white;
            }
        }

        // ���� ȭ��ǥ ȸ��
        if (directionArrow != null)
        {
            Vector3 direction = (targetPos - playerPos).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            directionArrow.transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
        }
    }

    /// <summary>
    /// �׺���̼� ����
    /// </summary>
    public void ClearNavigation()
    {
        currentTargetNPC = null;
        currentTargetNPCId = null;

        if (navigationPanel != null)
            navigationPanel.SetActive(false);

        Debug.Log("[Navigation] ���� ����");
    }

    /// <summary>
    /// ���� ���� ���� ����Ʈ�� ù ��° ��ǥ�� ����
    /// </summary>
    public void TrackCurrentQuest(string questId)
    {
        if (QuestManager.Instance == null) return;

        QuestData quest = QuestManager.Instance.GetQuestData(questId);
        if (quest == null || quest.status != QuestStatus.Accepted)
            return;

        // ù ��° �̿Ϸ� Dialogue ��ǥ ã��
        foreach (var obj in quest.objectives)
        {
            if (!obj.IsCompleted && obj.type == QuestType.Dialogue)
            {
                SetNavigationTarget(obj.targetId);
                return;
            }
        }

        Debug.Log($"[Navigation] ���� ������ ��ǥ ����: {questId}");
    }
}