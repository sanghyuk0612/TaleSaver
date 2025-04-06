using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class LavaGiant : MonoBehaviour
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
    public int dashDemage= 30;
    public float jumpHorizontalForce = 3f;
    Animator anim;
    Transform Pivot;
    public GameObject CirclePrefab;
    public float jumpForce = 5f;

    void Start()
    {
        // GameManager에서 값 가져오기
        moveSpeed = GameManager.Instance.meleeEnemyMoveSpeed;
        detectionRange = GameManager.Instance.meleeEnemyDetectionRange;
        knockbackForce = GameManager.Instance.meleeEnemyKnockbackForce;
        damageCooldown = GameManager.Instance.meleeEnemyDamageCooldown;
        Pivot = transform.Find("pivot");

        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
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
        anim.SetInteger("animNum",0);
        //StartCoroutine(StopMovement(4f));
    }
    Vector2 direction;
    bool canMove;
    private int direc;


    private void Awake()
    {
        if (InventoryManager.Instance == null)
        {
            Debug.LogError("InventoryManager not found in the scene.");
            return; // InventoryManager가 없으면 메서드 종료
        }
        canMove = true;

        inventoryManager = InventoryManager.Instance; // inventoryManager 초기화
    }
        void Update()
    {
        // 플레이어가 죽었거나 없으면 더 이상 진행하지 않음
        if (PlayerController.IsDead || playerTransform == null)
        {
            // 정지
            anim.SetInteger("animNum",0);
            rb.velocity = new Vector2(0, rb.velocity.y);
            return;
        }
        direction = (playerTransform.position - Pivot.position).normalized;
        if(direction.x>0){
        direc = 1;
        }
        else{
            direc = -1;
        }
        skillTimer+=Time.deltaTime;
        int skillNum;
        if (skillTimer >= skillInterval) // 1분마다 한번씩 랜덤으로 스킬 실행
        {
            skillNum = Random.Range(0,4);// 스킬 4개
            skillTimer = 0f; // 타이머 초기화
            StartCoroutine(mySkill(skillNum));
        }

        // 플레이어와의 거리 체크
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        // x축 방향으로만 이동
        if(canMove){
            anim.SetInteger("animNum",1);
            rb.velocity = new Vector2(direc* moveSpeed, rb.velocity.y);
        }
        else{
            rb.velocity =new Vector2(0, rb.velocity.y);
        }
            // 스프라이트 방향 전환
            if (direction.x < 0 && !isFacingRight)
            {
                Flip();
            }
            else if (direction.x > 0 && isFacingRight)
            {
                Flip();
            }
        if(isDashing){
            rb.velocity = new Vector2(direc * dashForce, rb.velocity.y);
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
    void Flip()
    {
        isFacingRight = !isFacingRight;
        //spriteRenderer.flipX = !spriteRenderer.flipX;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
        transform.position = transform.position;
    }
    
    private void myFlip(){
        
    }
    private float skillTimer = 0f;
    public float skillInterval = 3f;
    public int custumSkillnum=0;
   IEnumerator mySkill(int skillNum){
        
        if (custumSkillnum==7){
            skillNum = Random.Range(0,5);
        }
        else{
            skillNum=custumSkillnum;
        }
        
        //캐스팅 시간
        StartCoroutine(StopMovement(1f));
        Debug.Log("스킬 캐스팅 시작 1초뒤 스킬사용");
        yield return new WaitForSeconds(1f);

        anim.SetInteger("skillNum",skillNum);
        switch (skillNum)
        {
        case 0:
            Dash();
            break;
        case 1:
            //SmashAttack();
            break;
        case 2:
            //DownAttack();
            break;
        case 3:
            //FrontAttack();
            break;
        case 4:
            Jump();
            break;
        default:
            Debug.Log("잘못된 스킬 번호");
            break;
        }
        Debug.Log("스킬"+skillNum+ "실행");
    }
    public void ResetToIdle() // 애니메이션 이벤트에서 호출될 함수
    {
        anim.SetInteger("skillNum", 9); // Idle 상태로 변경
    }
private void Jump(){
    
    Vector2 jumpVector = new Vector2(jumpHorizontalForce *direc , jumpForce);
    rb.velocity = new Vector2(rb.velocity.x, 0); // 기존 Y축 속도를 초기화
    rb.AddForce(jumpVector, ForceMode2D.Impulse); // 힘을 순간적으로 가함
}
// private void FrontAttack(){
//     //StopMovement(0.5f);
//     // 이펙트 생성
//     if (FrontAttackPrefab != null)
//     {
//         GameObject effect = Instantiate(FrontAttackPrefab, Pivot.position+new Vector3(2*direc,0.3f,0), Quaternion.identity);
//         effect.transform.SetParent(transform);
//         //StartCoroutine(DashCoroutine(0.1f)); //앞으로 이동하며 공격
        
//         Destroy(effect, 0.6f); // 0.5초 후 이펙트 제거
//         Debug.Log("이펙트 출력");
//     }
//     Debug.Log("휘두르기 사용");
//     //anim.SetInteger("skillNum",9);
// }

private void SmashAttack()
{
    StartCoroutine(StopMovement(0.5f));
    // 이펙트 생성
    
    if (CirclePrefab != null)
    {
        GameObject effect = Instantiate(CirclePrefab, Pivot.position+new Vector3(direc*1,0,0), Quaternion.identity);
        effect.transform.SetParent(transform);
        Destroy(effect, 0.4f); // 0.5초 후 이펙트 제거
        Debug.Log("이펙트 출력");
    }
    //anim.SetInteger("skillNum",9);
    Debug.Log("주위공격 공격 사용");
}


// private void DownAttack()
// {
//     StartCoroutine(StopMovement(1.0f)); // 몬스터 멈추기

//     // 방향 벡터 정규화
//     Vector2 shootDirection = direction.normalized;

//     // 이펙트 생성 (플레이어 방향으로)
//     if (DownAttackPrefab != null)
//     {

//         // 레이저 이펙트 생성
//         GameObject effect = Instantiate(DownAttackPrefab, Pivot.position+new Vector3(3*direc,-0.7f,0), Quaternion.identity);
        
//         // 이펙트 이동 (속도 조절 가능)
//         Rigidbody2D effectRb = effect.GetComponent<Rigidbody2D>();
//         if (effectRb != null)
//         {
//             effectRb.velocity = shootDirection * 5f; // 속도 조절
//         }

//         Destroy(effect, 0.7f); // 1.0초 후 이펙트 제거
//     }

//     Debug.Log("슬라임 아래공격 사용");
// }

private IEnumerator StopMovement(float stopDuration)
    {
        canMove =false;
        Debug.Log("보스몬스터 정지");
        anim.SetInteger("animNum",0);
        yield return new WaitForSeconds(stopDuration);
        canMove =true; // 원래 속도로 복귀
    }

    private bool canDash = true;
    private float dashCooldownTimer = 0f;
    private bool isDashing = false;
    private void Dash(){
        
        float dashDirection = direction.x>=0 ? 1f : -1f;
        // 현재 속도를 초기화하고 대시 방향으로 힘을 가함
        // 대시 속도 설정
        // 대시 속도 직접 설정
        // 대시 코루틴 시작
        StartCoroutine(DashCoroutine(0.5f));
        // 쿨다운 시작
        canDash = false;
        Debug.Log($"보스몬스터 대쉬사용");
    }

    private IEnumerator DashCoroutine(float dashTime)
    {
        isDashing = true;
        int tmp = baseDamage;
        baseDamage = dashDemage;
        canDash = false;
        // 대시 지속 시간
        yield return new WaitForSeconds(dashTime);
        baseDamage = tmp;
        anim.SetInteger("skillNum",9);
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
            anim.SetTrigger("death");
        }
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
