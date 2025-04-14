using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyProjectile : MonoBehaviour
{
    private int damageAmount;  // 투사체가 주는 데미지
    private float knockbackForce;  // 원거리 공격의 약한 넉백

    private Rigidbody2D rb;
    private Camera mainCamera;
    private PoolManager poolManager;
    private string poolKey;
    
    private float spawnTime;

    private bool isReturning = false; // 반환 중인지 확인하는 플래그

    private void Awake()
    {
        // GameManager에서 넉백 힘 가져오기
        knockbackForce = GameManager.Instance.enemyProjectileKnockbackForce;
        
        rb = GetComponent<Rigidbody2D>();
        mainCamera = Camera.main;
        
        // Rigidbody2D 설정
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        
        // 레이어 설정
        gameObject.layer = LayerMask.NameToLayer("EnemyProjectile");

        // Collider가 없다면 추가
        if (GetComponent<Collider2D>() == null)
        {
            CircleCollider2D collider = gameObject.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = 0.1f; // 적절한 크기로 조정
        }
    }

    public void Initialize(Vector2 direction, float speed, int damage)
    {
        damageAmount = damage;  // RangedEnemy의 공격력을 저장
        rb.velocity = direction * speed;
        
        // 발사 시간 기록
        spawnTime = Time.time;

        // 투사체 회전 (발사 방향으로)
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    public void SetPoolManager(PoolManager manager, string key)
    {
        poolManager = manager;
        poolKey = key;
    }

    private void Update()
    {
        // 카메라가 없으면 재설정
        if (mainCamera == null) 
        {
            mainCamera = Camera.main;
            if (mainCamera == null) return;
        }

        // 화면 밖으로 나갔는지 체크 (여유 있게 설정)
        Vector3 viewPos = mainCamera.WorldToViewportPoint(transform.position);
        
        // 좀 더 넓은 범위로 검사 (-0.3f ~ 1.3f)
        if (viewPos.x < -0.3f || viewPos.x > 1.3f || 
            viewPos.y < -0.3f || viewPos.y > 1.3f)
        {
            // 화면 밖으로 나갔을 때 비활성화
            ReturnToPool();
        }
        
        // 발사 후 10초가 지나면 자동 회수 (안전장치)
        if (Time.time - spawnTime > 10f)
        {
            ReturnToPool();
        }
    }

    private void OnEnable()
    {
        // 활성화될 때마다 발사 시간 갱신
        spawnTime = Time.time;
        
        // 카메라 참조 확인
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            IDamageable damageable = other.GetComponent<IDamageable>();
            if (damageable != null)
            {
                Vector2 knockbackDir = rb.velocity.normalized;
                damageable.TakeDamage(damageAmount, knockbackDir, knockbackForce);
            }
            
            ReturnToPool();
        }
    }

    // 디버그용 시각화
    private void OnDrawGizmos()
    {
        // 콜라이더 범위 시각화
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, ((CircleCollider2D)col).radius);
        }
    }

    private void OnDisable()
    {
        // 이미 반환 중이면 중복 호출 방지
        if (isReturning) return;
        
        // 풀매니저와 키가 설정되어 있는 경우에만 반환
        if (poolManager != null && !string.IsNullOrEmpty(poolKey))
        {
            isReturning = true;
            poolManager.ReturnObject(poolKey, gameObject);
            isReturning = false;
        }
        else
        {
            // poolManager가 설정되지 않은 경우 경고 로그만 출력
            Debug.LogWarning("PoolManager가 설정되지 않은 EnemyProjectile이 비활성화됨");
        }
    }

    private void ReturnToPool()
    {
        if (gameObject.activeInHierarchy && !isReturning)
        {
            isReturning = true;
            gameObject.SetActive(false);
            isReturning = false;
        }
    }
}
