using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Progress")]
    [SerializeField] private int score;
    [SerializeField] private int stage;
    [SerializeField] private int chapter;
    [SerializeField] private bool isPlayerInRange;
    [SerializeField] private float playTime;
    [SerializeField] public int location = 0;

    private int lastStageBeforeStore = -1;

    public void EnterStore() //store 진입 전 last변수에 현재 stage 저장
    {
        lastStageBeforeStore = stage;
        stage = 0;
    }
    public void ExitStore() //store 나오면 stage에 last stage 값 재 삽입
    {
        if (lastStageBeforeStore != -1)
            stage = lastStageBeforeStore;
    }
    public float PlayTime
    {
        get => playTime;
        set => playTime = value;
    }


    // 프로퍼티를 통한 접근
    public int Score => score;
    public int Stage
    {
        get => stage;
        set => stage = value;
    }
    public int Chapter => chapter;
    public bool IsPlayerInRange
    {
        get => isPlayerInRange;
        set => isPlayerInRange = value;
    }

    [Header("Player Settings")]
    public float playerMoveSpeed = 5f;
    public float playerJumpForce = 9.6f;
    public float playerGravityScale = 2.5f;
    public int playerMaxJumpCount = 2;
    public float playerDashForce = 15f;
    public float playerDashCooldown = 5f;
    public int playerMaxHealth = 100; // 기본값으로만 사용됨

    [Header("Melee Enemy Settings")]
    public float meleeEnemyMoveSpeed = 3f;
    public float meleeEnemyDetectionRange = 5f;
    public int meleeEnemyDamage = 10;
    public float meleeEnemyKnockbackForce = 9f;
    public float meleeEnemyDamageCooldown = 0.5f;

    [Header("Ranged Enemy Settings")]
    public float rangedEnemyMoveSpeed = 3f;
    public float rangedEnemyAttackRange = 7f;
    public float rangedEnemyDetectionRange = 15f;
    public int rangedEnemyDamage = 10;
    public float rangedEnemyAttackCooldown = 2f;
    public float rangedEnemyProjectileSpeed = 8f;

    [Header("Enemy Projectile Settings")]
    public float enemyProjectileKnockbackForce = 8f;

    private const int MAX_STAGE = 10;  // 스테이지 최대값 상수 추가

    // 플레이어 현재 상태 저장용 변수
    private int currentPlayerHealth;
    public int CurrentPlayerHealth
    {
        get => currentPlayerHealth;
        set => currentPlayerHealth = value;
    }

    [Header("UI Prefabs")]
    public GameObject playerUIPrefab; // PlayerUI 프리팹을 위한 변수
    public SpriteRenderer playerSpriteRenderer; // 게임 캐릭터의 SpriteRenderer
    public Text monsterNumber;


    // 새로운 기능: 현재 선택된 캐릭터 데이터
    public CharacterData CurrentCharacter { get; private set; }
    private SkillManager skillManager;
    private int currentHealth;
    private int maxHealth;
    private float[] skillCooldownTimers;

    // 게임오버 UI 관련 변수 추가
    [Header("Game Over UI")]
    public GameObject gameOverPanel; // 게임오버 UI 패널
    public Button restartButton; // 재시작 버튼
    public Button ExitButton; // 재시작 버튼
    public Text DeathStage;
    public Text DeathTime;

    // 플레이어 체력 상태 저장용 변수 (단순화)
    private int savedPlayerHealth = -1;
    
    // 체력 관련 메서드
    public bool HasSavedPlayerHealth() => savedPlayerHealth > 0;
    
    public int GetSavedPlayerHealth() => savedPlayerHealth;
    
    public void SavePlayerHealth(int currentHealth, int maxHealth)
    {
        savedPlayerHealth = currentHealth;
        Debug.Log($"GameManager에 플레이어 체력 저장: {savedPlayerHealth}");
    }
    
    // PlayerController에서 직접 호출하도록 개선
    public void RestorePlayerState(PlayerController player)
    {
        if (player != null && HasSavedPlayerHealth())
        {
            Debug.Log($"GameManager에서 플레이어 체력 복원: {savedPlayerHealth}");
            player.RestoreHealth(savedPlayerHealth);
        }
    }
    
    // ModifyHealth 대신 PlayerController에서 처리하도록 변경
    public void HealPlayer(int amount)
    {
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            player.RestoreHealth(amount);
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
        FindAndConnectGameOverUI();
    }


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // SkillManager 컴포넌트 추가
            skillManager = gameObject.AddComponent<SkillManager>();
            
            // 캐릭터 데이터 불러오기 시도
            LoadSelectedCharacter();
            
            // 초기화는 캐릭터 로드 후에 수행
            InitializeGameState();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void FindAndConnectGameOverUI()
    {
        // 게임오버 패널 찾기
        if (gameOverPanel == null)
            gameOverPanel = GameObject.Find("GameOverPanel");

        if (gameOverPanel != null)
        {
            if (DeathStage == null)
                DeathStage = gameOverPanel.transform.Find("DeathStage")?.GetComponent<Text>();

            if (DeathTime == null)
                DeathTime = gameOverPanel.transform.Find("DeathTime")?.GetComponent<Text>();
        }

        // 재시작 버튼 찾기
        if (restartButton == null)
        {
            restartButton = gameOverPanel.GetComponentInChildren<Button>();
            if (restartButton == null)
            {
                Debug.LogWarning("RestartButton을 찾을 수 없습니다!");
                return;
            }
        }

        if (ExitButton == null)
        {
            ExitButton = GameObject.Find("Exit")?.GetComponent<Button>();
        }
        if (ExitButton != null)
        {
            ExitButton.onClick.RemoveAllListeners();
            ExitButton.onClick.AddListener(QuitGame);
        }


        // 재시작 버튼에 이벤트 연결
        restartButton.onClick.RemoveAllListeners();
        restartButton.onClick.AddListener(RestartGame);

        // 초기에는 게임오버 패널 비활성화
        gameOverPanel.SetActive(false);
    }

    private void Start()
    {
        //위치 랜덤 지정
        // location = Random.Range(0,6);
        // while(location==4){
        //     location = Random.Range(0,6);
        // }
        //FindAndConnectGameOverUI();
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);  // 시작 시 숨기기
        // 현재 캐릭터의 최대 체력 설정
        maxHealth = CurrentCharacter != null ? CurrentCharacter.maxHealth : playerMaxHealth;

        // 실시간 디버깅을 위해 체력 초기값 로깅
        Debug.Log($"GameManager Start - 설정된 최대 체력: {maxHealth}, 현재 체력: {currentPlayerHealth}");
        
        // 현재 체력이 초기화되지 않았거나 최대 체력보다 크면 최대 체력으로 설정 
        if (currentPlayerHealth <= 0 || currentPlayerHealth > maxHealth)
        {
            currentPlayerHealth = maxHealth;
            Debug.Log($"체력 초기화 - currentPlayerHealth: {currentPlayerHealth}");
        }
        
        // 플레이어 컨트롤러와 체력 동기화
        SyncPlayerHealth();
        
        skillCooldownTimers = new float[5];
        Debug.Log("스킬쿨 초기화");
        Debug.Log(skillCooldownTimers);
        
        // 게임 시작 시간 기록
        //gameStartTime = Time.time;
        
        // 게임오버 UI 초기화
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }

    // 플레이어 컨트롤러와 체력 동기화하는 메서드
    private void SyncPlayerHealth()
    {
        PlayerController playerController = FindObjectOfType<PlayerController>();
        if (playerController != null)
        {
            // 현재 플레이어의 체력 확인
            int playerHealth = playerController.CurrentHealth;
            
            Debug.Log($"체력 동기화 - GameManager: {currentPlayerHealth}, PlayerController: {playerHealth}");
            
            // 플레이어 컨트롤러의 체력 업데이트
            playerController.UpdateHealth(currentPlayerHealth);
        }
    }

    private void Update()
    {
        // 스킬 쿨타임 타이머 업데이트
        for (int i = 0; i < skillCooldownTimers.Length; i++)
        {
            if (skillCooldownTimers[i] > 0)
            {
                skillCooldownTimers[i] -= Time.deltaTime;
            }
        }

        // 키 입력 감지 및 스킬 사용
        if (Input.GetKeyDown(KeyCode.T))
        {
            UseSkill(0);
        }
        if (Input.GetKeyDown(KeyCode.Y))
        {
            UseSkill(1);
        }
        if (Input.GetKeyDown(KeyCode.U))
        {
            UseSkill(2);
        }
        if (Input.GetKeyDown(KeyCode.I))
        {
            UseSkill(3);
        }
        // 기본 공격 (V키)
        if (Input.GetKeyDown(KeyCode.V))
        {
            UseSkill(4);  // BaseG 스킬 사용 (인덱스 4)
        }
        if (Input.GetKeyDown(KeyCode.O))
        {
            // 현재 선택된 캐릭터의 이름과 스킬 출력
            LogCurrentCharacterInfo();
        }
    }

    private void InitializeGameState()
    {
        chapter = 1;
        stage = 1;
        score = 0;
        
        isPlayerInRange = false;
    }
    

    // 스테이지 진행 관련 메서드
    public void AdvanceStage()
    {
        SavePlayerState();  // 씬 전환 전에 플레이어 상태 저장
        stage++;
        
        if (stage > MAX_STAGE)
        {
            chapter++;
            stage = 1;
            Debug.Log($"Chapter {chapter} Started!");
        }
        else
        {
            Debug.Log($"Stage {stage} Started!");
        }

        SavePlayerState();
    }

    public void AddScore(int points)
    {
        score += points;
    }

    // 플레이어 상태 저장/복원 메서드!
    public void SavePlayerState()
    {
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            currentPlayerHealth = player.CurrentHealth;
            //Debug.Log($"Saved player health: {currentPlayerHealth}");  // 디버그용
            // 필요한 다른 플레이어 상태도 여기서 저장
        }
        else
        {
            Debug.LogWarning("Player not found when trying to save state!");
        }
    }

    // 새로운 씬에서 호출될 메서드
    public void RestorePlayerState(IDamageable player)
    {
        if (player != null)
        {
            // 디버그 로그 추가 - 이 메서드가 호출되었을 때 상태 확인
            //Debug.Log($"RestorePlayerState called with currentPlayerHealth: {currentPlayerHealth}, MaxHealth: {MaxHealth}");
            
            // 항상 최신 체력 값을 사용하도록 단순화
            if (CurrentCharacter != null)
            {
                // 체력이 최대 체력을 초과하지 않도록 확인
                if (currentPlayerHealth > CurrentCharacter.maxHealth)
                {
                    currentPlayerHealth = CurrentCharacter.maxHealth;
                    Debug.Log($"Health exceeds max health, limiting to: {currentPlayerHealth}");
                }
                
                // 체력이 0 이하인 경우 최대 체력으로 설정
                if (currentPlayerHealth <= 0)
                {
                    currentPlayerHealth = CurrentCharacter.maxHealth;
                    Debug.Log($"Health was 0 or negative, setting to max health: {currentPlayerHealth}");
                }
            }
            else
            {
                // CurrentCharacter가 null인 경우 기본값 사용
                if (currentPlayerHealth <= 0 || currentPlayerHealth > playerMaxHealth)
                {
                    currentPlayerHealth = playerMaxHealth;
                    Debug.Log($"CurrentCharacter is null, using default max health: {currentPlayerHealth}");
                }
            }
            
            player.RestoreHealth(currentPlayerHealth); // 현재 체력을 복원
            Debug.Log($"Restored player health: {currentPlayerHealth}");  // 디버그용
        }
        else
        {
            Debug.LogWarning("Player not found when trying to restore state!");
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void CreateInstance()
    {
        if (Instance == null && !FindObjectOfType<GameManager>())
        {
            GameObject go = new GameObject("GameManager");
            Instance = go.AddComponent<GameManager>();
        }
    }
    public void LoadNextCapter(){
        stage = 0;
        chapter++;
        LoadNextStage();  
    }

    public void LoadNextStage()
    {
        AdvanceStage();     // 스테이지 증가 및 플레이어 상태 저장

        // 현재 씬에서 새로운 스테이지 생성
        MapManager.Instance.GenerateStage();
        // 모든 적 제거
        DestroyAllEnemies();
        // 모든 드랍템 제거
        DestroyAllDroppedItems();

        DestroyNPC();

        // 플레이어 위치 리셋
        ResetPlayerPosition();
        PortalManager.Instance.enemyNumber=0;
        PortalManager.Instance.enemyText.text = "0";
        // 새로운 적 스폰
        SpawnManager.Instance.SpawnEntities();
    }

    private void DestroyAllEnemies()
    {
        foreach (var enemy in FindObjectsOfType<MeleeEnemy>())
        {
            Destroy(enemy.gameObject);
        }
        foreach (var enemy in FindObjectsOfType<RangedEnemy>())
        {
            Destroy(enemy.gameObject);
        }
    }

    private void DestroyAllDroppedItems()
    {
        foreach (var items in FindObjectsOfType<DroppedItem>())
        {
            Destroy(items.gameObject);
        }
    }

    public void DestroyNPC()
    {
        foreach (var npc in FindObjectsOfType<NPCInteraction>())
        {
            Debug.Log("Destroy NPC");
            Destroy(npc.gameObject);

        }
    }


    private void ResetPlayerPosition()
    {
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            // 맵의 왼쪽 시작 지점으로 플레이어 이동
            //Vector3 startPosition = MapManager.Instance.GetStartPosition();
            player.transform.position = new Vector3(2.0f, 4.0f, 0.0f);
        }
    }

    // 새로운 메서드: 현재 캐릭터 설정
    public void SetCurrentCharacter(CharacterData character)
    {
        CurrentCharacter = character;
    }

    public void UseSkill(int skillIndex)
    {
        if (CurrentCharacter == null)
        {
            Debug.LogWarning("CurrentCharacter is not set!");
            return;
        }

        if (CurrentCharacter.skills == null || skillIndex < 0 || skillIndex >= CurrentCharacter.skills.Length)
        {
            Debug.LogWarning("Invalid skill index or skills array is not initialized!");
            return;
        }

        CharacterSkill skill = CurrentCharacter.skills[skillIndex];

        if (skill == null)
        {
            Debug.LogWarning("Skill is null!");
            return;
        }

        // 쿨타임 체크
        if (skillCooldownTimers[skillIndex] > 0)
        {
            Debug.LogWarning($"Skill {skill.skillName} is on cooldown for {skillCooldownTimers[skillIndex]:F1} more seconds.");
            return; // 쿨타임이 남아있으면 사용하지 않음
        }

        skillManager.UseSkill(skill, transform); // 스킬 사용
        skillCooldownTimers[skillIndex] = skill.skillCooldown; // 쿨타임 설정
    }

    public void ModifyHealth(int amount)
    {
        Debug.Log($"[디버깅] ModifyHealth({amount}) 시작 - 현재 GameManager.currentPlayerHealth={currentPlayerHealth}, MaxHealth={MaxHealth}");
        
        // 플레이어 컨트롤러 찾기
        PlayerController playerController = FindObjectOfType<PlayerController>();
        int playerHealth = playerController != null ? playerController.CurrentHealth : -1;
        
        Debug.Log($"[디버깅] 체력 비교 - GameManager: {currentPlayerHealth}, PlayerController: {playerHealth}");
        
        // PlayerController가 있고, 값이 다르면 PlayerController의 값을 사용
        if (playerController != null && playerHealth != currentPlayerHealth)
        {
            Debug.LogWarning($"[디버깅] 체력 불일치 감지! GameManager 체력을 PlayerController 체력으로 설정: {currentPlayerHealth} -> {playerHealth}");
            currentPlayerHealth = playerHealth;
        }
        
        // 현재 체력을 업데이트 (최대 체력 초과 방지)
        int previousHealth = currentPlayerHealth;
        currentPlayerHealth = Mathf.Clamp(currentPlayerHealth + amount, 0, MaxHealth);
        
        // 실제 변경된 체력량
        int actualChange = currentPlayerHealth - previousHealth;
        
        Debug.Log($"체력 변경 - 이전: {previousHealth}, 이후: {currentPlayerHealth}, 실제 변경량: {actualChange}");

        // PlayerController의 체력 동기화 처리 개선
        if (playerController != null)
        {
            // 이전 값 기억
            int prevPlayerHealth = playerController.CurrentHealth;
            
            // RestoreHealth 대신 UpdateHealth를 사용하여 값을 정확히 설정
            playerController.UpdateHealth(currentPlayerHealth);
            
            Debug.Log($"[디버깅] PlayerController 체력 업데이트: {prevPlayerHealth} -> {playerController.CurrentHealth}");
            
            // PlayerUI에 직접 체력 비율 업데이트
            PlayerUI playerUI = FindObjectOfType<PlayerUI>();
            if (playerUI != null)
            {
                float healthPercent = (float)currentPlayerHealth / MaxHealth;
                playerUI.UpdateHealthSlider(healthPercent);
                Debug.Log($"[디버깅] PlayerUI 체력 슬라이더 업데이트: {healthPercent}");
            }
            
            Debug.Log($"PlayerController와 체력 동기화 완료: {currentPlayerHealth}");
        }
        else
        {
            Debug.LogWarning("PlayerController를 찾을 수 없습니다.");
        }
        
        Debug.Log($"[디버깅] ModifyHealth 완료 - 결과 GameManager.currentPlayerHealth={currentPlayerHealth}");
    }

    public int MaxHealth 
    { 
        get 
        {
            // 현재 캐릭터가 있으면 그 캐릭터의 maxHealth를, 없으면 기본값 반환
            return CurrentCharacter != null ? CurrentCharacter.maxHealth : playerMaxHealth;
        }
    }

    public int GetCurrentMaxHealth()
    {
        return CurrentCharacter != null ? CurrentCharacter.maxHealth : playerMaxHealth;
    }

    private void LogCurrentCharacterInfo()
    {
        if (CurrentCharacter != null)
        {
            Debug.Log($"Current Character: {CurrentCharacter.characterName}");

            if (CurrentCharacter.skills != null && CurrentCharacter.skills.Length > 0)
            {
                foreach (var skill in CurrentCharacter.skills)
                {
                    if (skill != null)
                    {
                        Debug.Log($"Loaded Skill: {skill.skillName}");
                    }
                    else
                    {
                        Debug.LogWarning("Skill is null!");
                    }
                }
            }
            else
            {
                Debug.LogWarning("No skills found for this character.");
            }
        }
        else
        {
            Debug.LogWarning("No character is currently selected.");
        }
    }

    private void LoadCurrentCharacterInfo()
    {
        if (CurrentCharacter != null)
        {
            Debug.Log($"Current Character: {CurrentCharacter.characterName}");

            if (CurrentCharacter.skills != null && CurrentCharacter.skills.Length > 0)
            {
                foreach (var skill in CurrentCharacter.skills)
                {
                    if (skill != null)
                    {
                        Debug.Log($"Loaded Skill: {skill.skillName}");
                    }
                    else
                    {
                        Debug.LogWarning("Skill is null!");
                    }
                }
            }
            else
            {
                Debug.LogWarning("No skills found for this character.");
            }
        }
        else
        {
            Debug.LogWarning("No character is currently selected.");
        }
    }

    public void LoadSelectedCharacter()
    {
        // 선택된 캐릭터의 데이터 로드
        if (CharacterSelectionData.Instance != null && CharacterSelectionData.Instance.selectedCharacterData != null)
        {
            Debug.Log("Loading selected character data...");
            CurrentCharacter = CharacterSelectionData.Instance.selectedCharacterData; // 선택된 캐릭터 데이터 로드
            
            // 캐릭터의 maxHealth를 게임매니저의 값으로 설정
            maxHealth = CurrentCharacter.maxHealth;
            Debug.Log($"Character {CurrentCharacter.characterName} loaded with maxHealth: {maxHealth}");
        }
        else
        {
            Debug.LogWarning("No character data found in CharacterSelectionData. Using default values.");
            maxHealth = playerMaxHealth; // 기본값 사용
        }
    }

    // 선택된 캐릭터의 스프라이트를 반환하는 메서드
    public Sprite GetSelectedCharacterSprite()
    {
        if (CurrentCharacter != null)
        {
            return CurrentCharacter.characterSprite; // characterSprite가 CharacterData에 정의되어 있어야 함
        }
        return null;
    }

    private void SaveClearTime(float clearTime)
    {
        // 기존 스테이지, 클리어타임 저장
        int currentStage = GameManager.Instance.Stage;

        PlayerProgressData data = new PlayerProgressData(clearTime, currentStage);
        SaveManager.Instance.SaveProgressData(data);

        Debug.Log($"Boss ClearTime 저장됨: {clearTime}초");
    }


    // 게임오버 UI 표시 메서드
    public void ShowGameOver()
    {
        FindAndConnectGameOverUI();  // 혹시 몰라 한 번 더 호출
                                     // 시간 멈춤 (선택 사항)


        if (gameOverPanel == null)
        {
            Debug.LogError("GameOverPanel is null!");
            return;
        }


        gameOverPanel.SetActive(true);

        // 스테이지 표시
        if (DeathStage != null)
        {
            StageUIController ui = FindObjectOfType<StageUIController>();
            if (ui != null)
            {
                DeathStage.text = ui.GetFormattedStageName();
            }
            else
            {
                // fallback: 기본 텍스트
                int stage = GameManager.Instance.Stage;
                DeathStage.text = $"Stage {stage}";
            }
        }

        // 시간 표시
        if (DeathTime != null)
        {
            float currentPlayTime = GameManager.Instance.PlayTime;
            int minutes = Mathf.FloorToInt(currentPlayTime / 60f);
            int seconds = Mathf.FloorToInt(currentPlayTime % 60f);
            DeathTime.text = $"Time: {minutes:00}:{seconds:00}";
        }

        // Boss 스테이지일 경우 clearTime 저장
        if (Stage == 10) // Boss 스테이지 인덱스
        {
            SaveManager.Instance.SaveProgressData(new PlayerProgressData(GameManager.Instance.PlayTime, GameManager.Instance.Stage));
            Debug.Log("Boss ClearTime 저장됨");
        }
    }



    // 점수 계산 메서드 (게임에 맞게 수정 필요)
    /*private int CalculateScore()
    {
        // 여기에 점수 계산 로직 구현
        // 예: 생존 시간, 처치한 적 수, 수집한 아이템 등을 기준으로 점수 계산
        float survivalTime = Time.time - gameStartTime;
        int timeScore = Mathf.FloorToInt(survivalTime * 10);
        
        // 추가 점수 요소를 더할 수 있음
        return timeScore;
    }*/

    // 게임 재시작 메서드 (UI 버튼에 연결)
    public void RestartGame()
    {
        // 시간 스케일 복원
        Time.timeScale = 1f;

        // 플레이타임 초기화 추가
        playTime = 0f;
        stage = 1;
        chapter = 1;
        score = 0;
        currentPlayerHealth = GetCurrentMaxHealth(); // 최대 체력으로 초기화

        // 현재 씬 다시 로드
        SceneManager.LoadScene("Lobby");
    }
    public void QuitGame()
    {
        Debug.Log("게임 종료 요청됨");

        if (Application.isEditor)
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#endif
        }
        else
        {
            Application.Quit();
        }
    }

}