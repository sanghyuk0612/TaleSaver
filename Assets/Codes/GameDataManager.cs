using System.Collections.Generic;
using UnityEngine;
using Firebase.Firestore;
using Firebase.Extensions;

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
        if (goodsData.ContainsKey("storybook page"))  // Firestore에 저장된 키 그대로 사용
            storybookPage = int.Parse(goodsData["storybook page"].ToString());

        if (goodsData.ContainsKey("machine parts"))
            machineParts = int.Parse(goodsData["machine parts"].ToString());

        Debug.Log($"?? 저장 완료: storybookPage={storybookPage}, machineParts={machineParts}");
    }

    // ?? 저장 함수
    public void SaveGoodsToFirestore()
    {
        string uid = FirebaseAuthManager.Instance.Auth.CurrentUser?.UserId;

        if (string.IsNullOrEmpty(uid))
        {
            Debug.LogError("? UID가 비어 있습니다. 로그인 여부 확인 필요");
            return;
        }

        Dictionary<string, object> goodsData = new Dictionary<string, object>()
        {
            { "storybook page", storybookPage },
            { "machine parts", machineParts }
        };

        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;

        db.Collection("goods").Document(uid).SetAsync(goodsData).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                Debug.Log("? Firestore에 goods 데이터 저장 완료");
            }
            else
            {
                Debug.LogError($"? goods 저장 실패: {task.Exception?.Message}");
            }
        });
    }
}
