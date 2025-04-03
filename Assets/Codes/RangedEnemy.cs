using UnityEngine;

public class RangedEnemy : MonoBehaviour
{
    [Header("Movement")]
    private float moveSpeed;
    private float attackRange;
    private float detectionRange;

    [Header("Edge Detection")]
    public float edgeCheckDistance = 1.0f; // 모서리 감지 거리
    public LayerMask platformLayer; // 플랫폼 레이어

    [Header("Damage")]

    public int baseDamage = 10; // 기본 공격력
    public DamageMultiplier damageMultiplier; // 공격력 비율을 위한 ScriptableObject
    public int attackDamage; // 공격력
    private float projectileSpeed; // 발사체 속도
    private float attackCooldown;
    protected string projectileKey = "EnemyProjectile";
    protected Transform firePoint;

    [Header("Health")]
    public float baseHealth = 80f; // 기본 체력
    public HealthMultiplier healthMultiplier; // 체력 비율을 위한 ScriptableObject
    public float calculatedHealth; // 계산된 체력
    public float currentHealth; // 현재 체력

    private float nextAttackTime;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Transform playerTransform;
    private bool isPlayerInRange = false;

    void Start()
    {
        // GameManager에서 값 가져오기
        moveSpeed = GameManager.Instance.rangedEnemyMoveSpeed;
        attackRange = GameManager.Instance.rangedEnemyAttackRange;
        detectionRange = GameManager.Instance.rangedEnemyDetectionRange;
        attackCooldown = GameManager.Instance.rangedEnemyAttackCooldown;
        projectileSpeed = GameManager.Instance.rangedEnemyProjectileSpeed;

        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;

        // 체력과 공격력 초기화
        float healthMultiplierValue = healthMultiplier.GetHealthMultiplier(GameManager.Instance.Stage, GameManager.Instance.Chapter);
        calculatedHealth = baseHealth * healthMultiplierValue;
        currentHealth = calculatedHealth;

        attackDamage = Mathf.RoundToInt(baseDamage * damageMultiplier.GetDamageMultiplier(GameManager.Instance.Stage, GameManager.Instance.Chapter));

        // Rigidbody2D 설정
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.gravityScale = 2.5f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        // 플랫폼 레이어 설정
        platformLayer = LayerMask.GetMask("Ground", "Half Tile");

        // 기존 Collider는 물리적 충돌용으로 사용
        BoxCollider2D existingCollider = GetComponent<BoxCollider2D>();
        if (existingCollider != null)
        {
            existingCollider.isTrigger = false;

            // 새로운 Trigger Collider 추가
            BoxCollider2D triggerCollider = gameObject.AddComponent<BoxCollider2D>();
            triggerCollider.isTrigger = true;
            triggerCollider.size = existingCollider.size;
            triggerCollider.offset = existingCollider.offset;
        }

        gameObject.layer = LayerMask.NameToLayer("Enemy");

        // 씬에 있는 모든 Enemy들과의 충돌을 무시
        RangedEnemy[] rangedEnemies = FindObjectsOfType<RangedEnemy>();
        MeleeEnemy[] meleeEnemies = FindObjectsOfType<MeleeEnemy>();

        foreach (var enemy in rangedEnemies)
        {
            if (enemy != this)  // 자기 자신은 제외
            {
                Physics2D.IgnoreCollision(GetComponent<Collider2D>(), enemy.GetComponent<Collider2D>(), true);
            }
        }

        foreach (var enemy in meleeEnemies)
        {
            Physics2D.IgnoreCollision(GetComponent<Collider2D>(), enemy.GetComponent<Collider2D>(), true);
        }

        // firePoint 자동 생성
        GameObject firePointObj = new GameObject("FirePoint");
        firePoint = firePointObj.transform;
        firePoint.SetParent(transform);
        firePoint.localPosition = new Vector3(0f, 0f, 0f); // 발사 위치 고정

        Debug.Log($"RangedEnemy spawned with current health: {currentHealth}");
    }

