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
    public static RankingManager Instance { get; private set; }
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject); // 중복 방지
        }
    }

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
    public void SaveClearData(string playerId, string character, float clearTime)
    {
        Debug.Log($"🔥 SaveClearData 시작 - Firebase 초기화 여부: {isFirebaseInitialized}");

        if (!isFirebaseInitialized)
        {
            Debug.LogError("❗ Firebase 초기화 안 됨");
            return;
        }

        string formattedTime = $"{Mathf.FloorToInt(clearTime / 60f):00}:{Mathf.FloorToInt(clearTime % 60f):00}";

        Dictionary<string, object> data = new Dictionary<string, object>
    {
        { "playerId", playerId },
        { "playcharacter", character },
        { "cleartime", formattedTime },
        { "timestamp", Timestamp.GetCurrentTimestamp() }
    };

        db.Collection("rankings").Document(playerId).SetAsync(data).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                Debug.Log("✅ 클리어 기록 Firebase 저장 완료!");
            }
            else
            {
                Debug.LogError("❌ 저장 실패: " + task.Exception?.Flatten().Message);
            }
        });
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
