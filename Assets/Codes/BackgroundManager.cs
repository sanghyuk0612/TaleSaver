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

    public void UpdateBackground()
    {
        int location = GameManager.Instance.location;

        if (location >= 0 && location < backgroundImages.Length)
        {
            backgroundRenderer.sprite = backgroundImages[location];
        }
        else
        {
            Debug.Log("배경 이미지 인덱스 오류");
        }
    }
}