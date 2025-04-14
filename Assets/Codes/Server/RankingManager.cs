using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.ComponentModel.Design;
using Firebase.Firestore;
using Firebase;
using Firebase.Extensions;
using System.Collections.Generic;
using System;

public class RankingManager : MonoBehaviour
{
    private FirebaseFirestore db;
    private bool isFirebaseInitialized = false;
    public RankingUI rankingUI;

    void Start()
    {
        rankingUI = FindObjectOfType<RankingUI>();
        if (rankingUI == null)
        {
            Debug.LogError("❌ RankingUI가 씬에서 찾을 수 없습니다.");
            return;
        }
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

            List<PlayerData> rankingList = new List<PlayerData>();

            foreach (var document in task.Result.Documents)
            {
                Dictionary<string, object> rankingData = document.ToDictionary();
                try
                {
                    string playerId = document.Id;
                    string cleartime = rankingData.ContainsKey("cleartime") ? rankingData["cleartime"].ToString() : "00:00";
                    string playcharacter = rankingData.ContainsKey("playcharacter") ? rankingData["playcharacter"].ToString() : "Unknown";
                    string playerID = rankingData.ContainsKey("playerId") ? rankingData["playerId"].ToString() : "Unknown";
                    int rank = rankingData.ContainsKey("rank") ? System.Convert.ToInt32(rankingData["rank"]) : -1;

                    rankingList.Add(new PlayerData(playerID, playcharacter, cleartime, rank));
                    Debug.Log($"🏆 랭킹 데이터: Player ID: {playerID} | Rank: {rank} | Character: {playcharacter} | Clear Time: {cleartime}");
                }
                catch (Exception e)
                {
                    Debug.LogError($"❌ 예외 발생: {e.Message}\n{e.StackTrace}");
                }
            }

            rankingList.Sort((a, b) => ConvertTimeToSeconds(a.clearTime).CompareTo(ConvertTimeToSeconds(b.clearTime)));
            for (int i = 0; i < rankingList.Count; i++)
            {
                rankingList[i].rank = i + 1;
            }

            rankingUI.UpdateRankingUI(rankingList);
        });
    }

    private int ConvertTimeToSeconds(string timeString)
    {
        try
        {
            string[] parts = timeString.Split(':');
            if (parts.Length == 2)
            {
                int minutes = int.Parse(parts[0]);
                int seconds = int.Parse(parts[1]);
                return minutes * 60 + seconds;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ 시간 변환 실패: {e.Message}");
        }
        return 0;
    }
}

[Serializable]
public class PlayerData
{
    public string playerID;
    public string playcharacter;
    public string clearTime;
    public int rank;

    public PlayerData(string id, string character, string time, int r)
    {
        playerID = id;
        playcharacter = character;
        clearTime = time;
        rank = r;
    }
}
