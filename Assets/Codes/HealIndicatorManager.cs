using UnityEngine;

public class HealIndicatorManager : MonoBehaviour
{
    public static HealIndicatorManager Instance { get; private set; }
    
    [Header("Heal Indicator Prefab")]
    public GameObject healIndicatorPrefab; // TextMeshPro가 붙은 프리팹

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

    public void ShowHealIndicator(Vector3 position, int heal)
    {
        if (healIndicatorPrefab == null)
        {
            Debug.LogWarning("Heal Indicator Prefab이 설정되지 않았습니다!");
            return;
        }

        // 회복 인디케이터 생성
        GameObject indicator = Instantiate(healIndicatorPrefab, position, Quaternion.identity);
        Debug.Log("회복 인디케이터 생성");
        // 회복 인디케이터 초기화
        HealIndicator healIndicator = indicator.GetComponent<HealIndicator>();
        if (healIndicator != null)
        {
            Color indicatorColor = Color.green;
            healIndicator.Initialize(heal, indicatorColor);
        }
    }
} 