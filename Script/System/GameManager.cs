// GameManager.cs (단순화 버전)
using Definitions;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    // ?? 수정: GameState 단순화 (게임창 관련 제거)
    public enum GameState
    {
        Playing,    // 게임 플레이 중
        Paused,     // 일시정지
        GameOver    // 게임 오버
    }

    private GameState _currentGameState = GameState.Playing;

    public GameState currentGameState
    {
        get { return _currentGameState; }
        set
        {
            if (_currentGameState != value)
            {
                _currentGameState = value;
                Debug.Log($"[GameManager] GameState 변경: {value}");
            }
        }
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // ?? 수정: 초기 씬 로드 로직 변경
        if (GameDataManager.Instance != null)
        {
            string initialScene = Def_Name.SCENE_NAME_DEFAULT_GAME;

            // 저장된 씬이 있으면 로드
            if (!string.IsNullOrEmpty(GameDataManager.Instance.currentGlobalData.currentSceneName))
            {
                initialScene = GameDataManager.Instance.currentGlobalData.currentSceneName;
            }

            GameDataManager.Instance.LoadGame("InitialLoad");
        }
    }

    /// <summary>
    /// 게임 일시정지
    /// </summary>
    public void PauseGame()
    {
        currentGameState = GameState.Paused;
        Time.timeScale = 0f;
    }

    /// <summary>
    /// 게임 재개
    /// </summary>
    public void ResumeGame()
    {
        currentGameState = GameState.Playing;
        Time.timeScale = 1f;
    }
}