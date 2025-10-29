using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI.Table;

public class QuestDataManager : MonoBehaviour
{
    public static QuestDataManager Instance { get; private set; }

    public List<QuestData> questList = new List<QuestData>();
    public TextAsset csvFile;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            if (csvFile != null)
                ParseCSV(csvFile.text);
            RegisterAll();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void ParseCSV(string csvText)
    {
        var lines = CSVUtility.GetLinesFromCSV(csvText);
        bool skipHeader = true;

        foreach (var raw in lines)
        {
            if (skipHeader) { skipHeader = false; continue; }
            if (string.IsNullOrWhiteSpace(raw)) continue;

            var parts = CSVUtility.SplitCSVLine(raw);
            if (parts.Count < 9) continue;

            QuestData quest = new QuestData
            {
                questId = parts[0].Trim(),
                questName = parts[1].Trim(),
                description = parts[2].Trim().Replace("\\n", "\n"),
                rewardExp = int.Parse(parts[7].Trim()),
                rewardGold = int.Parse(parts[8].Trim())
            };

            // 선행 조건 파싱
            string prereqType = parts[3].Trim();
            string prereqValue = parts[4].Trim();

            if (!string.IsNullOrEmpty(prereqType) && prereqType != "None")
            {
                if (System.Enum.TryParse(prereqType, out PrerequisiteType pType))
                {
                    quest.prerequisite.type = pType;
                    quest.prerequisite.value = prereqValue;
                }
            }

            // Objectives 파싱
            quest.objectives = new List<QuestObjective>();
            string objectivesStr = parts[5].Trim();
            if (!string.IsNullOrEmpty(objectivesStr))
            {
                var objectives = objectivesStr.Split(';');
                foreach (var obj in objectives)
                {
                    var seg = obj.Split(':');
                    if (seg.Length < 3) continue;

                    if (System.Enum.TryParse(seg[0].Trim(), out QuestType qType))
                    {
                        quest.objectives.Add(new QuestObjective
                        {
                            type = qType,
                            targetId = seg[1].Trim(),
                            requiredCount = int.Parse(seg[2].Trim()),
                            currentCount = 0
                        });
                    }
                }
            }

            // Reward Items 파싱
            quest.rewards = new List<RewardItem>();
            string rewardsStr = parts[6].Trim();
            if (!string.IsNullOrEmpty(rewardsStr))
            {
                var rewards = rewardsStr.Split(';');
                foreach (var r in rewards)
                {
                    var seg = r.Split(':');
                    if (seg.Length >= 3 && seg[0].Trim() == "Item")
                    {
                        string itemId = seg[1].Trim();
                        int count = int.Parse(seg[2].Trim());
                        quest.rewards.Add(new RewardItem { itemId = itemId, quantity = count });
                    }
                }
            }

            questList.Add(quest);
            Debug.Log($"[QuestDataManager] 퀘스트 로드: {quest.questId} (조건: {prereqType})");
        }

        Debug.Log($"[QuestDataManager] CSV에서 {questList.Count}개의 퀘스트 로드 완료");
    }

    void RegisterAll()
    {
        foreach (var quest in questList)
        {
            QuestManager.Instance.RegisterQuest(quest);
        }
    }

    // 수락 가능한 퀘스트 목록 가져오기
    public List<QuestData> GetAvailableQuests()
    {
        List<QuestData> available = new List<QuestData>();
        foreach (var quest in questList)
        {
            if (quest.CanAccept())
            {
                available.Add(quest);
            }
        }
        return available;
    }
}