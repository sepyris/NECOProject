using UnityEngine;
using UnityEngine.SceneManagement;

// 씬 로드 시 전역 CameraController.Instance 카메라를 기준으로
// 나머지 카메라들을 정리하고 Canvas들을 재연결하는 유틸.
// 씬별로 유지해야 하는 카메라는 "KeepCamera" 태그를 붙이세요.
public class CameraPersistenceHelper : MonoBehaviour
{
    void Awake()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        DontDestroyOnLoad(gameObject); // 이 매니저도 영속으로 둬도 무방
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 전역 CameraController 인스턴스가 있으면 기준 카메라로 사용
        var cameraController = CameraController.Instance;
        Camera persistentCam = cameraController != null ? cameraController.GetComponent<Camera>() : Camera.main;

        if (persistentCam == null)
        {
            Debug.LogWarning("[CameraPersistenceHelper] 기준 카메라를 찾을 수 없습니다.");
            return;
        }

        // 1) 씬의 카메라 정리: persistentCam이 아니고 태그가 KeepCamera가 아니면 비활성화
        Camera[] allCams = FindObjectsOfType<Camera>(true);
        foreach (var cam in allCams)
        {
            if (cam == persistentCam) continue;
            if (cam.gameObject.CompareTag("KeepCamera")) continue;

            // AudioListener 중복 방지
            var al = cam.GetComponent<AudioListener>();
            if (al != null) al.enabled = false;

            // 비주얼 카메라는 필요하면 비활성화
            cam.enabled = false;
            Debug.Log($"[CameraPersistenceHelper] Disabled scene camera: {cam.gameObject.name}");
        }

        // 2) Canvas 재연결: ScreenSpace - Camera로 설정된 Canvas를 persistentCam에 연결
        Canvas[] allCanvases = FindObjectsOfType<Canvas>(true);
        foreach (var cv in allCanvases)
        {
            if (cv.renderMode == RenderMode.ScreenSpaceCamera)
            {
                cv.worldCamera = persistentCam;
                Debug.Log($"[CameraPersistenceHelper] Canvas '{cv.gameObject.name}' assigned to persistent camera.");
            }
            else if (cv.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                // Overlay이면 변경 불필요
            }
            else if (cv.renderMode == RenderMode.WorldSpace)
            {
                // WorldSpace는 변경 불필요
            }
        }
    }
}