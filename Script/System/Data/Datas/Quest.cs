using Definitions;
using GameData.Common;
using System;
using System.Collections.Generic;
using UnityEngine;

public class QuestDataSO : ScriptableObject
{
    // Dictionary 대신 직렬화 가능한 List를 사용합니다. (Unity는 Dictionary를 Inspector에서 표시하지 못함)
    // 데이터 접근을 빠르게 하기 위해 런타임에 List를 Dictionary로 변환할 것입니다.
    public List<QuestData> Items = new();
}

[System.Serializable]
public class QuestData
{
    public string questId;
    public string questName;
    [TextArea] public string description;

    public QuestPrerequisite prerequisite = new();
    public List<QuestObjective> objectives = new();

    public int rewardGold;
    public int rewardExp;

    // ItemReward로 통합
    public List<ItemReward> rewards = new();

    public QuestStatus status = QuestStatus.None;

    public bool CanAccept()
    {
        if (status != QuestStatus.None && status != QuestStatus.Offered)
            return false;

        return prerequisite.IsMet();
    }

    public string GetObjectiveLocationHint()
    {
        if (objectives == null || objectives.Count == 0)
            return "";

        List<string> hints = new();

        foreach (var obj in objectives)
        {
            if (obj.IsCompleted) continue;

            if (obj.type == QuestType.Dialogue && NPCInfoManager.Instance != null)
            {
                Npcs npcInfo = NPCInfoManager.Instance.GetNPCInfo(obj.targetId);
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

// RewardItem 클래스 삭제 (ItemReward로 대체)

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