
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
    public RankingUI rankingUI;  // UI 연결

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

    /// <summary>
    /// 🔥 랭킹 데이터 저장 (게임 클리어 시 호출)
    /// </summary>
    public void SaveRanking(string clearTime, string playCharacter)
    {
        string uid = FirebaseAuthManager.Instance.Auth.CurrentUser.UserId;
        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;

        db.Collection("users").Document(uid).GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                DocumentSnapshot snapshot = task.Result;
                if (snapshot.Exists && snapshot.ToDictionary().ContainsKey("username"))
                {
                    string username = snapshot.GetValue<string>("username");

                    var rankingData = new Dictionary<string, object>()
                    {
                        { "playerId", uid },
                        { "username", username },
                        { "cleartime", clearTime },
                        { "playcharacter", playCharacter },
                        { "rank", 0 } // 초기값, UI에서 계산
                    };

                    db.Collection("rankings").Document(uid).SetAsync(rankingData).ContinueWithOnMainThread(saveTask =>
                    {
                        if (saveTask.IsCompletedSuccessfully)
                            Debug.Log("✅ 랭킹 저장 성공");
                        else
                            Debug.LogError("❌ 랭킹 저장 실패: " + saveTask.Exception?.Message);
                    });
                }
                else
                {
                    Debug.LogError("❌ username 필드 없음");
                }
            }
            else
            {
                Debug.LogError("❌ users 문서 불러오기 실패: " + task.Exception?.Message);
            }
        });
    }

    /// <summary>
    /// 🧠 랭킹 불러오기 (게임 시작 시 자동 호출)
    /// </summary>
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
                    string username = rankingData.ContainsKey("username") ? rankingData["username"].ToString() : "Unknown";
                    string cleartime = rankingData.ContainsKey("cleartime") ? rankingData["cleartime"].ToString() : "00:00";
                    string playcharacter = rankingData.ContainsKey("playcharacter") ? rankingData["playcharacter"].ToString() : "Unknown";
                    int rank = rankingData.ContainsKey("rank") ? System.Convert.ToInt32(rankingData["rank"]) : -1;

                    rankingList.Add(new PlayerData(playerId, username, playcharacter, cleartime, rank));
                    Debug.Log($"🏆 랭킹: {username} | Rank: {rank} | Char: {playcharacter} | Time: {cleartime}");
                }
                catch (Exception e)
                {
                    Debug.LogError($"❌ 예외 발생: {e.Message}\n{e.StackTrace}");
                }
            }

            // ⏱ 클리어 시간 기준 정렬
            rankingList.Sort((a, b) => ConvertTimeToSeconds(a.clearTime).CompareTo(ConvertTimeToSeconds(b.clearTime)));
            for (int i = 0; i < rankingList.Count; i++)
                rankingList[i].rank = i + 1;

            rankingUI.UpdateRankingUI(rankingList); // UI에 반영
        });
    }

    /// <summary>
    /// "MM:SS" → 초 변환
    /// </summary>
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
    public string playerID;      // UID
    public string username;      // 닉네임
    public string playcharacter;
    public string clearTime;
    public int rank;

    public PlayerData(string uid, string name, string character, string time, int r)
    {
        playerID = uid;
        username = name;
        playcharacter = character;
        clearTime = time;
        rank = r;
    }
}
