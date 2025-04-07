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

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
