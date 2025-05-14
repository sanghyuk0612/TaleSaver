using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.SceneManagement;
public class MapManager : MonoBehaviour
{
    public static MapManager Instance { get; private set; }

    public Tilemap targetTilemap;
    public Tilemap TrapTilemap;
    public Tilemap HalfTilemap;
    public Vector3 portalPosition;
    public float responTime = 10;
    public bool isBoss = false;
    public List<Vector3> spawnPoints = new List<Vector3>(); // 원을 그릴 위치 목록
    [SerializeField] private GameObject stagePortalPrefab;
    [SerializeField] private GameObject storePortalPrefab;
    [SerializeField] private GameObject BossPortalPrefab;
    [SerializeField] private GameObject playerPrefab;
    private List<GameObject> mapPrefabs = new List<GameObject>();
    private List<GameObject> currentMapSections = new List<GameObject>();

    [Header("Monster Database")]
    public MonsterDatabase monsterDatabase;
    private List<MonsterData> currentMonsterList; // 현재 맵에서 사용할 몬스터 목록


    public float right;
    public int location;
    public string mapName;

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

    void Start()
    {
        // GameManager의 LoadSelectedCharacter 메서드 호출
        GameManager.Instance.LoadSelectedCharacter();
        location = GameManager.Instance.location;

        // 캐릭터 로드 후 currentPlayerHealth를 maxHealth로 명시적으로 설정
        if (GameManager.Instance.CurrentCharacter != null)
        {
            // vitality가 반영된 최대 체력 계산 방식 사용
            int characterMaxHealth = GameManager.Instance.MaxHealth;
            // GameManager의 currentPlayerHealth와 maxHealth 설정
            GameManager.Instance.CurrentPlayerHealth = characterMaxHealth;
            Debug.Log($"MapManager: Character {GameManager.Instance.CurrentCharacter.characterName} loaded. Setting health to {characterMaxHealth} (with vitality bonus)");
        }
        else
        {
            Debug.LogWarning("MapManager: CurrentCharacter is null! Using default health.");
            GameManager.Instance.CurrentPlayerHealth = GameManager.Instance.playerMaxHealth;
        }

        // 게임오버 UI 요소들 자동으로 찾아서 연결
        
        if (SceneManager.GetActiveScene().name == "GameScene")
        {
            GameManager.Instance.FindAndConnectGameOverUI();
        }



        // PoolManager가 있는지 확인
        if (PoolManager.Instance != null)
        {
            Debug.Log("PoolManager가 초기화되었습니다. EnemyProjectile은 필요시 자동으로 생성됩니다.");
        }
        else
        {
            Debug.LogWarning("PoolManager.Instance가 null입니다. 객체 풀링이 작동하지 않을 수 있습니다.");
        }
        ClearStage();
        LoadMapPrefabs();
        if (SceneManager.GetActiveScene().name == "GameScene")
        {
            GenerateStage();
            LoadMonsterDataForMap();
            StartCoroutine(RepeatFunction());
        }
        else if (SceneManager.GetActiveScene().name == "Store")
        {
            GenerateStoreMap();
        }
        else if (SceneManager.GetActiveScene().name == "BossStage")
        {
            GenerateBossMap();
            SpawnManager.Instance.SpawnBoss();
        }
        
        
        SpawnInitialEntities();
    }

    // IEnumerator를 반환하는 메서드
    IEnumerator RepeatFunction()
    {
        while (true) // 무한 반복
        {
            yield return new WaitForSeconds(responTime); // 10초 대기
            SpawnManager.Instance.SpawnEntities();
            SpawnManager.Instance.SpawnEntities();
        }
    }
    GameObject wallPrefab;
    GameObject BossMapPrefab;
    GameObject storePrefab;

