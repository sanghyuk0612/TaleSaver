using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;  // Tilemap 사용을 위해 추가

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance { get; private set; }

    [SerializeField] private GameObject meleeEnemyPrefab;
    [SerializeField] private GameObject rangedEnemyPrefab;

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
        Tilemap targetTilemap = MapManager.Instance.targetTilemap;
        if (targetTilemap == null) 
        {
            Debug.LogWarning("Tilemap not found!");
            return;
        }

        // 타일맵의 경계 가져오기
        BoundsInt bounds = targetTilemap.cellBounds;
        
        // 적 스폰 위치 (오른쪽 끝에서 약간 왼쪽)
        Vector3 baseSpawnPosition = targetTilemap.CellToWorld(
            new Vector3Int(bounds.xMax - 5, bounds.yMin + 1, 0)
        );

        // 근접 적 스폰 (약간 왼쪽에)
        if (meleeEnemyPrefab != null)
        {
            Vector3 meleePosition = baseSpawnPosition + new Vector3(-2f, 0f, 0f);
            PortalManager.Instance.updateEnemy(1);
            Instantiate(meleeEnemyPrefab, meleePosition, Quaternion.identity);
            Debug.Log("Melee Enemy spawned at: " + meleePosition);
        }
        else
        {
            Debug.LogWarning("Melee Enemy Prefab is not assigned!");
        }

        // 원거리 적 스폰 (약간 오른쪽에)
        if (rangedEnemyPrefab != null)
        {
            Vector3 rangedPosition = baseSpawnPosition + new Vector3(2f, 0f, 0f);
            PortalManager.Instance.updateEnemy(1);
            Instantiate(rangedEnemyPrefab, rangedPosition, Quaternion.identity);
            Debug.Log("Ranged Enemy spawned at: " + rangedPosition);
        }
        else
        {
            Debug.LogWarning("Ranged Enemy Prefab is not assigned!");
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
