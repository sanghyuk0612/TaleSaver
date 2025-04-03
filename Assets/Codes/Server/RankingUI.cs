using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
public class RankingUI : MonoBehaviour
{
    public GameObject rankingScorePrefab;  // 프리팹 (Inspector에서 할당)
    public Transform content;              //  Content의 Transform (스크롤뷰 안)
    private List<GameObject> rankingEntries = new List<GameObject>();  //  생성된 UI 리스트
    //private int initialPoolSize = 1; // 초기 프리팹 개수 (1개)

    //처음 시작할 때 프리팹 미리 생성(오브젝트 풀링)
    private void Awake()
    {
        GameObject entry = Instantiate(rankingScorePrefab, content);
        entry.SetActive(false); // 비활성화
        rankingEntries.Add(entry);
    }

    // Update is called once per frame
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

        //(1) 초기 씬에 존재하는 RankingScore 프리팹을 찾아서 리스트에 추가
        if (rankingEntries.Count == 0)
        {
            foreach (Transform child in content) // Content의 모든 자식 객체 확인
            {
                rankingEntries.Add(child.gameObject);
            }
        }

        //(2) 기존 UI 프리팹을 비활성화 (렌더링되지 않도록)
        foreach (GameObject entry in rankingEntries)
        {
            entry.SetActive(false);
        }
        Debug.Log("기존 랭킹 UI 비활성화 완료");


        // 필요한 만큼 UI 활성화 또는 생성
        for (int i = 0; i < rankingList.Count; i++)
        {
            GameObject entry;

            if (i < rankingEntries.Count)
            {
                entry = rankingEntries[i]; // 기존에 있던 프리팹 재사용
                entry.SetActive(true);
            }
            else
            {
                entry = Instantiate(rankingScorePrefab, content); // 부족하면 새로 생성
                rankingEntries.Add(entry);
            }

            // UI 요소 값 설정
            RankingEntry entryScript = entry.GetComponent<RankingEntry>();
            entryScript.placeText.text = (i+1).ToString();
            entryScript.playerIDText.text = rankingList[i].playerID;
            entryScript.playcharacterText.text = rankingList[i].playcharacter;
            entryScript.cleartimeText.text = rankingList[i].clearTime;

            Debug.Log($"UI 업데이트: {rankingList[i].playerID}, {rankingList[i].playcharacter}, {rankingList[i].clearTime}");
        }
    }

}
