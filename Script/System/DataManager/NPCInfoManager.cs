using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI.Table;

[System.Serializable]
public class NPCInfo
{
    public string npcId;
    public string npcName;
    public string npcTitle;
    public string npcDescription;

    // ⭐ 위치 정보 추가 ⭐
    public string mapId;         // NPC가 있는 맵
    public Vector2 position;     // 맵 내 위치 (posX, posY)

    /// <summary>
    /// NPC 위치 정보를 포함한 설명
    /// </summary>
    public string GetLocationDescription()
    {
        if (MapInfoManager.Instance != null && !string.IsNullOrEmpty(mapId))
        {
            string mapName = MapInfoManager.Instance.GetMapName(mapId);
            return $"{mapName}에 위치";
        }
        return "위치 정보 없음";
    }
}

public class NPCInfoManager : MonoBehaviour
{
    public static NPCInfoManager Instance { get; private set; }

    [Header("CSV 파일")]
    public TextAsset npcInfoCsvFile;

    private Dictionary<string, NPCInfo> npcInfoDictionary = new Dictionary<string, NPCInfo>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (npcInfoCsvFile != null)
                LoadNPCInfoFromCSV(npcInfoCsvFile.text);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void LoadNPCInfoFromCSV(string csvText)
    {
        var lines = CSVUtility.GetLinesFromCSV(csvText);
        bool skipHeader = true;

        foreach (var raw in lines)
        {
            if (skipHeader) { skipHeader = false; continue; }
            if (string.IsNullOrWhiteSpace(raw)) continue;

            var parts = CSVUtility.SplitCSVLine(raw);
            if (parts.Count < 4) continue;

            string npcId = parts[0].Trim();
            if (string.IsNullOrEmpty(npcId)) continue;

            NPCInfo info = new NPCInfo
            {
                npcId = npcId,
                npcName = parts[1].Trim(),
                npcTitle = parts[2].Trim(),
                npcDescription = parts[3].Trim()
            };

            if (!npcInfoDictionary.ContainsKey(npcId))
            {
                npcInfoDictionary.Add(npcId, info);
                Debug.Log($"[NPCInfoManager] NPC 로드: {npcId} - {info.npcName}");
            }
        }

        Debug.Log($"[NPCInfoManager] CSV에서 {npcInfoDictionary.Count}개의 NPC 정보 로드 완료");
    }

    /// <summary>
    /// NPC ID로 이름 가져오기
    /// </summary>
    public string GetNPCName(string npcId)
    {
        if (npcInfoDictionary.TryGetValue(npcId, out NPCInfo info))
        {
            return info.npcName;
        }

        Debug.LogWarning($"[NPCInfoManager] NPC 정보를 찾을 수 없음: {npcId}");
        return npcId; // 이름을 못 찾으면 ID 반환
    }

    /// <summary>
    /// NPC ID로 전체 정보 가져오기
    /// </summary>
    public NPCInfo GetNPCInfo(string npcId)
    {
        npcInfoDictionary.TryGetValue(npcId, out NPCInfo info);
        return info;
    }

    /// <summary>
    /// 타이틀 포함한 전체 이름 가져오기 (예: "헨리 (마을 상인)")
    /// </summary>
    public string GetNPCNameWithTitle(string npcId)
    {
        if (npcInfoDictionary.TryGetValue(npcId, out NPCInfo info))
        {
            if (!string.IsNullOrEmpty(info.npcTitle))
            {
                return $"{info.npcName} ({info.npcTitle})";
            }
            return info.npcName;
        }

        return npcId;
    }
}