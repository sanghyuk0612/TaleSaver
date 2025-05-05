using System.Collections;
using UnityEngine;
using Firebase;
using Firebase.Analytics;
using Firebase.Extensions;
using Firebase.Auth;

public class FirebaseInit : MonoBehaviour
{
    void Start()
    {
        Debug.Log("?? FirebaseInit Start() 시작됨");

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            var dependencyStatus = task.Result;

            if (dependencyStatus == DependencyStatus.Available)
            {
                FirebaseApp app = FirebaseApp.DefaultInstance;
                FirebaseAnalytics.LogEvent("unity_start");
                Debug.Log("? Firebase 초기화 완료!");

                FirebaseAuth.DefaultInstance.SignOut();
                Debug.Log("?? Firebase 자동 로그아웃 실행");

                // ?? logout 반영 기다린 뒤 초기화
                StartCoroutine(WaitForLogoutThenInitAuth());
            }
            else
            {
                Debug.LogError($"? Firebase 초기화 실패: {dependencyStatus}");
            }
        });
    }

    private IEnumerator WaitForLogoutThenInitAuth()
    {
        float timeout = 5f;
        float timer = 0f;

        // ?? SignOut()이 비동기라서 CurrentUser가 null이 될 때까지 기다림
        while (FirebaseAuth.DefaultInstance.CurrentUser != null && timer < timeout)
        {
            Debug.Log($"? 로그아웃 대기 중... UID: {FirebaseAuth.DefaultInstance.CurrentUser.UserId}");
            timer += Time.deltaTime;
            yield return null;
        }

        if (FirebaseAuth.DefaultInstance.CurrentUser == null)
        {
            Debug.Log("? 로그아웃 반영 완료. 초기화로 진행합니다.");
        }
        else
        {
            Debug.LogWarning("?? 로그아웃 반영되지 않음. 시간 초과로 강제 진행합니다.");
        }

        StartCoroutine(WaitAndInitAuth());  // 기존 초기화 코루틴 실행
    }

    IEnumerator WaitAndInitAuth()
    {
        float timer = 0f;
        float timeout = 5f;

        while ((FirebaseAuthManager.Instance == null || !FirebaseAuthManager.Instance.isActiveAndEnabled) && timer < timeout)
        {
            Debug.Log($"? 기다리는 중... FirebaseAuthManager.Instance == null ? {FirebaseAuthManager.Instance == null}");
            timer += Time.deltaTime;
            yield return null;
        }

        if (FirebaseAuthManager.Instance != null)
        {
            Debug.Log("? FirebaseAuthManager 인스턴스 발견됨! 초기화 호출");
            FirebaseAuthManager.Instance.OnFirebaseInitialized();
        }
        else
        {
            Debug.LogError("? FirebaseAuthManager.Instance 여전히 null입니다. 초기화 실패");
        }
    }
}
