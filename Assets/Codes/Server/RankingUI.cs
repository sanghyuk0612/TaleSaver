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
        if (rankingScorePrefab == null || content == null)
        {
            Debug.LogError(" rankingScorePrefab 또는 content가 연결되어 있지 않습니다.");
            return;
        }

        // 템플릿용으로 하나만 미리 만들어둠 (비활성화)
        GameObject entry = Instantiate(rankingScorePrefab, content);
        entry.SetActive(false);
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

        // 새로운 UI 생성
        foreach (var player in rankingList)
        {
            GameObject entry = Instantiate(rankingScorePrefab, content);
            entry.SetActive(true); // 활성화

            RankingEntry entryScript = entry.GetComponent<RankingEntry>();
            if (entryScript != null)
            {
                entryScript.SetPlayerEntry(
                    player.playerID,
                    player.playcharacter,
                    player.clearTime,
                    player.rank
                );
                Debug.Log($" UI 추가: {player.playerID}, {player.playcharacter}, {player.clearTime}");
            }
            else
            {
                Debug.LogError("RankingEntry 스크립트가 프리팹에 붙어있지 않습니다.");
            }

            rankingEntries.Add(entry);
        }
    }
}
