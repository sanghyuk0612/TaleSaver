using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroppedItem : MonoBehaviour
{
    [Header("Item Drop")]
    [SerializeField] private GameObject itemPrefab; // 아이템 프리팹
    private InventoryManager inventoryManager;

    private int itemId;
    private string itemName;
    private int quantity = 1; // 기본 수량
    //드랍 아이템 에셋
    [SerializeField] private Sprite[] itemSprites;

    public void Initialize(int id, string name)
    {
        itemId = id;
        itemName = name;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null && itemSprites != null && id >= 0 && id < itemSprites.Length)
        {
            sr.sprite = itemSprites[id]; // 아이템 ID에 해당하는 스프라이트로 교체
        }
        else
        {
            Debug.LogWarning($"⚠️ Sprite 적용 실패: id={id}, itemSprites 설정 확인 필요");
        }
    }

    private void Awake()
    {
        if (InventoryManager.Instance == null)
        {
            Debug.LogError("InventoryManager not found in the scene.");
            return; // InventoryManager가 없으면 메서드 종료
        }

        inventoryManager = InventoryManager.Instance; // inventoryManager 초기화
    }

    public void DropItem()
    {
        if (inventoryManager == null)
        {
            Debug.LogError("InventoryManager가 설정되지 않음!");
            return;
        }

        // 랜덤 아이템 ID 설정 (0~7 중 하나)
        int randomId = Random.Range(0, 7);
        string randomItemName = inventoryManager.GetItemNameById(randomId);

        // 현재 아이템 오브젝트를 초기화
        Initialize(randomId, randomItemName);

        Debug.Log($"✅ {randomItemName} 아이템 드랍됨! 위치: {transform.position}");
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log($"Collision detected with: {collision.name}");

        if (collision.CompareTag("Player"))
        {
            Debug.Log("Player collided with dropped item!");

            if (InventoryManager.Instance == null)
            {
                Debug.LogError("InventoryManager 인스턴스를 찾을 수 없음!");
                return;
            }

            // Firebase 로그인 준비가 되었을 때만 AddItem 호출
            StartCoroutine(FirebaseAuthManager.Instance.WaitUntilUserIsReady(() =>
            {
                InventoryManager.Instance.AddItem(itemId, quantity);
                Destroy(gameObject);  // 아이템 제거는 여기서 해야 함
            }));
        }
        else
        {
            Debug.Log("!CompareTag Player");
        }
    }
}

