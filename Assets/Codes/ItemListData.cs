using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ItemList
{
    public int id;           // 아이템 고유 ID
    public string name;      // 아이템 이름
    public int price;        // 아이템 가격
    public string explaination;  // 아이템 설명
    public Sprite ItemSprite; // 아이템 스프라이트 추가

    // 생성자
    public ItemList(int id, string name, int price, string explaination)
    {
        this.id = id;
        this.name = name;
        this.price = price;
        this.explaination = explaination;
        this.ItemSprite = null; // 초기값 설정
    }
}

public class ItemListData
{
    public static List<ItemList> items = new List<ItemList>
    {
        new ItemList(0, "강철 망토", 500, "대시 중 무적 효과 추가"),          // Steel Cloak
        new ItemList(1, "구름 장화", 400, "점프 후 1초정도 부유효과"),        // Cloud Boots
        new ItemList(2, "롱소드", 700, "공격 데미지 15% 추가"),              // Long Sword
        new ItemList(3, "너클", 600, "치명타 확률 5% 증가"),                 // Knuckle
        new ItemList(4, "플레이트 갑옷", 800, "받는 데미지 15% 감소"),        // Plate Armor
        new ItemList(5, "덤벨", 550, "최대 체력 현재 체력의 15% 증가"),        // Dumbbell
        new ItemList(6, "주사위", 300, "처치 시 재료 드랍 확률 5% 증가"),      // Dice
        new ItemList(7, "얼음 장화", 450, "이동 속도 15% 증가"),             // Ice Boots
        new ItemList(8, "고급 기름", 200, "체력 25% 회복 (소모품)"),         // Premium Oil
        new ItemList(9, "윤활유", 350, "공속 10% 증가"),                     // Lubricant
        new ItemList(10, "쉴드", 750, "공격 1회 무효"),                      // Shield
        new ItemList(11, "피닉스(고오급)", 1000, "죽을 시 25% 체력으로 자동 부활 (1회)"), // Phoenix HighGrade
        new ItemList(12, "토르의 망치", 950, "치명타 적중 시 번개"),          // Thor Hammer
        new ItemList(13, "가시갑옷", 850, "피격 시 5% 공격반사"),            // Thorn Armor
        new ItemList(14, "얼음 칼날", 900, "일정 거리 내 적 둔화 (적 이동 속도 20% 감소)"), // Ice Blade
        new ItemList(15, "회전 회오리", 1200, "35초마다 회오리 생성 (회오리로 적 쓸어버림)"), // Spinning Tornado
        new ItemList(16, "제우스의 번개", 1500, "55초마다 해당 맵 절반만큼의 적에게 벼락 공격"), // Zeus Lightning
        new ItemList(17, "광전사의 가면", 1100, "체력 비율이 30% 이하일 때 데미지 50% 증가")   // Berserker Mask
    };

    public void InitializeSprites()
    {
        foreach (var item in items)
        {
            item.ItemSprite = Resources.Load<Sprite>($"Sprites/{item.name}");
        }
    }
}