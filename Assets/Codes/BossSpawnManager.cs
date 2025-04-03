using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;  // Tilemap 사용을 위해 추가

public class BossSpawnManager : MonoBehaviour
{
    public static BossSpawnManager Instance { get; private set; }

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
    

    

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
