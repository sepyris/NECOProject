using System.Collections;
using UnityEngine;
using Definitions;

/// <summary>
/// 플레이어의 채집 및 NPC 상호작용 관리
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
    /// 매 프레임 가장 가까운 상호작용 오브젝트 감지 (채집물 또는 NPC)
    /// </summary>
    public void UpdateNearestInteractable()
    {
        if (ControlsLocked || IsGathering)
        {
            HideAllPrompts();
            return;
        }

        // 1. 가장 가까운 채집물 찾기
        GatheringObject closestGathering = FindNearestGathering();

        // 2. 가장 가까운 NPC 찾기
        NPCController closestNPC = FindNearestNPC();

        // 3. 둘 중 더 가까운 것 선택
        float gatheringDist = closestGathering != null
            ? Vector2.Distance(playerTransform.position, closestGathering.transform.position)
            : float.MaxValue;

        float npcDist = closestNPC != null
            ? Vector2.Distance(playerTransform.position, closestNPC.transform.position)
            : float.MaxValue;

        // 채집물이 더 가까운 경우
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
        // NPC가 더 가까운 경우
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
        // 둘 다 없는 경우
        else
        {
            HideAllPrompts();
        }
    }

    /// <summary>
    /// 가장 가까운 채집물 찾기
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
    /// 가장 가까운 NPC 찾기
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
    /// 모든 프롬프트 숨김
    /// </summary>
    private void HideAllPrompts()
    {
        currentNearestGathering?.HidePrompt();
        currentNearestGathering = null;

        currentNearestNPC?.HidePrompt();
        currentNearestNPC = null;
    }

    /// <summary>
    /// 상호작용 입력 처리 (E키)
    /// </summary>
    public void HandleInteractionInput()
    {
        if (ControlsLocked || IsGathering) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            // 채집물이 선택된 경우
            if (currentNearestGathering != null)
            {
                PlayerController.Instance.StartCoroutine(GatherCoroutine(currentNearestGathering));
            }
            // NPC가 선택된 경우
            else if (currentNearestNPC != null)
            {
                InteractWithNPC(currentNearestNPC);
            }
        }
    }

    /// <summary>
    /// NPC와 상호작용
    /// </summary>
    private void InteractWithNPC(NPCController npc)
    {
        // NPC 방향으로 플레이어 회전
        Vector2 directionToNPC = (npc.transform.position - playerTransform.position).normalized;
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.SetFacingDirection(directionToNPC);
        }

        // NPC 상호작용 시작
        npc.Interact();

        // 프롬프트 숨김
        npc.HidePrompt();
        currentNearestNPC = null;
    }

    /// <summary>
    /// 채집 코루틴
    /// </summary>
    private IEnumerator GatherCoroutine(GatheringObject targetObject)
    {
        IsGathering = true;

        // 1. 채집물 방향으로 플레이어 회전
        Vector2 directionToTarget = (targetObject.transform.position - playerTransform.position).normalized;
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.SetFacingDirection(directionToTarget);
        }

        // 2. 플레이어 조작 잠금
        if (PlayerController.Instance != null)
            PlayerController.Instance.SetControlsLocked(true);

        // 3. 채집 애니메이션 실행
        animationController?.PlayAnimation("Gather");

        // 4. 채집 진행 (3초 대기)
        yield return new WaitForSeconds(3f);

        // 5. 실제 채집 처리
        if (targetObject != null)
        {
            targetObject.Gather();
        }

        // 6. 플레이어 조작 복구
        animationController?.PlayAnimation("Idle");
        if (PlayerController.Instance != null)
            PlayerController.Instance.SetControlsLocked(false);

        IsGathering = false;
        currentNearestGathering = null;
    }

    /// <summary>
    /// 현재 감지된 대상 반환 (디버깅용)
    /// </summary>
    public object GetCurrentTarget()
    {
        if (currentNearestGathering != null) return currentNearestGathering;
        if (currentNearestNPC != null) return currentNearestNPC;
        return null;
    }
}