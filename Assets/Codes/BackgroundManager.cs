using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BackgroundManager : MonoBehaviour
{
    public static BackgroundManager Instance;

    public SpriteRenderer backgroundRenderer;
    public Sprite[] backgroundImages;

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

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        UpdateBackground();
    }
    //임시
    public void UpdateBackground()
    {
        int location = GameManager.Instance.location;

        // location 값을 인덱스로 매핑 (예: location 5 → index 0, location 4 → index 1)
        int index = -1;

        switch (location)
        {
            case 5: index = 0; break; // 화산
            case 4: index = 1; break; // 연구소 보스
            default:
                Debug.LogWarning("지원되지 않는 location입니다: " + location);
                break;
        }

        if (index >= 0 && index < backgroundImages.Length)
        {
            backgroundRenderer.sprite = backgroundImages[index];
        }
    }

}