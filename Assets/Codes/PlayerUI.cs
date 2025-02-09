using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [Header("Health UI")]
    [SerializeField] private Slider healthSlider;

    [Header("Position Settings")]
    [SerializeField] private Vector3 offset = new Vector3(0, -85f, 0);

    private PlayerController player;
    private Camera mainCamera;
    private RectTransform rectTransform;
    

    private void Start()
    {
        mainCamera = Camera.main;
        rectTransform = GetComponent<RectTransform>();
        
        offset = new Vector3(0, -85f, 0);
        
        // PlayerController를 찾고 슬라이더 초기화
        player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            InitializeHealthUI();
        }
    }
    

    private void Update()
    {
        if (player != null && mainCamera != null)
        {
            Vector3 screenPos = mainCamera.WorldToScreenPoint(player.transform.position);
            rectTransform.position = screenPos + offset;
            
            healthSlider.value = player.CurrentHealth; // 슬라이더의 값을 플레이어의 현재 체력으로 업데이트
        }
    }

    private void InitializeHealthUI()
    {
        healthSlider.maxValue = player.MaxHealth; // 플레이어의 최대 체력으로 설정
        healthSlider.value = player.CurrentHealth; // 초기 체력 설정
    }

    public void SetPlayer(PlayerController playerController)
    {
        player = playerController; // PlayerController를 설정
        InitializeHealthUI(); // 슬라이더 초기화
    }
}
