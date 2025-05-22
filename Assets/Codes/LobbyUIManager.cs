using TMPro;
using UnityEngine;

public class LobbyUI : MonoBehaviour
{
    public TextMeshProUGUI storybookText;
    public TextMeshProUGUI machinePartsText;

    void Start()
    {
        Debug.Log($"[·Îºñ] storybook: {GameDataManager.Instance.storybookPage}, parts: {GameDataManager.Instance.machineParts}");
        UpdateGoodsUI();
    }

    public void UpdateGoodsUI()
    {
        storybookText.text = GameDataManager.Instance.storybookPage.ToString();
        machinePartsText.text = GameDataManager.Instance.machineParts.ToString();
    }
}
