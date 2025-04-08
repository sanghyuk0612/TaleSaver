using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Networking;

public class PlayerDetailUI : MonoBehaviour
{
    [Header("Stat UI")]
    public Text agilityText;
    public Text healthText;
    public Text powerText;
    public Text luckText;

    [Header("Item UI")]
    public Transform itemContentParent; // ScrollView의 Content 오브젝트
    public GameObject itemPrefab; // 아이템 UI 프리팹

    private List<GameObject> currentItemObjects = new List<GameObject>();

    public void DisplayPlayerDetails(PlayerDetailData data)
    {
        //스탯 표시
        agilityText.text = $"민첩 {data.stats["민첩"]}.Lv";
        healthText.text = $"생명 {data.stats["생명력"]}.Lv";
        powerText.text = $"파워 {data.stats["파워"]}.Lv";
        luckText.text = $"행운 {data.stats["행운"]}.Lv";

        //기존 아이템 UI 삭제
        foreach (var obj in currentItemObjects)
        {
            Destroy(obj);
            Debug.Log("기존 아이템 UI 삭제");
        }
        currentItemObjects.Clear();

        //새로운 아이템 UI 생성
        foreach (var item in data.items)
        {
            GameObject itemObj = Instantiate(itemPrefab, itemContentParent);
            currentItemObjects.Add(itemObj);

            var nameText = itemObj.transform.Find("ItemNameText").GetComponent<Text>();
            var image = itemObj.transform.Find("Image").GetComponent<Image>();

            nameText.text = item.name;
            StartCoroutine(LoadImageFromURL(item.imageUrl, image));
        }
    }

    private System.Collections.IEnumerator LoadImageFromURL(string url, Image imageComponent)
    {
        using (UnityEngine.Networking.UnityWebRequest request = UnityEngine.Networking.UnityWebRequestTexture.GetTexture(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
                imageComponent.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
            }
            else
            {
                Debug.LogWarning($"이미지 로딩 실패: {url}");
            }
        }
    }
}
