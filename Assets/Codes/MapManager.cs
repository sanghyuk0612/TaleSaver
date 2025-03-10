using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using Unity.VisualScripting;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance { get; private set; }
    
    public Tilemap targetTilemap;
    public Tilemap TrapTilemap;
    public Tilemap HalfTilemap;
    [SerializeField] private GameObject stagePortalPrefab;
    [SerializeField] private GameObject groundPrefab;
    [SerializeField] private GameObject playerPrefab;
    private List<GameObject> mapPrefabs = new List<GameObject>();
    private List<GameObject> currentMapSections = new List<GameObject>();
    private List<GameObject> droppedItems = new List<GameObject>();
    private float right;
    int location;

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

        
        GenerateStage();
        SpawnInitialEntities();
        
    }
    GameObject wallPrefab;

    private void LoadMapPrefabs()
    {
        GameObject[] loadedPrefabs= Resources.LoadAll<GameObject>("Prefabs/Map/Cave");
        location =3;
        List<GameObject> filteredPrefabs = new List<GameObject>();

        switch(location){
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
        if (!prefabPath.EndsWith("(wall)")) 
        {
            filteredPrefabs.Add(prefab);
            // 프리팹을 인스턴스화하여 게임 오브젝트를 생성
            
        }
        else{
            
            wallPrefab =prefab;
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

    public void GenerateStage()
    {
        // 기존 몬스터와 투사체 제거
        DestroyAllEnemies();
        DestroyAllProjectiles();

        // 기존 타일맵이 존재하는지 확인
        if (targetTilemap == null)
        {
            // 타일맵이 없으면 새로 생성
            targetTilemap = new GameObject("Tilemap").AddComponent<Tilemap>();
            // TilemapRenderer 추가
            targetTilemap.gameObject.AddComponent<TilemapRenderer>();
        }
        else
        {
            // 기존 타일맵 클리어
            targetTilemap.ClearAllTiles();
        }

        // 기존 맵 섹션들 제거
        foreach (var section in currentMapSections)
        {
            if (section != null) Destroy(section);
        }
        currentMapSections.Clear();

        // // 기존 Ground 오브젝트들 제거
        // foreach (var ground in GameObject.FindGameObjectsWithTag("Ground"))
        // {
        //     Destroy(ground);
        // }

        // 기존 포탈 제거
        GameObject[] portals = GameObject.FindGameObjectsWithTag("Portal");  // Portal 태그가 있다고 가정
        foreach (var portal in portals)
        {
            Destroy(portal);
        }

        // 기존 타일맵 클리어
        if (targetTilemap != null)
        {
            targetTilemap.ClearAllTiles();
        }
        LoadMapPrefabs();

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
                    
                    if(child.tag=="Half Tile"){
                        Tilemap halfTile = child.GetComponent<Tilemap>();
                        CopyTilemapToTarget(halfTile, HalfTilemap,bounds,offset);
                    }
                    if (child.tag == "Trap Tile"){   
                        Tilemap TrapTile = child.GetComponent<Tilemap>();
                        CopyTilemapToTarget(TrapTile, TrapTilemap, bounds, offset);
                        }
                    if(child.tag == "SpawnPoint"){//스폰포인트

                    }

                }
            }

            
            Debug.Log("타일맵의 태그"+ mapSection.tag);
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

        //SpawnPortal();

        // 스테이지가 새로 생성될 때마다 적 소환 (첫 스테이지 제외)
        if (Time.timeSinceLevelLoad > 1f)  // 게임 시작 직후가 아닐 때만
        {
            SpawnManager.Instance.SpawnEntities();
        }
    }

// ✅ **타일맵 복사 함수 (공백 없이 실제 타일만 복사)**
void CopyTilemapToTarget(Tilemap source, Tilemap target, BoundsInt bounds, Vector3Int offset)
{

    bool test = false;
    Debug.Log(source.name + "의 태그: " + source.tag);
    foreach (Vector3Int pos in bounds.allPositionsWithin)
    {
        if (!source.HasTile(pos)) {
            continue;
        }
        test = true;
        TileBase tile = source.GetTile(pos);
        
        target.SetTile(pos + offset - bounds.min, tile); // 공백을 제거하고 복사
    }
    if(!test){
        Debug.Log(source.name+"가 합쳐지지않음");
    }
}
BoundsInt GetTrapBounds(Tilemap tilemap){
    return tilemap.cellBounds; // 최적화된 범위 반환
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
    private void SpawnGround(Vector3Int offset, int width)
    {
        if (groundPrefab == null)
        {
            Debug.LogError("Ground Prefab not assigned!");
            return;
        }

        // 타일맵의 바닥 위치 계산
        BoundsInt targetBounds = targetTilemap.cellBounds;
        int bottomY = targetBounds.min.y;  // 타일맵의 가장 아래 Y 좌표

        // Ground의 위치 계산 (타일맵의 바닥을 따라)
        Vector3 groundPosition = new Vector3(
            offset.x - 5f,           // 왼쪽으로 5칸 이동
            bottomY + 0.5f,          // 위로 0.5 이동
            0
        );

        GameObject ground = Instantiate(groundPrefab, groundPosition, Quaternion.identity);
        
        // Ground의 pivot이 중앙에 있으므로, 위치를 왼쪽으로 조정
        ground.transform.position = new Vector3(
            groundPosition.x + (width / 2f),  // 너비의 절반만큼 오른쪽으로 이동
            groundPosition.y,
            0
        );

        // Ground의 크기 설정
        Vector3 scale = ground.transform.localScale;
        scale.x = width;    // 타일맵 너비만큼
        scale.y = 1;        // 높이는 1로 고정
        ground.transform.localScale = scale;

        Debug.Log($"Created ground at position: {groundPosition}, width: {width}, offset: {offset}, bottomY: {bottomY}");
    }
    void GenerateMap()
{
    
}


// }
// void CopyTilemapToTargetWithTag(Tilemap source, Tilemap target, BoundsInt bounds, Vector3Int offset, string tag)
// {
//     foreach (Vector3Int pos in bounds.allPositionsWithin)
//     {
//         if (!source.HasTile(pos)) continue;
//         TileBase tile = source.GetTile(pos);

//         // 태그에 따른 추가 로직
//         if (tag == "TargetHalfGround")
//         {
//             // 예: HalfGround는 Y 위치를 조정하여 반높이로 처리
//             Vector3Int adjustedPos = pos + offset - bounds.min;
//             adjustedPos.y -= 1; // 반높이로 낮춤
//             target.SetTile(adjustedPos, tile);
//         }
//         else if (tag == "TargetSpawnPoint")
//         {
//             // 예: SpawnPoint는 특정 위치에만 적용
//             Vector3Int adjustedPos = pos + offset - bounds.min;
//             if (adjustedPos.x == offset.x + bounds.size.x / 2) // 중앙에만 배치
//                 target.SetTile(adjustedPos, tile);
//         }
//         else if (tag == "TargetSpike")
//         {
//             // 예: Spike는 위험 지역으로 표시 (커스텀 타일 사용 가능)
//             Vector3Int adjustedPos = pos + offset - bounds.min;
//             target.SetTile(adjustedPos, tile); // 스파이크 타일로 대체 가능
//         }
//         else
//         {
//             // 기본 처리 (Ground, Grid 등)
//             target.SetTile(pos + offset - bounds.min, tile);
//         }
//     }
// }
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
            
            Vector3 portalPosition = new Vector3(
                right - 2f,  // 오른쪽 끝에서 2칸 왼쪽
                groundY + 1.5f,    // Ground 위로 1.5칸
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
        SpawnManager.Instance.SpawnEntities();
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
}
