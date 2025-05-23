using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; } // Singleton 인스턴스

    public PlayerItemData inventory = new PlayerItemData(); // 인벤토리 리스트

    // ID와 이름 매핑
    private Dictionary<int, string> itemNames = new Dictionary<int, string>
    {
        { 0, "Stone" },
        { 1, "Tree" },
        { 2, "Skin" },
        { 3, "Steel" },
        { 4, "Gold" },
        { 5, "Machineparts" },
        { 6, "Storybookpages" },
        { 7, "Battery" }
    };

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬 전환 시에도 유지
        }
        else
        {
            Destroy(gameObject); // 중복 인스턴스 제거
        }
    }

    // ID에 따른 이름 반환
    public string GetItemNameById(int id)
    {
        if (itemNames.TryGetValue(id, out string name))
        {
            return name;
        }
        return "Unknown";
    }

    // 아이템 추가
    public void AddItem(int id, int quantity)
    {
        switch (id)
        {
            case 0: inventory.stone += quantity; break;
            case 1: inventory.tree += quantity; break;
            case 2: inventory.skin += quantity; break;
            case 3: inventory.steel += quantity; break;
            case 4: inventory.gold += quantity; break;
            case 5: // ✅ 로비 재화
                inventory.machineparts += quantity;
                GameDataManager.Instance.machineParts += quantity;
                break;
            case 6: // ✅ 로비 재화
                inventory.storybookpages += quantity;
                GameDataManager.Instance.storybookPage += quantity;
                break;
            case 7: inventory.battery += quantity; break;
            case 8 :
                inventory.items.Add(quantity);
                break;
        }

        // ✅ 로비 재화일 때만 Firebase에 저장
        if (id == 5 || id == 6)
        {
            GameDataManager.Instance.SaveGoodsToFirestore();
        }
        // ✅ 인게임 재화일 때만 인벤토리 UI 저장
        if (id != 5 && id != 6)
        {
            if (InventoryUIManager.Instance != null)
                InventoryUIManager.Instance.UpdateAllSlots();
            else
                Debug.Log("InventoryUIManager is null");
        }
        
        Debug.Log($"Added {quantity} {GetItemNameById(id)} to inventory.");
    }


    // 아이템 제거
    public void RemoveItem(int id, int quantity)
    {
        switch (id)
        {
            case 0: // 돌
                inventory.stone -= quantity;
                if (inventory.stone <= 0)
                    inventory.stone = 0;
                break;
            case 1: // 나무
                inventory.tree -= quantity;
                if (inventory.tree <= 0)
                    inventory.tree = 0;
                break;
            case 2: // 가죽
                inventory.skin -= quantity;
                if (inventory.skin <= 0)
                    inventory.skin = 0;
                break;
            case 3: // 철
                inventory.steel -= quantity;
                if (inventory.steel <= 0)
                    inventory.steel = 0;
                break;
            case 4: // 금
                inventory.gold -= quantity;
                if (inventory.gold <= 0)
                    inventory.gold = 0;
                break;
            case 5://기계 조각
                inventory.machineparts -= quantity;
                GameDataManager.Instance.machineParts -= quantity;
                break;
            case 6: //동화 페이지
                inventory.storybookpages -= quantity;
                GameDataManager.Instance.storybookPage -= quantity;
                break;
            case 7: // 배터리
                inventory.battery -= quantity;
                break;
        }
        // ✅ 인게임 재화일 때만 인벤토리 UI 저장
        if (id != 5 && id != 6)
        {
            if (InventoryUIManager.Instance != null)
                InventoryUIManager.Instance.UpdateAllSlots();
            else
                Debug.Log("InventoryUIManager is null");
        }

        GameDataManager.Instance.SaveGoodsToFirestore(); // ✅ 즉시 Firebase 저장
    }

    public void SaveInventory()
    {
        SaveManager.Instance.SaveItemData(inventory);
    }

    public void LoadInventory()
    {
        inventory = SaveManager.Instance.LoadItemData();
    }
    public void ResetInventory()
    {
        // 인벤토리 초기화
        inventory = new PlayerItemData();

        Debug.Log("Inventory has been reset.");
    }

}