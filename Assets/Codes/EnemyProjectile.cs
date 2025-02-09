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
        if (mainCamera == null) return;

        // 화면 밖으로 나갔는지 체크
        Vector3 viewPos = mainCamera.WorldToViewportPoint(transform.position);
        if (viewPos.x < -0.1f || viewPos.x > 1.1f || 
            viewPos.y < -0.1f || viewPos.y > 1.1f)
        {
            gameObject.SetActive(false);
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
            
            gameObject.SetActive(false);
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
        if (poolManager != null)
        {
            poolManager.ReturnObject(poolKey, gameObject);
        }
    }
}
