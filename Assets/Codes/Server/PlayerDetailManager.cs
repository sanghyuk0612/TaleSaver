
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class PlayerDetailManager : MonoBehaviour
{
    //�ֹķ����Ϳ� �Լ� url
    private string playerDeteailsUrl = "http://127.0.0.1:5001/tale-saver/us-central1/getPlayerDetails";

    // Ư�� �÷��̾ Ŭ���� �� ȣ��Ǵ� �޼ҵ�
    /*public void OnPlayerClicked(string playerId)
    {
        GetPlayerDetails(playerId);
    }*/

    //�ڷ�ƾ���� ���� ���� �Լ� �ҷ�����
    public void GetPlayerDetails(string playerId)
    {
        string url = playerDeteailsUrl + "?playerId=" + playerId;
        StartCoroutine(CallFirebaseFunctions(playerDeteailsUrl));
    }

    private IEnumerator CallFirebaseFunctions(string url)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string jsonRes = request.downloadHandler.text;
                Debug.Log("res: " + jsonRes);
                HandleResponse(jsonRes);
            }

            //���� ó��
            else
            {
                Debug.LogError("���� �߻�: " + request.error);
            }
        }
    }

    private void HandleResponse(string jsonRes)
    {
        Debug.Log("�Ľ̵� �÷��̾� res: " + jsonRes);
    }
}
