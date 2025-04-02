using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
public class RankingUI : MonoBehaviour
{
    public GameObject rankingScorePrefab;  // 프리팹 (Inspector에서 할당)
    public Transform content;              //  Content의 Transform (스크롤뷰 안)
    private List<GameObject> rankingEntries = new List<GameObject>();  //  생성된 UI 리스트



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

        //새 데이터 맞이하게 기존 UI 삭제
        foreach (GameObject entry in rankingEntries)
        {
            Destroy(entry);
        }
        rankingEntries.Clear();
        Debug.Log("기존 랭킹 UI 삭제");


        foreach(var data in rankingList)
        {
            if (data == null)
            {
                Debug.LogError("PlayerData가 null입니다.");
                continue;
            }
            //프리팹 생성
            GameObject newEntry = Instantiate(rankingScorePrefab, content);
            newEntry.transform.localScale = Vector3.one;

            //RankingEntry 컴포넌트 가져오기
            RankingEntry entryScript = newEntry.GetComponent<RankingEntry>();
            if (entryScript == null)
            {
                Debug.LogError("RankingEntry 스크립트를 찾을 수 없습니다! 프리팹을 확인하세요.");
                continue;
            }

            // TextMeshProUGUI 가져오는 방식 개선
            entryScript.playerIDText = newEntry.transform.Find("playerID")?.GetComponent<TextMeshProUGUI>();
            entryScript.playcharacterText = newEntry.transform.Find("playcharacter")?.GetComponent<TextMeshProUGUI>();
            entryScript.cleartimeText = newEntry.transform.Find("cleartime")?.GetComponent<TextMeshProUGUI>();


            if (entryScript.playerIDText == null || entryScript.playcharacterText == null || entryScript.cleartimeText == null)
            {
                Debug.LogError("UI 요소를 찾을 수 없음: playerIDText, playcharacterText, cleartimeText 중 하나가 null");
                continue;
            }

            // 값 설정
            entryScript.playerIDText.text = data.playerID;
            entryScript.playcharacterText.text = data.playcharacter;
            entryScript.cleartimeText.text = data.clearTime;


            rankingEntries.Add(newEntry);


        }
        Debug.Log("기존 랭킹 UI 불러오기 성공");
    }
}
