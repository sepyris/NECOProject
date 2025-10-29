using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI.Table;

[System.Serializable]
public class DialogueLine
{
    public string Text;

    // ⭐ Speaker는 런타임에 npcId로부터 NPCInfoManager를 통해 가져옴 ⭐
    public string GetSpeakerName(string npcId)
    {
        if (NPCInfoManager.Instance != null)
        {
            return NPCInfoManager.Instance.GetNPCName(npcId);
        }
        return npcId; // NPCInfoManager가 없으면 npcId 그대로 반환
    }
}

[System.Serializable]
public class DialogueSequence
{
    public string npcId;
    public string dialogueType;
    public string questId;
    public List<DialogueLine> lines = new List<DialogueLine>();
}

public class DialogueDataManager : MonoBehaviour
{
    public static DialogueDataManager Instance { get; private set; }

    [Header("CSV 파일")]
    public TextAsset dialogueCsvFile;

    private List<DialogueSequence> allDialogues = new List<DialogueSequence>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (dialogueCsvFile != null)
                LoadDialoguesFromCSV(dialogueCsvFile.text);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void LoadDialoguesFromCSV(string csvText)
    {
        var lines = CSVUtility.GetLinesFromCSV(csvText);
        bool skipHeader = true;
        DialogueSequence currentSequence = null;

        foreach (var raw in lines)
        {
            if (skipHeader) { skipHeader = false; continue; }
            if (string.IsNullOrWhiteSpace(raw)) continue;

            string trimmed = raw.TrimStart();
            if (trimmed.StartsWith("#")) continue;

            // ⭐ CSV 구조: npcId, DialogueType, QuestID, Text (4개 컬럼) ⭐
            var parts = CSVUtility.SplitCSVLine(raw);
            if (parts.Count < 4) continue;

            string npcId = parts[0].Trim();
            string dialogueType = parts[1].Trim();
            string questId = parts[2].Trim();
            string text = parts[3].Trim();

            // 새로운 시퀀스 시작 (npcId와 dialogueType이 모두 있는 경우)
            if (!string.IsNullOrEmpty(npcId) && !string.IsNullOrEmpty(dialogueType))
            {
                currentSequence = new DialogueSequence
                {
                    npcId = npcId,
                    dialogueType = dialogueType,
                    questId = questId
                };
                allDialogues.Add(currentSequence);

                Debug.Log($"[DialogueDataManager] 시퀀스 로드: NPC={npcId}, Type={dialogueType}, QuestID={questId}");
            }

            // 대사 추가 (text가 비어있지 않으면)
            // ⭐ Speaker 정보는 저장하지 않음 - 런타임에 npcId로 조회 ⭐
            if (currentSequence != null && !string.IsNullOrEmpty(text))
            {
                currentSequence.lines.Add(new DialogueLine
                {
                    Text = text
                });
            }
        }

        Debug.Log($"[DialogueDataManager] CSV에서 {allDialogues.Count}개의 대화 시퀀스 로드 완료");
    }

    // questId 없이 검색 (기본 대화용)
    public List<DialogueLine> GetDialogueSequence(string npcId, string dialogueType)
    {
        foreach (var seq in allDialogues)
        {
            if (seq.npcId == npcId &&
                seq.dialogueType == dialogueType &&
                string.IsNullOrEmpty(seq.questId))
            {
                return seq.lines;
            }
        }

        Debug.LogWarning($"[DialogueDataManager] 대화 못 찾음: NPC={npcId}, Type={dialogueType}");
        return null;
    }

    // questId와 함께 검색 (퀘스트 관련 대화용)
    public List<DialogueLine> GetDialogueSequence(string npcId, string dialogueType, string questId)
    {
        foreach (var seq in allDialogues)
        {
            if (seq.npcId == npcId &&
                seq.dialogueType == dialogueType &&
                seq.questId == questId)
            {
                return seq.lines;
            }
        }

        Debug.LogWarning($"[DialogueDataManager] 대화 못 찾음: NPC={npcId}, Type={dialogueType}, QuestID={questId}");
        return null;
    }
}