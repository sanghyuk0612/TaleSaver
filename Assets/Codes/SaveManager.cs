using System;
using System.Collections;
using System.IO;
using UnityEngine;


public class SaveManager : MonoBehaviour
{
    private string savePath;
    public static SaveManager Instance { get; private set; }

    void Start()
    {
        savePath = Path.Combine(Application.persistentDataPath, "saveFile.json");
    }

    public void SaveData(PlayerItemData data)
    {
        string json = JsonUtility.ToJson(data, true); // JSON 변환
        File.WriteAllText(savePath, json); // 파일로 저장
        Debug.Log("Data Saved: " + savePath);
    }

    public PlayerItemData LoadData()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath); // 파일에서 읽기
            PlayerItemData data = JsonUtility.FromJson<PlayerItemData>(json); // JSON -> 객체
            Debug.Log("Data Loaded");
            return data;
        }
        Debug.LogWarning("No Save File Found!");
        return null;
    }
}