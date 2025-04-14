using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Networking;

public class PlayerDetailUI : MonoBehaviour
{
    public Text playerIdText;
    public Text playerCharacterText;

    public Text statAgilityText;
    public Text statHealthText;
    public Text statPowerText;
    public Text statLuckText;

    public GameObject itemPrefab;         // Inspector에서 프리팹 연결
    public Transform itemContainer;       // itemPrefab들이 들어갈 부모 (ex: ItemContent)

    public void ClearItems()
    {
        foreach (Transform child in itemContainer)
        {
            Destroy(child.gameObject);
        }
    }

    [System.Serializable]
    public class ItemSlot
    {
        public Image itemImage;
        public Text itemNameText;
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

        ClearItems();

        if (data.TryGetValue("item", out object itemObj) && itemObj is Dictionary<string, object> itemMap)
        {
            foreach (var entry in itemMap)
            {
                var itemData = entry.Value as Dictionary<string, object>;
                if (itemData != null)
                {
                    // 로그 1: 아이템 키 + name, image
                    string itemName = itemData.ContainsKey("name") ? itemData["name"].ToString() : "이름 없음";
                    string imageUrl = itemData.ContainsKey("image") ? itemData["image"].ToString() : "(없음)";
                    Debug.Log($"[ 아이템 로딩] key={entry.Key}, name={itemName}, image={imageUrl}");

                    GameObject itemGO = Instantiate(itemPrefab, itemContainer);
                    var itemImage = itemGO.transform.Find("ItemImage")?.GetComponent<Image>();
                    var itemText = itemGO.transform.Find("ItemName")?.GetComponent<Text>();

                    if (itemText != null)
                    {
                        //itemText.text = itemData.ContainsKey("name") ? itemData["name"].ToString() : "이름 없음";
                        itemText.text = itemName;
                        itemText.color = Color.black; // 혹시 투명할까봐 강제 설정
                        Debug.Log($"[텍스트 설정됨] {itemName}");
                        Debug.Log($"[텍스트 위치] anchoredPos={itemText.rectTransform.anchoredPosition}, size={itemText.rectTransform.sizeDelta}");

                    }


                    else
                    {
                        Debug.LogError($"[itemText == null] → 'ItemName' 오브젝트를 찾지 못했습니다.");
                    }

                    //if (itemImage != null)
                    //{
                    //    if (!string.IsNullOrEmpty(imageUrl))
                    //    {
                    //        StartCoroutine(LoadItemImage(imageUrl, itemImage));
                    //    }
                    //    else
                    //    {
                    //        Debug.Log($"[이미지 없음] name={itemName}");
                    //        // image.sprite = defaultSprite;
                    //    }
                    //}

                    //else
                    {
                        //Debug.Log($"image 필드 없음. name={itemText?.text}");
                        // image.sprite = defaultSprite; // 기본 이미지 설정 가능
                    }
                }
            }
        }
    }


    private int GetInt(Dictionary<string, object> dict, string key)
    {
        return dict.ContainsKey(key) ? int.Parse(dict[key].ToString()) : 0;
    }

    //url 방식
    //private IEnumerator LoadItemImage(string url, Image image)
    //{
    //    UnityWebRequest req = UnityWebRequestTexture.GetTexture(url);
    //    yield return req.SendWebRequest();

    //    if (req.result == UnityWebRequest.Result.Success)
    //    {
    //        Texture2D tex = DownloadHandlerTexture.GetContent(req);
    //        image.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.one * 0.5f);
    //    }
    //    else
    //    {
    //        Debug.LogError("이미지 로드 실패: " + req.error);
    //    }
    //}
}
