using TMPro;
using UnityEngine;

public class LobbyUI : MonoBehaviour
{
    // ✅ 싱글톤 인스턴스 선언
    public static LobbyUI Instance { get; private set; }

    [Header("재화 UI")]
    public TextMeshProUGUI storybookText;
    public TextMeshProUGUI machinePartsText;

    private void Awake()
    {
        // ✅ 싱글톤 초기화 (중복 방지)
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("중복 LobbyUI 인스턴스가 존재하여 파괴됩니다.");
            Destroy(gameObject);
        }
    }

    void Start()
    {
        Debug.Log($"[로비UI] storybook: {GameDataManager.Instance.storybookPage}, parts: {GameDataManager.Instance.machineParts}");
        UpdateGoodsUI();
    }

    // ✅ 외부에서 호출 가능한 UI 갱신 함수
    public void UpdateGoodsUI()
    {
        storybookText.text = GameDataManager.Instance.storybookPage.ToString();
        machinePartsText.text = GameDataManager.Instance.machineParts.ToString();
    }
}