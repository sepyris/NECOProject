using Definitions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class MapTransition : MonoBehaviour, IPointerDownHandler
{
    public string targetSceneName;
    public string targetSpawnPointID;

    public void OnPointerDown(PointerEventData eventData)
    {
        GoToNewScene();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"[MapTransition] Trigger entered by '{other.gameObject.name}', tag='{other.gameObject.tag}'");

        if (other.CompareTag(Def_Name.PLAYER_TAG) || other.GetComponentInParent<PlayerController>() != null)
        {
            // **핵심: 씬 전환 직전에 현재 입력 저장**
            InputManager.SaveInputForSceneTransition();
            Debug.Log($"[MapTransition] 씬 전환용 입력 저장 완료");

            GoToNewScene();
        }
    }

    private void GoToNewScene()
    {
        Debug.Log($"[MapTransition] GoToNewScene called. targetScene='{targetSceneName}', targetSpawnID='{targetSpawnPointID}', GameDataManager.Instance={(GameDataManager.Instance == null ? "null" : "present")}");

        if (GameDataManager.Instance == null)
        {
            Debug.LogWarning("[MapTransition] GameDataManager.Instance가 null입니다. Resources에서 GameDataManager prefab 로드 시도.");
            GameObject gmPrefab = Resources.Load<GameObject>("GameDataManager");
            if (gmPrefab != null)
            {
                Instantiate(gmPrefab);
                Debug.Log("[MapTransition] GameDataManager prefab 인스턴스화 완료.");
            }
            else
            {
                Debug.LogWarning("[MapTransition] Resources/GameDataManager.prefab 을 찾을 수 없습니다. 수동으로 부트스트랩에 배치하세요.");
            }
        }

        if (string.IsNullOrEmpty(targetSceneName) || string.IsNullOrEmpty(targetSpawnPointID))
        {
            Debug.LogError("[MapTransition] 이동할 씬 이름 또는 스폰 ID가 설정되지 않았습니다!");
            return;
        }

        // 1. 캐릭터 상태 저장
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            player.SaveStateBeforeDeactivation();
        }

        // 2. GameDataManager에 씬 정보와 스폰 ID 저장
        if (GameDataManager.Instance != null)
        {
            GameDataManager.Instance.currentGlobalData.currentSceneName = targetSceneName;
            GameDataManager.Instance.nextSceneSpawnPointID = targetSpawnPointID;
            Debug.Log($"[MapTransition] 다음 씬({targetSceneName}) 스폰 ID 저장: {targetSpawnPointID}");

            // 3. 씬 로드 요청
            GameDataManager.Instance.LoadSceneByName(targetSceneName);
        }
        else
        {
            Debug.LogWarning("[MapTransition] GameDataManager.Instance가 null입니다. SceneManager로 직접 로드 시도.");
            SceneManager.LoadSceneAsync(targetSceneName, LoadSceneMode.Single);
        }
    }
}