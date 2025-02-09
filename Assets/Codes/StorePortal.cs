using UnityEngine;
using UnityEngine.SceneManagement;

public class StorePortal : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerController player = collision.GetComponent<PlayerController>();
            if (player != null)
            {
                int currentStage = GameManager.Instance.Stage;

                SceneManager.LoadScene("GameScene");

                // 다음 스테이지로 이동
                GameManager.Instance.SavePlayerState();
                GameManager.Instance.LoadNextStage();
            }
        }
    }
}