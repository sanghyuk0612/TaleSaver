using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class Store : MonoBehaviour
{
    public static Store Instance { get; private set; }

    public float interactionRange = 2f;
    List<int> selectedItem;
    private int[] prePrice;
    private int[] nowPrice;
    public static int[] changePrice = new int[5];

    [Header("First Visit 안내")]
    public TextMeshProUGUI firstStoreText;       // "스페이스바를 눌러 상점에 진입하세요."
    //public TextMeshProUGUI firstStoreWindowText; // "아이템을 눌러 설명을 확인하세요."

    [Header("Store Object")]
    public GameObject StoreWindow;
    ItemListData itemList;
    PlayerItemData inventory;
    private bool isFirstStoreVisit = true;

    [Header("아이템 이미지 목록")]
    public Sprite[] itemSprites;  // Inspector에 0~N번 아이템 이미지 순으로 넣기

    public Image item1image;
    public Image item2image;
    public Image item3image;


    [Header("Item Object")]
    public List<Button> buttonList;
    //public Image item1image;
    public Text item1Name;
    public Text item1Cost;
   // public Image item2image;
    public Text item2Name;
    public Text item2Cost;
    //public Image item3image;
    public Text item3Name;
    public Text item3Cost;
    public Text itemEx;
    public Text nowMoney;
    [Header("Ingredient Object")]
    public Text Stone;
    public Text Tree;
    public Text Skin;
    public Text Steel;
    public Text Gold;

    [Header("Ingredient Price Data")]
    public List<Text> prePriceData; // 돌, 나무, 가죽, 철, 금 순서
    public List<Text> nowPriceData;
    public List<Text> changeRate;

    private void Start()
    {
        ItemListData data = new ItemListData();
        data.InitializeSprites();
    }


    private void Awake() {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        GameManager.Instance.npcShow = 1;

        selectedItem = new List<int>();
        prePrice = new int[5];
        nowPrice = new int[5];
        nowMoney.text = "보유머니 : "+InventoryManager.Instance.inventory.battery.ToString();
        
        int randomIndex;
        for (int i = 0; i < 3; i++)
        {
            randomIndex = UnityEngine.Random.Range(0, ItemListData.items.Count);
            if(!InventoryManager.Instance.inventory.items.Exists(x => x == randomIndex)&&!selectedItem.Exists(x => x == randomIndex)){
                selectedItem.Add(randomIndex);
                continue;
            }
            i--;
        }

        //이미지 설정 코드
        item1image.sprite = itemSprites[ItemListData.items[selectedItem[0]].id];
        item2image.sprite = itemSprites[ItemListData.items[selectedItem[1]].id];
        item3image.sprite = itemSprites[ItemListData.items[selectedItem[2]].id];

        item1Name.text = ItemListData.items[selectedItem[0]].name;
        item1Cost.text = ItemListData.items[selectedItem[0]].price.ToString()+ " Gold";
        item2Name.text = ItemListData.items[selectedItem[1]].name;
        item2Cost.text = ItemListData.items[selectedItem[1]].price.ToString()+ " Gold";
        item3Name.text = ItemListData.items[selectedItem[2]].name;
        item3Cost.text = ItemListData.items[selectedItem[2]].price.ToString()+ " Gold";
        //stockUpdate();
        if (isFirstStoreVisit)
        {
            stockUpdate();
            isFirstStoreVisit = false;
        }
        Stone.text = InventoryManager.Instance.inventory.stone+"개 소유";
        Tree.text = InventoryManager.Instance.inventory.tree+"개 소유";
        Skin.text = InventoryManager.Instance.inventory.skin+"개 소유";
        Steel.text = InventoryManager.Instance.inventory.steel+"개 소유";
        Gold.text = InventoryManager.Instance.inventory.gold+"개 소유";
        Debug.Log("첫번째: "+selectedItem[0]+"두번째 : "+selectedItem[1]+"세번째 : " + selectedItem[2]);

        // 최초 실행 여부 확인
        if (!PlayerPrefs.HasKey("FirstStoreShown"))
        {
            if (firstStoreText != null)
            {
                firstStoreText.gameObject.SetActive(true);
                blinkCoroutine = StartCoroutine(BlinkText(firstStoreText));
            }

            PlayerPrefs.SetInt("FirstStoreShown", 1); // 다음부터는 비활성
            PlayerPrefs.Save();
        }
        else
        {
            // 이미 본 경우는 무조건 꺼버림
            if (firstStoreText != null)
            {
                firstStoreText.gameObject.SetActive(false);
            }
        }
    }
    public void stockUpdate(){
        prePrice[0] = InventoryManager.Instance.inventory.stonePrice;
        prePrice[1] = InventoryManager.Instance.inventory.treePrice;
        prePrice[2] = InventoryManager.Instance.inventory.skinPrice;
        prePrice[3] = InventoryManager.Instance.inventory.steelPrice;
        prePrice[4] = InventoryManager.Instance.inventory.goldPrice;
        for(int i=0;i<5;i++){
            int randomInt = UnityEngine.Random.Range(1, 6); //1~5로 등락률을 정함. 1은 하한가, 5는 상한가, 3는 그대로
            nowPrice[i] = prePrice[i]+InventoryManager.Instance.inventory.stockUpdateData[i][randomInt-1]; //가격 업데이트
            if(nowPrice[i]<InventoryManager.Instance.inventory.itemPriceRange[i,0]){
                nowPrice[i] = InventoryManager.Instance.inventory.itemPriceRange[i,0];
            }
            else if(nowPrice[i]>InventoryManager.Instance.inventory.itemPriceRange[i,1]){
                nowPrice[i] = InventoryManager.Instance.inventory.itemPriceRange[i,1];
            }
            changeRate[i].text = "(등락수치 : "+(nowPrice[i]-prePrice[i]).ToString()+")";
            changePrice[i] = nowPrice[i] - prePrice[i];
            if (nowPrice[i]-prePrice[i]>0){
                changeRate[i].color = Color.red;
            }
            else if(nowPrice[i]-prePrice[i]<0){
                changeRate[i].color = Color.blue;
            }
            else{
                changeRate[i].color = Color.gray;
            }
            prePriceData[i].text = "저번 라운드 가격 : "+prePrice[i].ToString();
            nowPriceData[i].text = "이번 라운드 가격 : "+nowPrice[i].ToString();
        }
        //가격 갱신
        InventoryManager.Instance.inventory.stonePrice=nowPrice[0];
        InventoryManager.Instance.inventory.treePrice=nowPrice[1];
        InventoryManager.Instance.inventory.skinPrice=nowPrice[2];
        InventoryManager.Instance.inventory.steelPrice=nowPrice[3];
        InventoryManager.Instance.inventory.goldPrice=nowPrice[4];
    }

    private void Update()
    {
        // 플레이어가 상호작용 범위에 있을 때 Space 바를 눌렀는지 확인
        if (GameManager.Instance.IsPlayerInRange && Input.GetKeyDown(KeyCode.Space))
        {
            OpenStore();
        }
    }
    private void OnTriggerEnter2D(Collider2D other) {
        Debug.Log("포탈들어옴");
        Debug.Log(ItemListData.items[0].name);
        if (other.CompareTag("Player"))
        {
            GameManager.Instance.IsPlayerInRange = true;
        }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        // 플레이어가 포탈 범위에서 나갔을 때
        if (other.CompareTag("Player"))
        {
            GameManager.Instance.IsPlayerInRange = false;
        }
    }
    private void OpenStore()
    {
        
        Debug.Log("상점 열림");
        Time.timeScale = 0;
        StoreWindow.SetActive(true);
        // 버튼에 효과음 주입
        itemEx.text = "각 아이템의 이미지를 눌러 설명을 확인하세요!";

        // ▶ 최초 안내 텍스트 중단 및 비활성화
        if (firstStoreText != null)
        {
            StopCoroutine(blinkCoroutine);
            firstStoreText.gameObject.SetActive(false);
        }

    }

    public void sellItem(int buttonId){
        switch(buttonId){
            case 0:
            InventoryManager.Instance.inventory.battery += InventoryManager.Instance.inventory.stone*nowPrice[buttonId];
            InventoryManager.Instance.inventory.stone=0;
            Stone.text = "0개 소유";
            break;
            case 1:
            InventoryManager.Instance.inventory.battery += InventoryManager.Instance.inventory.tree*nowPrice[buttonId];
            InventoryManager.Instance.inventory.tree=0;
            Tree.text = "0개 소유";
            break;
            case 2:
            InventoryManager.Instance.inventory.battery += InventoryManager.Instance.inventory.skin*nowPrice[buttonId];
            InventoryManager.Instance.inventory.skin=0;
            Skin.text = "0개 소유";
            break;
            case 3:
            InventoryManager.Instance.inventory.battery += InventoryManager.Instance.inventory.steel*nowPrice[buttonId];
            InventoryManager.Instance.inventory.steel=0;
            Steel.text = "0개 소유";
            break;
            case 4:
            InventoryManager.Instance.inventory.battery += InventoryManager.Instance.inventory.gold*nowPrice[buttonId];
            InventoryManager.Instance.inventory.gold=0;
            Gold.text = "0개 소유";
            break;
        }
        nowMoney.text = "보유 머니 : "+ InventoryManager.Instance.inventory.battery.ToString();
    }
    public void CloseStore(){
        Time.timeScale=1;
        StoreWindow.SetActive(false);
    }

    public void showItemEx(int buttonId){
        itemEx.text = ItemListData.items[selectedItem[buttonId]].explaination;
    }
    public void buyItem(int buyButtonId){
        if(InventoryManager.Instance.inventory.battery<ItemListData.items[selectedItem[buyButtonId]].price){
            Debug.Log(InventoryManager.Instance.inventory.battery);
            Debug.Log("돈없음");
        }
        else{
            InventoryManager.Instance.RemoveItem(5,ItemListData.items[selectedItem[buyButtonId]].price);
            buttonList[buyButtonId].interactable =false;
            InventoryManager.Instance.AddItem(8,ItemListData.items[selectedItem[buyButtonId]].id);
            nowMoney.text = "보유머니 : "+InventoryManager.Instance.inventory.battery.ToString();
        }
    }

    private Coroutine blinkCoroutine;

    private IEnumerator BlinkText(TextMeshProUGUI text)
    {
        Color originalColor = text.color;
        float alphaMin = 0.2f;
        float alphaMax = 1f;
        float blinkSpeed = 1f;

        while (true)
        {
            float alpha = Mathf.Lerp(alphaMin, alphaMax, Mathf.PingPong(Time.time * blinkSpeed, 1f));
            text.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;  // 매 프레임 갱신
        }
    }


    private IEnumerator HideTextAfterSeconds(TextMeshProUGUI text, float seconds)
    {
        yield return new WaitForSecondsRealtime(seconds); // Time.timeScale = 0 상태이므로 Realtime 사용
        text.gameObject.SetActive(false);
    }


}
