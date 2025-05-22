using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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

    public GameObject sfxCheckMark; // ✅ 효과음 체크 표시 오브젝트
    public Sprite sfxOnSprite;  // 효과음 켜짐 이미지
    public Sprite sfxOffSprite; // 효과음 꺼짐 이미지
    public Image sfxButtonImage;  // 버튼 안의 아이콘 이미지 (Image 컴포넌트)

    public AudioSource bgmSource;  // 🎵 BGM 전용
    public AudioSource seSource;   // 🔊 SE 전용
    public Text bgmButtonText;
    public Text sfxButtonText;

    private bool isBGMOn = true;
    private bool isSFXOn = true;
    public Slider bgmSlider;
    public Slider sfxSlider;


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

        if (sfxCheckMark != null)
            sfxCheckMark.SetActive(isSFXOn); // 시작 시 체크 상태 적용

        if (sfxButtonImage != null)
            sfxButtonImage.sprite = isSFXOn ? sfxOnSprite : sfxOffSprite;
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
    public void ToggleBGM()
    {
        isBGMOn = !isBGMOn;
        bgmSource.mute = !isBGMOn;
        bgmButtonText.text = isBGMOn ? "BGM 끄기" : "BGM 키기";
    }

    public void ToggleSFX()
    {
        isSFXOn = !isSFXOn;
        seSource.mute = !isSFXOn;
        sfxButtonText.text = isSFXOn ? "효과음 끄기" : "효과음 키기";

        if (sfxCheckMark != null)
            sfxCheckMark.SetActive(isSFXOn);

        if (sfxButtonImage != null)
            sfxButtonImage.sprite = isSFXOn ? sfxOnSprite : sfxOffSprite;
    }

    public void OnBGMVolumeChanged(float volume)
    {
        bgmSource.volume = volume;  // 0.0 ~ 1.0
        //isBGMOn = volume > 0f;
        //bgmButtonText.text = isBGMOn ? "BGM 끄기" : "BGM 켜기";
    }

    public void OnSFXVolumeChanged(float volume)
    {
        seSource.volume = volume;
        //isSFXOn = volume > 0f;
        //sfxButtonText.text = isSFXOn ? "효과음 끄기" : "효과음 켜기";
    }
}