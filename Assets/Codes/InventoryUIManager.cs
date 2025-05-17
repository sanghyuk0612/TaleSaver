using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class InventoryUIManager : MonoBehaviour
{
    public static InventoryUIManager Instance { get; private set; } // Singleton 인스턴스

    [System.Serializable]
    public class InventorySlot
    {
        public int id;
        public Image iconImage;
        public Text countText;
    }

    public List<InventorySlot> slots; // 인스펙터에서 슬롯 수동 연결

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬 전환에도 유지
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        UpdateAllSlots();
    }

    public void UpdateAllSlots()
    {
        foreach (var slot in slots)
        {
            UpdateSlotUI(slot.id);
        }
    }

    private void UpdateSlotUI(int id)
    {
        InventorySlot slot = slots.Find(s => s.id == id);
        if (slot == null) return;

        int count = 0;

        switch (id)
        {
            case 0: count = InventoryManager.Instance.inventory.stone; break;
            case 1: count = InventoryManager.Instance.inventory.tree; break;
            case 2: count = InventoryManager.Instance.inventory.skin; break;
            case 3: count = InventoryManager.Instance.inventory.steel; break;
            case 4: count = InventoryManager.Instance.inventory.gold; break;
        }

        // 개수 표시
        slot.countText.text = $"{count}개";
    }
}
