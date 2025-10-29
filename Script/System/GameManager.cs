using Definitions;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public enum GameState
    {
        Playing,
        Paused,
        GameOver
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
        // ✅ GameSaveManager 기반으로 수정
        if (GameDataManager.Instance != null)
        {
            string initialScene = Def_Name.SCENE_NAME_DEFAULT_MAP;

            if (!string.IsNullOrEmpty(GameDataManager.Instance.currentGlobalData.currentSceneName))
                initialScene = GameDataManager.Instance.currentGlobalData.currentSceneName;

            GameDataManager.Instance.LoadGame("InitialLoad");
        }
    }

    public void PauseGame()
    {
        currentGameState = GameState.Paused;
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        currentGameState = GameState.Playing;
        Time.timeScale = 1f;
    }
}
