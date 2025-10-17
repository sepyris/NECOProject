using System.Collections.Generic;
using UnityEngine;

// 고정형 NPC 컨트롤러
public class NPCController : MonoBehaviour
{
    public enum NPCType { Dialogue, Quest, Shop }

    [Header("NPC Settings")]
    public NPCType npcType = NPCType.Dialogue;
    public float interactRadius = 1.5f;
    public KeyCode interactKey = KeyCode.E;

    [Header("Dialogue")]
    [TextArea(2, 6)]
    public List<string> dialogueLines = new List<string>();

    [Header("Quest")]
    [TextArea(2, 4)]
    public List<string> questOfferLines = new List<string>();
    [TextArea(2, 4)]
    public List<string> questAcceptedLines = new List<string>();
    [TextArea(2, 4)]
    public List<string> questDeclinedLines = new List<string>();

    [Header("Shop")]
    // shopData는 나중에 확장 (아이템 리스트 등)
    public string shopId = "";

    // 내부 상태
    private bool playerInRange = false;
    private Transform playerTransform;

    void Update()
    {
        // 플레이어 위치 검색(최소 비용: 씬 시작에 캐시 가능)
        if (PlayerController.Instance != null)
        {
            playerTransform = PlayerController.Instance.transform;
            float dist = Vector2.Distance(playerTransform.position, transform.position);
            bool inRange = dist <= interactRadius;

            if (inRange && !playerInRange)
            {
                OnPlayerEnterRange();
                playerInRange = true;
            }
            else if (!inRange && playerInRange)
            {
                OnPlayerExitRange();
                playerInRange = false;
            }

            if (playerInRange && Input.GetKeyDown(interactKey))
            {
                TryInteract();
            }
        }
    }

    private void OnPlayerEnterRange()
    {
        // 힌트 UI 표시
        if (DialogueUIManager.Instance != null)
            DialogueUIManager.Instance.ShowInteractHint(true, this);
        else
            Debug.Log("[NPC] 상호작용 가능: E키");
    }

    private void OnPlayerExitRange()
    {
        if (DialogueUIManager.Instance != null)
            DialogueUIManager.Instance.ShowInteractHint(false, this);
    }

    private void TryInteract()
    {
        if (DialogueUIManager.Instance != null)
        {
            DialogueUIManager.Instance.OpenInteraction(this);
        }
        else
        {
            Debug.Log("[NPC] DialogueUIManager가 없음. 콘솔로 대화 출력:");
            switch (npcType)
            {
                case NPCType.Dialogue:
                    foreach (var l in dialogueLines) Debug.Log($"NPC: {l}");
                    break;
                case NPCType.Quest:
                    foreach (var l in questOfferLines) Debug.Log($"NPC (QuestOffer): {l}");
                    break;
                case NPCType.Shop:
                    Debug.Log("NPC (Shop) 열기 → ShopId: " + shopId);
                    break;
            }
            // 예시: 플레이어 잠금은 생략 (UI 없음)
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }
}