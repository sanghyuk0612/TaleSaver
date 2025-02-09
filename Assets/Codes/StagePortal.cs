using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StagePortal : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                int currentStage = GameManager.Instance.Stage;
                GameManager.Instance.SavePlayerState();

                // 스테이지 4와 8 이후에는 Store 씬으로 이동
                if (currentStage == 4 || currentStage == 8)
                {
                    // 기존 몬스터와 투사체 제거
                    MapManager.Instance.DestroyAllEnemies();
                    MapManager.Instance.DestroyAllProjectiles();

                    SceneManager.LoadScene("Store"); // Store 씬으로 이동
                }
                else
                {
                    // 다음 스테이지로 이동
                    GameManager.Instance.LoadNextStage();
                }
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
