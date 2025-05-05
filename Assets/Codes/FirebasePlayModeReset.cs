using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using Firebase.Auth;

[InitializeOnLoad]
public static class FirebasePlayModeReset
{
    static FirebasePlayModeReset()
    {
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
    }

    private static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode)
        {
            Debug.Log("?? 에디터 플레이 모드 진입 → Firebase 로그아웃 실행");
            FirebaseAuth.DefaultInstance?.SignOut();
        }
    }
}