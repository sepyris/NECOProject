using Steamworks; // Steamworks.NET ���̺귯�� ���
using UnityEngine;

using static LocKeys;
using static DebugDisplayManager;
public class SteamManager : MonoBehaviour
{
    private static SteamManager s_Instance;
    public static bool Initialized { get; private set; } = false;
    void Awake()
    {
        // �̱��� �������� ���� �ν��Ͻ� ����
        if (s_Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        s_Instance = this;
        DontDestroyOnLoad(gameObject);

        if (!SteamAPI.Init())
        {
            Initialized = false;
            // 2. ȭ�鿡 ���� �޽��� ���
            DisplayError(STEME_CONNECT_FAIL);
        }
        else
        {
            Initialized = true;
            // 2. ȭ�鿡 ���� �޽��� ���
            DisplaySuccess(STEME_CONNECT_SUCCESS);
        }
    }

    void Update()
    {
        if (Initialized)
        {
            if (SteamManager.s_Instance != null)
            {
                SteamAPI.RunCallbacks();
            }
        }
        
    }
    void OnDestroy()
    {
        if (s_Instance == this)
        {
        }
    }

    void OnApplicationQuit()
    {
        if (Initialized)
        {
            SteamAPI.Shutdown();
            // 2. ȭ�鿡 ���� �޽��� ���
            DisplaySuccess(STEME_DISCONNECT);
        }
    }


}