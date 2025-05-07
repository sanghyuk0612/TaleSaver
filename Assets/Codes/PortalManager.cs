using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PortalManager : MonoBehaviour
{
    public Text enemyText;
    public int enemyNumber;
    public int killNumber;
    public static PortalManager Instance { get; private set; }
    public GameObject Portal;

    private void Awake()
    {
        enemyNumber = 0;

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);  // 씬 전환 후에도 유지
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindEnemyText();
    }

    void Start()
    {
        FindEnemyText(); // 처음 시작할 때도 연결 시도
    }

    private void FindEnemyText()
    {
        GameObject textObj = GameObject.Find("/UI/monsterNumber/monster_Number_text");

        if (textObj != null)
        {
            enemyText = textObj.GetComponent<Text>();
            enemyText.text = "0";
            Debug.Log("enemyText 연결 성공");
        }
        else
        {
            enemyText = null;
            Debug.LogWarning("enemyText를 찾을 수 없습니다.");
        }
    }

    public void updateEnemy(int i)
    {
        enemyNumber += i;
        Debug.Log("updateEnemy: " + enemyNumber);

        if (enemyText != null)
            enemyText.text = enemyNumber.ToString();
    }

    public void killEnemy(int i)
    {
        killNumber += i;
        enemyNumber -= i;

        if (enemyNumber == 0)
        {
            MapManager.Instance.SpawnPortal();
        }

        if (enemyText != null)
            enemyText.text = enemyNumber.ToString();
    }

    void Update()
    {
        // 필요 시 추가 로직
    }
}
