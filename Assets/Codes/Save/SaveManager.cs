using System.IO;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    private string progressPath;
    private string itemPath;
    public static SaveManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        progressPath = Path.Combine(Application.persistentDataPath, "progress.json");
        itemPath = Path.Combine(Application.persistentDataPath, "item.json");
    }

    // --- Progress 저장/불러오기 ---
    public void SaveProgressData(PlayerProgressData progressData)
    {
        string json = JsonUtility.ToJson(progressData, true);
        string path = Path.Combine(Application.persistentDataPath, "progressData.json");
        File.WriteAllText(path, json);
        Debug.Log("Progress Data Saved: " + path);
    }

    public PlayerProgressData LoadProgressData()
    {
        string path = Path.Combine(Application.persistentDataPath, "progressData.json");
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            PlayerProgressData data = JsonUtility.FromJson<PlayerProgressData>(json);
            Debug.Log("Progress Data Loaded");
            return data;
        }
        Debug.LogWarning("No Progress Save File Found!");
        return null;
    }

    // --- 아이템 저장/불러오기 ---
    public void SaveItemData(PlayerItemData itemData)
    {
        string json = JsonUtility.ToJson(itemData, true);
        File.WriteAllText(itemPath, json);
        Debug.Log("아이템 데이터 저장됨");
    }

    public PlayerItemData LoadItemData()
    {
        if (File.Exists(itemPath))
        {
            string json = File.ReadAllText(itemPath);
            return JsonUtility.FromJson<PlayerItemData>(json);
        }

        Debug.LogWarning("아이템 데이터 없음");
        return null;
    }
}
