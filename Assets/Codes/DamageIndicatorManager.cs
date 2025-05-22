using UnityEngine;

public class DamageIndicatorManager : MonoBehaviour
{
    public static DamageIndicatorManager Instance { get; private set; }
    
    [Header("Damage Indicator Prefab")]
    public GameObject damageIndicatorPrefab; // TextMeshPro가 붙은 프리팹

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ShowDamageIndicator(Vector3 position, int damage, bool isPlayer = false)
    {
        if (damageIndicatorPrefab == null)
        {
            Debug.LogError("DamageIndicator 프리팹이 설정되지 않았습니다!");
            return;
        }

        GameObject indicator = Instantiate(damageIndicatorPrefab, position, Quaternion.identity);
        DamageIndicator damageIndicator = indicator.GetComponent<DamageIndicator>();
        if (damageIndicator != null)
        {
            // 플레이어는 노란색, 적은 파란색으로 표시
            Color indicatorColor = isPlayer ? Color.yellow : Color.blue;
            Debug.Log($"데미지 인디케이터 생성: damage={damage}, isPlayer={isPlayer}, color={indicatorColor}");
            damageIndicator.Initialize(damage, indicatorColor);
        }
    }

    public void ShowCriticalDamageIndicator(Vector3 position, int damage, bool isPlayer = false)
    {
        if (damageIndicatorPrefab == null)
        {
            Debug.LogWarning("Damage Indicator Prefab이 설정되지 않았습니다!");
            return;
        }

        // 데미지 인디케이터 생성
        GameObject indicator = Instantiate(damageIndicatorPrefab, position, Quaternion.identity);
        
        // 데미지 인디케이터 초기화
        DamageIndicator damageIndicator = indicator.GetComponent<DamageIndicator>();
        if (damageIndicator != null)
        {
            // 크리티컬 데미지는 더 큰 크기와 다른 색상으로 표시
            damageIndicator.Initialize(damage, Color.magenta);
            damageIndicator.transform.localScale *= 1.5f; // 50% 더 크게
        }
    }
} 