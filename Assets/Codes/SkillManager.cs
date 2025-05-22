using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillManager : MonoBehaviour
{
    // Inspector에서 확인할 디버그 정보
    [Header("스킬 데미지 디버그 정보")]
    [SerializeField] private int currentPowerLevel = 0;
    [SerializeField] private float damageMultiplier = 1.0f;
    [SerializeField] private string selectedCharacterName = "";
    [SerializeField] private List<SkillDamageInfo> skillDamageInfos = new List<SkillDamageInfo>();


    [System.Serializable]
    public class SkillDamageInfo
    {
        public string skillName;
        public int baseDamage;
        public int calculatedDamage;
        
        public SkillDamageInfo(string name, int baseDmg, int calcDmg)
        {
            skillName = name;
            baseDamage = baseDmg;
            calculatedDamage = calcDmg;
        }
    }


    // Start is called before the first frame update
    void Start()
    {
        // 타격 이펙트 프리팹 로드
        hitEffectPrefab = Resources.Load<GameObject>("Prefabs/Particle/HitEffect");
        // Fireball 이펙트 프리팹 로드
        fireballEffectPrefab = Resources.Load<GameObject>("Prefabs/Particle/Fireball");
        // Ultimo 이펙트 프리팹 로드
        ultimoEffectPrefab = Resources.Load<GameObject>("Prefabs/Particle/Ultimo");
    }

    // Update is called once per frame
    void Update()
    {
        // 현재 캐릭터 정보 업데이트 (Inspector 디버그용)
        UpdateCharacterDebugInfo();
    }
    
    // 캐릭터 및 스킬 디버그 정보 업데이트
    private void UpdateCharacterDebugInfo()
    {
        CharacterData currentCharacter = GameManager.Instance?.CurrentCharacter;
        if (currentCharacter != null)
        {
            selectedCharacterName = currentCharacter.characterName;
            currentPowerLevel = currentCharacter.power;
            damageMultiplier = 1 + (currentPowerLevel * 0.1f);
            
            // 기존 리스트 초기화
            skillDamageInfos.Clear();
            
            // 모든 스킬에 대한 데미지 정보 업데이트
            if (currentCharacter.skills != null)
            {
                foreach (CharacterSkill skill in currentCharacter.skills)
                {
                    if (skill != null)
                    {
                        int baseDamage = skill.skillDamage;
                        int calculatedDamage = Mathf.RoundToInt(baseDamage * damageMultiplier);
                        skillDamageInfos.Add(new SkillDamageInfo(skill.skillName, baseDamage, calculatedDamage));
                    }
                }
            }
        }
    }

    private Vector3 fireballPosition; // Fireball 스킬 사용 시 위치 저장
    private Vector3 ultimoPosition; // Ultimo 스킬 사용 시 위치 저장
    private Vector3 baseAttackPosition; // BaseG 스킬 사용 시 위치 저장
    private Vector3 closestEnemyPosition; // 가장 가까운 적의 위치 저장

    private GameObject hitEffectPrefab; // 타격 이펙트 프리팹
    private GameObject fireballEffectPrefab; // Fireball 이펙트 프리팹
    private GameObject ultimoEffectPrefab; // Ultimo 이펙트 프리팹
    [SerializeField] private GameObject healEffectPrefab;
    public GameObject cowImpactFXPrefab;

    public void UseSkill(CharacterSkill skill, Transform characterTransform, CharacterData casterData)
    {
        ApplySkillEffect(skill, characterTransform, casterData);
    }


    /*public void UseSkill(CharacterSkill skill, Transform characterTransform)
    {
        Debug.Log($"Using skill: {skill.skillName} with damage: {skill.skillDamage} or effect value: {skill.effectValue}");
        ApplySkillEffect(skill, characterTransform);
    }*/

    private void ApplySkillEffect(CharacterSkill skill, Transform characterTransform, CharacterData casterData)
    {
        if (skill == null)
        {
            Debug.LogError("Skill is null!");
            return;
        }
        
        // STR(power) 레벨 가져오기
        int powerLevel = casterData.power;
        
        // 실제 데미지 계산 (STR 레벨 적용)
        int actualDamage = skill.CalculateActualDamage(powerLevel);
        
        Debug.Log($"적용할 스킬: {skill.skillName}, 기본 데미지: {skill.skillDamage}, STR 레벨: {powerLevel}, 계산된 데미지: {actualDamage}");

        // 플레이어 객체와 위치 가져오기
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        PlayerController playerController = playerObject != null ? playerObject.GetComponent<PlayerController>() : null;

        // 스킬 이름에 따라 다른 효과 적용
        switch (skill.skillName)
        {
            case "BaseG":
                // BaseG 스킬 효과 적용
                Debug.Log("BaseG Skill Used!");

                // 플레이어 위치 저장
                GameObject playerObject_baseG = GameObject.FindGameObjectWithTag("Player");
                
                if (playerObject_baseG == null)
                {
                    Debug.LogError("Player object not found for BaseG!");
                    return;
                }

                // 플레이어 위치
                baseAttackPosition = playerObject_baseG.transform.position;

                // 모든 Enemy 오브젝트 찾기
                GameObject[] enemies_baseG = GameObject.FindGameObjectsWithTag("Enemy");
                int enemyCount_baseG = 0;

                // Player의 SpriteRenderer 컴포넌트 가져오기
                SpriteRenderer spriteRenderer_baseG = playerObject_baseG.GetComponent<SpriteRenderer>();
                bool isFlipped_baseG = spriteRenderer_baseG != null && spriteRenderer_baseG.flipX;
                Debug.Log($"BaseG - Player found - flipX: {isFlipped_baseG}, SpriteRenderer: {(spriteRenderer_baseG != null ? "Found" : "Not Found")}");

                // 가장 가까운 적 찾기 변수 초기화
                GameObject closestEnemy = null;
                float closestDistance = float.MaxValue;

                // 범위 내의 적 찾기 (Fireball과 동일한 방식)
                foreach (GameObject enemy in enemies_baseG)
                {
                    // 캐릭터 위치와 적의 거리 계산
                    Vector3 directionToEnemy = enemy.transform.position - playerObject_baseG.transform.position;
                    float distance = directionToEnemy.magnitude;

                    // Y좌표 차이 계산
                    float yDifference = Mathf.Abs(enemy.transform.position.y - playerObject_baseG.transform.position.y);
                    float maxYDifference = 2f; // 최대 허용 Y좌표 차이

                    // 적이 캐릭터가 바라보는 방향에 있는지 확인
                    bool isEnemyInDirection = false;
                    if (isFlipped_baseG && directionToEnemy.x < 0) // 캐릭터가 왼쪽을 바라보고 적이 왼쪽에 있음
                    {
                        isEnemyInDirection = true;
                    }
                    else if (!isFlipped_baseG && directionToEnemy.x > 0) // 캐릭터가 오른쪽을 바라보고 적이 오른쪽에 있음
                    {
                        isEnemyInDirection = true;
                    }

                    // 디버그 로그 추가
                    Debug.Log($"BaseG 체크 - Enemy: {enemy.name}, Distance: {distance}, Y차이: {yDifference}, 방향 일치: {isEnemyInDirection}");

                    // 조건에 맞는 적이고 현재까지 찾은 가장 가까운 적보다 더 가까우면 갱신
                    if (distance <= 7f && yDifference <= maxYDifference && isEnemyInDirection && distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestEnemy = enemy;
                        Debug.Log($"BaseG - 새로운 가장 가까운 적 찾음: {enemy.name}, 거리: {distance}");
                    }
                }

                // 가장 가까운 적에게 데미지 적용
                if (closestEnemy != null)
                {
                    closestEnemyPosition = closestEnemy.transform.position; // 가장 가까운 적의 위치 저장
                    
                    // 캐릭터별 FX 선택 (null 병합 연산자 활용)
                    GameObject baseGFX = casterData.baseGEffectOverride ?? hitEffectPrefab;
                    
                    // 타격 이펙트 생성
                    if (baseGFX != null)
                    {
                        GameObject hitEffect = Instantiate(baseGFX, closestEnemy.transform.position, Quaternion.identity);
                        Destroy(hitEffect, 0.5f); // 0.5초 후 이펙트 제거
                    }
                    
                    int i = Random.Range(0,2);
                    if(i==0){
                        BGMManager.instance.PlaySE(BGMManager.instance.slashSE,1f);
                    }
                    else{
                        BGMManager.instance.PlaySE(BGMManager.instance.slash2SE,1f);
                    }

                    // 데미지 적용
                    if (closestEnemy.TryGetComponent(out MeleeEnemy meleeEnemy))
                    {
                        enemyCount_baseG++;
                        meleeEnemy.TakeDamage(actualDamage);
                    }
                    else if (closestEnemy.TryGetComponent(out RangedEnemy rangedEnemy))
                    {
                        enemyCount_baseG++;
                        rangedEnemy.TakeDamage(actualDamage);
                    }
                    else if (closestEnemy.TryGetComponent(out LavaGiant lavaGiant))
                    {
                        enemyCount_baseG++;
                        lavaGiant.TakeDamage(actualDamage);
                    }
                    else if (closestEnemy.TryGetComponent(out Slime slime))
                    {
                        enemyCount_baseG++;
                        slime.TakeDamage(actualDamage);
                    }
                    else if (closestEnemy.TryGetComponent(out Wolf wolf))
                    {
                        enemyCount_baseG++;
                        wolf.TakeDamage(actualDamage);
                    }
                    Debug.Log($"BaseG - Attacking closest enemy: {closestEnemy.name} with damage: {actualDamage}");
                }
                else
                {
                    Debug.Log("BaseG - No valid enemies found within range and direction!");
                }
                
                Debug.Log($"Number of enemies hit by BaseG: {enemyCount_baseG}");
                break;

            case "Fireball":
                // Fireball 스킬 효과
                Debug.Log("Fireball Skill Used!");

                // 플레이어 위치 저장
                GameObject playerObject_fireball = GameObject.FindGameObjectWithTag("Player");
                
                if (playerObject_fireball == null)
                {
                    Debug.LogError("Player object not found!");
                    return;
                }
                BGMManager.instance.PlaySE(BGMManager.instance.blackBirdSE,1.5f);

                // 플레이어 위치에서 약간 앞쪽에 파이어볼 생성
                fireballPosition = playerObject_fireball.transform.position;

                // 모든 Enemy 오브젝트 찾기
                GameObject[] enemies_fireball = GameObject.FindGameObjectsWithTag("Enemy");
                int enemyCount_fireball = 0; // 변수명 변경하여 중복 방지

                // 캐릭터가 바라보는 방향
                Vector3 forwardDirection_fireball = characterTransform.forward; // 변수명 변경하여 중복 방지

                // Player의 SpriteRenderer 컴포넌트 가져오기
                SpriteRenderer spriteRenderer = playerObject_fireball.GetComponent<SpriteRenderer>();
                bool isFlipped_fireball = spriteRenderer != null && spriteRenderer.flipX; // 변수명 변경하여 중복 방지
                Debug.Log($"Player found - flipX: {isFlipped_fireball}, SpriteRenderer: {(spriteRenderer != null ? "Found" : "Not Found")}");

                // Fireball 이펙트 생성
                //if (fireballEffectPrefab != null)
                GameObject fireballFX = casterData.fireballEffectOverride ?? fireballEffectPrefab;
                if (fireballFX != null)
                {
                    // 캐릭터의 방향에 따라 회전 설정
                    float rotationZ = isFlipped_fireball ? 180f : 0f;
                    GameObject fireballEffect = Instantiate(fireballFX, fireballPosition, Quaternion.Euler(0f, 0f, rotationZ));
                    //GameObject fireballEffect = Instantiate(fireballEffectPrefab, fireballPosition, Quaternion.Euler(0f, 0f, rotationZ));
                    FireballBehavior behavior = fireballEffect.GetComponent<FireballBehavior>();
                    if (behavior != null)
                    {
                        Vector3 moveDirection = isFlipped_fireball ? Vector3.left : Vector3.right;
                        behavior.Initialize(actualDamage, skill.effectRadius, moveDirection);
                    }
                    //Destroy(fireballEffect, 0.4f);
                }

                // 범위 내의 적 찾기
                foreach (GameObject enemy in enemies_fireball)
                {
                    // 캐릭터 위치와 적의 거리 계산
                    Vector3 directionToEnemy = enemy.transform.position - playerObject_fireball.transform.position; // 플레이어 기준으로 적까지의 방향 벡터
                    float distance = directionToEnemy.magnitude; // 거리 계산

                    // Y좌표 차이 계산
                    float yDifference = Mathf.Abs(enemy.transform.position.y - playerObject_fireball.transform.position.y);
                    float maxYDifference = 3f; // 최대 허용 Y좌표 차이를 3으로 수정

                    // 디버그 로그 추가
                    Debug.Log($"Enemy: {enemy.name}, Distance: {distance}, Direction: {directionToEnemy}, isFlipped: {isFlipped_fireball}, Player Position: {playerObject_fireball.transform.position}, Enemy Position: {enemy.transform.position}");

                    // 적이 캐릭터가 바라보는 방향에 있는지 확인
                    bool isEnemyInDirection = false;
                    if (isFlipped_fireball && directionToEnemy.x < 0) // 캐릭터가 왼쪽을 바라보고 적이 왼쪽에 있음
                    {
                        isEnemyInDirection = true;
                    }
                    else if (!isFlipped_fireball && directionToEnemy.x > 0) // 캐릭터가 오른쪽을 바라보고 적이 오른쪽에 있음
                    {
                        isEnemyInDirection = true;
                    }

                    // flipX 상태에 따라 거리 인식 조정 및 방향 확인
                    if (distance <= skill.effectRadius && yDifference <= maxYDifference && isEnemyInDirection) // 범위 내에 있고, Y좌표 차이가 허용 범위 내이며, 캐릭터가 바라보는 방향에 있는 경우
                    {
                        // 적에게 데미지 주기
                        if (enemy.TryGetComponent(out MeleeEnemy meleeEnemy))
                        {
                            enemyCount_fireball++; // 카운터 증가
                            meleeEnemy.TakeDamage(actualDamage); // MeleeEnemy의 currentHealth 감소 (STR 레벨 적용된 데미지)
                        }
                        else if (enemy.TryGetComponent(out RangedEnemy rangedEnemy))
                        {
                            enemyCount_fireball++; // 카운터 증가
                            rangedEnemy.TakeDamage(actualDamage); // RangedEnemy의 currentHealth 감소 (STR 레벨 적용된 데미지)
                        }
                        else if (enemy.TryGetComponent(out LavaGiant lavaGiant))
                        {
                            enemyCount_fireball++; // 카운터 증가
                            lavaGiant.TakeDamage(actualDamage);
                        }
                        else if (enemy.TryGetComponent(out Slime slime))
                        {
                            enemyCount_fireball++; // 카운터 증가
                            slime.TakeDamage(actualDamage);
                        }
                        else if (enemy.TryGetComponent(out Wolf wolf))
                        {
                            enemyCount_fireball++; // 카운터 증가
                            wolf.TakeDamage(actualDamage);
                        }
                    }
                }

                Debug.Log($"Number of enemies hit by Fireball: {enemyCount_fireball}");
                break;

            case "Lightning":
                // Lightning 스킬 효과
                Debug.Log("Casting Lightning!");
                // Lightning 애니메이션 및 효과 적용
                break;

            case "Poison":
                // 카메라의 위치를 사용하여 현재 씬에서 Tag가 Enemy인 오브젝트 감지
                Vector3 cameraPosition = Camera.main.transform.position; // 카메라의 위치 가져오기
                Vector3 poisonPosition = cameraPosition; // Poison 위치 저장
                
                // 모든 Enemy 오브젝트 찾기
                GameObject[] enemies_poison = GameObject.FindGameObjectsWithTag("Enemy");
                int enemyCount_poison = 0; // 변수명 변경하여 중복 방지
                
                Debug.Log($"Found {enemies_poison.Length} enemies in the scene for Poison skill");

                // 캐릭터가 바라보는 방향
                Vector3 forwardDirection_poison = characterTransform.forward; // 변수명 변경하여 중복 방지

                // flipX 상태 확인
                bool isFlipped_poison = characterTransform.localScale.x < 0; // 변수명 변경하여 중복 방지
                Debug.Log($"Character is flipped: {isFlipped_poison}, Forward direction: {forwardDirection_poison}");

                // 범위 내의 적 찾기
                foreach (GameObject enemy in enemies_poison)
                {
                    // 카메라 위치와 적의 거리 계산
                    Vector3 directionToEnemy = enemy.transform.position - cameraPosition; // 적까지의 방향 벡터
                    float distance = directionToEnemy.magnitude; // 거리 계산

                    // 거리와 방향을 기준으로 양수와 음수로 판단
                    float dotProduct = Vector3.Dot(forwardDirection_poison, directionToEnemy.normalized);
                    
                    Debug.Log($"Enemy: {enemy.name}, Distance: {distance}, Direction: {directionToEnemy}, DotProduct: {dotProduct}");

                    // flipX 상태에 따라 거리 인식 조정
                    if (distance <= skill.effectRadius && dotProduct > 0) // 범위 내에 있고, 바라보는 방향에 있는 경우
                    {
                        // flipX가 되어있으면 왼쪽 방향만 인식
                        if (isFlipped_poison && directionToEnemy.x < 0)
                        {
                            enemyCount_poison++;
                            Debug.Log($"Enemy {enemy.name} is in range and to the left of flipped character");
                            if (enemy.TryGetComponent(out MeleeEnemy meleeEnemy))
                            {
                                Debug.Log($"Starting poison effect on MeleeEnemy: {enemy.name}");
                                StartCoroutine(ApplyPoisonEffectMelee(skill, enemy, 5));
                            }
                            else if (enemy.TryGetComponent(out RangedEnemy rangedEnemy))
                            {
                                Debug.Log($"Starting poison effect on RangedEnemy: {enemy.name}");
                                StartCoroutine(ApplyPoisonEffectRanged(skill, enemy, 5));
                            }
                        }
                        // flipX가 안되어있으면 오른쪽 방향만 인식
                        else if (!isFlipped_poison && directionToEnemy.x > 0)
                        {
                            enemyCount_poison++;
                            Debug.Log($"Enemy {enemy.name} is in range and to the right of non-flipped character");
                            if (enemy.TryGetComponent(out MeleeEnemy meleeEnemy))
                            {
                                Debug.Log($"Starting poison effect on MeleeEnemy: {enemy.name}");
                                StartCoroutine(ApplyPoisonEffectMelee(skill, enemy, 5));
                            }
                            else if (enemy.TryGetComponent(out RangedEnemy rangedEnemy))
                            {
                                Debug.Log($"Starting poison effect on RangedEnemy: {enemy.name}");
                                StartCoroutine(ApplyPoisonEffectRanged(skill, enemy, 5));
                            }
                        }
                        else
                        {
                            Debug.Log($"Enemy {enemy.name} is in range but not in the correct direction. isFlipped: {isFlipped_poison}, directionToEnemy.x: {directionToEnemy.x}");
                        }
                    }
                    else
                    {
                        Debug.Log($"Enemy {enemy.name} is not in range or not in front of character. Distance: {distance}, DotProduct: {dotProduct}, EffectRadius: {skill.effectRadius}");
                    }
                }

                Debug.Log($"Number of enemies hit by Poison: {enemyCount_poison}");
                break;
                
            case "Ultimo":
                // Player(Clone) 오브젝트 찾기
                
                GameObject playerObject_ultimo = GameObject.Find("Player(Clone)");
                if (playerObject_ultimo == null)
                {
                    Debug.LogError("Player(Clone) object not found!");
                    return;
                }
                BGMManager.instance.PlaySE(BGMManager.instance.CowSE,1.5f);

                // 플레이어의 위치를 기준으로 Ultimo 위치 설정
                ultimoPosition = playerObject_ultimo.transform.position;

                // Ultimo 이펙트 생성
                //if (ultimoEffectPrefab != null)
                GameObject ultimoFX = casterData.ultimoEffectOverride ?? ultimoEffectPrefab;
                if (ultimoFX != null)
                {
                    Vector3 cowSpawnPosition = playerObject_ultimo.transform.position + new Vector3(0f, 5f, 0f);
                    GameObject cowEffect = Instantiate(ultimoFX, cowSpawnPosition, Quaternion.identity);

                    // 착지 기준을 플레이어 위치로
                    CowDrop cowDrop = cowEffect.GetComponent<CowDrop>();
                    if (cowDrop != null)
                    {
                        cowDrop.SetLandingY(playerObject_ultimo.transform.position.y + 1.5f);
                        Debug.Log($"cowImpactFXPrefab: {(cowImpactFXPrefab == null ? "null" : cowImpactFXPrefab.name)}");

                        cowDrop.impactFXPrefab = cowImpactFXPrefab; // 등록 필요

                    }

                    //Destroy(cowEffect, 1.5f); // 효과 시간 후 제거
                }

                // 모든 Enemy 오브젝트 찾기
                GameObject[] enemies_ultimo = GameObject.FindGameObjectsWithTag("Enemy");
                int enemyCount_ultimo = 0; // 변수명 변경하여 중복 방지

                // 범위 내의 적 찾기 - 바라보는 방향 상관없이 모든 적에게 효과 적용
                foreach (GameObject enemy in enemies_ultimo)
                {
                    // 카메라 위치와 적의 거리 계산
                    Vector3 directionToEnemy = enemy.transform.position - playerObject_ultimo.transform.position;
                    float distance = directionToEnemy.magnitude;

                    // 범위 내에 있다면 방향에 상관없이 효과 적용
                    if (distance <= skill.effectRadius)
                    {
                        enemyCount_ultimo++;
                        if (enemy.TryGetComponent(out MeleeEnemy meleeEnemy))
                        {
                            // KnockBack 효과 먼저 적용
                            Rigidbody2D enemyRb = enemy.GetComponent<Rigidbody2D>();
                            if (enemyRb != null)
                            {
                                Debug.Log($"Applying knockback to MeleeEnemy: {enemy.name}");
                                Vector2 knockbackDirection = new Vector2(0f, 1f); // 위로만 넉백
                                enemyRb.velocity = Vector2.zero;
                                enemyRb.AddForce(knockbackDirection * 10f, ForceMode2D.Impulse);
                                Debug.Log($"Knockback applied to {enemy.name} - Direction: {knockbackDirection}, Force: {knockbackDirection * 10f}");
                                // 위치 고정 및 해제 코루틴 시작
                                StartCoroutine(FreezePosition(enemyRb, 0.3f, 0.4f));
                            }
                            else
                            {
                                Debug.LogWarning($"MeleeEnemy {enemy.name} has no Rigidbody2D component!");
                            }
                            // 1초 후 데미지 적용
                            StartCoroutine(DelayedDamage(meleeEnemy, actualDamage, 0.5f));
                        }
                        else if (enemy.TryGetComponent(out LavaGiant lavaGiant))
                        {
                            // KnockBack 효과 먼저 적용
                            Rigidbody2D enemyRb = enemy.GetComponent<Rigidbody2D>();
                            if (enemyRb != null)
                            {
                                Debug.Log($"Applying knockback to RangedEnemy: {enemy.name}");
                                Vector2 knockbackDirection = new Vector2(0f, 1f); // 위로만 넉백
                                enemyRb.velocity = Vector2.zero;
                                enemyRb.AddForce(knockbackDirection * 10f, ForceMode2D.Impulse);
                                Debug.Log($"Knockback applied to {enemy.name} - Direction: {knockbackDirection}, Force: {knockbackDirection * 10f}");
                                // 위치 고정 및 해제 코루틴 시작
                                StartCoroutine(FreezePosition(enemyRb, 0.3f, 0.4f));
                            }
                            else
                            {
                                Debug.LogWarning($"RangedEnemy {enemy.name} has no Rigidbody2D component!");
                            }
                            // 1초 후 데미지 적용
                            StartCoroutine(DelayedDamage(lavaGiant, actualDamage, 0.5f));
                        }
                        else if (enemy.TryGetComponent(out Wolf wolf))
                        {
                            // KnockBack 효과 먼저 적용
                            Rigidbody2D enemyRb = enemy.GetComponent<Rigidbody2D>();
                            if (enemyRb != null)
                            {
                                Debug.Log($"Applying knockback to RangedEnemy: {enemy.name}");
                                Vector2 knockbackDirection = new Vector2(0f, 1f); // 위로만 넉백
                                enemyRb.velocity = Vector2.zero;
                                enemyRb.AddForce(knockbackDirection * 10f, ForceMode2D.Impulse);
                                Debug.Log($"Knockback applied to {enemy.name} - Direction: {knockbackDirection}, Force: {knockbackDirection * 10f}");
                                // 위치 고정 및 해제 코루틴 시작
                                StartCoroutine(FreezePosition(enemyRb, 0.3f, 0.4f));
                            }
                            else
                            {
                                Debug.LogWarning($"RangedEnemy {enemy.name} has no Rigidbody2D component!");
                            }
                            // 1초 후 데미지 적용
                            StartCoroutine(DelayedDamage(wolf, actualDamage, 0.5f));
                        }
                        else if (enemy.TryGetComponent(out Slime slime))
                        {
                            // KnockBack 효과 먼저 적용
                            Rigidbody2D enemyRb = enemy.GetComponent<Rigidbody2D>();
                            if (enemyRb != null)
                            {
                                Debug.Log($"Applying knockback to RangedEnemy: {enemy.name}");
                                Vector2 knockbackDirection = new Vector2(0f, 1f); // 위로만 넉백
                                enemyRb.velocity = Vector2.zero;
                                enemyRb.AddForce(knockbackDirection * 10f, ForceMode2D.Impulse);
                                Debug.Log($"Knockback applied to {enemy.name} - Direction: {knockbackDirection}, Force: {knockbackDirection * 10f}");
                                // 위치 고정 및 해제 코루틴 시작
                                StartCoroutine(FreezePosition(enemyRb, 0.3f, 0.4f));
                            }
                            else
                            {
                                Debug.LogWarning($"RangedEnemy {enemy.name} has no Rigidbody2D component!");
                            }
                            // 1초 후 데미지 적용
                            StartCoroutine(DelayedDamage(slime, actualDamage, 0.5f));
                        }
                        else if (enemy.TryGetComponent(out RangedEnemy rangedEnemy))
                        {
                            // KnockBack 효과 먼저 적용
                            Rigidbody2D enemyRb = enemy.GetComponent<Rigidbody2D>();
                            if (enemyRb != null)
                            {
                                Debug.Log($"Applying knockback to RangedEnemy: {enemy.name}");
                                Vector2 knockbackDirection = new Vector2(0f, 1f); // 위로만 넉백
                                enemyRb.velocity = Vector2.zero;
                                enemyRb.AddForce(knockbackDirection * 10f, ForceMode2D.Impulse);
                                Debug.Log($"Knockback applied to {enemy.name} - Direction: {knockbackDirection}, Force: {knockbackDirection * 10f}");
                                // 위치 고정 및 해제 코루틴 시작
                                StartCoroutine(FreezePosition(enemyRb, 0.3f, 0.4f));
                            }
                            else
                            {
                                Debug.LogWarning($"RangedEnemy {enemy.name} has no Rigidbody2D component!");
                            }
                            // 1초 후 데미지 적용
                            StartCoroutine(DelayedDamage(rangedEnemy, actualDamage, 0.5f));
                        }
                    }
                }

                Debug.Log($"Number of enemies hit by Ultimo: {enemyCount_ultimo}");
                break;

            default: // Heal 계열 스킬
                ApplyDefaultEffect(skill, characterTransform, casterData);
                break;
        }
    }

    private void ApplyDefaultEffect(CharacterSkill skill, Transform characterTransform, CharacterData casterData)
    {
        // 모든 객체의 체력 값을 로그로 출력
        PlayerController playerCtrl = FindObjectOfType<PlayerController>();
        int playerCurrentHealth = playerCtrl != null ? playerCtrl.CurrentHealth : -1;
        int gameManagerCurrentHealth = GameManager.Instance.CurrentPlayerHealth;
        
        Debug.Log($"[디버깅] SkillManager - 체력 값들: PlayerController.currentHealth = {playerCurrentHealth}, GameManager.currentPlayerHealth = {gameManagerCurrentHealth}");
        
        // 체력 불일치 감지 및 즉시 동기화
        if (playerCurrentHealth != gameManagerCurrentHealth && playerCurrentHealth != -1)
        {
            Debug.LogWarning($"[디버깅] 체력 불일치 발견! PlayerController: {playerCurrentHealth}, GameManager: {gameManagerCurrentHealth}");
            
            // PlayerController의 체력을 우선시하여 GameManager 동기화
            GameManager.Instance.SavePlayerHealth(playerCurrentHealth, playerCtrl.MaxHealth);
            Debug.Log($"[디버깅] GameManager 체력을 PlayerController 체력으로 동기화: {gameManagerCurrentHealth} -> {playerCurrentHealth}");
        }
        
        switch (skill.effectType)
        {
            case CharacterSkill.EffectType.Damage:
            case CharacterSkill.EffectType.Debuff:
                // 유효 범위 내의 적 찾기
                Collider[] hitColliders = Physics.OverlapSphere(characterTransform.position, skill.effectRadius);
                foreach (var hitCollider in hitColliders)
                {
                    // 적에게 데미지 주는 로직
                    if (hitCollider.CompareTag("Enemy"))
                    {
                        Debug.Log($"Dealing {skill.effectValue} damage to {hitCollider.name}.");
                        // 적에게 데미지 적용 로직 추가
                    }
                }
                break;
            case CharacterSkill.EffectType.Heal:
                // 현재 체력은 항상 PlayerController에서 직접 가져옴 (GameManager 무시)
                int currentHealth = playerCtrl != null ? playerCtrl.CurrentHealth : GameManager.Instance.GetSavedPlayerHealth();
                int maxHealth = playerCtrl != null ? playerCtrl.MaxHealth : GameManager.Instance.MaxHealth;
                
                // 체력 회복량 계산
                int healAmount = CalculateHealAmount(skill.effectValue);
                
                // 현재 체력 상태 기록
                Debug.Log($"Heal 스킬 사용 전 - 현재 체력: {currentHealth}, 최대 체력: {maxHealth}");
                
                // 회복 후 체력 계산 (최대 체력 초과 방지)
                int newHealth = Mathf.Min(currentHealth + healAmount, maxHealth);
                
                // 변화량 계산
                int actualHealAmount = newHealth - currentHealth;
                
                // 체력 업데이트 (실제 변화량이 있을 때만)
                if (actualHealAmount > 0)
                {
                    Debug.Log($"[디버깅] Heal 스킬 적용 전 - PlayerController 체력: {playerCurrentHealth}, GameManager 체력: {gameManagerCurrentHealth}, 적용할 체력: {newHealth}");

                    // Heal 이펙트 생성
                    GameObject healFXPrefabToUse = casterData?.healEffectOverride ?? healEffectPrefab;
                    if (healFXPrefabToUse != null && playerCtrl != null)
                    {
                        GameObject healFX = Instantiate(healFXPrefabToUse, playerCtrl.transform.position + new Vector3(0f, 2.5f, 0f), Quaternion.identity);

                        HealEffectFollow followScript = healFX.GetComponent<HealEffectFollow>();
                        if (followScript != null)
                        {
                            followScript.Initialize(playerCtrl.transform, 2.5f); // 생성 위치와 동일한 y offset
                        }

                        Destroy(healFX, 1.0f);
                    }

                    // 회복 인디케이터 표시
                    if (HealIndicatorManager.Instance != null)
                    {
                        HealIndicatorManager.Instance.ShowHealIndicator(playerCtrl.transform.position, actualHealAmount);
                    }

                    // PlayerController 직접 찾아서 체력 동기화
                    if (playerCtrl != null)
                    {
                        BGMManager.instance.PlaySE(BGMManager.instance.HealSE,1f);
                        playerCtrl.UpdateHealth(newHealth);
                        Debug.Log($"[디버깅] PlayerController.UpdateHealth({newHealth}) 호출 완료");


                        // PlayerUI 찾아서 슬라이더 직접 업데이트
                        PlayerUI playerUI = FindObjectOfType<PlayerUI>();
                        if (playerUI != null)
                        {
                            float healthPercent = (float)newHealth / maxHealth;
                            playerUI.UpdateHealthSlider(healthPercent);
                            Debug.Log($"[디버깅] PlayerUI.UpdateHealthSlider({healthPercent}) 호출 완료");
                        }
                    }
                    
                    // GameManager 체력 업데이트
                    GameManager.Instance.CurrentPlayerHealth = newHealth;
                    Debug.Log($"[디버깅] GameManager.CurrentPlayerHealth = {newHealth} 설정 완료");
                    
                    Debug.Log($"체력 회복 적용 - 현재 체력: {newHealth}, 회복량: {healAmount}, 실제 회복량: {actualHealAmount}, 최대 체력: {maxHealth}");
                    
                    // 적용 후 체력 값 확인
                    Debug.Log($"[디버깅] Heal 스킬 적용 후 - PlayerController 체력: {playerCtrl.CurrentHealth}, GameManager 체력: {GameManager.Instance.CurrentPlayerHealth}");
                }
                else
                {
                    Debug.Log($"체력이 이미 최대이거나 회복량이 0입니다. 현재 체력: {currentHealth}, 최대 체력: {maxHealth}");
                }
                break;
            case CharacterSkill.EffectType.Buff:
                // 캐릭터 버프 적용 로직
                Debug.Log($"Buffing character with {skill.effectValue}.");
                break;
            default:
                break;
        }
    }

    private int CalculateHealAmount(string effectValue)
    {
        // 현재 사용 중인 캐릭터의 최대 체력 가져오기
        int characterMaxHealth = GameManager.Instance.MaxHealth;
        
        Debug.Log($"캐릭터의 최대 체력: {characterMaxHealth}, 효과 값: {effectValue}");
        
        if (effectValue.EndsWith("%"))
        {
            // 퍼센트로 해석
            if (int.TryParse(effectValue.TrimEnd('%'), out int percentage))
            {
                int healAmount = Mathf.RoundToInt(characterMaxHealth * (percentage / 100f));
                Debug.Log($"백분율 회복: {percentage}%, 회복량: {healAmount}");
                return healAmount;
            }
            else
            {
                Debug.LogError($"유효하지 않은 회복 백분율 값: {effectValue}");
                return 0;
            }
        }
        else
        {
            // 정수로 해석
            if (int.TryParse(effectValue, out int fixedAmount))
            {
                Debug.Log($"고정 회복량: {fixedAmount}");
                return fixedAmount;
            }
            else
            {
                Debug.LogError($"유효하지 않은 회복 값: {effectValue}");
                return 0;
            }
        }
    }

    private IEnumerator ApplyPoisonEffectMelee(CharacterSkill skill, GameObject enemy, int effectTime)
    {
        // effectValue를 float로 변환
        float damagePerSecond;
        if (!float.TryParse(skill.effectValue, out damagePerSecond))
        {
            Debug.LogError("Invalid effect value for poison damage. Please check the value.");
            yield break; // 변환 실패 시 코루틴 종료
        }

        // 적이 MeleeEnemy 컴포넌트를 가지고 있는지 확인
        MeleeEnemy meleeEnemy = enemy.GetComponent<MeleeEnemy>();
        if (meleeEnemy == null)
        {
            Debug.LogError($"Enemy {enemy.name} does not have MeleeEnemy component.");
            yield break; // MeleeEnemy 컴포넌트가 없으면 코루틴 종료
        }

        Debug.Log($"Starting poison effect on MeleeEnemy: {enemy.name} for {effectTime} seconds with {damagePerSecond} damage per second");
        float elapsedTime = 0f; // 경과 시간

        // 독 데미지 적용 시작 시 현재 체력 기록
        float initialHealth = meleeEnemy.currentHealth;
        Debug.Log($"MeleeEnemy {enemy.name} initial health: {initialHealth}");

        while (elapsedTime < effectTime && enemy != null && enemy.activeInHierarchy)
        {
            // 적에게 독 데미지 적용
            meleeEnemy.TakeDamage(damagePerSecond);
            Debug.Log($"MeleeEnemy {enemy.name} took {damagePerSecond} poison damage. Remaining time: {effectTime - elapsedTime}s, Current health: {meleeEnemy.currentHealth}, Health change: {initialHealth - meleeEnemy.currentHealth}");

            elapsedTime += 1f; // 1초 경과
            yield return new WaitForSeconds(1f); // 1초 대기
        }
        
        Debug.Log($"Poison effect on MeleeEnemy: {enemy.name} has ended. Total damage: {initialHealth - meleeEnemy.currentHealth}");
    }

    private IEnumerator ApplyPoisonEffectRanged(CharacterSkill skill, GameObject enemy, int effectTime)
    {
        // effectValue를 float로 변환
        float damagePerSecond;
        if (!float.TryParse(skill.effectValue, out damagePerSecond))
        {
            Debug.LogError("Invalid effect value for poison damage. Please check the value.");
            yield break; // 변환 실패 시 코루틴 종료
        }

        // 적이 RangedEnemy 컴포넌트를 가지고 있는지 확인
        RangedEnemy rangedEnemy = enemy.GetComponent<RangedEnemy>();
        if (rangedEnemy == null)
        {
            Debug.LogError($"Enemy {enemy.name} does not have RangedEnemy component.");
            yield break; // RangedEnemy 컴포넌트가 없으면 코루틴 종료
        }

        Debug.Log($"Starting poison effect on RangedEnemy: {enemy.name} for {effectTime} seconds with {damagePerSecond} damage per second");
        float elapsedTime = 0f; // 경과 시간

        // 독 데미지 적용 시작 시 현재 체력 기록
        float initialHealth = rangedEnemy.currentHealth;
        Debug.Log($"RangedEnemy {enemy.name} initial health: {initialHealth}");

        while (elapsedTime < effectTime)
        {
            if (enemy == null || !enemy.activeInHierarchy)
                break;

            if (rangedEnemy == null)
                break;

            rangedEnemy.TakeDamage(damagePerSecond);

            elapsedTime += 1f;
            yield return new WaitForSeconds(1f);
        }


        while (elapsedTime < effectTime && enemy != null && enemy.activeInHierarchy)
        {
            // 적에게 독 데미지 적용
            rangedEnemy.TakeDamage(damagePerSecond);
            Debug.Log($"RangedEnemy {enemy.name} took {damagePerSecond} poison damage. Remaining time: {effectTime - elapsedTime}s, Current health: {rangedEnemy.currentHealth}, Health change: {initialHealth - rangedEnemy.currentHealth}");

            elapsedTime += 1f; // 1초 경과
            yield return new WaitForSeconds(1f); // 1초 대기
        }

        Debug.Log($"Poison effect on RangedEnemy: {enemy.name} has ended. Total damage: {initialHealth - rangedEnemy.currentHealth}");
    }

    private IEnumerator DelayedDamage(MeleeEnemy enemy, float damage, float delay)
    {
        yield return new WaitForSeconds(delay);
        enemy.TakeDamage(damage);
    }
    private IEnumerator DelayedDamage(Slime enemy, float damage, float delay)
    {
        yield return new WaitForSeconds(delay);
        enemy.TakeDamage(damage);
    }
    private IEnumerator DelayedDamage(Wolf enemy, float damage, float delay)
    {
        yield return new WaitForSeconds(delay);
        enemy.TakeDamage(damage);
    }

    private IEnumerator DelayedDamage(RangedEnemy enemy, float damage, float delay)
    {
        yield return new WaitForSeconds(delay);
        enemy.TakeDamage(damage);
    }
    private IEnumerator DelayedDamage(LavaGiant enemy, float damage, float delay)
    {
        yield return new WaitForSeconds(delay);
        enemy.TakeDamage(damage);
    }

    private IEnumerator FreezePosition(Rigidbody2D rb, float freezeDelay, float unfreezeDelay)
    {
        // freezeDelay 초 후에 위치 고정
        yield return new WaitForSeconds(freezeDelay);
        rb.constraints = RigidbodyConstraints2D.FreezeAll;
        Debug.Log($"Position frozen for {rb.gameObject.name}");
        
        // unfreezeDelay 초 후에 위치 고정 해제 (회전만 고정)
        yield return new WaitForSeconds(unfreezeDelay);
        rb.constraints = RigidbodyConstraints2D.FreezeRotation; // 회전만 고정
        Debug.Log($"Position unfrozen for {rb.gameObject.name}");
    }


    public IEnumerator ShakeCamera(float duration, float magnitude)
    {
        Vector3 originalPos = Camera.main.transform.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            Camera.main.transform.localPosition = new Vector3(x, y, originalPos.z);

            elapsed += Time.deltaTime;
            yield return null;
        }

        Camera.main.transform.localPosition = originalPos;
    }

    //public IEnumerator ShakeCamera(float duration, float magnitude)
    // {
    //     Vector3 originalPos = Camera.main.transform.localPosition;
    //     float elapsed = 0f;

    //     while (elapsed < duration)
    //     {
    //         float x = Random.Range(-1f, 1f) * magnitude;
    //         float y = Random.Range(-1f, 1f) * magnitude;

    //         Camera.main.transform.localPosition = new Vector3(x, y, originalPos.z);

    //         elapsed += Time.deltaTime;
    //         yield return null;
    //     }

    //     Camera.main.transform.localPosition = originalPos;
    // }

    private void OnDrawGizmos()
    {
        // Fireball 스킬의 효과 범위 표시 (파란색)
        Gizmos.color = Color.blue;
        // 기즈모로 effectRadius 범위 그리기
        Gizmos.DrawWireSphere(fireballPosition, 15f); // Fireball 위치에 기즈모 그리기
        
        // Ultimo 스킬의 효과 범위 표시 (빨간색)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(ultimoPosition, 50f); // Ultimo 위치에 기즈모 그리기

        // BaseG 스킬의 효과 범위 표시 (노란색)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(baseAttackPosition, 10f); // BaseG 위치에 기즈모 그리기

        // BaseG 공격과 가장 가까운 적 사이의 직선 표시 (초록색)
        Gizmos.color = Color.green;
        Gizmos.DrawLine(baseAttackPosition, closestEnemyPosition);
    }
}
