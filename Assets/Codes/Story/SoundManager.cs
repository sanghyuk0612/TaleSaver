using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public AudioSource audioSource;

    public AudioClip bgm;
    public AudioClip typeSound;
    //public AudioClip clickSound;

    [Range(0f, 1f)] public float typeSoundVolume = 0.3f;
    [Range(0f, 1f)] public float clickSoundVolume = 0.5f;

    void Start()
    {
        audioSource.volume = 1.0f; // 전체 기준 유지
        PlayBGM();
    }



    public void PlayBGM()
    {
        if (bgm != null)
        {
            audioSource.clip = bgm;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    public void PlayTypeSound()
    {
        if (typeSound != null)
            audioSource.PlayOneShot(typeSound, typeSoundVolume); // 0~1 사이 값 적용됨

    }

    /*public void PlayClickSound()
    {
        if (clickSound != null)
            audioSource.PlayOneShot(clickSound);
    }*/
}
