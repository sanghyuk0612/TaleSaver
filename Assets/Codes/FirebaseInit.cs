using System.Collections;
using UnityEngine;
using Firebase;
using Firebase.Analytics;
using Firebase.Extensions;

public class FirebaseInit : MonoBehaviour
{
    void Start()
    {
        Debug.Log("?? FirebaseInit Start() 시작됨");

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            Debug.Log("?? FirebaseApp.CheckAndFixDependenciesAsync 완료됨");

            var dependencyStatus = task.Result;

            if (dependencyStatus == DependencyStatus.Available)
            {
                FirebaseApp app = FirebaseApp.DefaultInstance;
                FirebaseAnalytics.LogEvent("unity_start");
                Debug.Log("? Firebase 초기화 완료!");

                // 여기서 코루틴 실행
                StartCoroutine(WaitAndInitAuth());
            }
            else
            {
                Debug.LogError($"? Firebase 초기화 실패: {dependencyStatus}");
            }
        });
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
