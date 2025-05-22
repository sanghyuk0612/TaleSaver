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

    public void ShowHealIndicator(Vector3 position, int healAmount)
    {
        if (healIndicatorPrefab == null)
        {
            Debug.LogError("HealIndicator 프리팹이 설정되지 않았습니다!");
            return;
        }

        // 회복 인디케이터 생성
        GameObject indicator = Instantiate(healIndicatorPrefab, position, Quaternion.identity);
        Debug.Log("회복 인디케이터 생성");
        // 회복 인디케이터 초기화
        HealIndicator healIndicator = indicator.GetComponent<HealIndicator>();
        if (healIndicator != null)
        {
            healIndicator.Initialize(healAmount, Color.green);
            Debug.Log($"힐 인디케이터 생성: healAmount={healAmount}");
        }
        else
        {
            Debug.LogError("HealIndicator 컴포넌트를 찾을 수 없습니다!");
        }
    }
} 