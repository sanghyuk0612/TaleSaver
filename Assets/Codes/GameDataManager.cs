using System.Collections.Generic;
using UnityEngine;
using Firebase.Firestore;
using Firebase.Extensions;
using Firebase.Auth;
using System.Collections;

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
                    Debug.Log($"? Firestore에 goods 저장 완료! machineparts: {machineParts}, storybook: {storybookPage}");
                }
                else
                {
                    Debug.LogError($"? Firestore 저장 실패: {task.Exception?.Message}");
                }
            });
        });
    }

}
