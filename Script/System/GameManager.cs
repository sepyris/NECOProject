// GameManager.cs (�ܼ�ȭ ����)
using Definitions;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    // ?? ����: GameState �ܼ�ȭ (����â ���� ����)
    public enum GameState
    {
        Playing,    // ���� �÷��� ��
        Paused,     // �Ͻ�����
        GameOver    // ���� ����
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
                Debug.Log($"[GameManager] GameState ����: {value}");
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
        // ?? ����: �ʱ� �� �ε� ���� ����
        if (GameDataManager.Instance != null)
        {
            string initialScene = Def_Name.SCENE_NAME_DEFAULT_GAME;

            // ����� ���� ������ �ε�
            if (!string.IsNullOrEmpty(GameDataManager.Instance.currentGlobalData.currentSceneName))
            {
                initialScene = GameDataManager.Instance.currentGlobalData.currentSceneName;
            }

            GameDataManager.Instance.LoadGame("InitialLoad");
        }
    }

    /// <summary>
    /// ���� �Ͻ�����
    /// </summary>
    public void PauseGame()
    {
        currentGameState = GameState.Paused;
        Time.timeScale = 0f;
    }

    /// <summary>
    /// ���� �簳
    /// </summary>
    public void ResumeGame()
    {
        currentGameState = GameState.Playing;
        Time.timeScale = 1f;
    }
}