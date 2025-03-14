using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PortalManager : MonoBehaviour
{
    public Text enemyText;
    public int enemyNumber;
    public int killNumber;
    public static PortalManager Instance { get; private set; }
    public GameObject Portal;
    
    // Start is called before the first frame update
    private void Awake() {
        enemyNumber=0;
        // 싱글톤 인스턴스 초기화
        
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("PortalManager Instance가 이미 존재합니다! 중복된 오브젝트를 삭제합니다.");
            Destroy(gameObject); // 중복된 오브젝트 방지
        }
    }
    public void updateEnemy(int i){
        enemyNumber+=i;
        Debug.Log("널인가? "+enemyNumber);
        enemyText.text = enemyNumber.ToString();
    }
    public void killEnemy(int i){
        killNumber+=i;
        enemyNumber-=i;
        if(enemyNumber==0){
            MapManager.Instance.SpawnPortal();
        }
        enemyText.text = enemyNumber.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
