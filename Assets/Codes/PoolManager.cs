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
        public int initialSize = 30;    // 초기 풀 크기
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
            CreatePoolInternal(pool.key, pool.prefab, pool.initialSize);
        }
    }

    // 내부 풀 생성 메서드 (private)
    private bool CreatePoolInternal(string key, GameObject prefab, int initialSize)
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
            if (obj != null)
            {
                objectPool.Enqueue(obj);
            }
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
        // EnemyProjectile 키가 없으면 동적으로 생성 (Resources에서 로드)
        if (key == "EnemyProjectile" && !poolDictionary.ContainsKey(key))
        {
            Debug.LogWarning($"EnemyProjectile 풀이 없습니다. 동적으로 생성합니다.");
            
            GameObject projectilePrefab = Resources.Load<GameObject>("Prefabs/EnemyProjectile");
            if (projectilePrefab != null)
            {
                CreatePoolInternal(key, projectilePrefab, 30);
            }
            else
            {
                Debug.LogError("Resources/Prefabs/EnemyProjectile 경로에서 프리팹을 찾을 수 없습니다!");
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
            Debug.Log($"풀 {key}에 여유 객체가 없어 새로 생성합니다. 현재 큐 크기: {poolDictionary[key].Count}");
            GameObject newObj = CreateNewObject(key);
            if (newObj != null)
            {
                // 바로 가져갈 것이므로 큐에 추가하지 않음
                // 활성화 상태로 반환
                newObj.SetActive(true);
                
                // ReturnToPool 컴포넌트 추가 및 초기화
                var returnToPool = newObj.GetComponent<ReturnToPool>();
                if (returnToPool == null)
                {
                    returnToPool = newObj.AddComponent<ReturnToPool>();
                }
                returnToPool.Initialize(key, this);
                
                return newObj;
            }
            else
            {
                Debug.LogError($"새 오브젝트 생성에 실패했습니다: {key}");
                return null;
            }
        }

        // 큐에서 객체 가져오기
        GameObject obj = poolDictionary[key].Dequeue();
        
        // 객체가 null인지 확인
        if (obj == null)
        {
            Debug.LogError($"풀에서 가져온 객체가 null입니다: {key}");
            return null;
        }

        // 객체가 이미 활성화되어 있는지 확인
        if (obj.activeInHierarchy)
        {
            Debug.LogWarning($"풀에서 가져온 객체가 이미 활성화되어 있습니다: {key}. 새 객체를 생성합니다.");
            GameObject newObj = CreateNewObject(key);
            if (newObj != null)
            {
                newObj.SetActive(true);
                
                var returnToPool = newObj.GetComponent<ReturnToPool>();
                if (returnToPool == null)
                {
                    returnToPool = newObj.AddComponent<ReturnToPool>();
                }
                returnToPool.Initialize(key, this);
                
                return newObj;
            }
            else
            {
                Debug.LogError($"새 오브젝트 생성에 실패했습니다: {key}");
                return null;
            }
        }

        // 비활성화된 객체 활성화
        obj.SetActive(true);
        
        // 비활성화될 때 자동으로 풀로 반환되도록 이벤트 추가
        var returnComponent = obj.GetComponent<ReturnToPool>();
        if (returnComponent == null)
        {
            returnComponent = obj.AddComponent<ReturnToPool>();
        }
        returnComponent.Initialize(key, this);

        // 큐 크기 로깅
        Debug.Log($"풀 {key}에서 객체를 가져갔습니다. 남은 객체 수: {poolDictionary[key].Count}");
        
        return obj;
    }

    public void ReturnObject(string key, GameObject obj)
    {
        if (!poolDictionary.ContainsKey(key))
        {
            Debug.LogError($"해당 키의 풀을 찾을 수 없습니다: {key}");
            return;
        }

        // 중복 반환 체크 (객체가 이미 비활성화되어 있으면 무시)
        if (!obj.activeInHierarchy)
        {
            // 이미 반환된 객체는 무시
            return;
        }

        // 객체 비활성화
        obj.SetActive(false);
        
        // 큐에 추가
        poolDictionary[key].Enqueue(obj);
        
        // 큐 크기 로깅
        Debug.Log($"객체를 풀 {key}로 반환했습니다. 현재 객체 수: {poolDictionary[key].Count}");
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
