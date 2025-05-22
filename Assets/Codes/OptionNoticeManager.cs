using UnityEngine;
using TMPro;

public class OptionNoticeManager : MonoBehaviour
{
    public GameObject noticeImage; // Image 오브젝트 전체
    public TextMeshProUGUI noticeText;
    public float blinkSpeed = 2f;
    public float minAlpha = 0.2f;
    public float maxAlpha = 1f;

    private bool isBlinking = false;
    private Color originalColor;

    void Start()
    {
        // 최초 실행 체크
        if (!PlayerPrefs.HasKey("OptionNoticeShown"))
        {
            ShowNotice();
        }
        else
        {
            noticeImage.SetActive(false);
        }
    }

    void Update()
    {
        if (isBlinking)
        {
            float alpha = Mathf.Lerp(minAlpha, maxAlpha, Mathf.PingPong(Time.time * blinkSpeed, 1f));
            Color newColor = originalColor;
            newColor.a = alpha;
            noticeText.color = newColor;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // 옵션창 열기 동작 여기에 연결해야 함 (별도 OptionManager 등에서 처리)
            HideNoticePermanently();
        }
    }

    void ShowNotice()
    {
        noticeImage.SetActive(true);
        originalColor = noticeText.color;
        isBlinking = true;
        noticeText.text = "로비와 게임 내에서는 ESC를 눌러 옵션창을 열고 닫을 수 있습니다.";
    }

    public void HideNoticePermanently()
    {
        isBlinking = false;
        noticeImage.SetActive(false);
        PlayerPrefs.SetInt("OptionNoticeShown", 1); // 다시 안보이도록 저장
        PlayerPrefs.Save();
    }
}
