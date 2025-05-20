using UnityEditor;
using UnityEngine;

public class PlayerPrefsResetMenu
{
    [MenuItem("Tools/초기화/PlayerPrefs 전체 삭제")]
    public static void ResetPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
        Debug.Log("PlayerPrefs 초기화 완료");
    }
}
