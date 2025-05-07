using System.Collections.Generic;
using UnityEngine;
using Firebase.Firestore;
using Firebase.Extensions;
using Firebase.Auth;
using System.Collections;
using System;

public class GameDataManager : MonoBehaviour
{
    public static GameDataManager Instance { get; private set; }

    public int storybookPage = 0;
    public int machineParts = 0;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ?? 로그인 시 호출되는 함수
    public void SetGoodsData(Dictionary<string, object> goodsData)
    {
        if (goodsData.ContainsKey("storybookpages"))
            storybookPage = int.Parse(goodsData["storybookpages"].ToString());

        if (goodsData.ContainsKey("machineparts"))
            machineParts = int.Parse(goodsData["machineparts"].ToString());

        // Inventory에도 반영
        InventoryManager.Instance.inventory.storybookpages = storybookPage;
        InventoryManager.Instance.inventory.machineparts = machineParts;
    }

    // ?? 저장 함수
    public void SaveGoodsToFirestore()
    {
        StartCoroutine(SaveGoodsWhenReady());
    }

    private IEnumerator SaveGoodsWhenReady()
    {
        yield return FirebaseAuthManager.Instance.WaitUntilUserIsReady(() =>
        {
            FirebaseUser user = FirebaseAuthManager.Instance.Auth.CurrentUser;

            if (user == null)
            {
                Debug.LogError("? Firestore 저장 실패: user null");
                return;
            }

            string uid = user.UserId;
            FirebaseFirestore db = FirebaseFirestore.DefaultInstance;

            // ?? Firestore에서 기존 재화 값 읽어오기
            db.Collection("goods").Document(uid).GetSnapshotAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsCompletedSuccessfully)
                {
                    int prevMachineParts = 0;
                    int prevStorybookPages = 0;

                    var snapshot = task.Result;
                    if (snapshot.Exists)
                    {
                        var data = snapshot.ToDictionary();
                        if (data.ContainsKey("machineparts"))
                            prevMachineParts = Convert.ToInt32(data["machineparts"]);
                        if (data.ContainsKey("storybookpages"))
                            prevStorybookPages = Convert.ToInt32(data["storybookpages"]);
                    }

                    // ?? 누적된 결과 계산
                    int newMachineParts = prevMachineParts + machineParts;
                    int newStorybookPages = prevStorybookPages + storybookPage;

                    Dictionary<string, object> goodsData = new Dictionary<string, object>()
                {
                    { "machineparts", newMachineParts },
                    { "storybookpages", newStorybookPages }
                };

                    // ?? Firestore에 저장
                    db.Collection("goods").Document(uid).SetAsync(goodsData).ContinueWithOnMainThread(saveTask =>
                    {
                        if (saveTask.IsCompletedSuccessfully)
                        {
                            Debug.Log($"? Firestore 누적 저장 완료: machineparts={newMachineParts}, storybookpages={newStorybookPages}");

                            // ?? 로컬 값 초기화 (중복 저장 방지)
                            machineParts = 0;
                            storybookPage = 0;

                            // ? 인벤토리에도 반영
                            InventoryManager.Instance.inventory.machineparts = 0;
                            InventoryManager.Instance.inventory.storybookpages = 0;
                        }
                        else
                        {
                            Debug.LogError($"? Firestore 저장 실패: {saveTask.Exception?.Message}");
                        }
                    });
                }
                else
                {
                    Debug.LogError($"? Firestore 기존 값 불러오기 실패: {task.Exception?.Message}");
                }
            });
        });
    }
}
