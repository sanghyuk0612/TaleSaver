using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // 씬 관리를 위한 네임스페이스 추가

public class PlayerUI : MonoBehaviour
{
    [Header("Health UI")]
    [SerializeField] private Slider healthSlider;

    [Header("Position Settings")]
    [SerializeField] private Vector3 offset = new Vector3(0, 70f, 0); // 위쪽으로 조금 더 올림

    private PlayerController player;
    private Camera mainCamera;
    private RectTransform rectTransform;
    private string currentSceneName; // 현재 씬 이름 저장
    

    private void Start()
    {
        mainCamera = Camera.main;
        rectTransform = GetComponent<RectTransform>();
        
        // 현재 씬 이름 저장
        currentSceneName = SceneManager.GetActiveScene().name;
        
        // PlayerController를 찾고 슬라이더 초기화
        player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            InitializeHealthUI();
            Debug.Log("PlayerUI: Start - 플레이어를 찾아 초기화했습니다");
        }
        else
        {
            Debug.LogWarning("PlayerUI: Start - 플레이어를 찾을 수 없습니다!");
        }
        
        // 씬 로드 이벤트 등록
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    private void OnDestroy()
    {
        // 이벤트 등록 해제
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    // 씬 로드 시 호출될 이벤트 핸들러
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 씬이 변경되었을 때만 처리
        if (scene.name != currentSceneName)
        {
            // 씬 이름 업데이트
            currentSceneName = scene.name;
            Debug.Log($"PlayerUI: 씬 전환 감지 - {currentSceneName}");
            
            // 약간의 딜레이 후 플레이어 참조와 UI 위치 갱신
            StartCoroutine(ReconnectAfterSceneLoad());
        }
    }
    
    private IEnumerator ReconnectAfterSceneLoad()
    {
        // 씬 로드 후 오브젝트들이 초기화될 시간 확보
        yield return new WaitForSeconds(0.1f);
        
        // 메인 카메라 다시 찾기
        mainCamera = Camera.main;
        
        // 플레이어 다시 찾기
        player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            // UI 초기화
            InitializeHealthUI();
            
            // 강제로 위치 업데이트
            if (mainCamera != null)
            {
                Vector3 screenPos = mainCamera.WorldToScreenPoint(player.transform.position);
                rectTransform.position = screenPos + offset;
                Debug.Log($"PlayerUI: 씬 전환 후 위치 재설정 - {rectTransform.position}");
            }
        }
        else
        {
            Debug.LogWarning("PlayerUI: 씬 전환 후 플레이어를 찾을 수 없습니다!");
        }
    }

    private void Update()
    {
        if (player == null)
        {
            // 플레이어를 다시 찾아보기 (씬 전환 후 유용)
            player = FindObjectOfType<PlayerController>();
            if (player != null)
            {
                InitializeHealthUI();
                Debug.Log("PlayerUI: Update - 플레이어를 다시 찾았습니다");
            }
        }
        
        if (player != null && mainCamera != null)
        {
            // 플레이어 위치를 화면 좌표로 변환
            Vector3 screenPos = mainCamera.WorldToScreenPoint(player.transform.position);
            
            // UI 위치 설정
            rectTransform.position = screenPos + offset;
            
            // 슬라이더 값 업데이트
            if (healthSlider != null)
            {
                // 비율이 아닌 실제 값 사용
                float healthPercent = (float)player.CurrentHealth / player.MaxHealth;
                healthSlider.value = healthPercent;
            }
        }
    }

    private void InitializeHealthUI()
    {
        if (healthSlider != null && player != null)
        {
            healthSlider.maxValue = 1.0f; // 0~1 사이 값 사용
            float healthPercent = (float)player.CurrentHealth / player.MaxHealth;
            healthSlider.value = healthPercent;
            Debug.Log($"PlayerUI: 체력 슬라이더 초기화 - {player.CurrentHealth}/{player.MaxHealth} ({healthPercent:P0})");
        }
    }

    public void SetPlayer(PlayerController playerController)
    {
        this.player = playerController; // PlayerController를 설정
        InitializeHealthUI(); // 슬라이더 초기화
        
        // 강제로 위치 업데이트
        if (mainCamera != null && playerController != null)
        {
            Vector3 screenPos = mainCamera.WorldToScreenPoint(playerController.transform.position);
            rectTransform.position = screenPos + offset;
            Debug.Log($"PlayerUI: SetPlayer - 플레이어 설정 완료, 위치: {rectTransform.position}");
        }
    }
    
    // 체력 슬라이더 업데이트 메서드 추가
    public void UpdateHealthSlider(float healthPercent)
    {
        if (healthSlider != null)
        {
            healthSlider.value = healthPercent;
            Debug.Log($"PlayerUI: UpdateHealthSlider - 체력 비율: {healthPercent:P0}");
        }
    }
}
