using Definitions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameDataManager : MonoBehaviour
{
    public static GameDataManager Instance { get; private set; }

    private const string SAVE_FILE_NAME = "SaveData.json";
    private static bool shouldStartNewGame = true;

    public GlobalSaveData currentGlobalData = new GlobalSaveData();
    public string nextSceneSpawnPointID = "";

    private string SavePath => Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (shouldStartNewGame)
            {
                StartNewGame();
            }
            else
            {
                LoadFromFile();
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetLoadGameMode(bool isLoadMode)
    {
        shouldStartNewGame = !isLoadMode;
    }

    // ===== 새 게임 =====
    public void StartNewGame()
    {
        currentGlobalData = new GlobalSaveData();

        // 인벤토리 초기화
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.ClearInventory();

            // 초기 아이템 지급 (선택사항)
            InventoryManager.Instance.AddItem("potion_health", 3);
            InventoryManager.Instance.AddItem("food_bread", 3);
            InventoryManager.Instance.AddItem("food_meat", 3);
            InventoryManager.Instance.AddItem("potion_health_large", 3);
            InventoryManager.Instance.AddItem("potion_elixir", 3);
            InventoryManager.Instance.AddItem("leather", 5);
            InventoryManager.Instance.AddItem("sword_iron", 1);
            InventoryManager.Instance.AddItem("quest_letter", 1);
        }

        SaveToFile();
        Debug.Log("[GameDataManager] 새 게임 시작됨 (데이터 초기화)");
    }

    // ===== 저장 =====
    public void SaveToFile()
    {
        try
        {
            // 인벤토리 데이터 저장
            if (InventoryManager.Instance != null)
            {
                currentGlobalData.inventoryData = InventoryManager.Instance.ToSaveData();
            }

            string json = JsonUtility.ToJson(currentGlobalData, true);
            File.WriteAllText(SavePath, json);
            Debug.Log($"[GameDataManager] 저장 완료: {SavePath}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[GameDataManager] 저장 실패: {ex.Message}");
        }
    }

    // ===== 로드 =====
    public void LoadFromFile()
    {
        if (!File.Exists(SavePath))
        {
            Debug.LogWarning("[GameDataManager] 저장 파일 없음. 새 데이터 생성");
            currentGlobalData = new GlobalSaveData();
            return;
        }

        try
        {
            string json = File.ReadAllText(SavePath);
            currentGlobalData = JsonUtility.FromJson<GlobalSaveData>(json);

            // 인벤토리 데이터 로드
            if (InventoryManager.Instance != null && currentGlobalData.inventoryData != null)
            {
                InventoryManager.Instance.LoadFromData(currentGlobalData.inventoryData);
            }

            Debug.Log("[GameDataManager] 저장 데이터 로드 완료");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[GameDataManager] 로드 실패: {ex.Message}");
            currentGlobalData = new GlobalSaveData();
        }
    }

    // ===== 씬 로드 =====
    public void LoadGame(string slotName = "")
    {
        string sceneName = currentGlobalData.currentSceneName;
        if (string.IsNullOrEmpty(sceneName))
            sceneName = Def_Name.SCENE_NAME_DEFAULT_MAP;

        StartCoroutine(LoadSceneAndRestore(sceneName));
    }

    public void LoadSceneByName(string sceneName)
    {
        currentGlobalData.currentSceneName = sceneName;
        StartCoroutine(LoadSceneAndRestore(sceneName));
    }

    private IEnumerator LoadSceneAndRestore(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
            sceneName = Def_Name.SCENE_NAME_DEFAULT_MAP;

        // 로딩화면 표시
        if (LoadingScreenManager.Instance != null)
            LoadingScreenManager.Instance.ShowGlobalLoading();

        // 기존 씬 정리
        yield return StartCoroutine(UnloadPreviousScenes(sceneName));

        // 새 씬 로드
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        while (!asyncLoad.isDone) yield return null;

        // 플레이어 위치 복원
        yield return StartCoroutine(HandlePlayerSpawn());

        // 미니맵 초기화
        if (MiniMapManager.Instance != null)
            MiniMapManager.Instance.ReInitialize();

        // 카메라 재설정
        Camera mainCam = Camera.main;
        if (mainCam != null && mainCam.CompareTag(Def_Name.TMPCAMERA_TAG))
            Destroy(mainCam.gameObject);

        var cameraCtrl = FindGameCameraController();
        if (cameraCtrl != null)
            cameraCtrl.ReInitialize();

        // 잠시 대기
        yield return new WaitForSeconds(1.0f);

        // 로딩화면 표시
        if (LoadingScreenManager.Instance != null)
            LoadingScreenManager.Instance.HideGlobalLoading();

        Debug.Log($"[GameDataManager] 씬 '{sceneName}' 로드 및 복원 완료");
    }

    private IEnumerator UnloadPreviousScenes(string newSceneName)
    {
        string prefix = Def_Name.SCENE_NAME_START_MAP;
        for (int i = SceneManager.sceneCount - 1; i >= 0; i--)
        {
            Scene s = SceneManager.GetSceneAt(i);
            if (s.name == newSceneName) continue;

            if (s.isLoaded && s.name.StartsWith(prefix))
            {
                AsyncOperation op = SceneManager.UnloadSceneAsync(s.name);
                if (op != null)
                    while (!op.isDone) yield return null;
            }
        }
    }

    private IEnumerator HandlePlayerSpawn()
    {
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player == null) yield break;

        if (!string.IsNullOrEmpty(nextSceneSpawnPointID))
        {
            MapSpawnPoint[] allPoints = FindObjectsOfType<MapSpawnPoint>();
            foreach (var point in allPoints)
            {
                if (point.spawnPointID == nextSceneSpawnPointID)
                {
                    player.transform.position = point.transform.position;
                    Debug.Log($"[Spawn] 플레이어를 '{nextSceneSpawnPointID}'로 이동시킴.");
                    break;
                }
            }
            nextSceneSpawnPointID = "";
        }
        else
        {
            var s = currentGlobalData.subSceneState;
            player.transform.position = new Vector3(s.positionX, s.positionY, s.positionZ);
            Debug.Log("[Spawn] 저장된 위치로 복원 완료");
        }

        yield return null;
    }

    private CameraController FindGameCameraController()
    {
        foreach (var cam in FindObjectsOfType<Camera>(true))
        {
            if (cam.CompareTag(Def_Name.GAME_CAMERA))
                return cam.GetComponent<CameraController>();
        }
        return null;
    }

    // ===== 플레이어 상태 저장 =====
    public void SavePlayerState(Vector3 position, int hp)
    {
        var state = currentGlobalData.subSceneState;
        state.positionX = position.x;
        state.positionY = position.y;
        state.positionZ = position.z;
        state.health = hp;
        currentGlobalData.subSceneState = state;
        SaveToFile();
        Debug.Log("[GameDataManager] 플레이어 상태 저장 완료");
    }

    // ===== 서브씬 상태 저장/로드 =====
    public void SaveSubSceneState(SubSceneData data)
    {
        currentGlobalData.subSceneState = data;
    }

    public SubSceneData LoadSubSceneState()
    {
        return currentGlobalData.subSceneState;
    }
}

/// <summary>
/// 글로벌 저장 데이터
/// </summary>
[Serializable]
public class GlobalSaveData
{
    public string currentSceneName = "";
    public Vector3 playerPosition = Vector3.zero;
    public int playerHealth = 100;
    public SubSceneData subSceneState = new SubSceneData();
    public InventorySaveData inventoryData = new InventorySaveData();
    public string integrityHash = "";
}

/// <summary>
/// 서브씬 저장 데이터
/// </summary>
[System.Serializable]
public struct SubSceneData
{
    public string currentSceneName;
    public float positionX;
    public float positionY;
    public float positionZ;
    public int health;
    public static SubSceneData Default() => new SubSceneData { health = 100 };
}