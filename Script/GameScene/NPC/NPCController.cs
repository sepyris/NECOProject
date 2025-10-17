using System.Collections.Generic;
using UnityEngine;

// ������ NPC ��Ʈ�ѷ�
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
    // shopData�� ���߿� Ȯ�� (������ ����Ʈ ��)
    public string shopId = "";

    // ���� ����
    private bool playerInRange = false;
    private Transform playerTransform;

    void Update()
    {
        // �÷��̾� ��ġ �˻�(�ּ� ���: �� ���ۿ� ĳ�� ����)
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
        // ��Ʈ UI ǥ��
        if (DialogueUIManager.Instance != null)
            DialogueUIManager.Instance.ShowInteractHint(true, this);
        else
            Debug.Log("[NPC] ��ȣ�ۿ� ����: EŰ");
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
            Debug.Log("[NPC] DialogueUIManager�� ����. �ַܼ� ��ȭ ���:");
            switch (npcType)
            {
                case NPCType.Dialogue:
                    foreach (var l in dialogueLines) Debug.Log($"NPC: {l}");
                    break;
                case NPCType.Quest:
                    foreach (var l in questOfferLines) Debug.Log($"NPC (QuestOffer): {l}");
                    break;
                case NPCType.Shop:
                    Debug.Log("NPC (Shop) ���� �� ShopId: " + shopId);
                    break;
            }
            // ����: �÷��̾� ����� ���� (UI ����)
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }
}