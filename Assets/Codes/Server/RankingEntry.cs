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

    public Button detailButton;

    private string playerId;

    public void SetPlayerEntry(string id, string character, string clearTime, int rank)
    {
        playerId = id;
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
        Debug.Log($"[Detail 버튼 클릭됨] playerId: {playerId}");

        GameObject popup = GameObject.Find("DetailPopupCanvas");
        if (popup != null)
        {
            popup.SetActive(true);
        }

        var manager = FindObjectOfType<PlayerDetailManager>();
        if (manager != null)
        {
            manager.LoadPlayerDetail(playerId);
        }
        else
        {
            Debug.LogError("PlayerDetailManager를 찾을 수 없습니다.");
        }
    }
}
