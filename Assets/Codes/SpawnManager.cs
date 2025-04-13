using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;  // Tilemap 사용을 위해 추가

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance { get; private set; }

    [SerializeField] private GameObject meleeEnemyPrefab;
    [SerializeField] private GameObject rangedEnemyPrefab;
    private GameObject BossPrefab;
    [SerializeField] private GameObject LavaPrefab;
    [SerializeField] private GameObject SlimePrefab;


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
                BossPrefab = SlimePrefab;            
                break;
            case 5: //용암
                BossPrefab = LavaPrefab;
                break;
            case 6: //테스트
            
                break;
        }

        // 근접 적 스폰 (약간 왼쪽에)
        if (BossPrefab != null)
        {
            PortalManager.Instance.updateEnemy(1);
            Instantiate(BossPrefab, MapManager.Instance.portalPosition+new Vector3(0,5,0), Quaternion.identity);
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
