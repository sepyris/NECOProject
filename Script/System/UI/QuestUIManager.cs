using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Definitions;

/// <summary>
/// Q키로 열리는 독립적인 퀘스트 창 UI 관리
/// </summary>
public class QuestUIManager : MonoBehaviour
{
    public static QuestUIManager Instance { get; private set; }

    [Header("메인 패널")]
    public GameObject questUIPanel;
    public Button closeButton;

    [Header("탭 버튼")]
    public Button availableTabButton;
    public Button inProgressTabButton;
    public Button completedTabButton;

    [Header("퀘스트 리스트")]
    public Transform questListContainer;
    public GameObject questListItemPrefab;

    [Header("퀘스트 상세 정보")]
    public GameObject questDetailPanel;
    public TextMeshProUGUI questNameText;
    public TextMeshProUGUI questDescriptionText;
    public TextMeshProUGUI questObjectivesText;
    public TextMeshProUGUI questRewardsText;
    public Image questStatusImage;
    public Sprite availableSprite;
    public Sprite inProgressSprite;
    public Sprite completedSprite;

    [Header("빈 상태 표시")]
    public GameObject emptyStatePanel;
    public TextMeshProUGUI emptyStateText;

    //처음 열릴때만 초기화
    private bool is_initialize = true;

    private enum QuestTab
    {
        Available,
        InProgress,
        Completed
    }

    private QuestTab currentTab = QuestTab.Available;
    private string selectedQuestId;
    private bool isOpen = false;

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

        if (questUIPanel != null)
            questUIPanel.SetActive(false);

