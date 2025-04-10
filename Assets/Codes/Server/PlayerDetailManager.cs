using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class PlayerDetailManager : MonoBehaviour
{
    // 클라우드 함수의 호출 url
    private string playerDetailsUrl = "http://127.0.0.1:5001/tale-saver/us-central1/getPlayerDetails";

    // 특정 플레이어를 클릭했을 때 호출되는 메서드
    /*
    public void OnPlayerClicked(string playerId)
    {
        GetPlayerDetails(playerId);
    }
    */

    // 서버로부터 플레이어 상세 정보 가져오기
    public void GetPlayerDetails(string playerId)
    {
        string url = playerDetailsUrl + "?playerId=" + playerId;
        StartCoroutine(CallFirebaseFunctions(url));
    }

    private IEnumerator CallFirebaseFunctions(string url)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string jsonRes = request.downloadHandler.text;
                Debug.Log("응답: " + jsonRes);
                HandleResponse(jsonRes);
            }
            // 에러 처리
            else
            {
                Debug.LogError("요청 실패: " + request.error);
            }
        }
    }

    [SerializeField] private PlayerDetailUI playerDetailUI;

    private void HandleResponse(string jsonRes)
    {
        Debug.Log("플레이어 세부 정보 응답: " + jsonRes);

        // PlayerDetailUI 찾아서 UI 업데이트
        PlayerDetailUI ui = FindObjectOfType<PlayerDetailUI>();
        if (ui != null)
        {
            //ui.UpdateDetailUI(jsonRes);
        }
    }


}
