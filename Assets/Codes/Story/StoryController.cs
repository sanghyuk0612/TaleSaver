using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StoryController : MonoBehaviour
{
    public Image storyImage;
    public TextMeshProUGUI storyText;
    public Button skipButton;

    public Sprite[] storySprites;
    [TextArea(2, 5)]
    public string[] storyLines = {
        "외계인으로부터 날아온 오염 물질로",
        "대륙의 대부분은 사람이 살 수 없게 되었다.",
        "결국 지구 사람들은 모두 다른 행성으로 이민을 가기로 결정했다.",

        "대규모 이민 기간이다.",
        "이민자들이 줄을 서서 하나 둘 대형 로켓을 타고 떠난다.",

        "이민자 중 한 남자는 파멸에 빠진 지구를 바라본다.",
        "비명만이 가득한 절규 속에서 그의 눈빛은 더욱 또렷해졌다.",

        "그의 직업은 바로 아바타 연구가",
        "아바타를 이용해 지구를 되찾을 계획을 세운다.",
        "하지만 강력한 아바타 소재를 얻는데 난관을 마주친다.",

        "5살짜리 아들에게 동화책을 읽어주다 문득 깨닫는다.",
        "동화 속 주인공들의 강력한 힘을 말이다.",

        "아바타 소재를 얻은 뒤부터, 연구는 순항이 이어졌다.",
        "그러던 중,",
        "그의 조교가 외계인들이 투척한 오염 물질의 근원지를 찾게 된다.",

        "바로 사람들은 잘 모르는, 지금은 쓰이지 않는 오래된 연구실이었다.",
        "계속해서 모든 동식물을 공격적으로 만드는 외계 물질의 농도가",
        "그 연구실에서 일정하게 유지됨을 확인했다.",

        "그는 의심할 여지 없이 그 연구소를 향해 아바타를 보내야겠다고 말했다.",
        "그 연구실에서 나오는 외계 오염 물질을 제거해,",
        "푸르던 지구를 꼭 되찾으리라."
    };

    public float typingSpeed = 0.05f;

    private int currentIndex = 0;
    private bool isTyping = false;
    private bool isFullTextShown = false;

    void Start()
    {
        skipButton.onClick.AddListener(SkipStory);
        ShowCurrentSlide();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (isTyping)
            {
                // 타자 효과 중일 때는 즉시 출력
                StopAllCoroutines();
                storyText.text = storyLines[currentIndex];
                isTyping = false;
                isFullTextShown = true;
            }
            else if (isFullTextShown)
            {
                // 이미 전체 글자가 보이면 다음 슬라이드로
                currentIndex++;
                if (currentIndex < storyLines.Length)
                    ShowCurrentSlide();
                else
                    EndStory();
            }
        }
    }

    void ShowCurrentSlide()
    {
        storyImage.sprite = storySprites[currentIndex];
        storyText.text = "";
        StartCoroutine(TypeText(storyLines[currentIndex]));
    }

    IEnumerator TypeText(string line)
    {
        isTyping = true;
        isFullTextShown = false;

        foreach (char c in line)
        {
            storyText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
        isFullTextShown = true;
    }

    void SkipStory()
    {
        // 원하는 씬으로 이동하거나 게임 시작
        UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
    }

    void EndStory()
    {
        // 모든 스토리 끝났을 때 처리
        UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
    }
}
