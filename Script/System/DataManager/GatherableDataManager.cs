using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// CSV 파일에서 채집물 데이터를 로드하고 관리하는 싱글톤 매니저
/// </summary>
public class GatherableDataManager : MonoBehaviour
{
    public static GatherableDataManager Instance { get; private set; }

    [Header("CSV 파일")]
    public TextAsset csvFile; // CSV 파일을 Inspector에서 할당

    private Dictionary<string, GatherableData> gatherableDatabase = new Dictionary<string, GatherableData>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (csvFile != null)
            {
                ParseCSV(csvFile.text);
            }
            else
            {
                Debug.LogWarning("[GatherableDataManager] CSV 파일이 할당되지 않았습니다.");
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// CSV 내용 파싱
    /// </summary>
    void ParseCSV(string csvText)
    {
        gatherableDatabase.Clear();

        var lines = CSVUtility.GetLinesFromCSV(csvText);
        bool skipHeader = true;

        foreach (var raw in lines)
        {
            if (skipHeader) { skipHeader = false; continue; }
            if (string.IsNullOrWhiteSpace(raw)) continue;

            string trimmed = raw.TrimStart();
            if (trimmed.StartsWith("#")) continue;

            var parts = CSVUtility.SplitCSVLine(raw);

            // CSV 구조: ID,이름,설명,채집도구,채집속도,보상아이템테이블
            if (parts.Count < 6) continue;

            GatherableData gatherable = new GatherableData
            {
                gatherableID = parts[0].Trim(),
                gatherableName = parts[1].Trim(),
                description = parts[2].Trim(),
                requiredTool = ParseGatherTool(parts[3].Trim()),
                gatherTime = ParseFloat(parts[4].Trim(), 1.0f)
            };

            // 보상 아이템 테이블 파싱
            if (parts.Count > 5 && !string.IsNullOrWhiteSpace(parts[5]))
            {
                gatherable.dropItems = ParseDropTable(parts[5].Trim());
            }

            // 데이터베이스에 추가
            if (!gatherableDatabase.ContainsKey(gatherable.gatherableID))
            {
                gatherableDatabase.Add(gatherable.gatherableID, gatherable);
            }
            else
            {
                Debug.LogWarning($"[GatherableDataManager] 중복 채집물 ID: {gatherable.gatherableID}");
            }
        }

        Debug.Log($"[GatherableDataManager] CSV에서 {gatherableDatabase.Count}개의 채집물 로드 완료");
    }

    /// <summary>
    /// 보상 아이템 테이블 파싱
    /// 형식: 아이템ID:확률:개수;아이템ID:확률:개수;...
    /// </summary>
    private List<DropItem> ParseDropTable(string tableStr)
    {
        List<DropItem> Drops = new List<DropItem>();

        if (string.IsNullOrWhiteSpace(tableStr))
            return Drops;

        // 세미콜론으로 각 아이템 분리
        string[] items = tableStr.Split(';');

        foreach (string itemStr in items)
        {
            if (string.IsNullOrWhiteSpace(itemStr))
                continue;

            // 콜론으로 아이템ID:확률:개수 분리
            string[] itemParts = itemStr.Trim().Split(':');

            if (itemParts.Length >= 3)
            {
                DropItem Drop = new DropItem
                {
                    itemID = itemParts[0].Trim(),
                    dropRate = ParseFloat(itemParts[1].Trim(), 0f),
                    quantity = ParseInt(itemParts[2].Trim(), 1)
                };

                Drops.Add(Drop);
            }
            else
            {
                Debug.LogWarning($"[GatherableDataManager] 잘못된 보상 아이템 형식: {itemStr}");
            }
        }

        return Drops;
    }

    /// <summary>
    /// 채집 도구 타입 파싱
    /// </summary>
    private GatherToolType ParseGatherTool(string toolStr)
    {
        switch (toolStr.ToLower())
        {
            case "pickaxe":
                return GatherToolType.Pickaxe;
            case "sickle":
                return GatherToolType.Sickle;
            case "fishingrod":
                return GatherToolType.FishingRod;
            case "axe":
                return GatherToolType.Axe;
            case "none":
                return GatherToolType.None;
            default:
                Debug.LogWarning($"[GatherableDataManager] 알 수 없는 채집 도구 타입: {toolStr}");
                return GatherToolType.None;
        }
    }

    // ==========================================
    // 안전한 파싱 헬퍼 메서드
    // ==========================================

    private int ParseInt(string str, int defaultValue = 0)
    {
        if (int.TryParse(str, out int result))
            return result;
        return defaultValue;
    }

    private float ParseFloat(string str, float defaultValue = 0f)
    {
        if (float.TryParse(str, out float result))
            return result;
        return defaultValue;
    }

    // ==========================================
    // 조회 메서드
    // ==========================================

    /// <summary>
    /// 채집물 ID로 데이터 가져오기
    /// </summary>
    public GatherableData GetGatherableData(string gatherableID)
    {
        if (gatherableDatabase.TryGetValue(gatherableID, out GatherableData data))
        {
            return data;
        }

        Debug.LogWarning($"[GatherableDataManager] 채집물을 찾을 수 없음: {gatherableID}");
        return null;
    }

    /// <summary>
    /// 모든 채집물 데이터 가져오기
    /// </summary>
    public Dictionary<string, GatherableData> GetAllGatherables()
    {
        return gatherableDatabase;
    }

    /// <summary>
    /// 특정 도구가 필요한 채집물만 가져오기
    /// </summary>
    public List<GatherableData> GetGatherablesByTool(GatherToolType tool)
    {
        List<GatherableData> result = new List<GatherableData>();

        foreach (var gatherable in gatherableDatabase.Values)
        {
            if (gatherable.requiredTool == tool)
            {
                result.Add(gatherable);
            }
        }

        return result;
    }
}