using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Newtonsoft.Json.Linq; // 설치 필요!

public class PlayerDetailUI : MonoBehaviour
{
    public TextMeshProUGUI playerIdText;
    public TextMeshProUGUI playerCharacterText;

    // Stats 텍스트 (민첩, 생명, 파워, 행운)
    public TextMeshProUGUI statAgilityText;
    public TextMeshProUGUI statHealthText;
    public TextMeshProUGUI statPowerText;
    public TextMeshProUGUI statLuckText;

    // Item UI (5개로 가정)
    [System.Serializable]
    public class ItemSlot
    {
        public Image itemImage;
        public TextMeshProUGUI itemNameText;
    }

    public List<ItemSlot> itemSlots = new List<ItemSlot>(); // Inspector에 5개 연결

    // UI 업데이트
    public void UpdateDetailUI(string json)
    {
        JObject data = JObject.Parse(json);

        playerIdText.text = data["playerId"]?.ToString();

        JObject stats = (JObject)data["stats"];
        statAgilityText.text = $"민첩 {stats["민첩"]}.Lv";
        statHealthText.text = $"생명 {stats["생명력"]}.Lv";
        statPowerText.text = $"파워 {stats["파워"]}.Lv";
        statLuckText.text = $"행운 {stats["행운"]}.Lv";

        JObject items = (JObject)data["item"];
        int i = 0;
        foreach (var item in items)
        {
            if (i >= itemSlots.Count) break;
            var slot = itemSlots[i];
            JObject itemObj = (JObject)item.Value;
            slot.itemNameText.text = itemObj["name"]?.ToString();

            // 이미지 로드 코루틴
            string imageUrl = itemObj["image"]?.ToString();
            StartCoroutine(LoadItemImage(imageUrl, slot.itemImage));
            i++;
        }
    }

    // 이미지 로드 함수
    private IEnumerator LoadItemImage(string url, Image image)
    {
        UnityWebRequest req = UnityWebRequestTexture.GetTexture(url);
        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            Texture2D tex = DownloadHandlerTexture.GetContent(req);
            image.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.one * 0.5f);
        }
        else
        {
            Debug.LogError(" 이미지 로드 실패: " + req.error);
        }
    }
}
