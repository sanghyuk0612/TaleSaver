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
            Queue<GameObject> objectPool = new Queue<GameObject>();
            
            // 프리팹 딕셔너리에 저장
            prefabDictionary.Add(pool.key, pool.prefab);

            // 초기 오브젝트 생성
            for (int i = 0; i < pool.initialSize; i++)
            {
                GameObject obj = CreateNewObject(pool.key);
                objectPool.Enqueue(obj);
            }

            poolDictionary.Add(pool.key, objectPool);
        }
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
        if (!poolDictionary.ContainsKey(key))
        {
            Debug.LogError($"해당 키의 풀을 찾을 수 없습니다: {key}");
            return null;
        }

        // 사용 가능한 오브젝트가 없으면 새로 생성
        if (poolDictionary[key].Count == 0)
        {
            GameObject newObj = CreateNewObject(key);
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
