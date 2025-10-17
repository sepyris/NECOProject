// GameDataManager.cs (단순화 버전)
using Definitions;
using GameSave;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using static DebugDisplayManager;
using static LocKeys;

public class GameDataManager : MonoBehaviour
{
    public static GameDataManager Instance { get; private set; }
    public GlobalSaveData currentGlobalData = new GlobalSaveData();
    public string nextSceneSpawnPointID = "";

    private const string SUB_SCENE_SAVE_KEY = "SubSceneTempData";
    private static bool shouldStartNewGame = true;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (shouldStartNewGame)
            {
                PlayerPrefs.DeleteKey(SUB_SCENE_SAVE_KEY);
                PlayerPrefs.Save();
                DisplaySuccess(START_NEW_GAME);
                currentGlobalData = new GlobalSaveData();
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

    public void LoadGame(string slotName)
    {
        StartCoroutine(LoadSceneAndRestoreState(currentGlobalData.currentSceneName));
    }

    public void LoadSceneByName(string sceneName)
    {
        currentGlobalData.currentSceneName = sceneName;
        StartCoroutine(LoadSceneAndRestoreState(sceneName));
    }

    private IEnumerator LoadSceneAndRestoreState(string sceneName)
    {
        // 📢 수정: MainScene 체크 제거
        if (string.IsNullOrEmpty(sceneName))
        {
            sceneName = Def_Name.SCENE_NAME_DEFAULT_GAME;
        }

        // 전역 로딩 화면 표시
        if (LoadingScreenManager.Instance != null)
        {
            LoadingScreenManager.Instance.ShowGlobalLoading();
        }

        // ===== 1단계: 기존 게임 씬 언로드 =====
        yield return StartCoroutine(UnloadPreviousGameScenes(sceneName));

        // ===== 2단계: 새로운 씬 로드 =====
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        while (!asyncLoad.isDone) yield return null;

        yield return null; // 씬 로드 후 한 프레임 대기

        // ===== 3단계: 플레이어 위치 설정 =====
        yield return StartCoroutine(HandlePlayerSpawn());

        // ===== 4단계: 카메라 재초기화 =====
        CameraController sceneCameraController = FindCameraControllerInScene(sceneName);
        if (sceneCameraController != null)
        {
            sceneCameraController.ReInitialize();
            Debug.Log("[GameDataManager] CameraController ReInitialize 호출 완료.");
        }

        // 전역 로딩 화면 숨김
        if (LoadingScreenManager.Instance != null)
        {
            LoadingScreenManager.Instance.HideGlobalLoading();
        }
    }

    // ===== 기존 게임 씬 언로드 =====
    private IEnumerator UnloadPreviousGameScenes(string newSceneName)
    {
        string gameScenePrefix = Def_Name.SCENE_NAME_START_GAME;

        for (int i = SceneManager.sceneCount - 1; i >= 0; i--)
        {
            Scene sceneToUnload = SceneManager.GetSceneAt(i);

            if (sceneToUnload.name == newSceneName) continue;

            if (sceneToUnload.isLoaded && sceneToUnload.name.StartsWith(gameScenePrefix))
            {
                Debug.Log($"[GameDataManager] 기존 게임 씬 '{sceneToUnload.name}' 언로드 시작.");

                // 안전하게 UnloadSceneAsync 호출 후 null 체크
                AsyncOperation unloadOp = null;
                try
                {
                    // string 오버로드 사용하여 더 안전하게 요청
                    unloadOp = SceneManager.UnloadSceneAsync(sceneToUnload.name);
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"[GameDataManager] 씬 언로드 시 예외 발생: {ex.Message}");
                }

                if (unloadOp == null)
                {
                    Debug.LogWarning($"[GameDataManager] 씬 '{sceneToUnload.name}' 언로드 요청 실패 (AsyncOperation == null). 계속 진행합니다.");
                    continue;
                }

                while (!unloadOp.isDone) yield return null;
                Debug.Log($"[GameDataManager] 기존 게임 씬 '{sceneToUnload.name}' 언로드 완료.");
            }
        }
    }

    // ===== 플레이어 스폰 처리 =====
    private IEnumerator HandlePlayerSpawn()
    {
        string targetID = nextSceneSpawnPointID;
        PlayerController player = FindObjectOfType<PlayerController>();

        if (!string.IsNullOrEmpty(targetID) && player != null)
        {
            MapSpawnPoint[] allPoints = FindObjectsOfType<MapSpawnPoint>();
            Vector3 spawnPosition = Vector3.zero;

            foreach (var point in allPoints)
            {
                if (point.spawnPointID == targetID)
                {
                    spawnPosition = point.transform.position;
                    Debug.Log($"[Spawn] ID '{targetID}'에 해당하는 스폰 지점을 찾았습니다. 위치: {spawnPosition}");
                    break;
                }
            }

            if (spawnPosition != Vector3.zero)
            {
                player.transform.position = spawnPosition;
                Debug.Log($"[Spawn] 플레이어 위치를 {targetID}로 이동시킴.");
            }
            else
            {
                Debug.LogError($"[Spawn] ID '{targetID}'에 해당하는 스폰 지점을 찾을 수 없음.");
            }

            nextSceneSpawnPointID = "";
        }

        yield return null;
    }

    // ===== 헬퍼 메서드 =====
    private CameraController FindCameraControllerInScene(string sceneName)
    {
        Camera[] allCameras = FindObjectsOfType<Camera>(true);
        foreach (var cam in allCameras)
        {
            if (cam.CompareTag(Def_Name.GAME_CAMERA))
            {
                return cam.GetComponent<CameraController>();
            }
        }
        return null;
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