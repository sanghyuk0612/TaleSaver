using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingPanelManager : MonoBehaviour
{
    public GameObject settingPanel; // 인스펙터에서 설정할 SettingPanel 오브젝트

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (settingPanel != null)
            {
                bool isOpening = !settingPanel.activeSelf;
                settingPanel.SetActive(isOpening);

                // 패널을 켤 때 UI 재연결 시도
                if (isOpening && BGMManager.instance != null)
                {
                    BGMManager.instance.TryReconnectUI();
                }

                // 게임 일시정지/재개도 같이 적용
                Time.timeScale = isOpening ? 0f : 1f;
            }
        }
    }
}