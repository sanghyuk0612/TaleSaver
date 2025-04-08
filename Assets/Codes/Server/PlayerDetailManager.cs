using System;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Firestore;
using Firebase;
using Firebase.Extensions;

public class PlayerDetailManager : MonoBehaviour
{
    private FirebaseFirestore db;
    private bool isFirebaseInitialized = false;
    public PlayerDetailUI detailUI; // UI 연결

    private void Start()
    {
        Debug.Log("PlayerDetailManager.cs 호출");
        detailUI = FindObjectOfType<PlayerDetailUI>();
        if (detailUI == null)
        {
            Debug.LogError("PlayerDetailUI가 씬에서 찾을 수 없습니다.");
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
                db = FirebaseFirestore.DefaultInstance;
                isFirebaseInitialized = true;
                Debug.Log("Firebase 초기화 완료!");
            }
            else
            {
                Debug.LogError("Firebase 초기화 실패: " + task.Result);
            }
        });
    }

    public void LoadPlayerDetail(string playerId)
    {
        if (!isFirebaseInitialized)
        {
            Debug.LogError("Firebase가 아직 초기화되지 않았습니다.");
            return;
        }

        db.Collection("players").Document(playerId).GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || !task.Result.Exists)
            {
                Debug.LogError("플레이어 데이터를 가져오는데 실패했거나 존재하지 않음.");
                return;
            }

            var doc = task.Result;
            Dictionary<string, object> data = doc.ToDictionary();

            try
            {
                // stats
                Dictionary<string, object> stats = new Dictionary<string, object>();
                if (data.ContainsKey("stats"))
                {
                    stats = data["stats"] as Dictionary<string, object>;
                }

                // items
                List<ItemData> items = new List<ItemData>();
                if (data.ContainsKey("item"))
                {
                    Dictionary<string, object> itemField = data["item"] as Dictionary<string, object>;
                    foreach (var entry in itemField)
                    {
                        Dictionary<string, object> itemData = entry.Value as Dictionary<string, object>;
                        if (itemData != null)
                        {
                            string image = itemData.ContainsKey("image") ? itemData["image"]?.ToString() : "";
                            string name = itemData.ContainsKey("name") ? itemData["name"]?.ToString() : "이름없음";
                           
                            items.Add(new ItemData
                            {
                                name = name,
                                imageUrl = image
                            });
                        }
                    }
                }

                PlayerDetailData detailData = new PlayerDetailData
                {
                    playerId = playerId,
                    stats = stats,
                    items = items
                };

                detailUI.DisplayPlayerDetails(detailData); //UI로 전달
            }
            catch (Exception e)
            {
                Debug.LogError($"데이터 파싱 중 오류 발생: {e.Message}");
            }
        });
    }
}

[Serializable]
public class ItemData
{
    public string name;
    public string imageUrl;
}

[Serializable]
public class PlayerDetailData
{
    public string playerId;
    public Dictionary<string, object> stats;
    public List<ItemData> items;
}
