using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameStartButton : MonoBehaviour
{
    public GameObject settingsPanel;  // SettingsPanel을 참조

    private void Start()
    {
        // 게임 시작 시 설정 창을 비활성화
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }

    // 게임 시작 버튼 클릭
    public void OnGameStartButtonClick()
    {
        SceneManager.LoadScene("Lobby");  // "Lobby"는 로비 씬의 이름
    }

    // 게임 종료 버튼 클릭
    public void OnGameExitButtonClick()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;  // 에디터에서 게임을 종료
#else
            Application.Quit();  // 빌드된 게임에서 종료
#endif
    }

    // 설정 창 열기
    public void OnSettingsOpen()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
        }
    }

    // 설정 창 닫기
    public void OnSettingsClose()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }
}
