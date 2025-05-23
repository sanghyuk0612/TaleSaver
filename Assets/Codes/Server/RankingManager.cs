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
    private bool shouldLoadDataAfterInit = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // ✅ 중복 오브젝트 제거
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);   // ✅ 씬 전환에도 유지
        InitializeFirebase();            // ✅ Firebase 초기화는 즉시 실행
    }

    void Start()
    {
        rankingUI = FindObjectOfType<RankingUI>();
        if (rankingUI == null)
        {
            Debug.LogWarning("⚠️ RankingUI가 씬에서 없습니다. BossStage 등일 수 있음.");
        }
        else
        {
            if (isFirebaseInitialized)
            {
                LoadData();  // 바로 실행
            }
            else
            {
                Debug.Log("🕓 Firebase 초기화 전 - LoadData 예약됨");
                shouldLoadDataAfterInit = true;
            }
        }
    }
    private static Queue<(string, string, float)> pendingSavesStatic = new Queue<(string, string, float)>();
    private Queue<(string, string, float)> pendingSaves = new Queue<(string, string, float)>();

    public static void QueueSaveRequest(string playerId, string character, float clearTime)
    {
        Debug.LogWarning("📥 RankingManager.Instance가 아직 생성되지 않았습니다. static 큐에 등록");
        pendingSavesStatic.Enqueue((playerId, character, clearTime));
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

                // 🔥 static 큐 병합
                while (pendingSavesStatic.Count > 0)
                {
                    var request = pendingSavesStatic.Dequeue();
                    pendingSaves.Enqueue(request);
                }

                // 🔥 대기 저장 실행
                while (pendingSaves.Count > 0)
                {
                    var (playerId, character, clearTime) = pendingSaves.Dequeue();
                    SaveClearData(playerId, character, clearTime);
                }

                LoadData(); // 기존 랭킹 불러오기
            }
            else
            {
                Debug.LogError("❌ Firebase 초기화 실패: " + task.Result);
            }
        });
    }

    private async void LoadData()
    {
        if (!isFirebaseInitialized)
        {
            Debug.LogError("❗ Firebase가 아직 초기화되지 않았습니다.");
            return;
        }

        QuerySnapshot snapshot = await db.Collection("rankings").GetSnapshotAsync();
        List<PlayerData> rankingList = new List<PlayerData>();

        foreach (var document in snapshot.Documents)
        {
            Dictionary<string, object> rankingData = document.ToDictionary();

            string playerId = rankingData.ContainsKey("playerId") ? rankingData["playerId"].ToString() : "Unknown";
            string cleartime = rankingData.ContainsKey("cleartime") ? rankingData["cleartime"].ToString() : "00:00";
            string playcharacter = rankingData.ContainsKey("playcharacter") ? rankingData["playcharacter"].ToString() : "Unknown";

            // 🔍 username 불러오기
            string username = playerId;
            try
            {
                DocumentSnapshot userDoc = await db.Collection("users").Document(playerId).GetSnapshotAsync();
                if (userDoc.Exists && userDoc.ContainsField("username"))
                {
                    username = userDoc.GetValue<string>("username");
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"⚠️ 사용자 이름 불러오기 실패: {e.Message}");
            }

            rankingList.Add(new PlayerData(username, playcharacter, cleartime, -1));
        }

        rankingList.Sort((a, b) => ConvertTimeToSeconds(a.clearTime).CompareTo(ConvertTimeToSeconds(b.clearTime)));
        for (int i = 0; i < rankingList.Count; i++)
        {
            rankingList[i].rank = i + 1;
        }

        rankingUI.UpdateRankingUI(rankingList);
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
        Debug.Log($"🔥 SaveClearData 진입 - Firebase 초기화 여부: {isFirebaseInitialized}");

        if (!isFirebaseInitialized)
        {
            Debug.LogWarning($"⏳ 초기화 전 - 저장 큐에 등록됨: {playerId}, {character}, {clearTime}");
            pendingSaves.Enqueue((playerId, character, clearTime));
            return;
        }

        Debug.Log($"📤 SaveClearData 실행됨: {playerId}, {character}, {clearTime}");

        string formattedTime = $"{Mathf.FloorToInt(clearTime / 60f):00}:{Mathf.FloorToInt(clearTime % 60f):00}";

        Dictionary<string, object> data = new Dictionary<string, object>
        {
            { "playerId", playerId },
            { "playcharacter", character },
            { "cleartime", formattedTime },
            { "timestamp", Timestamp.GetCurrentTimestamp() }
        };

        Debug.Log($"📄 Firestore에 저장될 데이터: {data["playerId"]}, {data["playcharacter"]}, {data["cleartime"]}");

        db.Collection("rankings").Document(playerId).SetAsync(data, SetOptions.MergeAll)
            .ContinueWithOnMainThread(task =>
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
    public void TryReconnectRankingUI()
    {
        rankingUI = FindObjectOfType<RankingUI>();
        if (rankingUI == null)
        {
            Debug.LogWarning("❌ ScoreBoard 씬에서도 RankingUI를 찾을 수 없습니다.");
            return;
        }

        Debug.Log("✅ ScoreBoard 씬에서 RankingUI 재연결 성공");
        LoadData();
    }

}



[Serializable]
public class PlayerData
{
    public string playerID;
    public string username;
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
