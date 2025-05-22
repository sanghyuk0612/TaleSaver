using UnityEngine;
using TMPro;

public class OffNoticeManager : MonoBehaviour
{
    public GameObject offNoticeImage;
    public GameObject settingsPanel;
    public TextMeshProUGUI offNoticeText;

    private bool shownOnce = false;
    private bool wasSettingsPanelActive = false;

    void Start()
    {
        if (PlayerPrefs.HasKey("OffNoticeShown"))
        {
            shownOnce = true;
            offNoticeImage.SetActive(false);
        }
    }

    void Update()
    {
        if (shownOnce) return;

        bool isNowActive = settingsPanel.activeInHierarchy;

        // 설정창이 꺼져 있다가 켜진 순간을 감지
        if (!wasSettingsPanelActive && isNowActive)
        {
            ShowNotice();
        }

        wasSettingsPanelActive = isNowActive;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            HideNoticePermanently();
        }
    }

    void ShowNotice()
    {
        offNoticeImage.SetActive(true);
        offNoticeText.text = "로비와 게임 내에서는 ESC를 눌러 옵션창을 열어볼 수 있습니다";
        shownOnce = true;
        PlayerPrefs.SetInt("OffNoticeShown", 1);
        PlayerPrefs.Save();
    }

    void HideNoticePermanently()
    {
        offNoticeImage.SetActive(false);
    }
}
