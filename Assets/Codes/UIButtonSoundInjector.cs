using UnityEngine;
using UnityEngine.UI;

public class UIButtonSoundInjector : MonoBehaviour
{
    public AudioClip clickSE; // 사용할 효과
     // 이 함수는 StoreWindow가 활성화될 때 호출됩니다.
    void Start()
    {
        Button[] allButtons = FindObjectsOfType<Button>(true);

        foreach (Button btn in allButtons)
        {
            btn.onClick.AddListener(() =>
            {
                if (BGMManager.instance != null && clickSE != null)
                {
                    BGMManager.instance.PlaySE(clickSE, 1.0f);
                }
            });
        }
    }
}