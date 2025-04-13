using UnityEngine;

public class CloseDetailPopup : MonoBehaviour
{
    public GameObject DetailPopupCanvas;

    public void ClosePopup()
    {
        if (DetailPopupCanvas != null)
        {
            DetailPopupCanvas.SetActive(false);
            Debug.Log("[DetailPopupCanvas 비활성화됨]");
        }
        else
        {
            Debug.LogError("detailPopupCanvas가 연결되지 않았습니다.");
        }
    }
}
