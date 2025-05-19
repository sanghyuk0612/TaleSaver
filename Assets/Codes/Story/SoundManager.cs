using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public AudioSource audioSource;

    public AudioClip bgm;
    public AudioClip typeSound;
    public AudioClip clickSound;

    void Start()
    {
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
            audioSource.PlayOneShot(typeSound);
    }

    public void PlayClickSound()
    {
        if (clickSound != null)
            audioSource.PlayOneShot(clickSound);
    }
}
