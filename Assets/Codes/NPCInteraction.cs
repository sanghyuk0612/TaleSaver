using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class NPCInteraction : MonoBehaviour
{
    public static NPCInteraction Instance { get; private set; }

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
    private bool isDialogueActive = false;

    private GameObject player;
    public bool hasHealed = false;

    private enum DialogueState { Normal, Event, Yes, No, Exception }
    private DialogueState currentState = DialogueState.Normal;

    private bool isPlayerInRange = false;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        hasHealed = false;

        if (player == null)
        {
            Debug.LogWarning("Player not found! Make sure the player object has the 'Player' tag.");
        }

    }

    private void Update()
    {
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.Z))
        {
            OnNPCButtonClicked();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
        }
    }

    public void OnNPCButtonClicked()
    {
        Debug.Log("OnNPCButtonClicked!");

        if (isDialogueActive)
            return;

        if (player != null)
        {
            player.GetComponent<PlayerController>().isControllable = false;
        }


        if (hasInteractedOnce && isEventNPC)
        {
            SetDialogueState(DialogueState.Exception); // 첫 상호작용 이후 예외 대화로 설정
        }
        else
        {
            SetDialogueState(DialogueState.Normal);
            hasInteractedOnce = true; // 첫 대화 완료로 표시
        }

        isDialogueActive = true;
        HideYesNoButtons();
        dialoguePanel.SetActive(true);
        DisplayNextDialogue();
    }

    private void RestoreHealth()
    {
        if (hasHealed) return; // 이미 회복했으면 무시

        player.GetComponent<PlayerController>().Heal(100);
        Debug.Log("Player HP fully restored!");
        hasHealed = true;

    }

    private void NPCEvent()
    {
        // 랜덤 아이템 지급
        if (InventoryManager.Instance != null)
        {
            int randomQuantity = Random.Range(0, 5);
            int randomId = Random.Range(0, 5);
            string randomItemName = InventoryManager.Instance.GetItemNameById(randomId);
            InventoryManager.Instance.AddItem(randomId, randomQuantity);

            Debug.Log($"{randomItemName} 아이템을 {randomQuantity} 개 얻음!");
        }
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
        RestoreHealth();
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
        NPCEvent(); // Yes 선택 시 이벤트 실행
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
        isDialogueActive = false;
        HideYesNoButtons();
        currentState = DialogueState.Normal;

        if (player != null)
        {
            player.GetComponent<PlayerController>().isControllable = true;
        }

    }
}
