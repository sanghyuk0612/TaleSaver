using System.Collections;
using UnityEngine;

public class LavaGaint : MonoBehaviour
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
    private Transform playerTransform;
    private bool isGrounded;
    private bool isFacingRight = true;

    [Header("Health")]
    public float baseHealth = 100f; // 기본 체력
    public HealthMultiplier healthMultiplier; // 체력 비율을 위한 ScriptableObject
    public float calculatedHealth;

    [Header("Item Drop")]
    [SerializeField] private GameObject itemPrefab; // 아이템 프리팹
    private InventoryManager inventoryManager;
    [Header("Attack")]
    public int dashForce = 30;
    public int dashCooltime = 3;
    

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

    private void Awake()
    {
        if (InventoryManager.Instance == null)
        {
            Debug.LogError("InventoryManager not found in the scene.");
            return; // InventoryManager가 없으면 메서드 종료
        }

        inventoryManager = InventoryManager.Instance; // inventoryManager 초기화
    }
    Vector2 direction;

    void Update()
    {
        // 플레이어가 죽었거나 없으면 더 이상 진행하지 않음
        if (PlayerController.IsDead || playerTransform == null)
        {
            // 정지
            rb.velocity = new Vector2(0, rb.velocity.y);
            return;
        }
        direction = (playerTransform.position - transform.position).normalized;

        // 플레이어와의 거리 체크
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
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
        
        // 대시 쿨다운 체크
        if (!canDash)
        {
            dashCooldownTimer -= Time.deltaTime;
            if (dashCooldownTimer <= 0)
            {
                canDash = true;
                Debug.Log("Dash is ready!");
            }
        }
        if(canDash&&Mathf.Abs(distanceToPlayer)>=3 ){ //일정 거리 이상 멀어지면 돌진 패턴
        
            Dash();
        }

        // 체력 체크
        if (calculatedHealth <= 0)
        {
            DropItem();
            Destroy(gameObject);
        }
        //테스트용
        // 디버그용: K 키를 누르면 몬스터 체력을 0으로 설정
        if (Input.GetKeyDown(KeyCode.K))
        {
            Debug.Log("Debug: Monster health set to 0 manually.");
            calculatedHealth = 0;
            CheckDeath();
        }
    }
    private bool canDash = true;
    private float dashCooldownTimer = 0f;
    private bool isDashing = false;
    private float dashCooldown=2f;
    private void Dash(){
        
        float dashDirection = direction.x>=0 ? 1f : -1f;
        // 현재 속도를 초기화하고 대시 방향으로 힘을 가함
        // 대시 속도 설정
        

        rb.AddForce(new Vector2(dashDirection * dashForce, 1f), ForceMode2D.Impulse);

        // 대시 코루틴 시작
        //StartCoroutine(DashCoroutine());

        // 쿨다운 시작
        canDash = false;
        dashCooldownTimer = dashCooldown;
        Debug.Log($"보스몬스터 대쉬사용");
    }

    private IEnumerator DashCoroutine()
    {
        isDashing = true;
        
        // 대시 지속 시간
        yield return new WaitForSeconds(0.35f);
        rb.velocity = Vector2.zero;
        
        isDashing = false;
    }
    
    //테스트용 
    // 체력 0이 되면 아이템 드롭 및 몬스터 파괴 처리
    private void CheckDeath()
    {
        if (calculatedHealth <= 0)
        {
            DropItem();
            Destroy(gameObject);
        }
    }

    void Flip()
    {
        isFacingRight = !isFacingRight;
        spriteRenderer.flipX = !isFacingRight;
    }

    void DropItem()
    {
        if (inventoryManager == null)
        {
            Debug.LogError("InventoryManager is not initialized.");
            return; // inventoryManager가 null이면 메서드 종료
        }

        if (itemPrefab == null)
        {
            Debug.LogError("Item prefab is not assigned.");
            return; // itemPrefab이 null이면 �서드 종료
        }

        string itemName = inventoryManager.GetItemNameById(0);
        
        // 랜덤 ID 생성 (0~4 중 하나)
        int randomId = Random.Range(0, 5);

        // 드랍 위치
        Vector3 dropPosition = transform.position;

        // 아이템 생성
        GameObject droppedItem = Instantiate(itemPrefab, dropPosition, Quaternion.identity);

        // 아이템 초기화
        DroppedItem itemComponent = droppedItem.GetComponent<DroppedItem>();
        if (itemComponent != null)
        {
            itemName = inventoryManager.GetItemNameById(randomId); // ID에 따른 이름
            itemComponent.Initialize(randomId, itemName);
        }
        else
        {
            Debug.LogError("DroppedItem component not found on the instantiated item.");
        }

        Debug.Log($"Dropped {itemName} at {dropPosition}");
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
}
