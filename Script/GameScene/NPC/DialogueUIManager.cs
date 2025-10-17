using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Definitions;

// ��ȭ/����Ʈ/���� UI �Ŵ��� (������ ����)
public class DialogueUIManager : MonoBehaviour
{
    public static DialogueUIManager Instance { get; private set; }

    [Header("UI References (�ɼ�)")]
    public GameObject interactHintPanel; // EŰ ��Ʈ
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

        // ��ư ���� ���� ó��
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

        // �÷��̾� ���� ���
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
            // ��ȭ�� �ܺο��� �������� Ȯ��
            if (currentNPC == null) yield break;

            if (dialogueText != null) dialogueText.text = line;
            Debug.Log(Def_UI.DIALOGUE_PREFIX + line);

            // Ű �Է�(E �Ǵ� Space)���� �������� ����
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

        // ������ UI Ȱ��ȭ
        if (questChoicePanel != null)
        {
            questChoicePanel.SetActive(true);

            // Ű �Է����� ���� ��� (����: E, �ź�: Q �Ǵ� Esc)
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
            // UI�� ������ �ַܼ� ����(�ڵ� �ź�)
            Debug.Log(Def_UI.QUEST_NO_UI);
            OnDeclineQuest();
        }
    }

    private IEnumerator WaitForAdvanceKey()
    {
        // ��ȭ ���� Ű: E �Ǵ� Space
        while (true)
        {
            if (currentNPC == null) yield break;

            if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space))
            {
                // �� ������ ����ؼ� �Է��� �ٸ� ó���� ��ġ�� �ʵ��� ��
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
        // ���� ���� ������ ���⼭ Ȯ��
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

        // TODO: ����Ʈ ������ ���� ���� ȣ�� (����Ʈ �Ŵ��� ��)
        CloseDialogue();
    }

    public void CloseDialogue()
    {
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (questChoicePanel != null) questChoicePanel.SetActive(false);
        if (shopPanel != null) shopPanel.SetActive(false);

        // �÷��̾� ���� ����
        if (PlayerController.Instance != null)
            PlayerController.Instance.SetControlsLocked(false);

        currentNPC = null;
    }
}