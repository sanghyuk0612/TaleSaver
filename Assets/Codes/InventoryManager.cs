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
        { 5, "Battery" },
        { 6, "Machineparts" },
        { 7, "Storybookpages" }
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
            case 5: inventory.battery += quantity; break;
            case 6: // machineparts는 로비 재화
                GameDataManager.Instance.machineParts += quantity;
                break;
            case 7: // storybookpages도 로비 재화
                GameDataManager.Instance.storybookPage += quantity;
                break;
            case 8:
                inventory.items.Add(quantity);
                break;
        }

        Debug.Log($"Added {quantity} {GetItemNameById(id)} to inventory.");

        // 로컬 재화가 아닌 경우만 저장
        if (id == 6 || id == 7)
        {
            GameDataManager.Instance.SaveGoodsToFirestore();
        }
    }

    // 아이템 제거
    public void RemoveItem(int id, int quantity)
    {
        switch (id)
        {
            case 0: // 돌
                inventory.stone -= quantity;
                break;
            case 1: // 나무
                inventory.tree -= quantity;
                break;
            case 2: // 가죽
                inventory.skin -= quantity;
                break;
            case 3: // 철
                inventory.steel -= quantity;
                break;
            case 4: // 금
                inventory.gold -= quantity;
                break;
            case 5: // 배터리
                inventory.battery -= quantity;
                break;
            case 6://기계 조각
                inventory.machineparts -= quantity;
                GameDataManager.Instance.machineParts -= quantity;
                break;
            case 7: //동화 페이지
                inventory.storybookpages -= quantity;
                GameDataManager.Instance.storybookPage -= quantity;
                break;
        }
        GameDataManager.Instance.SaveGoodsToFirestore(); // ✅ 즉시 Firebase 저장
    }

    public void ResetLocalResources()
    {
        inventory.stone = 0;
        inventory.tree = 0;
        inventory.skin = 0;
        inventory.steel = 0;
        inventory.gold = 0;
        inventory.battery = 0;

        Debug.Log("🧹 로컬 재화 초기화 완료");
    }

    public void SaveInventory()
    {
        SaveManager.Instance.SaveItemData(inventory);
    }

    public void LoadInventory()
    {
        inventory = SaveManager.Instance.LoadItemData();
    }
}