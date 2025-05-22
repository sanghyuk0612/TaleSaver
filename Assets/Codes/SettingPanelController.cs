using UnityEngine;

public class SettingPanelController : MonoBehaviour
{
    public GameObject settingPanel;  // 인스펙터에서 설정
    private bool isPanelOpen = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleSettingPanel();
        }
    }

    public void ToggleSettingPanel()
    {
        isPanelOpen = !isPanelOpen;
        settingPanel.SetActive(isPanelOpen);

        // 게임 일시정지/재개
        Time.timeScale = isPanelOpen ? 0f : 1f;

        if (isPanelOpen && BGMManager.instance != null)
        {
            BGMManager.instance.TryReconnectUI(); // 패널이 열릴 때만 연결 시도
        }
    }

    // 닫기 버튼에서 호출할 수 있도록 별도 메서드
    public void ClosePanel()
    {
        isPanelOpen = false;
        settingPanel.SetActive(false);
        Time.timeScale = 1f;
    }
}
