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

    public void Initialize(int id, string name)
    {
        itemId = id;
        itemName = name;

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

        if (itemPrefab == null)
        {
            Debug.LogError("Item prefab이 설정되지 않음!");
            return;
        }

        // 랜덤 아이템 ID 설정 (0~8 중 하나)
        int randomId = Random.Range(0, 9);
        string randomItemName = inventoryManager.GetItemNameById(randomId);

        // 드랍 위치
        Vector3 dropPosition = transform.position;

        // 아이템 생성
        GameObject droppedItem = Instantiate(itemPrefab, dropPosition, Quaternion.identity);

        // 아이템 초기화
        DroppedItem itemComponent = droppedItem.GetComponent<DroppedItem>();
        if (itemComponent != null)
        {
            itemComponent.Initialize(randomId, randomItemName);
        }
        else
        {
            Debug.LogError("DroppedItem 컴포넌트를 찾을 수 없음!");
        }

        Debug.Log($"✅ {randomItemName} 아이템 드랍됨! 위치: {dropPosition}");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log($"Collision detected with: {collision.name}");

        if (collision.CompareTag("Player"))
        {
            Debug.Log("Player collided with dropped item!");
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.AddItem(itemId, quantity);
            }
            else
            {
                Debug.LogError("InventoryManager 인스턴스를 찾을 수 없음!");
            }

            // 아이템 제거
            Destroy(gameObject);
        }
    }
}
