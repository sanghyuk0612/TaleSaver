// GameOverUI.cs
using UnityEngine;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    void Awake()
    {
        // 중복 제거
        if (FindObjectsOfType<GameOverUI>().Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        // GameManager에 직접 연결
        if (GameManager.Instance != null)
        {
            GameManager.Instance.gameOverPanel = this.gameObject;

            Button restartBtn = GetComponentInChildren<Button>();
            if (restartBtn != null)
            {
                GameManager.Instance.restartButton = restartBtn;
                restartBtn.onClick.RemoveAllListeners();
                restartBtn.onClick.AddListener(GameManager.Instance.RestartGame);
            }
            else
            {
                Debug.LogWarning("RestartButton not found inside GameOverPanel.");
            }

            gameObject.SetActive(false); // 초기엔 꺼둠
        }
    }
}
