using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BGMManager : MonoBehaviour
{
    public static BGMManager instance;
    public AudioClip lobbyBGM;
    public AudioClip GameBGM;
    public AudioSource audioSource;
    public AudioClip slashSE;
    public AudioClip blackBirdSE;
    public AudioClip CowSE;
    public AudioClip HealSE;
    public AudioClip slash2SE;


    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = lobbyBGM;
        audioSource.loop = true;
        audioSource.Play();
    }
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject); // 중복 방지
        }
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    public void PlaySE(AudioClip clip, float vol)
    {
        if (clip != null)
            audioSource.PlayOneShot(clip,vol);
    }
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if(scene.name=="GameScene"||scene.name=="BossStage"){
            audioSource.clip = GameBGM;
        }
        else{
            audioSource.clip = lobbyBGM;
        }
        audioSource.Play();
    }
}