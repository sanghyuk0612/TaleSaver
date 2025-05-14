using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BGMManager : MonoBehaviour
{
    public static BGMManager instance;

    public AudioClip lobbyBGM;
    public AudioClip GameBGM;

    public AudioClip slashSE;
    public AudioClip blackBirdSE;
    public AudioClip CowSE;
    public AudioClip HealSE;
    public AudioClip slash2SE;
    public AudioClip demagedSE;
    public AudioClip demagedSE2;

    public AudioSource bgmSource;  // 🎵 BGM 전용
    public AudioSource seSource;   // 🔊 SE 전용

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        bgmSource.clip = lobbyBGM;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "GameScene" || scene.name == "BossStage")
        {
            bgmSource.clip = GameBGM;
        }
        else
        {
            bgmSource.clip = lobbyBGM;
        }
        bgmSource.Play();
    }

    public void PlaySE(AudioClip clip, float vol = 1.0f)
    {
        if (clip != null)
            seSource.PlayOneShot(clip, vol);
    }
}