using System.Collections;
using UnityEngine;

public class MeleeEnemy : MonoBehaviour
{
    [Header("Movement")]
    private float moveSpeed;
    private float detectionRange;

    [Header("Ground Check")]
    public float groundCheckDistance = 0.2f;
    public LayerMask groundLayer;
    public Vector2 groundCheckSize = new Vector2(0.5f, 0.1f);

    [Header("Collision")]
    public CompositeCollider2D compositeCollider;

    [Header("Damage")]
    private float knockbackForce;
    private float damageCooldown;
    private float nextDamageTime;
    public int baseDamage = 10; // 기본 공격력
    public DamageMultiplier damageMultiplier; // 공격력 비율을 위한 ScriptableObject
    public int attackDamage;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private Transform playerTransform;
    private bool isFacingRight = true;

    [Header("Health")]
    public float baseHealth = 100f; // 기본 체력
    public HealthMultiplier healthMultiplier; // 체력 비율을 위한 ScriptableObject
    public float calculatedHealth;
    public float currentHealth; // 현재 체력

    [Header("Item Drop")]
    [SerializeField] private GameObject itemPrefab; // 아이템 프리팹

    void Start()
    {
        // GameManager에서 값 가져오기
        moveSpeed = GameManager.Instance.meleeEnemyMoveSpeed;
        detectionRange = GameManager.Instance.meleeEnemyDetectionRange;
        knockbackForce = GameManager.Instance.meleeEnemyKnockbackForce;
        damageCooldown = GameManager.Instance.meleeEnemyDamageCooldown;

        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

        // 체력과 공격력 초기화
        float healthMultiplierValue = healthMultiplier.GetHealthMultiplier(GameManager.Instance.Stage, GameManager.Instance.Chapter);
        calculatedHealth = baseHealth * healthMultiplierValue;
        currentHealth = calculatedHealth;
        Debug.Log($"MeleeEnemy spawned with current health: {currentHealth}");

        attackDamage = Mathf.RoundToInt(baseDamage * damageMultiplier.GetDamageMultiplier(GameManager.Instance.Stage, GameManager.Instance.Chapter));

        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.gravityScale = 2.5f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;  // 연속 충돌 감지 모드로 변경

        // 기존 Collider는 물리적 충돌용으로 사용
        GetComponent<Collider2D>().isTrigger = false;

        // 새로운 Trigger Collider 추가
        BoxCollider2D triggerCollider = gameObject.AddComponent<BoxCollider2D>();
        triggerCollider.isTrigger = true;
        // 기존 Collider와 같은 크기로 설정
        triggerCollider.size = GetComponent<BoxCollider2D>().size;
        triggerCollider.offset = GetComponent<BoxCollider2D>().offset;

        gameObject.layer = LayerMask.NameToLayer("Enemy");

        // 씬에 있는 모든 Enemy들과의 충돌을 무시
        RangedEnemy[] rangedEnemies = FindObjectsOfType<RangedEnemy>();
        MeleeEnemy[] meleeEnemies = FindObjectsOfType<MeleeEnemy>();

        foreach (var enemy in meleeEnemies)
        {
            if (enemy != this)  // 자기 자신은 제외
            {
                Physics2D.IgnoreCollision(GetComponent<Collider2D>(), enemy.GetComponent<Collider2D>(), true);
            }
        }

        foreach (var enemy in rangedEnemies)
        {
            Physics2D.IgnoreCollision(GetComponent<Collider2D>(), enemy.GetComponent<Collider2D>(), true);
        }
    }

