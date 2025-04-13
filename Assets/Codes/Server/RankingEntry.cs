using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RankingEntry : MonoBehaviour
{
    public TextMeshProUGUI placeText;
    public TextMeshProUGUI playerIDText;
    public TextMeshProUGUI playcharacterText;
    public TextMeshProUGUI cleartimeText;
    public GameObject detailPopupCanvas;
    private string playerCharacter; // 추가
    private Vector2 defaultPopupPosition;
private bool defaultPositionSet = false;

    public Button detailButton;

    private string playerId;

    public void SetPlayerEntry(string id, string character, string clearTime, int rank)
    {
        playerId = id;
        playerCharacter = character;  // 저장해둠
        placeText.text = rank.ToString();
        playerIDText.text = id;
        playcharacterText.text = character;
        cleartimeText.text = clearTime;

        if (detailButton != null)
        {
            detailButton.onClick.RemoveAllListeners();
            detailButton.onClick.AddListener(OnDetailButtonClick);
        }
    }

    private void OnDetailButtonClick()
    {

        if (detailPopupCanvas != null)
        {
            detailPopupCanvas.SetActive(true);
            Debug.Log("[DetailPopupCanvas 활성화됨]");
        }
        else
        {
            Debug.LogError("Inspector에 DetailPopupCanvas가 연결되지 않았습니다.");
        }

        PlayerDetailManager manager = FindObjectOfType<PlayerDetailManager>();
        if (manager != null)
        {
            manager.LoadPlayerDetail(playerId, playerCharacter);
        }
        else
        {
            Debug.LogError("PlayerDetailManager를 찾을 수 없습니다.");
        }
    
    }

}