    private void LoadMapPrefabs()
    {
        GameObject[] loadedPrefabs = Resources.LoadAll<GameObject>("Prefabs/Map/Cave");
        location = GameManager.Instance.location;
        List<GameObject> filteredPrefabs = new List<GameObject>();

        switch (location)
        {
            case 0:  //동굴
                loadedPrefabs = Resources.LoadAll<GameObject>("Prefabs/Map/Cave");
                break;
            case 1: //사막
                loadedPrefabs = Resources.LoadAll<GameObject>("Prefabs/Map/Desert");
                break;
            case 2: //숲
                loadedPrefabs = Resources.LoadAll<GameObject>("Prefabs/Map/Forest");
                break;
            case 3: //얼음
                loadedPrefabs = Resources.LoadAll<GameObject>("Prefabs/Map/Ice");
                break;
            case 4: //연구실
                loadedPrefabs = Resources.LoadAll<GameObject>("Prefabs/Map/Lab");
                break;
            case 5: //용암
                loadedPrefabs = Resources.LoadAll<GameObject>("Prefabs/Map/Lava");
                break;
            case 6: //테스트
                loadedPrefabs = Resources.LoadAll<GameObject>("Prefabs/Map/Test");
                break;
        }
        foreach (var prefab in loadedPrefabs)
        {
            string prefabPath = prefab.name; // Resources.LoadAll은 폴더 정보를 주지 않음 (이름만 가져옴)

            // Cave 관련 프리팹을 제외하는 조건 (예: "Cave_"로 시작하는 이름 제외)
            if (!prefabPath.EndsWith("(wall)") && !prefabPath.EndsWith("boss map") && !prefabPath.EndsWith("store"))
            {
                filteredPrefabs.Add(prefab);
                // 프리팹을 인스턴스화하여 게임 오브젝트를 생성
            }
            else if (prefabPath.EndsWith("(wall)"))
            {
                wallPrefab = prefab;
            }
            else if (prefabPath.EndsWith("boss map"))
            {
                BossMapPrefab = prefab;
            }
            else if (prefabPath.EndsWith("store"))
            {
                storePrefab = prefab;
            }
        }

        mapPrefabs.Clear();
        mapPrefabs.Add(wallPrefab);
        mapPrefabs.AddRange(filteredPrefabs);

        if (mapPrefabs.Count == 0)
        {
            Debug.LogError("No map prefabs found in Resources/Prefabs/Map folder!");
        }
        else
        {
            Debug.Log($"Successfully loaded {mapPrefabs.Count} map prefabs");
            Debug.Log("불러온 맵의 개수 " + mapPrefabs.Count);
        }
    }

    private void LoadMonsterDataForMap()
    {
        mapName = GetMapNameFromLocation(location);
        currentMonsterList = monsterDatabase.GetMonstersForMap(mapName);
        Debug.Log($"Loaded {currentMonsterList.Count} monsters for {mapName}");
    }

    // 특정 맵에 맞는 몬스터 데이터 가져오기
    private string GetMapNameFromLocation(int loc)
    {
        switch (loc)
        {
            case 0: return "Cave";
            case 1: return "Desert";
            case 2: return "Forest";
            case 3: return "Ice";
            case 4: return "Lab";
            case 5: return "Lava";
            case 6: return "Test";
            default: return "Unknown";
        }
    }
    
    public MonsterData GetRandomMonsterForCurrentMap()
    {
        if (currentMonsterList == null || currentMonsterList.Count == 0)
        {
            Debug.LogWarning("No monsters available for this map!");
            return null;
        }
        return currentMonsterList[Random.Range(0, currentMonsterList.Count)];
    }


    private float GetMapWidth(Tilemap tilemap)
    {
        BoundsInt bounds = tilemap.cellBounds;
        Vector3Int min = bounds.min;
        Vector3Int max = bounds.max;

        // 실제 타일이 있는 영역만 계산
        int minX = int.MaxValue;
        int maxX = int.MinValue;

        for (int x = min.x; x < max.x; x++)
        {
            for (int y = min.y; y < max.y; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);
                if (tilemap.HasTile(pos))
                {
                    minX = Mathf.Min(minX, x);
                    maxX = Mathf.Max(maxX, x);
                }
            }
        }

        if (minX != int.MaxValue)
        {
            return maxX - minX + 1;
        }

