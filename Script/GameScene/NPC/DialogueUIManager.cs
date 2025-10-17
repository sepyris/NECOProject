using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Definitions;

// 대화/퀘스트/상점 UI 매니저 (간단한 구현)
public class DialogueUIManager : MonoBehaviour
{
    public static DialogueUIManager Instance { get; private set; }

    [Header("UI References (옵션)")]
    public GameObject interactHintPanel; // E키 힌트
    public Text dialogueText;
    public GameObject dialoguePanel;
    public GameObject questChoicePanel;
    public Button acceptButton;
    public Button declineButton;
    public GameObject shopPanel;

    private NPCController currentNPC;

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else Destroy(gameObject);

        // 버튼 연결 안전 처리
        if (acceptButton != null) acceptButton.onClick.AddListener(OnAcceptQuest);
        if (declineButton != null) declineButton.onClick.AddListener(OnDeclineQuest);
    }

    public void ShowInteractHint(bool show, NPCController npc)
    {
        if (interactHintPanel != null)
        {
            interactHintPanel.SetActive(show);
        }
        else if (show)
        {
            Debug.Log(string.Format(Def_UI.UI_INTERACT_HINT, npc != null ? npc.interactKey.ToString() : Def_UI.INTERACT_KEY_LABEL));
        }
    }

    public void OpenInteraction(NPCController npc)
    {
        if (currentNPC != null)
        {
            Debug.Log(Def_UI.DIALOGUE_ALREADY_INTERACTING);
            return;
        }

        currentNPC = npc;

        // 플레이어 제어 잠금
        if (PlayerController.Instance != null)
            PlayerController.Instance.SetControlsLocked(true);

        switch (npc.npcType)
        {
            case NPCController.NPCType.Dialogue:
                StartCoroutine(ShowDialogueCoroutine(npc.dialogueLines));
                break;
            case NPCController.NPCType.Quest:
                StartCoroutine(ShowQuestOfferCoroutine(npc));
                break;
            case NPCController.NPCType.Shop:
                OpenShop(npc);
                break;
        }
    }

    private IEnumerator ShowDialogueCoroutine(List<string> lines)
    {
        if (dialoguePanel != null) dialoguePanel.SetActive(true);

        foreach (var line in lines)
        {
            // 대화가 외부에서 닫혔는지 확인
            if (currentNPC == null) yield break;

            if (dialogueText != null) dialogueText.text = line;
            Debug.Log(Def_UI.DIALOGUE_PREFIX + line);

            // 키 입력(E 또는 Space)으로 다음으로 진행
            yield return StartCoroutine(WaitForAdvanceKey());
        }

        CloseDialogue();
    }

    private IEnumerator ShowQuestOfferCoroutine(NPCController npc)
    {
        if (dialoguePanel != null) dialoguePanel.SetActive(true);
        if (questChoicePanel != null) questChoicePanel.SetActive(false);

        // offer lines
        foreach (var line in npc.questOfferLines)
        {
            if (currentNPC == null) yield break;

            if (dialogueText != null) dialogueText.text = line;
            Debug.Log(Def_UI.QUEST_OFFER_PREFIX + line);
            yield return StartCoroutine(WaitForAdvanceKey());
        }

        // 선택지 UI 활성화
        if (questChoicePanel != null)
        {
            questChoicePanel.SetActive(true);

            // 키 입력으로 선택 대기 (수락: E, 거부: Q 또는 Esc)
            bool decisionMade = false;
            while (!decisionMade)
            {
                if (currentNPC == null) yield break;

                if (Input.GetKeyDown(KeyCode.E))
                {
                    OnAcceptQuest();
                    decisionMade = true;
                    break;
                }
                if (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.Escape))
                {
                    OnDeclineQuest();
                    decisionMade = true;
                    break;
                }
                yield return null;
            }
        }
        else
        {
            // UI가 없으면 콘솔로 선택(자동 거부)
            Debug.Log(Def_UI.QUEST_NO_UI);
            OnDeclineQuest();
        }
    }

    private IEnumerator WaitForAdvanceKey()
    {
        // 대화 진행 키: E 또는 Space
        while (true)
        {
            if (currentNPC == null) yield break;

            if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space))
            {
                // 한 프레임 대기해서 입력이 다른 처리와 겹치지 않도록 함
                yield return null;
                yield break;
            }

            yield return null;
        }
    }

    private void OpenShop(NPCController npc)
    {
        if (shopPanel != null) shopPanel.SetActive(true);
        Debug.Log(Def_UI.SHOP_OPEN_PREFIX + npc.shopId);
        // 실제 상점 로직은 여기서 확장
    }

    public void OnAcceptQuest()
    {
        if (currentNPC == null) return;
        StartCoroutine(HandleQuestResult(currentNPC.questAcceptedLines, true));
    }

    public void OnDeclineQuest()
    {
        if (currentNPC == null) return;
        StartCoroutine(HandleQuestResult(currentNPC.questDeclinedLines, false));
    }

    private IEnumerator HandleQuestResult(List<string> lines, bool accepted)
    {
        if (questChoicePanel != null) questChoicePanel.SetActive(false);

        foreach (var line in lines)
        {
            if (currentNPC == null) yield break;

            if (dialogueText != null) dialogueText.text = line;
            Debug.Log((accepted ? Def_UI.QUEST_ACCEPTED_PREFIX : Def_UI.QUEST_DECLINED_PREFIX) + line);
            yield return StartCoroutine(WaitForAdvanceKey());
        }

        // TODO: 퀘스트 수락시 게임 로직 호출 (퀘스트 매니저 등)
        CloseDialogue();
    }

    public void CloseDialogue()
    {
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (questChoicePanel != null) questChoicePanel.SetActive(false);
        if (shopPanel != null) shopPanel.SetActive(false);

        // 플레이어 제어 해제
        if (PlayerController.Instance != null)
            PlayerController.Instance.SetControlsLocked(false);

        currentNPC = null;
    }
}