// LoadingScreenManager.cs (개선 버전)
using UnityEngine;
using System.Collections;

public class LoadingScreenManager : MonoBehaviour
{
    public static LoadingScreenManager Instance { get; private set; }

    [Header("Global Loading (전체 화면)")]
    public GameObject globalLoadingPanel;

    [Header("Auto Hide Settings")]
    [SerializeField] private float autoHideDelay = 0.5f; // 안전장치: 자동 숨김 시간

    public bool IsLoading { get; private set; } = false;
    private Coroutine autoHideCoroutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (globalLoadingPanel != null)
            {
                globalLoadingPanel.SetActive(false);
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 전역 로딩 화면 표시
    /// </summary>
    public void ShowGlobalLoading()
    {
        // 기존 자동 숨김 코루틴 정지
        if (autoHideCoroutine != null)
        {
            StopCoroutine(autoHideCoroutine);
            autoHideCoroutine = null;
        }

        if (globalLoadingPanel != null)
        {
            globalLoadingPanel.SetActive(true);
            IsLoading = true;
            Debug.Log("[Loading] 전역 로딩 화면 표시.");
        }
    }

    /// <summary>
    /// 전역 로딩 화면 숨김
    /// </summary>
    public void HideGlobalLoading()
    {
        if (globalLoadingPanel != null)
        {
            globalLoadingPanel.SetActive(false);
            IsLoading = false;
            Debug.Log("[Loading] 전역 로딩 화면 숨김.");
        }

        // 안전장치 코루틴도 정지
        if (autoHideCoroutine != null)
        {
            StopCoroutine(autoHideCoroutine);
            autoHideCoroutine = null;
        }
    }

    /// <summary>
    /// 📢 추가: 안전장치 - 일정 시간 후 강제로 로딩 숨김
    /// </summary>
    public void ShowGlobalLoadingWithAutoHide(float maxDuration = 5f)
    {
        ShowGlobalLoading();

        // 기존 코루틴 정지
        if (autoHideCoroutine != null)
        {
            StopCoroutine(autoHideCoroutine);
        }

        // 새 코루틴 시작
        autoHideCoroutine = StartCoroutine(AutoHideRoutine(maxDuration));
    }

    private IEnumerator AutoHideRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (IsLoading)
        {
            Debug.LogWarning($"[Loading] {delay}초 경과. 강제로 로딩 화면 숨김.");
            HideGlobalLoading();
        }
    }

    /// <summary>
    /// 📢 추가: 즉시 로딩 상태 해제 (긴급용)
    /// </summary>
    public void ForceStopLoading()
    {
        if (globalLoadingPanel != null)
        {
            globalLoadingPanel.SetActive(false);
        }

        IsLoading = false;

        if (autoHideCoroutine != null)
        {
            StopCoroutine(autoHideCoroutine);
            autoHideCoroutine = null;
        }

        Debug.LogWarning("[Loading] 강제로 로딩 상태 해제!");
    }
}