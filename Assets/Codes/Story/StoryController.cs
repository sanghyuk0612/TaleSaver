using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

[System.Serializable]
public class StorySlide
{
    public Sprite image;
    [TextArea(2, 5)]
    public string[] lines;
}

public class StoryController : MonoBehaviour
{
    public Image storyImage;
    public TextMeshProUGUI storyText;
    public Button skipButton;
    public StorySlide[] slides;
    public SoundManager soundManager; // StoryManager에서 할당

    public float typingSpeed = 0.05f;

    private int slideIndex = 0;
    private int lineIndex = 0;
    private bool isTyping = false;
    private bool isFullTextShown = false;

    void Start()
    {
        if (PlayerPrefs.GetInt("StoryPlayed", 0) == 1)
        {
            SceneManager.LoadScene("GameStart");
            return;
        }

        skipButton.onClick.AddListener(SkipStory);
        ShowCurrentLine();
    }

    IEnumerator TypeText(string line)
    {
        isTyping = true;
        isFullTextShown = false;

        foreach (char c in line)
        {
            storyText.text += c;
            soundManager.PlayTypeSound();
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
        isFullTextShown = true;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            soundManager.PlayClickSound(); //페이지 넘기는 소리
            if (isTyping)
            {
                StopAllCoroutines();
                storyText.text = slides[slideIndex].lines[lineIndex];
                isTyping = false;
                isFullTextShown = true;
            }
            else if (isFullTextShown)
            {
                lineIndex++;
                if (lineIndex < slides[slideIndex].lines.Length)
                {
                    ShowCurrentLine();
                }
                else
                {
                    slideIndex++;
                    if (slideIndex < slides.Length)
                    {
                        lineIndex = 0;
                        ShowCurrentLine();
                    }
                    else
                    {
                        EndStory();
                    }
                }
            }
        }
    }

    void ShowCurrentLine()
    {
        storyImage.sprite = slides[slideIndex].image;
        storyText.text = "";
        StartCoroutine(TypeText(slides[slideIndex].lines[lineIndex]));
    }

    void SkipStory()
    {
        EndStory();
    }

    void EndStory()
    {
        PlayerPrefs.SetInt("StoryPlayed", 1);
        SceneManager.LoadScene("GameStart");
    }
}
