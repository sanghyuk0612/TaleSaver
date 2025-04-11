using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class RankingUI : MonoBehaviour
{
    public GameObject rankingScorePrefab;  // 프리팹 (Inspector에서 할당)
    public Transform content;              //  Content의 Transform (스크롤뷰 안)
    private List<GameObject> rankingEntries = new List<GameObject>();  //  생성된 UI 리스트

    private void Awake()
    {
        GameObject entry = Instantiate(rankingScorePrefab, content);
        entry.SetActive(false); // 비활성화
        rankingEntries.Add(entry);
    }

    public void UpdateRankingUI(List<PlayerData> rankingList)
    {
        Debug.Log($"UpdateRankingUI 실행 - 랭킹 개수: {rankingList.Count}");

        if (rankingScorePrefab == null)
        {
            Debug.LogError("rankingScorePrefab이 설정되지 않음");
            return;
        }
        if (content == null)
        {
            Debug.LogError("content가 설정되지 않음");
            return;
        }

        if (rankingEntries.Count == 0)
        {
            foreach (Transform child in content)
            {
                rankingEntries.Add(child.gameObject);
            }
        }

        foreach (GameObject entry in rankingEntries)
        {
            entry.SetActive(false);
        }
        Debug.Log("기존 랭킹 UI 비활성화 완료");

        for (int i = 0; i < rankingList.Count; i++)
        {
            GameObject entry;

            if (i < rankingEntries.Count)
            {
                entry = rankingEntries[i];
                entry.SetActive(true);
            }
            else
            {
                entry = Instantiate(rankingScorePrefab, content);
                rankingEntries.Add(entry);
            }

            RankingEntry entryScript = entry.GetComponent<RankingEntry>();
            entryScript.SetPlayerEntry(
                rankingList[i].playerID,
                rankingList[i].playcharacter,
                rankingList[i].clearTime,
                rankingList[i].rank
            );

            Debug.Log($"UI 업데이트: {rankingList[i].playerID}, {rankingList[i].playcharacter}, {rankingList[i].clearTime}");
        }
    }
}
