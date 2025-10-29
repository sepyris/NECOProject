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
    public string parentMapId;      // 상위 맵 (계층 구조용)

    // 런타임 데이터 (나중에 추가)
    public Vector3 spawnPoint;      // 맵 진입 위치
    public List<string> connectedMaps = new List<string>(); // 연결된 맵들
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

    [Header("CSV 파일")]
    public TextAsset mapInfoCsvFile;

    [Header("현재 맵 정보")]
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
                Debug.Log($"[MapInfoManager] 맵 로드: {mapId} - {info.mapName}");
            }
        }

        Debug.Log($"[MapInfoManager] CSV에서 {mapInfoDictionary.Count}개의 맵 정보 로드 완료");
    }

    /// <summary>
    /// 맵 ID로 맵 이름 가져오기
    /// </summary>
    public string GetMapName(string mapId)
    {
        if (mapInfoDictionary.TryGetValue(mapId, out MapInfo info))
        {
            return info.mapName;
        }

        Debug.LogWarning($"[MapInfoManager] 맵 정보를 찾을 수 없음: {mapId}");
        return mapId;
    }

    /// <summary>
    /// 맵 ID로 전체 정보 가져오기
    /// </summary>
    public MapInfo GetMapInfo(string mapId)
    {
        mapInfoDictionary.TryGetValue(mapId, out MapInfo info);
        return info;
    }

    /// <summary>
    /// 현재 맵 이름 가져오기
    /// </summary>
    public string GetCurrentMapName()
    {
        return GetMapName(currentMapId);
    }

    /// <summary>
    /// 맵 변경 (씬 전환 시 호출)
    /// </summary>
    public void ChangeMap(string newMapId)
    {
        if (mapInfoDictionary.ContainsKey(newMapId))
        {
            string oldMapName = GetMapName(currentMapId);
            currentMapId = newMapId;
            string newMapName = GetMapName(currentMapId);

            Debug.Log($"[MapInfoManager] 맵 이동: {oldMapName} → {newMapName}");
        }
        else
        {
            Debug.LogWarning($"[MapInfoManager] 존재하지 않는 맵: {newMapId}");
        }
    }

    /// <summary>
    /// 모든 맵 목록 가져오기 (특정 타입)
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
    /// 두 맵 간의 경로 찾기 (간단한 계층 구조 기반)
    /// </summary>
    public List<string> FindPathBetweenMaps(string fromMapId, string toMapId)
    {
        List<string> path = new List<string>();

        // 현재는 간단한 구현 - 나중에 A* 알고리즘 등으로 개선 가능
        MapInfo fromMap = GetMapInfo(fromMapId);
        MapInfo toMap = GetMapInfo(toMapId);

        if (fromMap == null || toMap == null)
        {
            Debug.LogWarning($"[MapInfoManager] 경로 탐색 실패: {fromMapId} → {toMapId}");
            return path;
        }

        // 같은 맵이면 경로 없음
        if (fromMapId == toMapId)
        {
            return path;
        }

        // 간단한 구현: 부모 맵을 거쳐가는 경로
        path.Add(fromMapId);

        // fromMap에서 공통 부모로 올라가기
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

        // toMap까지의 경로 추가 (역순)
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

        // 공통 부모 찾아서 중복 제거
        // (실제로는 더 정교한 알고리즘 필요)
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