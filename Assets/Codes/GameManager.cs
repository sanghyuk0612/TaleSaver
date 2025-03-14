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
    public int playerMaxHealth = 100;

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

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeGameState();

            // SkillManager 컴포넌트 추가
            skillManager = gameObject.AddComponent<SkillManager>();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        maxHealth = CurrentCharacter != null ? CurrentCharacter.maxHealth : 100;
        currentHealth = maxHealth;
        skillCooldownTimers = new float[4];
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
        currentPlayerHealth = playerMaxHealth;
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
            Debug.Log($"Saved player health: {currentPlayerHealth}");  // 디버그용
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

    public void LoadNextStage()
    {
        AdvanceStage();     // 스테이지 증가 및 플레이어 상태 저장

        // 현재 씬에서 새로운 스테이지 생성
        MapManager.Instance.GenerateStage();
        
        // 모든 적 제거
        DestroyAllEnemies();

        // 모든 드랍템 제거
        DestroyAllDroppedItems();

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

    private void ResetPlayerPosition()
    {
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            // 맵의 왼쪽 시작 지점으로 플레이어 이동
            Vector3 startPosition = MapManager.Instance.GetStartPosition();
            player.transform.position = new Vector3(2f, 4f, 0f);
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
        Debug.Log($"Current health: {currentPlayerHealth}");
        // 현재 체력을 업데이트
        currentPlayerHealth = Mathf.Min(currentPlayerHealth + amount, maxHealth);
        Debug.Log($"Current health after healing: {currentPlayerHealth}");

        // PlayerController의 currentPlayerHealth 업데이트
        PlayerController playerController = FindObjectOfType<PlayerController>();
        if (playerController != null)
        {
            playerController.UpdateHealth(currentPlayerHealth); // PlayerController의 메서드 호출
        }
    }

    public int MaxHealth => maxHealth;

    public int GetCurrentMaxHealth()
    {
        return CurrentCharacter != null ? CurrentCharacter.maxHealth : 100; // 기본값 100
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
        }
        else
        {
            Debug.LogWarning("No character data found in CharacterSelectionData.");
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
}