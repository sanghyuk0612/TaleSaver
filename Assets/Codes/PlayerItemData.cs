using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class PlayerItemData
{
    public int stone;            // 아이템: Stone 개수
    public int stonePrice;       // 돌 가격 
    public int tree;             // 아이템: 나무 개수
    public int treePrice;        // 나무 가격 
    public int skin;             // 아이템: Skin 개수
    public int skinPrice;        // 가죽 가격 
    public int steel;            // 아이템: Steel 개수
    public int steelPrice;       // 철 가격 
    public int gold;             // 아이템: Gold 개수
    public int goldPrice;        //  금 가격 
    public int money;            // 돈
    public int battery;          // 아이템: Battery 개수
    public int steelPiece;       //기계 조각
    public int bookPage;         //동화책 조각각
    public List<int> items;   // 플레이어가 소유한 아이템 목록
    public int[,] itemPriceRange=new int[5,2]; //아이템 가격 최저가 최고가 정의, 돌, 나무, 가죽, 철, 금 순서
    public int[][] stockUpdateData = new int[5][]; //아이템별 등락률 수치 정의
    // 생성자: 모든 값을 초기화
    public PlayerItemData()
    {
        stone = 10;
        stonePrice = 3;
        tree = 10;
        treePrice = 17;
        skin = 10;
        skinPrice = 50;
        steel = 10;
        steelPrice = 95;
        gold = 10;
        goldPrice = 165;
        battery = 0;
        money = 0;
        steelPiece = 0;
        bookPage = 0;
        items = new List<int>(); // 빈 리스트로 초기화
        itemPriceRange[0,0] = 1; //돌 최저가
        itemPriceRange[0,1] = 5; //돌 최고가
        itemPriceRange[1,0] = 10;
        itemPriceRange[1,1] = 25;
        itemPriceRange[2,0] = 40;
        itemPriceRange[2,1] = 60;
        itemPriceRange[3,0] = 85;
        itemPriceRange[3,1] = 105;
        itemPriceRange[4,0] = 130;
        itemPriceRange[4,1] = 200;
        //얼마나 오를지 정의
        stockUpdateData[0] = new int[]{-4,-2,0,2,4};
        stockUpdateData[1] = new int[]{-6,-3,0,3,6};
        stockUpdateData[2] = new int[]{-10,-5,0,5,10};
        stockUpdateData[3] = new int[]{-10,-5,0,5,10};
        stockUpdateData[4] = new int[]{-15,-7,0,7,15};
    }
}

