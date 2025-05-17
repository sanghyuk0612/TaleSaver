using UnityEngine;
using UnityEngine.UI;


public class BossHPUI : MonoBehaviour
{
    public static BossHPUI Instance;

    public Slider hpSlider;
    public Text bossNameText;

    void Awake()
    {
        Instance = this;
        //gameObject.SetActive(false); // 시작 시 비활성화
    }

    public void ShowBossUI(string name, int maxHP)
    {
        bossNameText.text = name;
        hpSlider.maxValue = maxHP;
        hpSlider.value = maxHP;
    }

    public void UpdateHP(int currentHP)
    {
        Debug.Log("ui에 체력반영");
        hpSlider.value = currentHP;
    }
}