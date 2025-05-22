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
            // 패널이 비활성화되어 있으면 활성화하고, 이미 켜져있으면 끄기 (토글 방식)
            if (settingPanel != null)
                settingPanel.SetActive(!settingPanel.activeSelf);
        }
    }
}
