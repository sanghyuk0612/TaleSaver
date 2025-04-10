using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Networking;

public class PlayerDetailUI : MonoBehaviour
{
    public TextMeshProUGUI playerIdText;
    public TextMeshProUGUI playerCharacterText;

    public TextMeshProUGUI statAgilityText;
    public TextMeshProUGUI statHealthText;
    public TextMeshProUGUI statPowerText;
    public TextMeshProUGUI statLuckText;

    [System.Serializable]
    public class ItemSlot
    {
        public Image itemImage;
        public TextMeshProUGUI itemNameText;
    }

    public List<ItemSlot> itemSlots = new List<ItemSlot>();

    public void UpdateDetailUI(Dictionary<string, object> data)
    {
        playerIdText.text = data.ContainsKey("playerId") ? data["playerId"].ToString() : "Unknown";
        playerCharacterText.text = data.ContainsKey("character") ? data["character"].ToString() : "Unknown";

        if (data.TryGetValue("stats", out object statsObj) && statsObj is Dictionary<string, object> stats)
        {
            statAgilityText.text = $"민첩 {GetInt(stats, "민첩")}.Lv";
            statHealthText.text = $"생명 {GetInt(stats, "생명력")}.Lv";
            statPowerText.text = $"파워 {GetInt(stats, "파워")}.Lv";
            statLuckText.text = $"행운 {GetInt(stats, "행운")}.Lv";
        }

        if (data.TryGetValue("item", out object itemObj) && itemObj is Dictionary<string, object> itemMap)
        {
            int i = 0;
            foreach (var entry in itemMap)
            {
                if (i >= itemSlots.Count) break;

                if (entry.Value is Dictionary<string, object> itemData)
                {
                    itemSlots[i].itemNameText.text = itemData["name"].ToString();
                    string imageUrl = itemData["image"].ToString();
                    StartCoroutine(LoadItemImage(imageUrl, itemSlots[i].itemImage));
                    i++;
                }
            }
        }
    }

    private int GetInt(Dictionary<string, object> dict, string key)
    {
        return dict.ContainsKey(key) ? int.Parse(dict[key].ToString()) : 0;
    }

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
            Debug.LogError("이미지 로드 실패: " + req.error);
        }
    }
}
