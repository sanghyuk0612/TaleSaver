using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public class PlayerController : MonoBehaviour, IDamageable
{
    private Rigidbody2D rb;

    [Header("Ground Check")]
    public float groundCheckDistance = 0.1f;
    public LayerMask groundLayer;
    public Vector2 groundCheckSize = new Vector2(0.4f, 0.1f);

    [Header("Jump Settings")]
    private int remainingJumps;
    private bool hasJumped;

    [Header("Dash Settings")]
    private bool canDash = true;
    private float dashCooldownTimer = 0f;
    private bool isDashing = false;

    [Header("Platform Drop")]
    private Coroutine currentDashCoroutine;
    private Collider2D playerCollider;
    private bool canDropDown = true;

    // IsGrounded 프로퍼티 추가
    public bool IsGrounded { get; private set; }

    // 캐싱된 GameManager 설정값들
    private float moveSpeed;
    private float jumpForce;
    private int maxJumpCount;
    private float dashForce;
    private float dashCooldown;

    [Header("Health")]
    [SerializeField] private int maxHealth = 100;
    private int currentHealth;

    // 체력 관련 프로퍼티 추가
    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;

    // 플레이어 사망 상태를 저장하는 static 변수 추가
    public static bool IsDead { get; private set; }

    // 입력 처리 가능 여부를 저장하는 변수 추가
    private bool canProcessInput = true;

    [Header("Knockback Settings")]
    [SerializeField] private float knockbackDuration = 0.2f;
    private bool isKnockedBack = false;
    private float knockbackTimer = 0f;

    [Header("Sprite Render Settings")]
    private SpriteRenderer spriteRenderer;

    [Header("Animator Settings")]
    private Animator playerAnimator;
    string characterName;
    string satyAnimName;
    string jumpAnimName;
    string runAnimName;
    string dashAnimName;

    string deadAnimName;


    void Start()
    {
        // GameManager에서 설정값 가져오기
        moveSpeed = GameManager.Instance.playerMoveSpeed;
        jumpForce = GameManager.Instance.playerJumpForce;
        maxJumpCount = GameManager.Instance.playerMaxJumpCount;
        dashForce = GameManager.Instance.playerDashForce;
        dashCooldown = GameManager.Instance.playerDashCooldown;
        
        // 최대 체력을 캐릭터 데이터에서 가져옴
        maxHealth = GameManager.Instance.MaxHealth;
        Debug.Log($"플레이어 초기화 - 최대 체력: {maxHealth}");
        
        // 컴포넌트 초기화
        rb = GetComponent<Rigidbody2D>();
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.gravityScale = GameManager.Instance.playerGravityScale;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.drag = 0f;

        spriteRenderer = GetComponent<SpriteRenderer>();
        remainingJumps = maxJumpCount;
        playerCollider = GetComponent<Collider2D>();
        IsDead = false;

        playerAnimator = GetComponent<Animator>();
        ApplyCharacterAnimator();

        // GameManager에서 저장된 상태 복원
        if (GameManager.Instance.CurrentPlayerHealth > 0)
        {
            // GameManager에 저장된 체력이 있으면 그 값을 사용
            currentHealth = GameManager.Instance.CurrentPlayerHealth;
            Debug.Log($"GameManager에서 체력 복원: {currentHealth}");
        }
        else
        {
            // 저장된 체력이 없으면 최대 체력으로 초기화
            currentHealth = maxHealth;
            GameManager.Instance.CurrentPlayerHealth = maxHealth;
            Debug.Log($"체력 초기화: {maxHealth}");
        }
        
        // UI 초기화
        PlayerUI playerUI = FindObjectOfType<PlayerUI>();
        if (playerUI != null)
        {
            playerUI.SetPlayer(this); // PlayerUI에 플레이어를 설정
            float healthPercent = (float)currentHealth / maxHealth;
            playerUI.UpdateHealthSlider(healthPercent);
            Debug.Log("PlayerUI 초기화 완료");
        }
        else
        {
            Debug.LogWarning("PlayerUI를 찾을 수 없습니다!");
        }
        
        // 상태 복원 후 체력 확인
        Debug.Log($"플레이어 초기화 완료 - 현재 체력: {currentHealth}, 최대 체력: {maxHealth}");

        // CharacterSelectionData에서 선택된 캐릭터의 스프라이트를 가져와서 적용
        if (CharacterSelectionData.Instance != null && CharacterSelectionData.Instance.selectedCharacterSprite != null)
        {
            spriteRenderer.sprite = CharacterSelectionData.Instance.selectedCharacterSprite;
        }
        else
        {
            Debug.LogError("Selected character sprite is missing!");
        }
    }


    void FixedUpdate()
    {
        CheckGround();

        // 사망 상태이거나 입력 처리가 불가능한 경우 입력 무시
        if (IsDead || !canProcessInput)
        {
            return;
        }

        // 넉백 중이면 플레이어 입력 무시
        if (isKnockedBack)
        {
            knockbackTimer -= Time.fixedDeltaTime;
            if (knockbackTimer <= 0)
            {
                isKnockedBack = false;
            }
            return;
        }

        if (!isDashing)
        {
            float moveInput = Input.GetAxisRaw("Horizontal");
            if (moveInput != 0)
            {
                float targetVelocityX = moveInput * moveSpeed;
                rb.velocity = new Vector2(targetVelocityX, rb.velocity.y);
            }
            else
            {
                float currentVelocityX = rb.velocity.x;
                rb.velocity = new Vector2(currentVelocityX * 0.9f, rb.velocity.y);
            }
        }
    }

    void Update()
    {
        // 사망 상태이거나 입력 처리가 불가능한 경우 입력 무시
        if (IsDead || !canProcessInput)
        {
            return;
        }

        characterName = CharacterSelectionData.Instance.selectedCharacterData.animatorController.name;

        float moveInput = Input.GetAxisRaw("Horizontal");
        if (moveInput != 0)
        {
            spriteRenderer.flipX = moveInput < 0;
            runAnimName = $"{characterName}_Run";
            playerAnimator.Play(runAnimName);
            playerAnimator.SetTrigger("Run");
        }

        // 점프 입력 처리
        if (!GameManager.Instance.IsPlayerInRange && Input.GetButtonDown("Jump"))
        {
            jumpAnimName = $"{characterName}_Jump";
            playerAnimator.Play(jumpAnimName);
            playerAnimator.SetTrigger("Jump");

            // 땅에 있을 때 점프
            if (IsGrounded)
            {
                rb.velocity = new Vector2(rb.velocity.x, 2f);
                rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
                remainingJumps = maxJumpCount; // 점프 횟수 초기화
                hasJumped = false; // 점프 상태 초기화
            }
            // 점프를 하지 않은 상태에서 떨어질 때 점프
            else if (remainingJumps > 0 && !hasJumped)
            {
                rb.velocity = new Vector2(rb.velocity.x, 0f);
                rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
                remainingJumps = 0; // 점프 횟수를 0으로 고정
                hasJumped = true; // 점프 상태 설정
            }
            // 땅에 있지 않을 때 더블 점프
            else if (remainingJumps > 0)
            {
                rb.velocity = new Vector2(rb.velocity.x, 0f);
                rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
                remainingJumps--; // 점프 횟수 감소
                hasJumped = true; // 점프 상태 설정
            }
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

        // 대시 실행
        if (Input.GetKeyDown(KeyCode.Q) && canDash)
        {
            dashAnimName = $"{characterName}_Dash";
            playerAnimator.Play(dashAnimName);
            playerAnimator.SetTrigger("Dash");
            Dash();
        }

        // 아래 방향키를 누르면 플랫폼 통과
        if (Input.GetAxisRaw("Vertical") < 0 && IsGrounded && canDropDown)
        {
            // 감지 위를 더 크게 설정하고 플레이어 발 위치에서 체크
            Vector2 feetPosition = new Vector2(transform.position.x,
                transform.position.y - GetComponent<Collider2D>().bounds.extents.y);

            Collider2D[] colliders = Physics2D.OverlapCircleAll(feetPosition, 0.3f, groundLayer);

            foreach (Collider2D col in colliders)
            {
                if (col.CompareTag("Half Tile"))
                {
                    canDropDown = false;  // 아래키 입력 비활성화
                    StartCoroutine(DisableCollisionCoroutine(col));
                    break;
                }
            }
        }
    }

    void CheckGround()
    {
        // BoxCast의 시작점을 플레이어의 발 위치로 조정
        Vector2 boxCastOrigin = new Vector2(
            transform.position.x,
            transform.position.y - (GetComponent<Collider2D>().bounds.extents.y - groundCheckSize.y / 2)
        );

        RaycastHit2D hit = Physics2D.BoxCast(
            boxCastOrigin,          // 시작점
            groundCheckSize,        // 크기
            0f,                     // 회전 각도
            Vector2.down,           // 방향
            groundCheckDistance,    // 거리
            groundLayer
        );

        bool wasGrounded = IsGrounded;  // 이전 상태 저장
        IsGrounded = hit.collider != null;

        // 땅에 착지했을 때 점프 횟수 초기화
        if (!wasGrounded && IsGrounded)
        {
            remainingJumps = maxJumpCount; // 점프 횟수 초기화
            hasJumped = false; // 점프 상태 초기화
            satyAnimName = $"{characterName}_Stay";
            playerAnimator.Play(satyAnimName);
            playerAnimator.SetTrigger("Stay");
        }

        // 상태 변경 시 로그 출력
        if (wasGrounded != IsGrounded)
        {
            //Debug.Log($"Ground state changed: {IsGrounded}");
            if (hit.collider != null)
            {
                //Debug.Log($"Detected ground: {hit.collider.gameObject.name}");
            }
        }
    }

    void Dash()
    {
        // 현재 바라보는 방향 확인 (spriteRenderer.flipX 기준)
        float dashDirection = spriteRenderer.flipX ? -1f : 1f;

        // 현재 속도를 초기화하고 대시 방향으로 힘을 가함
        rb.velocity = Vector2.zero;
        rb.AddForce(new Vector2(dashDirection * dashForce, 0f), ForceMode2D.Impulse);
        isDashing = true;

        // 대시 코루틴 시작
        StartCoroutine(DashCoroutine());

        // 쿨다운 시작
        canDash = false;
        dashCooldownTimer = dashCooldown;
        Debug.Log($"Dash used! Cooldown started: {dashCooldown} seconds");
    }

    private IEnumerator DashCoroutine()
    {
        isDashing = true;

        // 대시 지속 시간
        yield return new WaitForSeconds(0.35f);

        isDashing = false;
    }

    // UI에 쿨다운 표시를 한 public 메서드
    public float GetDashCooldownRemaining()
    {
        return canDash ? 0f : dashCooldownTimer;
    }

    public bool IsDashReady()
    {
        return canDash;
    }

    // 디버그용 시화 (선택사항)
    void OnDrawGizmos()
    {
        if (GetComponent<Collider2D>() != null)
        {
            // BoxCast 시작 위치 계산
            Vector2 boxCastOrigin = new Vector2(
                transform.position.x,
                transform.position.y - (GetComponent<Collider2D>().bounds.extents.y - groundCheckSize.y / 2)
            );

            // BoxCast 영역 시각
            Gizmos.color = Color.green;

            // 시작 위치의 박스
            Gizmos.DrawWireCube(boxCastOrigin, groundCheckSize);

            // 끝 위치의 박스
            Vector2 endPosition = boxCastOrigin + Vector2.down * groundCheckDistance;
            Gizmos.DrawWireCube(endPosition, groundCheckSize);

            // BoxCast 경로
            Vector2 leftStart = boxCastOrigin + new Vector2(-groundCheckSize.x / 2, 0);
            Vector2 leftEnd = leftStart + Vector2.down * groundCheckDistance;
            Vector2 rightStart = boxCastOrigin + new Vector2(groundCheckSize.x / 2, 0);
            Vector2 rightEnd = rightStart + Vector2.down * groundCheckDistance;

            Gizmos.DrawLine(leftStart, leftEnd);
            Gizmos.DrawLine(rightStart, rightEnd);
        }

        // OverlapCircle 범위 시각화
        if (GetComponent<Collider2D>() != null)
        {
            Vector2 feetPosition = new Vector2(transform.position.x,
                transform.position.y - GetComponent<Collider2D>().bounds.extents.y);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(feetPosition, 0.3f);
        }
    }

    // OnDisable 추가
    void OnDisable()
    {
        // 스크립트가 비활성화될 때 코루틴 정리
        if (currentDashCoroutine != null)
        {
            StopCoroutine(currentDashCoroutine);
            isDashing = false;
            currentDashCoroutine = null;
        }
    }

    private void OnEnable()
    {
        ApplyCharacterAnimator();
    }


    private IEnumerator DisableCollisionCoroutine(Collider2D platformCollider)
    {
        Physics2D.IgnoreCollision(playerCollider, platformCollider, true);
        rb.velocity = new Vector2(rb.velocity.x, -2f);

        // 다른 플랫폼에 착지하거나 0.5초가 지날 때까지 대기
        float timer = 0;
        bool hasLanded = false;

        while (timer < 0.5f && !hasLanded)
        {
            timer += Time.deltaTime;

            // 새로운 플랫폼에 착지했는지 확인
            if (IsGrounded && !Physics2D.GetIgnoreCollision(playerCollider, platformCollider))
            {
                hasLanded = true;
            }

            yield return null;
        }

        Physics2D.IgnoreCollision(playerCollider, platformCollider, false);
        canDropDown = true;  // 아래키 입력 다시 활성화
        Debug.Log("플랫폼 통과 완료 - 아래키 입력 가능");
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Enemy 태그로 수정
        if (collision.gameObject.CompareTag("Enemy"))
        {
            // 충돌 지점이 몬스터의 위쪽인지 확인
            float monsterTop = collision.collider.bounds.max.y;
            float playerBottom = playerCollider.bounds.min.y;

            if (playerBottom >= monsterTop - 0.1f)
            {
                // 몬스터 머리 위에서 충돌했을 때 점프 횟수만 초기화
                remainingJumps = maxJumpCount;
                Debug.Log("Monster head hit - jumps reset!");
            }
        }
    }

    // IDamageable 인터페이스 구현
    public void TakeDamage(int damage, Vector2 knockbackDirection, float knockbackForce)
    {
        // 이전 체력 값 저장
        int previousHealth = currentHealth;
        
        // 체력 감소
        currentHealth = Mathf.Max(0, currentHealth - damage);
        
        // GameManager에 체력 동기화
        int gameManagerPrevHealth = GameManager.Instance.CurrentPlayerHealth;
        GameManager.Instance.CurrentPlayerHealth = currentHealth;
        
        Debug.Log($"[디버깅] 데미지 적용 - 이전: PC={previousHealth}, GM={gameManagerPrevHealth} / 이후: PC={currentHealth}, GM={GameManager.Instance.CurrentPlayerHealth}");
        Debug.Log($"Player took {damage} damage. Current health: {currentHealth}");

        // 슬라이더 업데이트를 위해 PlayerUI에 알림
        PlayerUI playerUI = FindObjectOfType<PlayerUI>();
        if (playerUI != null)
        {
            playerUI.SetPlayer(this); // PlayerUI에 플레이어를 설정하여 슬라이더 업데이트
            
            // 체력 백분율 계산 후 직접 업데이트
            float healthPercent = (float)currentHealth / maxHealth;
            playerUI.UpdateHealthSlider(healthPercent);
            Debug.Log($"[디버깅] PlayerUI.UpdateHealthSlider({healthPercent}) 호출 완료");
        }

        if (knockbackDirection != default)
        {
            ApplyKnockback(knockbackDirection, knockbackForce);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void ApplyKnockback(Vector2 direction, float force)
    {
        isKnockedBack = true;
        knockbackTimer = knockbackDuration;

        rb.velocity = Vector2.zero;
        rb.AddForce(direction.normalized * force, ForceMode2D.Impulse);
    }

    private void Die()
    {
        if (IsDead) return;
        Debug.Log("Player died! ShowGameOver() 호출 예정");

        Debug.Log("Player died!");
        IsDead = true; // 사망 상태 설정
        canProcessInput = false; // 입력 처리 불가능 상태로 설정

        // 현재 속도를 0으로 설정하여 움직임 중지
        rb.velocity = Vector2.zero;

        // 모든 활성화된 EnemyProjectile 찾기 및 비활성화
        GameObject[] projectiles = GameObject.FindGameObjectsWithTag("EnemyProjectile");
        foreach (GameObject projectile in projectiles)
        {
            if (projectile.activeInHierarchy)
            {
                projectile.SetActive(false);
            }
        }

        // 모든 활성화된 Enemy 찾기 및 비활성화
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            if (enemy.activeInHierarchy)
            {
                enemy.SetActive(false);
            }
        }

        deadAnimName = $"{characterName}_Death";
        playerAnimator.Play(deadAnimName);
        playerAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;
        playerAnimator.SetTrigger("Death");

        StartCoroutine(WaitUntilCurrentAnimationEndsThenGameOver());
        // 게임오버 UI 표시 (약간의 딜레이 후)
        //StartCoroutine(ShowGameOverWithDelay(2.5f));

    }

    private IEnumerator WaitUntilCurrentAnimationEndsThenGameOver()
    {
        // 죽는 애니메이션은 마지막으로 설정된 상태이므로,
        // 해당 상태가 재생되는 동안 기다림
        AnimatorStateInfo stateInfo = playerAnimator.GetCurrentAnimatorStateInfo(0);

        // 현재 상태가 전환될 때까지 기다림 (가장 먼저 접근한 상태가 바로 Death 상태가 아닐 수 있음)
        while (stateInfo.normalizedTime == 0)
        {
            yield return null;
            stateInfo = playerAnimator.GetCurrentAnimatorStateInfo(0);
        }

        // 현재 상태가 끝날 때까지 기다림 (normalizedTime: 0 ~ 1)
        while (stateInfo.normalizedTime < 1.0f)
        {
            yield return null;
            stateInfo = playerAnimator.GetCurrentAnimatorStateInfo(0);
        }

        Debug.Log("현재 애니메이션 끝남 → 시간 정지 및 GameOver 호출");

        // 시간 정지 및 게임오버 호출
        Time.timeScale = 0f;
        GameManager.Instance.ShowGameOver();
    }


    // 딜레이 후 게임오버 UI 표시
    //private IEnumerator ShowGameOverWithDelay(float delay)
    //{
    //    // 지정된 시간만큼 대기
    //    yield return new WaitForSeconds(delay);

    //    // 게임오버 UI 표시
    //    GameManager.Instance.ShowGameOver();
    //}

    public void RestoreHealth(int health)
    {
        Debug.Log($"[디버깅] RestoreHealth({health}) 시작 - 현재 PlayerController.currentHealth={currentHealth}, GameManager.currentPlayerHealth={GameManager.Instance.CurrentPlayerHealth}");
        
        // GameManager에서 최대 체력 가져오기
        maxHealth = GameManager.Instance.MaxHealth;
        Debug.Log($"체력 회복 시도 - 입력값: {health}, 현재 체력: {currentHealth}, 최대 체력: {maxHealth}");
        
        // 이전 체력 저장
        int previousHealth = currentHealth;
        
        // 체력을 제공된 값으로 설정하되, 최대 체력 초과 방지
        currentHealth = Mathf.Clamp(health, 0, maxHealth);
        
        // 실제 변경된 체력량
        int actualChange = currentHealth - previousHealth;
        
        // GameManager와 상태 동기화 (현재 체력이 변경된 경우에만)
        if (actualChange != 0)
        {
            GameManager.Instance.CurrentPlayerHealth = currentHealth;
            Debug.Log($"체력 회복 완료 - 현재 체력: {currentHealth}, 변경량: {actualChange}");
            
            // UI 업데이트
            UpdateHealthUI();
        }
        else
        {
            Debug.Log($"체력이 변경되지 않음 - 현재 체력: {currentHealth}");
        }
        
        Debug.Log($"[디버깅] RestoreHealth 완료 - 결과 PlayerController.currentHealth={currentHealth}, GameManager.currentPlayerHealth={GameManager.Instance.CurrentPlayerHealth}");
    }

    public void UpdateHealth(int newHealth)
    {
        Debug.Log($"[디버깅] UpdateHealth({newHealth}) 시작 - 현재 PlayerController.currentHealth={currentHealth}, GameManager.currentPlayerHealth={GameManager.Instance.CurrentPlayerHealth}");
        
        // 이전 체력 저장
        int previousHealth = currentHealth;
        
        // 최대 체력 업데이트 (GameManager에서 가져옴)
        maxHealth = GameManager.Instance.MaxHealth;
        
        // 새로운 체력 값을 제한 (0 ~ maxHealth)
        currentHealth = Mathf.Clamp(newHealth, 0, maxHealth);
        
        // 체력 변경 로그
        Debug.Log($"플레이어 체력 업데이트 - 이전: {previousHealth}, 이후: {currentHealth}, 최대: {maxHealth}");
        
        // 실제 변경이 있을 때만 UI 업데이트
        if (currentHealth != previousHealth)
        {
            // UI 업데이트
            UpdateHealthUI();
            
            // GameManager와 동기화
            if (GameManager.Instance.CurrentPlayerHealth != currentHealth)
            {
                int prevGMHealth = GameManager.Instance.CurrentPlayerHealth;
                GameManager.Instance.CurrentPlayerHealth = currentHealth;
                Debug.Log($"GameManager 체력과 동기화: {prevGMHealth} -> {currentHealth}");
            }
        }
        
        // 체력이 0이 되면 사망 처리
        if (currentHealth <= 0 && !IsDead)
        {
            Die();
        }
        
        Debug.Log($"[디버깅] UpdateHealth 완료 - 결과 PlayerController.currentHealth={currentHealth}, GameManager.currentPlayerHealth={GameManager.Instance.CurrentPlayerHealth}");
    }
    
    // 체력 UI 업데이트 헬퍼 메서드
    private void UpdateHealthUI()
    {
        PlayerUI playerUI = FindObjectOfType<PlayerUI>();
        if (playerUI != null)
        {
            // 비율 계산 후 슬라이더 업데이트
            float healthPercent = (float)currentHealth / maxHealth;
            playerUI.UpdateHealthSlider(healthPercent);
        }
    }

    private void ApplyCharacterAnimator()
    {
        if (CharacterSelectionData.Instance != null && CharacterSelectionData.Instance.selectedCharacterAnimator != null)
        {
            CharacterData selectedCharacter = CharacterSelectionData.Instance.selectedCharacterData;

            if (selectedCharacter != null)
            {
                playerAnimator.runtimeAnimatorController = selectedCharacter.animatorController; // 애니메이터 컨트롤러 할당

                // 애니메이션 상태를 전환
                playerAnimator.Play($"{selectedCharacter.characterName}_Idle");
            }
        }
    }

}