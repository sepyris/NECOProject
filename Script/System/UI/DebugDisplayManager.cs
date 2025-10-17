using UnityEngine;
using TMPro;

public class DebugDisplayManager : MonoBehaviour
{
    private static DebugDisplayManager _instance;

    public static DebugDisplayManager Instance
    {
        get
        {
            if (_instance == null)
            {
                Debug.LogError("DebugDisplayManager instance not found. Script Execution Order 확인 필요.");
            }
            return _instance;
        }
    }

    public TMP_Text statusText;

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    // --- 내부 텍스트 설정 메서드 (private) ---
    // 모든 텍스트 변경은 이 메서드를 통해 이루어집니다.
    private void DisplayStatusInternal(string message, Color color)
    {
        if (statusText != null)
        {
            statusText.text = message;
            statusText.color = color;

            // 이전 단계에서 논의된 '메시지 숨김 코루틴' 로직이 여기에 들어갑니다.
            // (현재는 생략되어 즉시 사라지지 않습니다.)
        }
    }

    // 💡 누락된 public DisplayStatus 메서드 (Update() 테스트용)
    // 이전에 Update()에서 호출하려 했던 메서드입니다.
    public void DisplayStatus(string message, Color color)
    {
        DisplayStatusInternal(message, color);
    }

    // 💡 Space 키 테스트 코드
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // 이 호출이 이제 정상적으로 DisplayStatusInternal을 호출합니다.
            DisplayStatus("TEST MESSAGE SUCCESS!", Color.yellow);
        }
    }

    // --- static 호출 메서드: 외부 API (`using static`을 위한 최종 형태) ---

    /// <summary>
    /// 에러 메시지를 화면에 출력합니다. (Localization Key 사용)
    /// </summary>
    public static void DisplayError(string localizationKey)
    {
        if (Instance != null && LocalizationManager.Instance != null)
        {
            string message = LocalizationManager.Instance.GetLocalizedValue(localizationKey);
            Instance.DisplayStatusInternal(message, Color.red);
        }
    }

    /// <summary>
    /// 성공 메시지를 화면에 출력합니다. (Localization Key 사용)
    /// </summary>
    public static void DisplaySuccess(string localizationKey)
    {
        if (Instance != null && LocalizationManager.Instance != null)
        {
            string message = LocalizationManager.Instance.GetLocalizedValue(localizationKey);
            Instance.DisplayStatusInternal(message, Color.green);
        }
    }
}