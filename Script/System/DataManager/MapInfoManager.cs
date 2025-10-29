using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI.Table;

[System.Serializable]
public class MapInfo
{
    public string mapId;
    public string mapName;
    public string mapType;          // Town, Forest, Dungeon, etc.
    public string mapDescription;
    public string parentMapId;      // ���� �� (���� ������)

    // ��Ÿ�� ������ (���߿� �߰�)
    public Vector3 spawnPoint;      // �� ���� ��ġ
    public List<string> connectedMaps = new List<string>(); // ����� �ʵ�
}

public enum MapType
{
    Town,
    Forest,
    Dungeon,
    Cave,
    Field
}

public class MapInfoManager : MonoBehaviour
{
    public static MapInfoManager Instance { get; private set; }

    [Header("CSV ����")]
    public TextAsset mapInfoCsvFile;

    [Header("���� �� ����")]
    public string currentMapId = "map_town_center";

    private Dictionary<string, MapInfo> mapInfoDictionary = new Dictionary<string, MapInfo>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (mapInfoCsvFile != null)
                LoadMapInfoFromCSV(mapInfoCsvFile.text);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void LoadMapInfoFromCSV(string csvText)
    {
        var lines = CSVUtility.GetLinesFromCSV(csvText);
        bool skipHeader = true;

        foreach (var raw in lines)
        {
            if (skipHeader) { skipHeader = false; continue; }
            if (string.IsNullOrWhiteSpace(raw)) continue;

            var parts = CSVUtility.SplitCSVLine(raw);
            if (parts.Count < 5) continue;

            string mapId = parts[0].Trim();
            if (string.IsNullOrEmpty(mapId)) continue;

            MapInfo info = new MapInfo
            {
                mapId = mapId,
                mapName = parts[1].Trim(),
                mapType = parts[2].Trim(),
                mapDescription = parts[3].Trim(),
                parentMapId = parts[4].Trim()
            };

            if (!mapInfoDictionary.ContainsKey(mapId))
            {
                mapInfoDictionary.Add(mapId, info);
                Debug.Log($"[MapInfoManager] �� �ε�: {mapId} - {info.mapName}");
            }
        }

        Debug.Log($"[MapInfoManager] CSV���� {mapInfoDictionary.Count}���� �� ���� �ε� �Ϸ�");
    }

    /// <summary>
    /// �� ID�� �� �̸� ��������
    /// </summary>
    public string GetMapName(string mapId)
    {
        if (mapInfoDictionary.TryGetValue(mapId, out MapInfo info))
        {
            return info.mapName;
        }

        Debug.LogWarning($"[MapInfoManager] �� ������ ã�� �� ����: {mapId}");
        return mapId;
    }

    /// <summary>
    /// �� ID�� ��ü ���� ��������
    /// </summary>
    public MapInfo GetMapInfo(string mapId)
    {
        mapInfoDictionary.TryGetValue(mapId, out MapInfo info);
        return info;
    }

    /// <summary>
    /// ���� �� �̸� ��������
    /// </summary>
    public string GetCurrentMapName()
    {
        return GetMapName(currentMapId);
    }

    /// <summary>
    /// �� ���� (�� ��ȯ �� ȣ��)
    /// </summary>
    public void ChangeMap(string newMapId)
    {
        if (mapInfoDictionary.ContainsKey(newMapId))
        {
            string oldMapName = GetMapName(currentMapId);
            currentMapId = newMapId;
            string newMapName = GetMapName(currentMapId);

            Debug.Log($"[MapInfoManager] �� �̵�: {oldMapName} �� {newMapName}");
        }
        else
        {
            Debug.LogWarning($"[MapInfoManager] �������� �ʴ� ��: {newMapId}");
        }
    }

    /// <summary>
    /// ��� �� ��� �������� (Ư�� Ÿ��)
    /// </summary>
    public List<MapInfo> GetMapsByType(string mapType)
    {
        List<MapInfo> maps = new List<MapInfo>();

        foreach (var map in mapInfoDictionary.Values)
        {
            if (map.mapType == mapType)
            {
                maps.Add(map);
            }
        }

        return maps;
    }

    /// <summary>
    /// �� �� ���� ��� ã�� (������ ���� ���� ���)
    /// </summary>
    public List<string> FindPathBetweenMaps(string fromMapId, string toMapId)
    {
        List<string> path = new List<string>();

        // ����� ������ ���� - ���߿� A* �˰��� ������ ���� ����
        MapInfo fromMap = GetMapInfo(fromMapId);
        MapInfo toMap = GetMapInfo(toMapId);

        if (fromMap == null || toMap == null)
        {
            Debug.LogWarning($"[MapInfoManager] ��� Ž�� ����: {fromMapId} �� {toMapId}");
            return path;
        }

        // ���� ���̸� ��� ����
        if (fromMapId == toMapId)
        {
            return path;
        }

        // ������ ����: �θ� ���� ���İ��� ���
        path.Add(fromMapId);

        // fromMap���� ���� �θ�� �ö󰡱�
        string currentId = fromMapId;
        while (!string.IsNullOrEmpty(currentId))
        {
            MapInfo current = GetMapInfo(currentId);
            if (current == null) break;

            if (!string.IsNullOrEmpty(current.parentMapId))
            {
                path.Add(current.parentMapId);
                currentId = current.parentMapId;
            }
            else
            {
                break;
            }
        }

        // toMap������ ��� �߰� (����)
        List<string> toPath = new List<string>();
        currentId = toMapId;
        while (!string.IsNullOrEmpty(currentId))
        {
            MapInfo current = GetMapInfo(currentId);
            if (current == null) break;

            toPath.Insert(0, currentId);

            if (!string.IsNullOrEmpty(current.parentMapId))
            {
                currentId = current.parentMapId;
            }
            else
            {
                break;
            }
        }

        // ���� �θ� ã�Ƽ� �ߺ� ����
        // (�����δ� �� ������ �˰��� �ʿ�)
        foreach (var mapId in toPath)
        {
            if (!path.Contains(mapId))
            {
                path.Add(mapId);
            }
        }

        return path;
    }
}