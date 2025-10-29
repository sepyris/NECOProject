using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// QuestManager (호환성 포함)
/// - 멀티 목표/멀티 보상 구조 지원
/// - 기존 코드에서 사용하던 UpdateKillProgress, UpdateItemProgress, UpdateDialogueProgress 등도 지원 (호환성 메서드)
/// </summary>
public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    // 모든 퀘스트을 ID -> QuestData로 보관
    private Dictionary<string, QuestData> questDictionary = new Dictionary<string, QuestData>();

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
    }

    #region 등록 / 조회
    public void RegisterQuest(QuestData quest)
    {
        if (quest == null || string.IsNullOrEmpty(quest.questId)) return;

        if (!questDictionary.ContainsKey(quest.questId))
        {
            questDictionary.Add(quest.questId, quest);
            Debug.Log($"[QuestManager] 퀘스트 등록: {quest.questId}");
        }
        else
        {
            // 기존 등록된 퀘스트가 있으면 덮어쓰기 없이 로그만 남김
            Debug.LogWarning($"[QuestManager] 이미 등록된 퀘스트입니다: {quest.questId}");
        }
    }

    public QuestData GetQuestData(string questId)
    {
        questDictionary.TryGetValue(questId, out QuestData quest);
        return quest;
    }

    public List<QuestData> GetAllQuests()
    {
        return new List<QuestData>(questDictionary.Values);
    }
    #endregion

    #region 상태 제어 (Offer / Accept / Finalize)
    public QuestStatus GetQuestStatus(string questId)
    {
        var q = GetQuestData(questId);
        return q != null ? q.status : QuestStatus.None;
    }

    public void OfferQuest(string questId)
    {
        var q = GetQuestData(questId);
        if (q == null) { Debug.LogWarning($"[QuestManager] OfferQuest: 퀘스트 없음 {questId}"); return; }

        if (q.status == QuestStatus.None)
        {
            q.status = QuestStatus.Offered;
            Debug.Log($"[QuestManager] Quest Offered: {questId}");
        }
    }

    public void AcceptQuest(string questId)
    {
        var q = GetQuestData(questId);
        if (q == null) { Debug.LogWarning($"[QuestManager] AcceptQuest: 퀘스트 없음 {questId}"); return; }

        if (q.status == QuestStatus.Offered || q.status == QuestStatus.None)
        {
            q.status = QuestStatus.Accepted;
            Debug.Log($"[QuestManager] Quest Accepted: {questId}");
        }
    }

    public void FinalizeQuest(string questId)
    {
        var q = GetQuestData(questId);
        if (q == null) { Debug.LogWarning($"[QuestManager] FinalizeQuest: 퀘스트 없음 {questId}"); return; }

        if (q.status == QuestStatus.Completed)
        {
            // 보상 지급 (여기에 실제 인벤토리/골드 시스템 연동)
            foreach (var r in q.rewards)
            {
                Debug.Log($"[QuestManager] 보상 지급: {r.itemId} x{r.quantity}");
                // 예: InventoryManager.Instance.AddItem(r.itemId, r.quantity);
            }

            q.status = QuestStatus.Rewarded;
            Debug.Log($"[QuestManager] Quest Rewarded: {questId}");
        }
        else
        {
            Debug.Log($"[QuestManager] FinalizeQuest 호출되었지만 상태가 Completed 아님: {questId} 상태={q.status}");
        }
    }
    #endregion

    #region 목표(객체) 업데이트 (핵심)
    /// <summary>
    /// 지정된 퀘스트의 특정 타겟 목표를 업데이트한다.
    /// (새 구조의 기본 메서드)
    /// </summary>
    public void UpdateObjective(string questId, string targetId, int count)
    {
        var q = GetQuestData(questId);
        if (q == null) return;
        if (q.status != QuestStatus.Accepted) return;

        bool anyChanged = false;
        foreach (var obj in q.objectives)
        {
            if (obj.targetId == targetId && obj.currentCount < obj.requiredCount)
            {
                obj.currentCount = Mathf.Min(obj.currentCount + count, obj.requiredCount);
                anyChanged = true;
                Debug.Log($"[QuestManager] [{questId}] 목표 업데이트: {obj.targetId} {obj.currentCount}/{obj.requiredCount}");
            }
        }

        if (anyChanged && q.IsCompleted())
        {
            q.status = QuestStatus.Completed;
            Debug.Log($"[QuestManager] 퀘스트 완료 상태로 전환: {questId}");
        }
    }
    #endregion

    #region 이전 API 호환성 메서드 (다른 스크립트에서 기존 이름을 그대로 사용 가능하게 함)
    // 기존 코드에서 많이 쓰이던 형태들을 지원하기 위해 아래 호환 메서드들을 제공합니다.
    // - UpdateKillProgress(monsterId)
    // - UpdateItemProgress(itemId, amount)
    // - UpdateDialogueProgress(npcId)

    /// <summary>
    /// (호환) 몬스터 처치 업데이트 — 모든 ACCEPTED 퀘스트에서 해당 monsterId 목표를 찾아 +1
    /// </summary>
    public void UpdateKillProgress(string monsterId)
    {
        if (string.IsNullOrEmpty(monsterId)) return;

        foreach (var q in questDictionary.Values)
        {
            if (q.status != QuestStatus.Accepted) continue;

            foreach (var obj in q.objectives)
            {
                if (obj.targetId == monsterId && obj.currentCount < obj.requiredCount)
                {
                    UpdateObjective(q.questId, monsterId, 1);
                    // 한 퀘스트에서 동일 id의 목표를 여러개 가질 가능성 있으므로 continue가 아닌 break는 조심
                    // 여기서는 한 퀘스트에서 동일 target이 여러 목표로 중복 등록되는 케이스는 드물다 가정
                    // 만약 다중 목표 중 여러개를 동시에 올리고 싶다면 break 제거
                }
            }
        }
    }

    /// <summary>
    /// (호환) 아이템 획득 업데이트 — 모든 ACCEPTED 퀘스트에서 해당 itemId 목표를 찾아 amount 만큼 추가
    /// </summary>
    public void UpdateItemProgress(string itemId, int amount = 1)
    {
        if (string.IsNullOrEmpty(itemId) || amount <= 0) return;

        foreach (var q in questDictionary.Values)
        {
            if (q.status != QuestStatus.Accepted) continue;

            foreach (var obj in q.objectives)
            {
                if (obj.targetId == itemId && obj.currentCount < obj.requiredCount)
                {
                    UpdateObjective(q.questId, itemId, amount);
                }
            }
        }
    }

    /// <summary>
    /// (호환) NPC 대화로 인한 진행 업데이트 — 모든 ACCEPTED 퀘스트에서 해당 npcId 목표를 찾아 +1
    /// </summary>
    public void UpdateDialogueProgress(string npcId)
    {
        if (string.IsNullOrEmpty(npcId)) return;

        foreach (var q in questDictionary.Values)
        {
            if (q.status != QuestStatus.Accepted) continue;

            foreach (var obj in q.objectives)
            {
                if (obj.targetId == npcId && obj.currentCount < obj.requiredCount)
                {
                    UpdateObjective(q.questId, npcId, 1);
                }
            }
        }
    }
    #endregion
}
