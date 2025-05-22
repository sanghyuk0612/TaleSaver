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
        new ItemList(0, "강철 망토", 1500, "대시 중 무적 효과 추가"),          // Steel Cloak 구현 완
        new ItemList(1, "구름 장화", 1200, "점프 후 1초정도 부유효과"),        // Cloud Boots '구현 취소'
        new ItemList(2, "롱소드", 2100, "공격 데미지 15% 추가"),              // Long Sword  구현 완
        new ItemList(3, "너클", 1800, "치명타 확률 5% 증가"),                 // Knuckle    
        new ItemList(4, "플레이트 갑옷", 2400, "받는 데미지 15% 감소"),        // Plate Armor 구현 완 
        new ItemList(5, "덤벨", 1650, "최대 체력 현재 체력의 15% 증가"),        // Dumbbell   구현 완
        new ItemList(6, "주사위", 900, "처치 시 재료 드랍 확률 5% 증가"),      // Dice
        new ItemList(7, "얼음 장화", 1350, "이동 속도 15% 증가"),             // Ice Boots   구현 완
        new ItemList(8, "고급 기름", 600, "체력 25% 회복 (소모품)"),         // Premium Oil  구현 완
        new ItemList(9, "윤활유", 1050, "공속 10% 증가"),                     // Lubricant  구현 완
        new ItemList(10, "쉴드", 2250, "공격 1회 무효"),                      // Shield      
        new ItemList(11, "피닉스(고오급)", 100, "죽을 시 25% 체력으로 자동 부활 (1회)"), // Phoenix HighGrade   1 (부활 확인하기 위해 가격 인하) 구현 완
        new ItemList(12, "토르의 망치", 2850, "치명타 적중 시 번개"),          // Thor Hammer       
        new ItemList(13, "가시갑옷", 2550, "피격 시 5% 공격반사"),            // Thorn Armor                     
        new ItemList(14, "얼음 칼날", 2700, "일정 거리 내 적 둔화 (적 이동 속도 20% 감소)"), // Ice Blade               
        new ItemList(15, "회전 회오리", 3600, "35초마다 회오리 생성 (회오리로 적 쓸어버림)"), // Spinning Tornado      
        new ItemList(16, "제우스의 번개", 4500, "55초마다 해당 맵 절반만큼의 적에게 벼락 공격"), // Zeus Lightning      
        new ItemList(17, "광전사의 가면", 3300, "체력 비율이 30% 이하일 때 데미지 50% 증가"),   // Berserker Mask      
        new ItemList(18, "작은 날개", 999, "점프력 증가")   // small wing                    구현 완      
    };

    public void InitializeSprites()
    {
        foreach (var item in items)
        {
            item.ItemSprite = Resources.Load<Sprite>($"Sprites/{item.name}");
        }
    }
}