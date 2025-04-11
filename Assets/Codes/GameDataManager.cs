using System.Collections.Generic;
using UnityEngine;
using Firebase.Firestore;
using Firebase.Extensions;
using Firebase.Auth;

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
        // FirebaseAuthManager가 준비되었는지 체크
        if (FirebaseAuthManager.Instance == null)
        {
            Debug.LogError("? FirebaseAuthManager.Instance 가 null입니다.");
            return;
        }

        if (!FirebaseAuthManager.Instance.IsLoggedIn())
        {
            Debug.LogError("? 로그인 상태가 아닙니다. Firestore 저장 불가.");
            return;
        }

        FirebaseUser user = FirebaseAuthManager.Instance.Auth.CurrentUser;

        if (user == null)
        {
            Debug.LogError("? CurrentUser가 null입니다.");
            return;
        }

        string uid = user.UserId;

        Dictionary<string, object> goodsData = new Dictionary<string, object>()
    {
        { "storybookpages", storybookPage },
        { "machineparts", machineParts }
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