    // 새로 스폰되는 Enemy들과도 충돌을 무시하기 위한 트리거 체크
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            Physics2D.IgnoreCollision(GetComponent<Collider2D>(), other, true);
        }
    }

    void Update()
    {
        // 플레이어가 죽었거나 없으면 더 이상 진행하지 않음
        if (PlayerController.IsDead || playerTransform == null)
        {
            StopMoving();
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer <= detectionRange)
        {
            if (distanceToPlayer > attackRange)
            {
                // 이동하기 전에 모서리 확인
                bool canMove = CheckGroundAhead((playerTransform.position.x > transform.position.x) ? 1f : -1f);
                
                if (canMove)
                {
                    MoveTowardsPlayer();
                }
                else
                {
                    StopMoving();
                }
                isPlayerInRange = false;
            }
            else
            {
                StopMoving();
                isPlayerInRange = true;

                // 공격 범위 안에 있고 쿨다운이 끝났으면 발사
                if (Time.time >= nextAttackTime)
                {
                    ShootProjectile();
                    nextAttackTime = Time.time + attackCooldown;
                }
            }

            UpdateFacingDirection();
        }
        else
        {
            StopMoving();
            isPlayerInRange = false;
        }
    }

    // 전방에 지면이 있는지 확인하는 메서드
    private bool CheckGroundAhead(float directionX)
    {
        // 캐릭터의 발 위치 계산 (캐릭터의 바닥부분)
        Vector2 footPosition = new Vector2(transform.position.x, transform.position.y - 0.5f);
        
        // 이동 방향으로의 레이캐스트 방향 설정
        Vector2 rayDirection = new Vector2(directionX, -0.5f).normalized;
        
        // 레이캐스트를 통해 전방의 지면 확인
        RaycastHit2D hit = Physics2D.Raycast(footPosition, rayDirection, edgeCheckDistance, platformLayer);
        
        // 디버그용 시각화
        Debug.DrawRay(footPosition, rayDirection * edgeCheckDistance, hit ? Color.green : Color.red);
        
        return hit.collider != null;
    }

    // virtual로 변경하여 오버라이드 가능하게 함
    protected virtual void ShootProjectile()
    {
        // 플레이어가 죽었다면 발사하지 않음
        if (PlayerController.IsDead || playerTransform == null) return;

        // PoolManager가 null인지 확인
        if (PoolManager.Instance == null)
        {
            Debug.LogError("PoolManager.Instance가 null입니다. Pool Manager 컴포넌트가 씬에 있는지 확인하세요.");
            return;
        }

        // 발사체 생성 시도
        GameObject projectile = null;
        try
        {
            projectile = PoolManager.Instance.GetObject(projectileKey);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"발사체 생성 중 예외 발생: {e.Message}");
            return;
        }
        
        // null 체크 추가
        if (projectile == null)
        {
            Debug.LogError("발사체를 가져오는 데 실패했습니다.");
            return;
        }

        // firePoint가 null인지 확인
        if (firePoint == null)
        {
            Debug.LogError("firePoint가 null입니다.");
            return;
        }

        Vector3 spawnPosition = firePoint.position;
        projectile.transform.position = spawnPosition;

        EnemyProjectile projectileComponent = projectile.GetComponent<EnemyProjectile>();
        if (projectileComponent != null)
        {
            // 플레이어 위치에 y값을 0.4 더해서 조준점을 약간 위로 조정
            Vector3 adjustedPlayerPosition = playerTransform.position + new Vector3(0f, 0.4f, 0f);
            Vector2 direction = (adjustedPlayerPosition - spawnPosition).normalized;
            projectileComponent.Initialize(direction, projectileSpeed, attackDamage);
            projectileComponent.SetPoolManager(PoolManager.Instance, projectileKey);
        }
        else
        {
            Debug.LogError("발사체에 EnemyProjectile 컴포넌트가 없습니다.");
            projectile.SetActive(false); // 발사체를 다시 비활성화
        }
    }

    void MoveTowardsPlayer()
    {
        // x축 방향으로만 이동하도록 수정
        float directionX = playerTransform.position.x > transform.position.x ? 1f : -1f;
        rb.velocity = new Vector2(directionX * moveSpeed, rb.velocity.y); // Y속도 유지
    }

    void StopMoving()
    {
        rb.velocity = new Vector2(0,rb.velocity.y);
    }

    void UpdateFacingDirection()
    {
        // 플레이어가 왼쪽에 있으면 true, 오른쪽에 있으면 false
        spriteRenderer.flipX = playerTransform.position.x < transform.position.x;
    }

    // 디버그용 시각화
    void OnDrawGizmosSelected()
    {
        // 공격 범위 표시 (빨간색)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // 감지 범위 표시 (노란색)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }

    // 현재 플레이어가 공격 범위 안에 있는지 확인하는 프로퍼티
    public bool IsPlayerInRange => isPlayerInRange;

    // 체력을 변경하는 메서드 예시
    public void TakeDamage(float damage)
    {
        currentHealth -= damage; // 데미지를 받아 현재 체력 감소
        Debug.Log($"RangedEnemy took damage: {damage}. Current health: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die(); // 체력이 0 이하가 되면 사망 처리
        }
    }

    private void Die()
    {
        Debug.Log("RangedEnemy died.");
        PortalManager.Instance.killEnemy(1);
        // 사망 처리 로직 (예: 게임 오브젝트 비활성화)
        gameObject.SetActive(false);
    }
}