        SetupButtons();
    }

    void Update()
    {
        // Q키로 퀘스트 창 토글
        if (Input.GetKeyDown(KeyCode.Q))
        {
            // 대화 중이면 퀘스트 창 열지 않음
            if (DialogueUIManager.Instance != null && DialogueUIManager.Instance.IsDialogueOpen)
                return;

            if (isOpen)
                CloseQuestUI();
            else
                OpenQuestUI();
        }

        // ESC키로 닫기
        if (isOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseQuestUI();
        }
    }

    private void SetupButtons()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(CloseQuestUI);

        if (availableTabButton != null)
            availableTabButton.onClick.AddListener(() => SwitchTab(QuestTab.Available));

        if (inProgressTabButton != null)
            inProgressTabButton.onClick.AddListener(() => SwitchTab(QuestTab.InProgress));

        if (completedTabButton != null)
            completedTabButton.onClick.AddListener(() => SwitchTab(QuestTab.Completed));
    }

    // ==========================================
    // 퀘스트 UI 열기/닫기
    // ==========================================
    public void OpenQuestUI()
    {
        if (isOpen) return;

        isOpen = true;
        questUIPanel.SetActive(true);

        if(is_initialize)
        {
            // 기본 탭으로 초기화
            SwitchTab(QuestTab.Available);
            is_initialize = false;
        }

        UpdateTabButtons();
        RefreshQuestList();
        Debug.Log("[QuestUI] 퀘스트 창 열림");
    }

    public void CloseQuestUI()
    {
        if (!isOpen) return;

        isOpen = false;
        questUIPanel.SetActive(false);
        Debug.Log("[QuestUI] 퀘스트 창 닫힘");
    }

    // ==========================================
    // 탭 전환
    // ==========================================
    private void SwitchTab(QuestTab tab)
    {
        currentTab = tab;
        UpdateTabButtons();
        RefreshQuestList();
    }

    private void UpdateTabButtons()
    {
        // 탭 버튼 활성화 상태 업데이트
        if (availableTabButton != null)
        {
            var colors = availableTabButton.colors;
            colors.normalColor = currentTab == QuestTab.Available ? new Color(1f, 1f, 0.5f) : Color.white;
            availableTabButton.colors = colors;
        }

        if (inProgressTabButton != null)
        {
            var colors = inProgressTabButton.colors;
            colors.normalColor = currentTab == QuestTab.InProgress ? new Color(0.5f, 1f, 0.5f) : Color.white;
            inProgressTabButton.colors = colors;
        }

        if (completedTabButton != null)
        {
            var colors = completedTabButton.colors;
            colors.normalColor = currentTab == QuestTab.Completed ? new Color(0.5f, 0.5f, 1f) : Color.white;
            completedTabButton.colors = colors;
        }
    }

    // ==========================================
    // 퀘스트 리스트 갱신
    // ==========================================
    private void RefreshQuestList()
    {
        // 기존 리스트 아이템 삭제
        foreach (Transform child in questListContainer)
            Destroy(child.gameObject);

        

        // 현재 탭에 맞는 퀘스트 가져오기
        List<QuestData> quests = GetQuestsForCurrentTab();

        if (quests == null || quests.Count == 0)
        {
            ShowEmptyState();
            
            return;
        }

        if (emptyStatePanel != null)
            emptyStatePanel.SetActive(false);

        // 퀘스트 리스트 아이템 생성
        bool is_selected = false;
        foreach (var quest in quests)
        {
            CreateQuestListItem(quest);
            if(quest.questId == selectedQuestId)
            {
                is_selected = true;
            }
        }
        if(!is_selected)
        {
            selectedQuestId = null;
            if (questDetailPanel != null)
                questDetailPanel.SetActive(false);
        }

        Debug.Log($"[QuestUI] {currentTab} 탭: {quests.Count}개 퀘스트 표시");
    }

    private List<QuestData> GetQuestsForCurrentTab()
    {
        if (QuestManager.Instance == null)
            return new List<QuestData>();

        var allQuests = QuestManager.Instance.GetAllQuests();

        switch (currentTab)
        {
            case QuestTab.Available:
                return allQuests.Where(q => q.status == QuestStatus.None || q.status == QuestStatus.Offered).ToList();

            case QuestTab.InProgress:
                return allQuests.Where(q => q.status == QuestStatus.Accepted).ToList();

            case QuestTab.Completed:
                return allQuests.Where(q => q.status == QuestStatus.Completed || q.status == QuestStatus.Rewarded).ToList();

            default:
                return new List<QuestData>();
        }
    }

    private void CreateQuestListItem(QuestData quest)
    {
        GameObject itemObj = Instantiate(questListItemPrefab, questListContainer);
        Button itemButton = itemObj.GetComponent<Button>();
        TextMeshProUGUI itemText = itemObj.GetComponentInChildren<TextMeshProUGUI>();

        if (itemText != null)
        {
            string statusIcon = GetStatusIcon(quest.status);
            itemText.text = $"{statusIcon} {quest.questName}";
        }

        string capturedQuestId = quest.questId;
        itemButton.onClick.AddListener(() => ShowQuestDetail(capturedQuestId));
    }

    private string GetStatusIcon(QuestStatus status)
    {
        switch (status)
        {
            case QuestStatus.None:
            case QuestStatus.Offered:
                return "[시작 가능]";
            case QuestStatus.Accepted:
                return "[진행중]";
            case QuestStatus.Completed:
                return "[완료]";
            case QuestStatus.Rewarded:
                return "[보상 완료]";
            default:
                return "";
        }
    }

    private void ShowEmptyState()
    {
        if (emptyStatePanel != null)
            emptyStatePanel.SetActive(true);

        string message = currentTab switch
        {
            QuestTab.Available => "시작 가능한 퀘스트가 없습니다.",
            QuestTab.InProgress => "진행중인 퀘스트가 없습니다.",
            QuestTab.Completed => "완료한 퀘스트가 없습니다.",
            _ => ""
        };

        if (emptyStateText != null)
            emptyStateText.text = message;

        // 상세 정보 패널 숨기기
        selectedQuestId = null;
        if (questDetailPanel != null)
            questDetailPanel.SetActive(false);
    }

    // ==========================================
    // 퀘스트 상세 정보 표시
    // ==========================================
    private void ShowQuestDetail(string questId)
    {
        selectedQuestId = questId;
        QuestData quest = QuestManager.Instance.GetQuestData(questId);

        if (quest == null)
        {
            Debug.LogWarning($"[QuestUI] 퀘스트를 찾을 수 없음: {questId}");
            return;
        }

        if (questDetailPanel != null)
            questDetailPanel.SetActive(true);

        // 퀘스트 이름
        if (questNameText != null)
            questNameText.text = quest.questName;

        // 퀘스트 설명
        if (questDescriptionText != null)
        {
            // description에 \n이 있으면 실제 줄바꿈으로 변환
            string desc = quest.description.Replace("\\n", "\n");
            questDescriptionText.text = $"<b>퀘스트 내용</b>\n{desc}";
        }

        // 퀘스트 목표
        if (questObjectivesText != null)
        {
            string objectives = GetQuestObjectives(quest);
            questObjectivesText.text = $"<b>목표</b>\n{objectives}";
        }

        // 보상
        if (questRewardsText != null)
        {
            string rewards = GetQuestRewards(quest);
            questRewardsText.text = $"<b>보상</b>\n{rewards}";
        }

        // 상태 아이콘
        if (questStatusImage != null)
        {
            questStatusImage.sprite = quest.status switch
            {
                QuestStatus.None or QuestStatus.Offered => availableSprite,
                QuestStatus.Accepted => inProgressSprite,
                QuestStatus.Completed or QuestStatus.Rewarded => completedSprite,
                _ => null
            };
        }

        Debug.Log($"[QuestUI] 퀘스트 상세 정보 표시: {questId}");
    }

    private string GetQuestObjectives(QuestData quest)
    {
        if (quest.objectives == null || quest.objectives.Count == 0)
            return "목표 정보 없음";

        List<string> objectiveTexts = new List<string>();

        foreach (var obj in quest.objectives)
        {
            string status = obj.IsCompleted ? "[완료]" : "[진행중]";
            string typeText = GetObjectiveTypeText(obj.type);
            string progress = $" ({obj.currentCount}/{obj.requiredCount})";

            string objectiveText = $"{status} {typeText}: {obj.targetId}{progress}";

            // ⭐ Dialogue 목표인 경우 NPC 위치 정보 추가 ⭐
            if (obj.type == QuestType.Dialogue && !obj.IsCompleted && NPCInfoManager.Instance != null)
            {
                NPCInfo npcInfo = NPCInfoManager.Instance.GetNPCInfo(obj.targetId);
                if (npcInfo != null)
                {
                    objectiveText += $"\n  → {npcInfo.npcName}: {npcInfo.GetLocationDescription()}";
                }
            }

            objectiveTexts.Add(objectiveText);
        }

        // ⭐ 추가 힌트 표시 ⭐
        string locationHint = quest.GetObjectiveLocationHint();
        if (!string.IsNullOrEmpty(locationHint))
        {
            objectiveTexts.Add($"\n<color=#FFD700>힌트: {locationHint}</color>");
        }

        return string.Join("\n", objectiveTexts);
    }

    private string GetObjectiveTypeText(QuestType type)
    {
        switch (type)
        {
            case QuestType.Dialogue:
                return "대화";
            case QuestType.Kill:
                return "처치";
            case QuestType.Collect:
                return "수집";
            case QuestType.Gather:
                return "채집";
            default:
                return "목표";
        }
    }

    private string GetQuestRewards(QuestData quest)
    {
        List<string> rewardTexts = new List<string>();

        // 경험치 보상
        if (quest.rewardExp > 0)
            rewardTexts.Add($"경험치 +{quest.rewardExp}");

        // 골드 보상
        if (quest.rewardGold > 0)
            rewardTexts.Add($"골드 +{quest.rewardGold}");

        // 아이템 보상
        if (quest.rewards != null && quest.rewards.Count > 0)
        {
            foreach (var reward in quest.rewards)
            {
                rewardTexts.Add($"{reward.itemId} x{reward.quantity}");
            }
        }

        return rewardTexts.Count > 0 ? string.Join("\n", rewardTexts) : "보상 없음";
    }


    // ==========================================
    // 외부에서 호출 가능한 갱신 메서드
    // ==========================================
    public void RefreshUI()
    {
        if (isOpen)
            RefreshQuestList();
    }

    public bool IsQuestUIOpen()
    {
        return isOpen;
    }
}