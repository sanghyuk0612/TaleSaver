using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;  // Tilemap 사용을 위해 추가

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance { get; private set; }

    [SerializeField] private GameObject meleeEnemyPrefab;
    [SerializeField] private GameObject rangedEnemyPrefab;
    private GameObject BossPrefab;
    [SerializeField] private GameObject LavaPrefab;
    [SerializeField] private GameObject SlimePrefab;
    [SerializeField] private GameObject Werewolf;
    [SerializeField] private GameObject NPCPrefab;
    [SerializeField] private GameObject EventNPCPrefab;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }


    public void SpawnEntities()
    {
        if (MapManager.Instance == null)
        {
            Debug.LogError("MapManager instance is missing!");
            return;
        }

        MonsterData monsterData = MapManager.Instance.GetRandomMonsterForCurrentMap();

       if (monsterData == null)
        {
            Debug.LogWarning("No monster data found for this map!");
            return;
        }

        // 스폰 위치 설정
        int ran = Random.Range(0, MapManager.Instance.spawnPoints.Count);
        Vector3 spawnPos = MapManager.Instance.spawnPoints[ran];

        // 몬스터 유형에 따라 적절한 프리팹 선택
        GameObject enemyPrefab = (monsterData.isRanged) ? rangedEnemyPrefab : meleeEnemyPrefab;

        // 몬스터 생성
        GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
        PortalManager.Instance.updateEnemy(1);
        if (!monsterData.isRanged)
        {
            // 근거리 몬스터 설정
            MeleeEnemy meleeEnemy = enemy.GetComponent<MeleeEnemy>();
            if (meleeEnemy != null)
            {
                meleeEnemy.ApplyMonsterData(monsterData);
            }
        }
        else
        {
            // 원거리 몬스터 설정
            RangedEnemy rangedEnemy = enemy.GetComponent<RangedEnemy>();
            if (rangedEnemy != null)
            {
                rangedEnemy.ApplyMonsterData(monsterData);
            }
        }
    }

    public void SpawnBoss()
    {
        int location = MapManager.Instance.location;
        switch (location)
        {
            case 0:  //동굴
                break;
            case 1: //사막

                break;
            case 2: //숲
            
                break;
            case 3: //얼음
            
                break;
            case 4: //연구실
                BossPrefab = Werewolf;
                //BossPrefab = SlimePrefab;            
                break;
            case 5: //용암
                BossPrefab = LavaPrefab;
                break;
            case 6: //테스트
            
                break;
        }

        if (BossPrefab != null)
        {
            PortalManager.Instance.updateEnemy(1);
            //BossPrefab = Werewolf;
            Instantiate(BossPrefab, MapManager.Instance.portalPosition+new Vector3(0,5,0), Quaternion.identity);
        }
        else
        {
            Debug.LogWarning("Boss Prefab is not assigned!");
        }
    }

    public void SpawnNPC()
    {
        if (MapManager.Instance == null)
        {
            Debug.LogError("MapManager instance is missing!");
            return;
        }
        Vector3 tmp = new Vector3(0, 0.75f, 0);

        GameObject npcPrefab;
        int currentStage = GameManager.Instance.Stage;

        // Event NPC 생성 
        if (SceneManager.GetActiveScene().name == "BossStage")
        {
            npcPrefab = EventNPCPrefab;
            tmp = new Vector3(0, 0, 0);
            Debug.Log($"{currentStage} EventNPCPrefab is assigned!");
        }
        // NPC 생성 ( 조건 필요시 else if로 조건 설정 필요 )
        else
        {
            if(MapManager.Instance.NPCspawnPoints.Count==0){
            return;
            }
            npcPrefab = NPCPrefab;
            Debug.Log($"{currentStage} NPCPrefab is assigned!");
        }

        // 스폰 위치 설정

        int ran = Random.Range(0, MapManager.Instance.NPCspawnPoints.Count);
        Vector3 npcSpawnPos = MapManager.Instance.NPCspawnPoints[ran];
        npcSpawnPos += tmp;



        GameObject npc = Instantiate(npcPrefab, npcSpawnPos, Quaternion.identity);
        
        npc.transform.localScale = new Vector3(1.1f, 1.1f, 1f); // 2배로 확대
        
        GameObject canvas = GameObject.Find("DialoguePanel"); // 캔버스

        if (canvas != null)
        {
            NPCInteraction interaction = npc.GetComponent<NPCInteraction>();

            interaction.dialoguePanel = canvas.transform.Find("DialoguePanel").gameObject;

            // 자식의 자식까지 경로로 찾아줌
            interaction.dialogueText = canvas.transform.Find("DialoguePanel/DialogueText")?.GetComponent<Text>();
            interaction.nextButton = canvas.transform.Find("DialoguePanel/NextButton")?.GetComponent<Button>();
            interaction.yesButton = canvas.transform.Find("DialoguePanel/YesButton")?.GetComponent<Button>();
            interaction.noButton = canvas.transform.Find("DialoguePanel/NoButton")?.GetComponent<Button>();

            interaction.nextButton.onClick.RemoveAllListeners();
            interaction.nextButton.onClick.AddListener(() =>
            {
                interaction.DisplayNextDialogue();
            });

            interaction.yesButton.onClick.RemoveAllListeners();
            interaction.yesButton.onClick.AddListener(() =>
            {
                interaction.OnYesButtonClicked();
            });

            interaction.noButton.onClick.RemoveAllListeners();
            interaction.noButton.onClick.AddListener(() =>
            {
                interaction.OnNoButtonClicked();
            });

            if (interaction.dialogueText == null) Debug.LogError("dialogueText 연결 실패");
            if (interaction.nextButton == null) Debug.LogError("nextButton 연결 실패");
            if (interaction.yesButton == null) Debug.LogError("yesButton 연결 실패");
            if (interaction.noButton == null) Debug.LogError("noButton 연결 실패");
        }
        else
        {
            Debug.LogError("Canvas 'DialoguePanel' 못 찾음");
        }

    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
