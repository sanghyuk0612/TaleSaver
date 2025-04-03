using UnityEngine;
using System.Collections.Generic;

public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance { get; private set; }

    [System.Serializable]
    public class Pool
    {
        public string key;              // 풀링할 오브젝트의 키
        public GameObject prefab;       // 실제 프리팹
        public int initialSize = 10;    // 초기 풀 크기
    }

    public List<Pool> pools;  // Inspector에서 설정할 풀 목록
    private Dictionary<string, Queue<GameObject>> poolDictionary;
    private Dictionary<string, GameObject> prefabDictionary;

    void Awake()
    {
        // 싱글톤 설정
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Initialize()
    {
        poolDictionary = new Dictionary<string, Queue<GameObject>>();
        prefabDictionary = new Dictionary<string, GameObject>();

        // 각 풀에 대해 초기화
        foreach (Pool pool in pools)
        {
            CreatePool(pool.key, pool.prefab, pool.initialSize);
        }
    }

    // 새 풀 생성 메서드 추가
    public bool CreatePool(string key, GameObject prefab, int initialSize)
    {
        // 이미 존재하는 키인지 확인
        if (poolDictionary.ContainsKey(key))
        {
            Debug.LogWarning($"이미 존재하는 풀 키입니다: {key}");
            return false;
        }

        if (prefab == null)
        {
            Debug.LogError($"프리팹이 null입니다: {key}");
            return false;
        }

        Queue<GameObject> objectPool = new Queue<GameObject>();
        
        // 프리팹 딕셔너리에 저장
        prefabDictionary[key] = prefab;

        // 초기 오브젝트 생성
        for (int i = 0; i < initialSize; i++)
        {
            GameObject obj = CreateNewObject(key);
            objectPool.Enqueue(obj);
        }

        poolDictionary[key] = objectPool;
        Debug.Log($"풀 생성 완료: {key} (크기: {initialSize})");
        return true;
    }

    private GameObject CreateNewObject(string key)
    {
        if (!prefabDictionary.ContainsKey(key))
        {
            Debug.LogError($"프리팹을 찾을 수 없습니다: {key}");
            return null;
        }

        GameObject obj = Instantiate(prefabDictionary[key]);
        obj.SetActive(false);
        obj.transform.SetParent(transform); // PoolManager의 자식으로 설정
        return obj;
    }

    public GameObject GetObject(string key)
    {
        // EnemyProjectile 키가 없으면 동적으로 생성
        if (key == "EnemyProjectile" && !poolDictionary.ContainsKey(key))
        {
            Debug.LogWarning($"EnemyProjectile 풀이 없습니다. 동적으로 생성합니다.");
            GameObject projectilePrefab = Resources.Load<GameObject>("Prefabs/EnemyProjectile");
            if (projectilePrefab != null)
            {
                CreatePool(key, projectilePrefab, 20);
            }
            else
            {
                Debug.LogError("EnemyProjectile 프리팹을 Resources/Prefabs/EnemyProjectile에서 찾을 수 없습니다!");
                return null;
            }
        }

        if (!poolDictionary.ContainsKey(key))
        {
            Debug.LogError($"해당 키의 풀을 찾을 수 없습니다: {key}");
            return null;
        }

        // 사용 가능한 오브젝트가 없으면 새로 생성
        if (poolDictionary[key].Count == 0)
        {
            GameObject newObj = CreateNewObject(key);
            if (newObj == null)
            {
                Debug.LogError($"새 오브젝트 생성에 실패했습니다: {key}");
                return null;
            }
            poolDictionary[key].Enqueue(newObj);
        }

        GameObject obj = poolDictionary[key].Dequeue();
        
        // 객체가 null인지 확인
        if (obj == null)
        {
            Debug.LogError($"풀에서 가져온 객체가 null입니다: {key}");
            return null;
        }

        obj.SetActive(true);
        
        // 비활성화될 때 자동으로 풀로 반환되도록 이벤트 추가
        var returnToPool = obj.GetComponent<ReturnToPool>();
        if (returnToPool == null)
        {
            returnToPool = obj.AddComponent<ReturnToPool>();
        }
        returnToPool.Initialize(key, this);

        return obj;
    }

    public void ReturnObject(string key, GameObject obj)
    {
        if (!poolDictionary.ContainsKey(key))
        {
            Debug.LogError($"해당 키의 풀을 찾을 수 없습니다: {key}");
            return;
        }

        obj.SetActive(false);
        poolDictionary[key].Enqueue(obj);
    }
}

// 오브젝트가 비활성화될 때 자동으로 풀로 반환되도록 하는 컴포넌트
public class ReturnToPool : MonoBehaviour
{
    private string poolKey;
    private PoolManager poolManager;

    public void Initialize(string key, PoolManager manager)
    {
        poolKey = key;
        poolManager = manager;
    }

    private void OnDisable()
    {
        if (poolManager != null)
        {
            poolManager.ReturnObject(poolKey, gameObject);
        }
    }
}
