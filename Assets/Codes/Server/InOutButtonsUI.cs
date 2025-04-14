using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InOutButtonsUI : MonoBehaviour
{
    public Button toggleOnOffButton;
    public Button buttonX;

    private void Start()
    {
        if (toggleOnOffButton != null)
            toggleOnOffButton.onClick.AddListener(RestartGame);

        if (buttonX != null)
            buttonX.onClick.AddListener(QuitGame);
    }

    private void RestartGame()
    {
        Debug.Log("ToggleOnOff 버튼 클릭됨 - Lobby 씬으로 이동");
        SceneManager.LoadScene("Lobby"); // Lobby 씬 이름 정확히 입력
    }

    public void QuitGame()
    {
        Debug.Log("게임 종료 요청됨");

        if (Application.isEditor)
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#endif
        }
        else
        {
            Application.Quit();
        }
    }

}
