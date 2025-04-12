using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;  // Tilemap 사용을 위해 추가

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance { get; private set; }

    [SerializeField] private GameObject meleeEnemyPrefab;
    [SerializeField] private GameObject rangedEnemyPrefab;
    [SerializeField] private GameObject BossPrefab;

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

        // 타일맵의 경계 가져오기
        int ran = Random.Range(0,MapManager.Instance.spawnPoints.Count);
        Vector3 pos = MapManager.Instance.spawnPoints[ran];

        // 근접 적 스폰 (약간 왼쪽에)
        if (meleeEnemyPrefab != null)
        {
            PortalManager.Instance.updateEnemy(1);
            Instantiate(meleeEnemyPrefab, pos, Quaternion.identity);
            Debug.Log("Melee Enemy spawned at: " + pos);
        }
        else
        {
            Debug.LogWarning("Melee Enemy Prefab is not assigned!");
        }
        ran = Random.Range(0,MapManager.Instance.spawnPoints.Count);
        pos = MapManager.Instance.spawnPoints[ran];
        Debug.Log(MapManager.Instance.spawnPoints.Count);
        // 원거리 적 스폰 (약간 오른쪽에)
        if (rangedEnemyPrefab != null)
        {
            PortalManager.Instance.updateEnemy(1);
            Instantiate(rangedEnemyPrefab, pos, Quaternion.identity);
            Debug.Log("Ranged Enemy spawned at: " + pos);
        }
        else
        {
            Debug.LogWarning("Ranged Enemy Prefab is not assigned!");
        }
    }
    public void SpawnBoss()
    {

        // 근접 적 스폰 (약간 왼쪽에)
        if (BossPrefab != null)
        {
            PortalManager.Instance.updateEnemy(1);
            Instantiate(BossPrefab, MapManager.Instance.portalPosition+new Vector3(0,0,0), Quaternion.identity);
        }
        else
        {
            Debug.LogWarning("Boss Prefab is not assigned!");
        }
        // 원거리 적 스폰 (약간 오른쪽에)
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