        return bounds.size.x;
    }

    public void GenerateStoreMap()
    {
        GameObject mapSection = Instantiate(storePrefab, Vector3.zero, Quaternion.identity);
        currentMapSections.Add(mapSection);
        Tilemap sourceTilemap = mapSection.GetComponentInChildren<Tilemap>();
        int offsetX = 0;
        BoundsInt bounds = GetTileBounds(sourceTilemap); // 실제 타일이 존재하는 영역만 가져오기
        Vector3Int offset = new Vector3Int(Mathf.RoundToInt(offsetX), 0, 0);
        // 타일맵 복사
        GameObject instance = Instantiate(storePrefab);
        // 자식 타일맵을 찾는 로직을 추가할 수 있습니다.
        CopyTilemapToTarget(sourceTilemap, targetTilemap, bounds, offset);
        Debug.Log("타일맵의 태그" + mapSection.tag);
        // 다음 맵을 위한 오프셋 증가 (공백이 아닌 타일 영역만큼 이동)
        offsetX += bounds.size.x;
        Destroy(instance);
        Destroy(mapSection);
        right = offsetX;
        SpawnPortal();
    }

    public void GenerateBossMap()
    {
        int offsetX = 0;
        GameObject mapSection = Instantiate(BossMapPrefab, Vector3.zero, Quaternion.identity);
        currentMapSections.Add(mapSection);
        Tilemap sourceTilemap = mapSection.GetComponentInChildren<Tilemap>();

        if (sourceTilemap != null)
        {
            BoundsInt bounds = GetTileBounds(sourceTilemap); // 실제 타일이 존재하는 영역만 가져오기
            Vector3Int offset = new Vector3Int(Mathf.RoundToInt(offsetX), 0, 0);
            // 타일맵 복사
            GameObject instance = Instantiate(BossMapPrefab);
            // 자식 타일맵을 찾는 로직을 추가할 수 있습니다.
            CopyTilemapToTarget(sourceTilemap, targetTilemap, bounds, offset);
            foreach (Transform child in instance.transform)
            {
                if (child.GetComponent<Tilemap>())
                {
                    if (child.tag == "Half Tile")
                    {
                        Tilemap halfTile = child.GetComponent<Tilemap>();
                        CopyTilemapToTarget(halfTile, HalfTilemap, bounds, offset);
                    }
                    if (child.tag == "Trap Tile")
                    {
                        Tilemap TrapTile = child.GetComponent<Tilemap>();
                        CopyTilemapToTarget(TrapTile, TrapTilemap, bounds, offset);
                    }
                    if (child.tag == "SpawnPoint")
                    {//스폰포인트
                        Tilemap SpawnPoint = child.GetComponent<Tilemap>();
                        Debug.Log("스폰포인트");
                        GetSpawnPoint(SpawnPoint, bounds, offset);
                    }

                }
            }
            Debug.Log("타일맵의 태그" + mapSection.tag);
            // 다음 맵을 위한 오프셋 증가 (공백이 아닌 타일 영역만큼 이동)
            offsetX += bounds.size.x;
            Destroy(instance);
            Destroy(mapSection);
            right = offsetX;
            portalPosition = new Vector3(
            offsetX - 2f,  // 오른쪽 끝에서 2칸 왼쪽
            0 + 2.5f,    // Ground 위로 2.5칸
            0
        );
        }
    }

    public void ClearStage()
    {
        TrapTilemap.ClearAllTiles();
        HalfTilemap.ClearAllTiles();
        // 기존 몬스터와 투사체 제거
        DestroyAllEnemies();
        DestroyAllProjectiles();
        DestroyNPC();
        DestroyAllDroppedItems();
        targetTilemap.ClearAllTiles();
        TrapTilemap.ClearAllTiles();
        HalfTilemap.ClearAllTiles();
        //기존 스폰포인트 초기화
        spawnPoints.Clear();
        // 기존 맵 섹션들 제거
        foreach (var section in currentMapSections)
        {
            if (section != null) Destroy(section);
        }
        currentMapSections.Clear();


        // 기존 포탈 제거
        GameObject[] portals = GameObject.FindGameObjectsWithTag("Portal");  // Portal 태그가 있다고 가정
        foreach (var portal in portals)
        {
            Destroy(portal);
        }
    }

    public void GenerateStage()
    {
        ClearStage();
        
        if (mapPrefabs.Count < 3)
        {
            Debug.LogError("Not enough map prefabs!");
            return;
        }

        // 3개의 랜덤한 맵 선택 및 생성
        List<GameObject> availablePrefabs = new List<GameObject>(mapPrefabs);
        float offsetX = 0;
        
        {
            GameObject mapPrefab = availablePrefabs[0];
            GameObject mapSection = Instantiate(mapPrefab, Vector3.zero, Quaternion.identity);
            currentMapSections.Add(mapSection);
            Tilemap sourceTilemap = mapSection.GetComponentInChildren<Tilemap>();
            if (sourceTilemap != null)
            {
                BoundsInt bounds = GetTileBounds(sourceTilemap); // 실제 타일이 존재하는 영역만 가져오기
                Vector3Int offset = new Vector3Int(Mathf.RoundToInt(offsetX), 0, 0);
                // 타일맵 복사
                CopyTilemapToTarget(sourceTilemap, targetTilemap, bounds, offset);
                // 다음 맵을 위한 오프셋 증가 (공백이 아닌 타일 영역만큼 이동)
                offsetX += bounds.size.x;
            }
            Destroy(mapSection);
        }

        for (int i = 0; i < 3; i++)
        {
            int randomIndex = Random.Range(1, availablePrefabs.Count);
            GameObject mapPrefab = availablePrefabs[randomIndex];
            GameObject mapSection = Instantiate(mapPrefab, Vector3.zero, Quaternion.identity);
            currentMapSections.Add(mapSection);
            Tilemap sourceTilemap = mapSection.GetComponentInChildren<Tilemap>();

            if (sourceTilemap != null)
            {
                BoundsInt bounds = GetTileBounds(sourceTilemap); // 실제 타일이 존재하는 영역만 가져오기
                Vector3Int offset = new Vector3Int(Mathf.RoundToInt(offsetX), 0, 0);
                // 타일맵 복사
                GameObject instance = Instantiate(mapPrefab);
                // 자식 타일맵을 찾는 로직을 추가할 수 있습니다.
                CopyTilemapToTarget(sourceTilemap, targetTilemap, bounds, offset);
                foreach (Transform child in instance.transform)
                {
                    if (child.GetComponent<Tilemap>())
                    {
                        if (child.tag == "Half Tile")
                        {
                            Tilemap halfTile = child.GetComponent<Tilemap>();
                            CopyTilemapToTarget(halfTile, HalfTilemap, bounds, offset);
                        }
                        if (child.tag == "Trap Tile")
                        {
                            Tilemap TrapTile = child.GetComponent<Tilemap>();
                            CopyTilemapToTarget(TrapTile, TrapTilemap, bounds, offset);
                        }
                        if (child.tag == "SpawnPoint")
                        {//스폰포인트
                            Tilemap SpawnPoint = child.GetComponent<Tilemap>();
                            Debug.Log("스폰포인트");
                            GetSpawnPoint(SpawnPoint, bounds, offset);
                        }

                    }
                }
                Debug.Log("타일맵의 태그" + mapSection.tag);
                // 다음 맵을 위한 오프셋 증가 (공백이 아닌 타일 영역만큼 이동)
                offsetX += bounds.size.x;
                Destroy(instance);
            }
            availablePrefabs.RemoveAt(randomIndex);
            Destroy(mapSection);
        }
        {
            GameObject mapPrefab = availablePrefabs[0];
            GameObject mapSection = Instantiate(mapPrefab, Vector3.zero, Quaternion.identity);
            currentMapSections.Add(mapSection);
            Tilemap sourceTilemap = mapSection.GetComponentInChildren<Tilemap>();
            if (sourceTilemap != null)
            {
                BoundsInt bounds = GetTileBounds(sourceTilemap); // 실제 타일이 존재하는 영역만 가져오기
                Vector3Int offset = new Vector3Int(Mathf.RoundToInt(offsetX), 0, 0);
                // 타일맵 복사
                CopyTilemapToTarget(sourceTilemap, targetTilemap, bounds, offset);

                //CopyTilemapToTargetWithTag(sourceTilemap, targetTilemap, bounds, offset, mapSection.tag);
                // 다음 맵을 위한 오프셋 증가 (공백이 아닌 타일 영역만큼 이동)
                offsetX += bounds.size.x;
                right = offsetX;
            }
            Destroy(mapSection);
        }
        SpawnPortal();
        
        // Event NPC 소환
        if (GameManager.Instance.Stage == 1)
        {
            SpawnManager.Instance.SpawnNPC();
        }
        
        // NPC 소환
        if (GameManager.Instance.Stage == 2 || GameManager.Instance.Stage == 6)
        {
            SpawnManager.Instance.SpawnNPC();
        }

        // // 스테이지가 새로 생성될 때마다 적 소환 (첫 스테이지 제외)
        // if (Time.timeSinceLevelLoad > 1f)  // 게임 시작 직후가 아닐 때만
        // {
        //     SpawnManager.Instance.SpawnEntities();
        // }
    }

    void GetSpawnPoint(Tilemap source, BoundsInt bounds, Vector3Int offset)
    {
        Debug.Log(source.name + "의 태그: " + source.tag);
        bool foundSpawnPoint = false;
        
        foreach (Vector3Int pos in bounds.allPositionsWithin)
        {
            if (!source.HasTile(pos))
            {
                continue;
            }
            Vector3 spawnPos = pos + offset - bounds.min;
            spawnPoints.Add(spawnPos); // 위치 저장
            foundSpawnPoint = true;
            Debug.Log("스폰 포인트 추가됨: " + spawnPos);
        }

        if (!foundSpawnPoint)
        {
            Debug.LogWarning("스폰 포인트를 찾을 수 없습니다. 태그: " + source.tag);
        }
    }
    void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        foreach (Vector3 pos in spawnPoints)
        {
            Gizmos.DrawWireSphere(pos, 0.5f); // 원형 그리기
        }
    }

    // ✅ **타일맵 복사 함수 (공백 없이 실제 타일만 복사)**
    void CopyTilemapToTarget(Tilemap source, Tilemap target, BoundsInt bounds, Vector3Int offset)
    {

        bool test = false;
        Debug.Log(source.name + "의 태그: " + source.tag);
        foreach (Vector3Int pos in bounds.allPositionsWithin)
        {
            if (!source.HasTile(pos))
            {
                continue;
            }
            test = true;
            TileBase tile = source.GetTile(pos);

            target.SetTile(pos + offset - bounds.min, tile); // 공백을 제거하고 복사
        }
        if (!test)
        {
            Debug.Log(source.name + "가 합쳐지지않음");
        }
    }
    // ✅ **타일이 존재하는 실제 영역을 계산하는 함수**
    BoundsInt GetTileBounds(Tilemap tilemap)
    {
        BoundsInt bounds = tilemap.cellBounds;
        int minX = bounds.xMax, minY = bounds.yMax;
        int maxX = bounds.xMin, maxY = bounds.yMin;

        foreach (Vector3Int pos in bounds.allPositionsWithin)
        {
            if (tilemap.HasTile(pos))
            {
                minX = Mathf.Min(minX, pos.x);
                minY = Mathf.Min(minY, pos.y);
                maxX = Mathf.Max(maxX, pos.x);
                maxY = Mathf.Max(maxY, pos.y);
            }
        }

        return new BoundsInt(minX, minY, 0, maxX - minX + 1, maxY - minY + 1, 1);
    }



    public void SpawnPortal()
    {
        if (stagePortalPrefab == null)
        {
            Debug.LogError("Stage Portal Prefab not assigned!");
            return;
        }

        GameObject[] grounds = GameObject.FindGameObjectsWithTag("Ground");
        if (grounds.Length > 0)
        {
            GameObject lastGround = grounds[grounds.Length - 1];

            // 마지막 Ground의 오른쪽 끝에서 약간 왼쪽으로 이동한 위치에 포탈 생성
            float groundRight = lastGround.transform.position.x + (lastGround.transform.localScale.x / 2f);
            float groundY = lastGround.transform.position.y;

            portalPosition = new Vector3(
                right - 2.5f,  // 오른쪽 끝에서 2칸 왼쪽
                groundY + 3.5f,    // Ground 위로 2.5칸
                0
            );

            Instantiate(stagePortalPrefab, portalPosition, Quaternion.identity);
            Debug.Log("Portal spawned at: " + portalPosition);
        }
    }

    public Vector3 GetStartPosition()
    {
        if (currentMapSections.Count > 0)
        {
            GameObject firstSection = currentMapSections[0];
            return new Vector3(
                firstSection.transform.position.x + 2f,
                firstSection.transform.position.y + 1f,
                0
            );
        }
        return Vector3.zero;
    }

    private void SpawnInitialEntities()
    {
        // 플레이어 소환
        if (playerPrefab != null)
        {
            Vector3 playerSpawnPosition = new Vector3(2f, 4f, 0f);
            GameObject player = Instantiate(playerPrefab, playerSpawnPosition, Quaternion.identity);

            // 메인 카메라 찾기
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                CameraFollow cameraFollow = mainCamera.GetComponent<CameraFollow>();
                if (cameraFollow != null)
                {
                    cameraFollow.target = player.transform;
                    Debug.Log("Camera target set to player");
                }
                else
                {
                    Debug.LogError("CameraFollow component not found on main camera!");
                }
            }
            else
            {
                Debug.LogError("Main camera not found!");
            }
        }
        else
        {
            Debug.LogError("Player Prefab not assigned!");
        }

        // SpawnManager를 통해 적 소환
        if (SceneManager.GetActiveScene().name == "GameScene")
        {
            for(int i=0;i<10;i++){
                Debug.Log("몬스터 소환");
                SpawnManager.Instance.SpawnEntities();
            }
        }
    }

    public void DestroyAllEnemies()
    {
        // 모든 몬스터 제거
        GameObject[] Enemies = GameObject.FindGameObjectsWithTag("Enemy");
        if (Enemies.Length > 0)
        {
            foreach (var enemy in Enemies)
            {
                Destroy(enemy);
            }
        }
    }

    public void DestroyAllProjectiles()
    {
        // 모든 원거리 몬스터의 투사체 제거
        GameObject[] projectiles = GameObject.FindGameObjectsWithTag("EnemyProjectile");
        if (projectiles.Length > 0)
        {
            foreach (var projectile in projectiles)
            {
                Destroy(projectile);
            }
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
}
