using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class QuestData
{
    public string questId;
    public string questName;
    [TextArea] public string description;

    // 퀘스트 수락 조건 추가
    public QuestPrerequisite prerequisite = new QuestPrerequisite();

    public List<QuestObjective> objectives = new List<QuestObjective>();
    public int rewardGold;
    public int rewardExp;
    public List<RewardItem> rewards = new List<RewardItem>();
    public QuestStatus status = QuestStatus.None;

    // 퀘스트를 수락할 수 있는지 체크
    public bool CanAccept()
    {
        if (status != QuestStatus.None && status != QuestStatus.Offered)
            return false;

        return prerequisite.IsMet();
    }

    /// <summary>
    /// ⭐ 퀘스트 목표 NPC의 위치 정보 가져오기 ⭐
    /// </summary>
    public string GetObjectiveLocationHint()
    {
        if (objectives == null || objectives.Count == 0)
            return "";

        List<string> hints = new List<string>();

        foreach (var obj in objectives)
        {
            if (obj.IsCompleted) continue;

            // Dialogue 목표인 경우 NPC 위치 정보 추가
            if (obj.type == QuestType.Dialogue && NPCInfoManager.Instance != null)
            {
                NPCInfo npcInfo = NPCInfoManager.Instance.GetNPCInfo(obj.targetId);
                if (npcInfo != null)
                {
                    string npcName = npcInfo.npcName;
                    string location = npcInfo.GetLocationDescription();
                    hints.Add($"{npcName}을(를) 찾으세요 ({location})");
                }
            }
        }

        return hints.Count > 0 ? string.Join("\n", hints) : "";
    }
    public bool IsCompleted()
    {
        foreach (var obj in objectives)
        {
            if (!obj.IsCompleted)
                return false;
        }
        return true;
    }
}

[Serializable]
public class QuestPrerequisite
{
    public PrerequisiteType type = PrerequisiteType.None;
    public string value = "";

    public bool IsMet()
    {
        if (type == PrerequisiteType.None || string.IsNullOrEmpty(value))
            return true;

        switch (type)
        {
            case PrerequisiteType.Level:
                if (int.TryParse(value, out int reqLevel))
                {
                    // TODO: 실제 플레이어 레벨 체크
                    // return PlayerController.Instance.Level >= reqLevel;
                    return true; // 임시
                }
                break;

            case PrerequisiteType.Item:
                // TODO: 실제 인벤토리 체크
                // return InventoryManager.Instance.HasItem(value);
                return true; // 임시

            case PrerequisiteType.QuestStatus:
                var parts = value.Split(':');
                if (parts.Length == 2)
                {
                    string questId = parts[0];
                    if (System.Enum.TryParse(parts[1], out QuestStatus status))
                    {
                        return QuestManager.Instance.GetQuestStatus(questId) == status;
                    }
                }
                break;

            case PrerequisiteType.MultipleQuests:
                // 여러 퀘스트를 동시에 체크 (Quest_001:Completed,Quest_002:Completed)
                var quests = value.Split(',');
                foreach (var quest in quests)
                {
                    var qParts = quest.Split(':');
                    if (qParts.Length == 2)
                    {
                        string qId = qParts[0].Trim();
                        if (System.Enum.TryParse(qParts[1], out QuestStatus qStatus))
                        {
                            if (QuestManager.Instance.GetQuestStatus(qId) != qStatus)
                                return false;
                        }
                    }
                }
                return true;
        }

        return false;
    }
}

[Serializable]
public class QuestObjective
{
    public QuestType type;
    public string targetId;
    public int requiredCount;
    public int currentCount;
    public bool IsCompleted => currentCount >= requiredCount;
}

[Serializable]
public class RewardItem
{
    public string itemId;
    public int quantity;
}

public enum QuestStatus
{
    None,
    Offered,
    Accepted,
    Completed,
    Rewarded
}

public enum QuestType
{
    Dialogue,
    Kill,
    Collect,
    Gather
}

public enum PrerequisiteType
{
    None,
    Level,
    Item,
    QuestStatus,
    MultipleQuests  // 여러 퀘스트 조건
}