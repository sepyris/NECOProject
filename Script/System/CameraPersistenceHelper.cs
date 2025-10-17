using UnityEngine;
using UnityEngine.SceneManagement;

// �� �ε� �� ���� CameraController.Instance ī�޶� ��������
// ������ ī�޶���� �����ϰ� Canvas���� �翬���ϴ� ��ƿ.
// ������ �����ؾ� �ϴ� ī�޶�� "KeepCamera" �±׸� ���̼���.
public class CameraPersistenceHelper : MonoBehaviour
{
    void Awake()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        DontDestroyOnLoad(gameObject); // �� �Ŵ����� �������� �ֵ� ����
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // ���� CameraController �ν��Ͻ��� ������ ���� ī�޶�� ���
        var cameraController = CameraController.Instance;
        Camera persistentCam = cameraController != null ? cameraController.GetComponent<Camera>() : Camera.main;

        if (persistentCam == null)
        {
            Debug.LogWarning("[CameraPersistenceHelper] ���� ī�޶� ã�� �� �����ϴ�.");
            return;
        }

        // 1) ���� ī�޶� ����: persistentCam�� �ƴϰ� �±װ� KeepCamera�� �ƴϸ� ��Ȱ��ȭ
        Camera[] allCams = FindObjectsOfType<Camera>(true);
        foreach (var cam in allCams)
        {
            if (cam == persistentCam) continue;
            if (cam.gameObject.CompareTag("KeepCamera")) continue;

            // AudioListener �ߺ� ����
            var al = cam.GetComponent<AudioListener>();
            if (al != null) al.enabled = false;

            // ���־� ī�޶�� �ʿ��ϸ� ��Ȱ��ȭ
            cam.enabled = false;
            Debug.Log($"[CameraPersistenceHelper] Disabled scene camera: {cam.gameObject.name}");
        }

        // 2) Canvas �翬��: ScreenSpace - Camera�� ������ Canvas�� persistentCam�� ����
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
                // Overlay�̸� ���� ���ʿ�
            }
            else if (cv.renderMode == RenderMode.WorldSpace)
            {
                // WorldSpace�� ���� ���ʿ�
            }
        }
    }
}