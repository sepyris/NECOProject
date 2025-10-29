using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// CSV ���Ͽ��� ä���� �����͸� �ε��ϰ� �����ϴ� �̱��� �Ŵ���
/// </summary>
public class GatherableDataManager : MonoBehaviour
{
    public static GatherableDataManager Instance { get; private set; }

    [Header("CSV ����")]
    public TextAsset csvFile; // CSV ������ Inspector���� �Ҵ�

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
                Debug.LogWarning("[GatherableDataManager] CSV ������ �Ҵ���� �ʾҽ��ϴ�.");
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// CSV ���� �Ľ�
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

            // CSV ����: ID,�̸�,����,ä������,ä���ӵ�,������������̺�
            if (parts.Count < 6) continue;

            GatherableData gatherable = new GatherableData
            {
                gatherableID = parts[0].Trim(),
                gatherableName = parts[1].Trim(),
                description = parts[2].Trim(),
                requiredTool = ParseGatherTool(parts[3].Trim()),
                gatherTime = ParseFloat(parts[4].Trim(), 1.0f)
            };

            // ���� ������ ���̺� �Ľ�
            if (parts.Count > 5 && !string.IsNullOrWhiteSpace(parts[5]))
            {
                gatherable.dropItems = ParseDropTable(parts[5].Trim());
            }

            // �����ͺ��̽��� �߰�
            if (!gatherableDatabase.ContainsKey(gatherable.gatherableID))
            {
                gatherableDatabase.Add(gatherable.gatherableID, gatherable);
            }
            else
            {
                Debug.LogWarning($"[GatherableDataManager] �ߺ� ä���� ID: {gatherable.gatherableID}");
            }
        }

        Debug.Log($"[GatherableDataManager] CSV���� {gatherableDatabase.Count}���� ä���� �ε� �Ϸ�");
    }

    /// <summary>
    /// ���� ������ ���̺� �Ľ�
    /// ����: ������ID:Ȯ��:����;������ID:Ȯ��:����;...
    /// </summary>
    private List<DropItem> ParseDropTable(string tableStr)
    {
        List<DropItem> Drops = new List<DropItem>();

        if (string.IsNullOrWhiteSpace(tableStr))
            return Drops;

        // �����ݷ����� �� ������ �и�
        string[] items = tableStr.Split(';');

        foreach (string itemStr in items)
        {
            if (string.IsNullOrWhiteSpace(itemStr))
                continue;

            // �ݷ����� ������ID:Ȯ��:���� �и�
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
                Debug.LogWarning($"[GatherableDataManager] �߸��� ���� ������ ����: {itemStr}");
            }
        }

        return Drops;
    }

    /// <summary>
    /// ä�� ���� Ÿ�� �Ľ�
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
                Debug.LogWarning($"[GatherableDataManager] �� �� ���� ä�� ���� Ÿ��: {toolStr}");
                return GatherToolType.None;
        }
    }

    // ==========================================
    // ������ �Ľ� ���� �޼���
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
    // ��ȸ �޼���
    // ==========================================

    /// <summary>
    /// ä���� ID�� ������ ��������
    /// </summary>
    public GatherableData GetGatherableData(string gatherableID)
    {
        if (gatherableDatabase.TryGetValue(gatherableID, out GatherableData data))
        {
            return data;
        }

        Debug.LogWarning($"[GatherableDataManager] ä������ ã�� �� ����: {gatherableID}");
        return null;
    }

    /// <summary>
    /// ��� ä���� ������ ��������
    /// </summary>
    public Dictionary<string, GatherableData> GetAllGatherables()
    {
        return gatherableDatabase;
    }

    /// <summary>
    /// Ư�� ������ �ʿ��� ä������ ��������
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