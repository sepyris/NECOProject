using GameData.Common;
using System.Collections.Generic;
using UnityEditor;
using Definitions;
using UnityEngine;

public class QuestDataManager : MonoBehaviour
{
    public static QuestDataManager Instance { get; private set; }

    [Header("SO 파일")]
    public TextAsset csvFile;
    public QuestDataSO questDatabaseSO;

    public Dictionary<string, QuestData> questList = new();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (questDatabaseSO != null)
            {
#if UNITY_EDITOR
                Debug.LogWarning("[QuestDataManager] Editor 모드에서는 CSV 파일을 다시 빌드합니다.");
                if (csvFile == null) return;

                string directory = System.IO.Path.GetDirectoryName(AssetDatabase.GetAssetPath(csvFile));
                string normalizedPath = directory.Replace('\\', '/');
                string soPath = normalizedPath + "/Database/" + csvFile.name + "Database.asset";
                QuestDataSO database = AssetDatabase.LoadAssetAtPath<QuestDataSO>(soPath);
                if (csvFile.name.Contains(Def_CSV.QUESTS))
                {
                    if (database == null)
                    {
                        database = ScriptableObject.CreateInstance<QuestDataSO>();
                        AssetDatabase.CreateAsset(database, soPath);
                    }
                    database.Items.Clear();
                    string csvText = csvFile.text;
                    List<string> lines = new();
                    
                    string currentLine = "";
                    bool inQuotes = false;

                    // ... (함수 내용 유지) ...
                    for (int i = 0; i < csvText.Length; i++)
                    {
                        char c = csvText[i];

                        if (c == '"')
                        {
                            inQuotes = !inQuotes;
                        }

                        // 줄바꿈 문자 확인
                        if (c == '\n')
                        {
                            // 큰따옴표 밖에 있을 때만 줄바꿈을 레코드의 끝으로 인식합니다.
                            if (!inQuotes)
                            {
                                // 캐리지 리턴(\r)이 포함되어 있다면 제거
                                lines.Add(currentLine.Trim('\r'));
                                currentLine = "";
                                continue;
                            }
                        }

                        currentLine += c;
                    }

                    // 마지막 줄 추가
                    if (!string.IsNullOrEmpty(currentLine.Trim('\r', '\n')))
                    {
                        lines.Add(currentLine.Trim('\r'));
                    }

                    bool skipHeader = true;

                    foreach (var raw in lines)
                    {
                        if (skipHeader) { skipHeader = false; continue; }
                        if (string.IsNullOrWhiteSpace(raw)) continue;

                        List<string> parts = new();
                        inQuotes = false;
                        string current = "";

                        for (int i = 0; i < raw.Length; i++)
                        {
                            char c = raw[i];

                            if (c == '"')
                            {
                                inQuotes = !inQuotes;
                            }
                            else if (c == ',' && !inQuotes)
                            {
                                parts.Add(current);
                                current = "";
                            }
                            else
                            {
                                current += c;
                            }
                        }

                        parts.Add(current);


                        if (parts.Count < 9) continue;

                        QuestData quest = new()
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
                        quest.rewards = new List<ItemReward>();
                        string rewardsStr = parts[6].Trim();
                        if (!string.IsNullOrEmpty(rewardsStr))
                        {
                            var rewards = rewardsStr.Split(';');
                            foreach (var r in rewards)
                            {
                                quest.rewards.Add(new ItemReward(r));
                            }
                        }
                        database.Items.Add(quest);
                    }

                    if (database != null)
                    {
                        EditorUtility.SetDirty(database);
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                        Debug.Log($"CSV 파싱 및 ScriptableObject **{soPath}** 생성 완료! ({database.Items.Count}개 데이터)");
                    }
                    else
                    {
                        Debug.LogWarning($"변환할 수 없는 CSV 파일입니다");
                    }
                }
                BuildDictionary(database);
#else
                BuildDictionary(questDatabaseSO);
#endif
            }
            else
            {
                Debug.LogError("[QuestDataManager] CSV 파일이 할당되지 않았습니다.");
            }

            RegisterAll();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void BuildDictionary(QuestDataSO database)
    {
        questList.Clear();
        foreach (var item in database.Items)
        {
            if (!questList.ContainsKey(item.questId))
            {
                questList.Add(item.questId, item);
            }
            else
            {
                Debug.LogWarning($"[ItemDataManager] 중복 ID 발견 (SO): {item.questId}");
            }
        }
        Debug.Log($"[ItemDataManager] ScriptableObject에서 {questList.Count}개의 아이템 로드 완료");
    }

    void RegisterAll()
    {
        foreach (var quest in questList)
        {
            QuestManager.Instance.RegisterQuest(quest.Value);
        }
    }

    // 수락 가능한 퀘스트 목록 가져오기
    public Dictionary<string, QuestData> GetAvailableQuests()
    {
        return questList;
    }
    /// <summary>
    /// 퀘스트 ID로 데이터 가져오기
    /// </summary>
    public QuestData GetGatherableData(string questid)
    {
        if (questList.TryGetValue(questid, out QuestData data))
        {
            return data;
        }

        Debug.LogWarning($"[QuestDataManager] 퀘스트를 찾을 수 없음: {questid}");
        return null;
    }
}