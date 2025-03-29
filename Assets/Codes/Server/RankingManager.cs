
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.ComponentModel.Design;
using Firebase.Firestore;
using Firebase;
using Firebase.Extensions;
using System.Collections.Generic;

public class RankingManager : MonoBehaviour
{
    private FirebaseFirestore db;
    private bool isFirebaseInitialized = false;

    void Start()
    {
        InitializeFirebase();
    }

    private void InitializeFirebase()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                FirebaseApp app = FirebaseApp.DefaultInstance;
                db = FirebaseFirestore.DefaultInstance;
                isFirebaseInitialized = true;
                Debug.Log("✅ Firebase 초기화 완료!");

                // Firebase가 정상적으로 초기화되면 데이터 로드
                LoadData();
            }
            else
            {
                Debug.LogError("❌ Firebase 초기화 실패: " + task.Result);
            }
        });
    }

    private void LoadData()
    {
        if (!isFirebaseInitialized)
        {
            Debug.LogError("❗ Firebase가 아직 초기화되지 않았습니다.");
            return;
        }

        db.Collection("rankings").GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("🔥 Firestore 데이터를 가져오는 데 실패했습니다: " + task.Exception);
                return;
            }

            foreach (var document in task.Result.Documents)
            {
                // ✅ Firestore 문서 데이터를 Dictionary로 변환
                Dictionary<string, object> rankingData = document.ToDictionary();

                // ✅ 필요한 값들을 가져오기 (형 변환 필수)
                string playerId = document.Id; // 문서 ID (필요하면 출력)
                string cleartime = rankingData.ContainsKey("cleartime") ? rankingData["cleartime"].ToString() : "N/A";
                string playcharacter = rankingData.ContainsKey("playcharacter") ? rankingData["playcharacter"].ToString() : "Unknown";
                string playerID = rankingData.ContainsKey("playerId") ? rankingData["playerId"].ToString() : "Unknown";
                int rank = rankingData.ContainsKey("rank") ? System.Convert.ToInt32(rankingData["rank"]) : -1;

                // ✅ 가독성 좋은 출력
                Debug.Log($"🏆 랭킹 데이터: Player ID: {playerID} | Rank: {rank} | Character: {playcharacter} | Clear Time: {cleartime}");
            }
        });
    }
}