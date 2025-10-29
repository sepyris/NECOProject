using Definitions;
using UnityEngine;

/// <summary>
/// 채집 가능한 오브젝트 (약초, 광석 등)
/// - 입력 처리는 PlayerGathering에서 담당
/// </summary>
public class GatheringObject : MonoBehaviour
{
    [Header("Gathering Info")]
    public string itemID = "Item_Herb"; // 획득할 아이템 ID
    public int itemAmount = 1;          // 획득 개수

    [Header("Visual")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private GameObject interactPrompt; // "E키" 표시

    private bool isGathered = false;
    private GatheringSpawnArea parentSpawnArea; // 스폰 영역 참조

    void Start()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (interactPrompt != null)
            interactPrompt.SetActive(false);
    }

    /// <summary>
    /// 채집 가능 여부 확인
    /// </summary>
    public bool CanGather()
    {
        return !isGathered;
    }

    /// <summary>
    /// 프롬프트 표시 (PlayerGathering에서 호출)
    /// </summary>
    public void ShowPrompt()
    {
        if (interactPrompt != null && !isGathered)
            interactPrompt.SetActive(true);
    }

    /// <summary>
    /// 프롬프트 숨김 (PlayerGathering에서 호출)
    /// </summary>
    public void HidePrompt()
    {
        if (interactPrompt != null)
            interactPrompt.SetActive(false);
    }

    /// <summary>
    /// 실제 채집 처리 (PlayerGathering에서 호출)
    /// </summary>
    public void Gather()
    {
        if (isGathered) return;

        isGathered = true;

        Debug.Log($"[Gathering] {itemID} {itemAmount}개 획득!");

        // ⭐ 퀘스트 매니저에 아이템 획득 알림 ⭐
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.UpdateItemProgress(itemID, itemAmount);
        }
        
        InventoryManager.Instance.AddItem(itemID, itemAmount);

        // 프롬프트 숨김
        HidePrompt();

        // 재생성 안함 (일회성 오브젝트) - 스폰 영역에 알림
        if (parentSpawnArea != null)
        {
            parentSpawnArea.OnGatheringObjectDestroyed(this.gameObject);
        }

        Destroy(gameObject, 0.5f);
    }

    /// <summary>
    /// 재생성
    /// </summary>
    private void Respawn()
    {
        isGathered = false;

        // 시각적 복구
        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = 1f;
            spriteRenderer.color = color;
        }

        Debug.Log($"[Gathering] {itemID} 재생성 완료");
    }

    /// <summary>
    /// 스폰 영역 설정 (GatheringSpawnArea에서 호출)
    /// </summary>
    public void SetSpawnArea(GatheringSpawnArea spawnArea)
    {
        parentSpawnArea = spawnArea;
    }

    /// <summary>
    /// 강제 재생성 (외부에서 호출 가능)
    /// </summary>
    public void ForceRespawn()
    {
        if (isGathered)
        {
            CancelInvoke(nameof(Respawn));
            Respawn();
        }
    }
}