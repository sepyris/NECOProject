using UnityEditor.U2D.Aseprite;
using UnityEngine;

/// <summary>
/// 맵 컨트롤러 - 각 씬에 배치하여 맵 정보 설정
/// MapInfoManager로부터 데이터를 자동으로 로드
/// </summary>
public class MapController : MonoBehaviour
{
    [Header("Map ID")]
    [SerializeField] private string mapId; // Inspector에서 설정

    [Header("Map Info (Read Only)")]
    [SerializeField] private string mapName;
    [SerializeField] private string mapType;
    [SerializeField] private string mapDescription;

    private Maps mapData;

    void Start()
    {
        LoadMapData();
        RegisterToManager();
    }

    /// <summary>
    /// CSV에서 맵 데이터 로드
    /// </summary>
    private void LoadMapData()
    {
        if (string.IsNullOrEmpty(mapId))
        {
            Debug.LogWarning("[MapController] 맵 ID가 설정되지 않았습니다.");
            return;
        }

        if (MapInfoManager.Instance == null)
        {
            Debug.LogError("[MapController] MapInfoManager가 없습니다!");
            return;
        }

        mapData = MapInfoManager.Instance.GetMapInfo(mapId);

        if (mapData == null)
        {
            Debug.LogError($"[MapController] 맵 데이터를 찾을 수 없음: {mapId}");
            return;
        }

        // Inspector에 표시용 (읽기 전용)
        mapName = mapData.mapName;
        mapType = mapData.mapType;
        mapDescription = mapData.mapDescription;

        Debug.Log($"[MapController] 맵 데이터 로드 완료: {mapName} ({mapId})");
    }

    /// <summary>
    /// MapInfoManager에 현재 맵 등록
    /// </summary>
    private void RegisterToManager()
    {
        if (MapInfoManager.Instance != null && mapData != null)
        {
            MapInfoManager.Instance.SetCurrentMap(mapId);
        }
    }

    /// <summary>
    /// 맵 ID 가져오기
    /// </summary>
    public string GetMapId()
    {
        return mapId;
    }

    /// <summary>
    /// 맵 이름 가져오기
    /// </summary>
    public string GetMapName()
    {
        return mapData != null ? mapData.mapName : mapId;
    }

    /// <summary>
    /// 맵 데이터 가져오기
    /// </summary>
    public Maps GetMapData()
    {
        return mapData;
    }

    /// <summary>
    /// Inspector에서 값 변경 시 미리보기 업데이트
    /// </summary>
    void OnValidate()
    {
        if (Application.isPlaying) return;

        // 에디터에서 mapId 변경 시 즉시 데이터 미리보기
        if (!string.IsNullOrEmpty(mapId) && MapInfoManager.Instance != null)
        {
            Maps previewData = MapInfoManager.Instance.GetMapInfo(mapId);
            if (previewData != null)
            {
                mapName = previewData.mapName;
                mapType = previewData.mapType;
                mapDescription = previewData.mapDescription;
            }
        }
    }
}