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
    public Image sfxButtonImage;  // 버튼 안의 아이콘 이미지 (Image 컴포넌트)

    public AudioSource bgmSource;  // 🎵 BGM 전용
    public AudioSource seSource;   // 🔊 SE 전용
    public Text bgmButtonText;
    public Text sfxButtonText;

    private bool isBGMOn = true;
    private bool isSFXOn = true;
    public Slider bgmSlider;
    public Slider sfxSlider;

    // 연결할 오브젝트
    public Toggle bgmButton;
    public Toggle sfxButton;


    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
            TryReconnectUI();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        TryReconnectUI(); // 첫 씬에서도 연결
        bgmSource.clip = lobbyBGM;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        TryReconnectUI();

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

    public void TryReconnectUI()
    {
        GameObject settingsPanel = GameObject.Find("SettingsPanel");

        if (settingsPanel == null)
        {
            Debug.LogWarning("[BGMManager] SettingPanel not found.");
            return;
        }

        // 비활성화된 오브젝트 하위까지 탐색
        Transform bgmButtonTf = settingsPanel.transform.Find("BGMSoundSetting");
        Transform sfxButtonTf = settingsPanel.transform.Find("SeSoundSetting");
        Transform bgmSliderTf = settingsPanel.transform.Find("BGMSlider");
        Transform sfxSliderTf = settingsPanel.transform.Find("SESlider");

        bgmButton = bgmButtonTf?.GetComponent<Toggle>();
        sfxButton = sfxButtonTf?.GetComponent<Toggle>();
        bgmSlider = bgmSliderTf?.GetComponent<Slider>();
        sfxSlider = sfxSliderTf?.GetComponent<Slider>();

        Debug.Log($"[BGMManager] bgmButton found? {bgmButton != null}");
        Debug.Log($"[BGMManager] sfxButton found? {sfxButton != null}");

        // 슬라이더 이벤트 연결
        if (bgmSlider != null)
        {
            bgmSlider.onValueChanged.RemoveAllListeners();
            bgmSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
            bgmSlider.value = bgmSource.volume;
        }

        if (sfxSlider != null)
        {
            sfxSlider.onValueChanged.RemoveAllListeners();
            sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
            sfxSlider.value = seSource.volume;
        }

        // 버튼 이벤트 연결
        if (bgmButton != null)
        {
            Debug.Log("[BGMManager] BGMSoundSetting 연결됨"); // 디버깅용
            bgmButton.onValueChanged.RemoveAllListeners();
            bgmButton.onValueChanged.AddListener((_) => ToggleBGM());
        }

        if (sfxButton != null)
        {
            Debug.Log("✅ sfxSoundSetting 버튼 연결 성공");
            sfxButton.onValueChanged.RemoveAllListeners();
            sfxButton.onValueChanged.AddListener((_) => ToggleSFX());
        }
        // 상태 초기화 UI 반영
        if (bgmButtonText != null)
            bgmButtonText.text = isBGMOn ? "BGM 끄기" : "BGM 키기";

        if (sfxButtonText != null)
            sfxButtonText.text = isSFXOn ? "효과음 끄기" : "효과음 키기";

        if (sfxCheckMark != null)
            sfxCheckMark.SetActive(isSFXOn);

        //if (sfxButtonImage != null)
            //sfxButtonImage.sprite = isSFXOn ? sfxOnSprite : sfxOffSprite;

    }

    public void PlaySE(AudioClip clip, float vol = 1.0f)
    {
        if (clip != null)
            seSource.PlayOneShot(clip, vol);
    }
    public void ToggleBGM()
    {
        Debug.Log("🔊 BGM 버튼 눌림");
        isBGMOn = !isBGMOn;
        bgmSource.mute = !isBGMOn;
        if (bgmButtonText != null)
            bgmButtonText.text = isBGMOn ? "BGM 끄기" : "BGM 키기";
    }

    public void ToggleSFX()
    {
        Debug.Log("🔊 SFX 버튼 눌림");
        isSFXOn = !isSFXOn;
        seSource.mute = !isSFXOn;
        if (sfxButtonText != null)
            sfxButtonText.text = isSFXOn ? "효과음 끄기" : "효과음 키기";

        if (sfxCheckMark != null)
            sfxCheckMark.SetActive(isSFXOn);

        //if (sfxButtonImage != null)
            //sfxButtonImage.sprite = isSFXOn ? sfxOnSprite : sfxOffSprite;
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