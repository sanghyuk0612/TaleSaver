using UnityEngine;
using Firebase.Firestore;
using Firebase;
using Firebase.Extensions;
using System.Collections.Generic;

public class PlayerDetailManager : MonoBehaviour
{
    private FirebaseFirestore db;
    private bool isFirebaseInitialized = false;

    [SerializeField] private PlayerDetailUI playerDetailUI;

    private void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                db = FirebaseFirestore.DefaultInstance;
                isFirebaseInitialized = true;
                Debug.Log("✅ PlayerDetailManager Firebase 초기화 완료");
            }
            else
            {
                Debug.LogError("❌ Firebase 초기화 실패: " + task.Result);
            }
        });
    }

    public void LoadPlayerDetail(string playerId)
    {
        if (!isFirebaseInitialized)
        {
            Debug.LogError("❗ Firebase가 아직 초기화되지 않았습니다.");
            return;
        }

        db.Collection("players").Document(playerId).GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || !task.Result.Exists)
            {
                Debug.LogError("❌ 플레이어 데이터를 가져오는데 실패했거나 존재하지 않음.");
                return;
            }

            Dictionary<string, object> data = task.Result.ToDictionary();
            if (playerDetailUI != null)
            {
                playerDetailUI.UpdateDetailUI(data); // 딕셔너리 그대로 전달
            }
            else
            {
                Debug.LogError("PlayerDetailUI가 연결되지 않았습니다.");
            }
        });
    }
}
