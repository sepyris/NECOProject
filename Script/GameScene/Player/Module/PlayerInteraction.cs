using System.Collections;
using UnityEngine;
using Definitions;

/// <summary>
/// �÷��̾��� ä�� �� NPC ��ȣ�ۿ� ����
/// </summary>
public class PlayerInteraction
{
    private Transform playerTransform;
    private PlayerAnimationController animationController;

    public bool ControlsLocked = false;
    public bool IsGathering { get; private set; } = false;

    private GatheringObject currentNearestGathering = null;
    private NPCController currentNearestNPC = null;
    private float detectionRadius = 2.5f;

    public PlayerInteraction(Transform playerTransform, PlayerAnimationController animController)
    {
        this.playerTransform = playerTransform;
        this.animationController = animController;
    }

    /// <summary>
    /// �� ������ ���� ����� ��ȣ�ۿ� ������Ʈ ���� (ä���� �Ǵ� NPC)
    /// </summary>
    public void UpdateNearestInteractable()
    {
        if (ControlsLocked || IsGathering)
        {
            HideAllPrompts();
            return;
        }

        // 1. ���� ����� ä���� ã��
        GatheringObject closestGathering = FindNearestGathering();

        // 2. ���� ����� NPC ã��
        NPCController closestNPC = FindNearestNPC();

        // 3. �� �� �� ����� �� ����
        float gatheringDist = closestGathering != null
            ? Vector2.Distance(playerTransform.position, closestGathering.transform.position)
            : float.MaxValue;

        float npcDist = closestNPC != null
            ? Vector2.Distance(playerTransform.position, closestNPC.transform.position)
            : float.MaxValue;

        // ä������ �� ����� ���
        if (gatheringDist < npcDist)
        {
            if (currentNearestGathering != closestGathering)
            {
                HideAllPrompts();
                currentNearestGathering = closestGathering;
                currentNearestNPC = null;
                currentNearestGathering?.ShowPrompt();
            }
        }
        // NPC�� �� ����� ���
        else if (npcDist < float.MaxValue)
        {
            if (currentNearestNPC != closestNPC)
            {
                HideAllPrompts();
                currentNearestNPC = closestNPC;
                currentNearestGathering = null;
                currentNearestNPC?.ShowPrompt();
            }
        }
        // �� �� ���� ���
        else
        {
            HideAllPrompts();
        }
    }

    /// <summary>
    /// ���� ����� ä���� ã��
    /// </summary>
    private GatheringObject FindNearestGathering()
    {
        int gatheringLayer = LayerMask.GetMask("Gathering");
        Collider2D[] nearbyColliders = Physics2D.OverlapCircleAll(playerTransform.position, detectionRadius, gatheringLayer);

        GatheringObject closest = null;
        float closestDistance = float.MaxValue;

        foreach (var col in nearbyColliders)
        {
            GatheringObject gatherObj = col.GetComponent<GatheringObject>();
            if (gatherObj != null && gatherObj.CanGather())
            {
                float distance = Vector2.Distance(playerTransform.position, col.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closest = gatherObj;
                }
            }
        }

        return closest;
    }

    /// <summary>
    /// ���� ����� NPC ã��
    /// </summary>
    private NPCController FindNearestNPC()
    {
        int npcLayer = LayerMask.GetMask(Def_Name.LAYER_NPC);
        Collider2D[] nearbyColliders = Physics2D.OverlapCircleAll(playerTransform.position, detectionRadius, npcLayer);

        NPCController closest = null;
        float closestDistance = float.MaxValue;

        foreach (var col in nearbyColliders)
        {
            NPCController npc = col.GetComponent<NPCController>();
            if (npc != null)
            {
                float distance = Vector2.Distance(playerTransform.position, col.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closest = npc;
                }
            }
        }

        return closest;
    }

    /// <summary>
    /// ��� ������Ʈ ����
    /// </summary>
    private void HideAllPrompts()
    {
        currentNearestGathering?.HidePrompt();
        currentNearestGathering = null;

        currentNearestNPC?.HidePrompt();
        currentNearestNPC = null;
    }

    /// <summary>
    /// ��ȣ�ۿ� �Է� ó�� (EŰ)
    /// </summary>
    public void HandleInteractionInput()
    {
        if (ControlsLocked || IsGathering) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            // ä������ ���õ� ���
            if (currentNearestGathering != null)
            {
                PlayerController.Instance.StartCoroutine(GatherCoroutine(currentNearestGathering));
            }
            // NPC�� ���õ� ���
            else if (currentNearestNPC != null)
            {
                InteractWithNPC(currentNearestNPC);
            }
        }
    }

    /// <summary>
    /// NPC�� ��ȣ�ۿ�
    /// </summary>
    private void InteractWithNPC(NPCController npc)
    {
        // NPC �������� �÷��̾� ȸ��
        Vector2 directionToNPC = (npc.transform.position - playerTransform.position).normalized;
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.SetFacingDirection(directionToNPC);
        }

        // NPC ��ȣ�ۿ� ����
        npc.Interact();

        // ������Ʈ ����
        npc.HidePrompt();
        currentNearestNPC = null;
    }

    /// <summary>
    /// ä�� �ڷ�ƾ
    /// </summary>
    private IEnumerator GatherCoroutine(GatheringObject targetObject)
    {
        IsGathering = true;

        // 1. ä���� �������� �÷��̾� ȸ��
        Vector2 directionToTarget = (targetObject.transform.position - playerTransform.position).normalized;
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.SetFacingDirection(directionToTarget);
        }

        // 2. �÷��̾� ���� ���
        if (PlayerController.Instance != null)
            PlayerController.Instance.SetControlsLocked(true);

        // 3. ä�� �ִϸ��̼� ����
        animationController?.PlayAnimation("Gather");

        // 4. ä�� ���� (3�� ���)
        yield return new WaitForSeconds(3f);

        // 5. ���� ä�� ó��
        if (targetObject != null)
        {
            targetObject.Gather();
        }

        // 6. �÷��̾� ���� ����
        animationController?.PlayAnimation("Idle");
        if (PlayerController.Instance != null)
            PlayerController.Instance.SetControlsLocked(false);

        IsGathering = false;
        currentNearestGathering = null;
    }

    /// <summary>
    /// ���� ������ ��� ��ȯ (������)
    /// </summary>
    public object GetCurrentTarget()
    {
        if (currentNearestGathering != null) return currentNearestGathering;
        if (currentNearestNPC != null) return currentNearestNPC;
        return null;
    }
}