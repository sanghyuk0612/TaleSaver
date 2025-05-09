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
                PortalManager.Instance.ResetEnemyCount();
                if (SceneManager.GetActiveScene().name == "GameScene")
                {
                    int currentStage = GameManager.Instance.Stage;
                    GameManager.Instance.SavePlayerState();
                    // 스테이지 4와 8 이후에는 Store 씬으로 이동
                    if (currentStage == 4 || currentStage == 8)
                    {
                        // 기존 몬스터와 투사체 제거
                        MapManager.Instance.DestroyAllEnemies();
                        MapManager.Instance.DestroyAllProjectiles();
                        GameManager.Instance.EnterStore(); // Store 진입 처리
                        SceneManager.LoadScene("Store"); // Store 씬으로 이동
                    }
                    else if (currentStage == 9)
                    {
                        GameManager.Instance.Stage = 10; // ✅ 보스 스테이지 진입 전 Stage를 10으로 설정
                        SceneManager.LoadScene("BossStage");
                    }
                    else
                    {
                        // 다음 스테이지로 이동
                        GameManager.Instance.LoadNextStage();
                    }
                }
                else if (SceneManager.GetActiveScene().name == "Store")
                {
                    if (player != null)
                    {
                        int currentStage = GameManager.Instance.Stage;

                        SceneManager.LoadScene("GameScene");

                        // 다음 스테이지로 이동
                        GameManager.Instance.SavePlayerState();
                        GameManager.Instance.ExitStore(); //Store에서 나가기 전 stage 복원
                        GameManager.Instance.LoadNextStage();
                    }
                }
                else if (SceneManager.GetActiveScene().name == "BossStage")
                {
                    Debug.Log("보스 포탈 사용");
                    GameManager.Instance.location = 4; //시연용 다음 맵은 연구소
                    SceneManager.LoadScene("GameScene");
                    GameManager.Instance.LoadNextCapter();
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
