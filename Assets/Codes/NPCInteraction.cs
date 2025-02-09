using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class NPCInteraction : MonoBehaviour
{
    public GameObject dialoguePanel;
    public Text dialogueText;
    public Button nextButton;
    public Button yesButton;
    public Button noButton;

    public string[] normalDialogue;
    public string[] eventDialogue;
    public string[] yesDialogue;
    public string[] noDialogue;
    public string[] exceptionDialogue; // 예외 대화 추가

    public bool isEventNPC = false;
    public float eventProbability = 0.8f; // 이벤트 발생 확률 (0.3 = 30%)

    private int currentDialogueIndex = 0;
    private string[] Dialogues;
    private bool hasEventOccurred = false; // 이벤트 대화 완료 여부
    private bool hasInteractedOnce = false; // NPC와 첫 대화 완료 여부
    private static bool isDialogueActive = false; // 대화창 활성화 상태

    private enum DialogueState { Normal, Event, Yes, No, Exception }
    private DialogueState currentState = DialogueState.Normal;

    private void Start()
    {
        nextButton.onClick.AddListener(DisplayNextDialogue);
        yesButton.onClick.AddListener(OnYesButtonClicked);
        noButton.onClick.AddListener(OnNoButtonClicked);

        dialoguePanel.SetActive(false);
        yesButton.gameObject.SetActive(false);
        noButton.gameObject.SetActive(false);
    }

    public void OnNPCButtonClicked()
    {
        if (isDialogueActive)
            return; // 대화창이 열려 있으면 클릭 무시

        if (hasInteractedOnce && isEventNPC)
        {
            SetDialogueState(DialogueState.Exception); // 첫 상호작용 이후 예외 대화로 설정
        }
        else
        {
            SetDialogueState(DialogueState.Normal);
            hasInteractedOnce = true; // 첫 대화 완료로 표시
        }

        isDialogueActive = true; // 대화 활성화
        dialoguePanel.SetActive(true);
        DisplayNextDialogue();
    }

    private void SetDialogueState(DialogueState newState)
    {
        currentState = newState;
        currentDialogueIndex = 0;

        switch (currentState)
        {
            case DialogueState.Normal:
                Dialogues = normalDialogue;
                break;
            case DialogueState.Event:
                Dialogues = eventDialogue;
                break;
            case DialogueState.Yes:
                Dialogues = yesDialogue;
                break;
            case DialogueState.No:
                Dialogues = noDialogue;
                break;
            case DialogueState.Exception:
                Dialogues = exceptionDialogue;
                break;
        }
    }

    public void DisplayNextDialogue()
    {
        if (currentDialogueIndex < Dialogues.Length)
        {
            dialogueText.text = Dialogues[currentDialogueIndex];
            currentDialogueIndex++;
        }
        else
        {
            EndOrTransitionDialogue();
        }
    }

    private void EndOrTransitionDialogue()
    {
        if (currentState == DialogueState.Normal && isEventNPC && !hasEventOccurred)
        {
            if (Random.value < eventProbability)
            {
                SetDialogueState(DialogueState.Event);
                hasEventOccurred = true;
                DisplayNextDialogue();
            }
            else
            {
                EndDialogue(); // 이벤트가 발생하지 않으면 대화 종료
            }
        }
        else if (currentState == DialogueState.Event)
        {
            OnEventDialogueFinished();
        }
        else
        {
            EndDialogue();
        }
    }

    private void OnEventDialogueFinished()
    {
        yesButton.gameObject.SetActive(true);
        noButton.gameObject.SetActive(true);
        nextButton.gameObject.SetActive(false);
    }

    public void OnYesButtonClicked()
    {
        SetDialogueState(DialogueState.Yes);
        HideYesNoButtons();
        nextButton.gameObject.SetActive(true);
        DisplayNextDialogue();
    }

    public void OnNoButtonClicked()
    {
        SetDialogueState(DialogueState.No);
        HideYesNoButtons();
        nextButton.gameObject.SetActive(true);
        DisplayNextDialogue();
    }

    private void HideYesNoButtons()
    {
        yesButton.gameObject.SetActive(false);
        noButton.gameObject.SetActive(false);
    }

    private void EndDialogue()
    {
        dialoguePanel.SetActive(false);
        HideYesNoButtons();
        currentState = DialogueState.Normal;
        isDialogueActive = false; // 대화 비활성화
    }
}