    void Update()
    {
        // 플레이어가 죽었거나 없으면 더 이상 진행하지 않음
        if (PlayerController.IsDead || playerTransform == null)
        {
            // 정지
            StopMoving();
            return;
        }

        // 플레이어와의 거리 체크
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer <= detectionRange)
        {
            // 플레이어 방향으로 이동
            Vector2 direction = (playerTransform.position - transform.position).normalized;

            animator.SetTrigger("Walk");

            // x축 방향으로만 이동
            rb.velocity = new Vector2(direction.x * moveSpeed, rb.velocity.y);

            // 스프라이트 방향 전환
            if (direction.x > 0 && !isFacingRight)
            {
                Flip();
            }
            else if (direction.x < 0 && isFacingRight)
            {
                Flip();
            }
        }
        else
        {
            // 플레이어가 감지 범위를 벗어나면 정지
            StopMoving();
            animator.SetTrigger("Idle");
        }

        //테스트용
        // 디버그용: K 키를 누르면 몬스터 체력을 0으로 설정
        if (Input.GetKeyDown(KeyCode.K))
        {
            Debug.Log("Debug: Monster health set to 0 manually.");
            Die();
        }
    }

    public void ApplyMonsterData(MonsterData data)
    {
        // 몬스터 데이터 적용
        baseHealth = data.health;
        baseDamage = data.damage;
        moveSpeed = data.moveSpeed;

        // healthMultiplier에 따라 체력 재계산
        float healthMultiplierValue = healthMultiplier.GetHealthMultiplier(GameManager.Instance.Stage, GameManager.Instance.Chapter);
        calculatedHealth = baseHealth * healthMultiplierValue;
        currentHealth = calculatedHealth;


        // 스프라이트 및 애니메이션 적용
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        RefreshPolygonCollider();

        if (spriteRenderer != null)
            spriteRenderer.sprite = data.GetMonsterSprite();

        if (animator != null)
            animator.runtimeAnimatorController = data.GetAnimatorController(); ;
    }

    void RefreshPolygonCollider()
    {
        PolygonCollider2D old = GetComponent<PolygonCollider2D>();
        if (old != null) Destroy(old);

        gameObject.AddComponent<PolygonCollider2D>();
    }


    public void UpdateHealth(int newHealth)
    {
        currentHealth = newHealth;
        Debug.Log($"Player health updated to: {currentHealth}");
    }

    void Flip()
    {
        isFacingRight = !isFacingRight;
        spriteRenderer.flipX = !isFacingRight;
    }

    // 디버그용 시각화
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }

    // 새로 스폰되는 Enemy들과도 충돌을 무시하기 위한 트리거 체크
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            Physics2D.IgnoreCollision(GetComponent<Collider2D>(), other, true);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (Time.time >= nextDamageTime)
            {
                IDamageable damageable = collision.gameObject.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    Vector2 knockbackDir = (collision.transform.position - transform.position).normalized;
                    knockbackDir.y = 0.5f;

                    damageable.TakeDamage(attackDamage, knockbackDir, knockbackForce);
                    nextDamageTime = Time.time + damageCooldown;
                }
            }
        }
    }

    // 체력을 변경하는 메서드 예시
    public void TakeDamage(float damage)
    {
        currentHealth -= damage; // 데미지를 받아 현재 체력 감소
        Debug.Log($"MeleeEnemy took damage: {damage}. Current health: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die(); // 체력이 0 이하가 되면 사망 처리
        }
    }

    void StopMoving()
    {
        rb.velocity = new Vector2(0, rb.velocity.y);
        animator.SetTrigger("Idle");
    }

    private void Die()
    {
        StopMoving();
        // 애니메이션을 Dead 상태로 전환
        if (animator != null)
        {
            animator.SetTrigger("Dead");
        }

        Debug.Log("MeleeEnemy died.");

        // 5초 후 게임 오브젝트 제거
        StartCoroutine(DestroyAfterDelay(1f));
    }

    // 일정 시간 후 몬스터 제거하는 코루틴
    private IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject); // 완전히 삭제
        // 아이템 드롭
        if (itemPrefab != null)
        {
            DroppedItem droppedItem = Instantiate(itemPrefab, transform.position, Quaternion.identity).GetComponent<DroppedItem>();
            droppedItem.DropItem();
        }
    }
}
