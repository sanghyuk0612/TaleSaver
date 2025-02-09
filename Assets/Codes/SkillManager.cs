using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UseSkill(CharacterSkill skill, Transform characterTransform)
    {
        Debug.Log($"Using skill: {skill.skillName} with damage: {skill.skillDamage} or effect value: {skill.effectValue}");
        ApplySkillEffect(skill, characterTransform);
    }

    private void ApplySkillEffect(CharacterSkill skill, Transform characterTransform)
    {
        switch (skill.skillName)
        {
            case "Fireball":
                // Fireball 스킬 효과
                Debug.Log("Casting Fireball!");
                // Fireball 애니메이션 및 효과 적용
                Collider[] hitColliders = Physics.OverlapSphere(characterTransform.position, skill.effectRadius);
                foreach (var hitCollider in hitColliders)
                {
                    // 적에게 데미지 주는 로직
                    if (hitCollider.CompareTag("Enemy"))
                    {
                        Debug.Log($"Dealing {skill.skillDamage} damage to {hitCollider.name}.");
                        // 적에게 데미지 적용 로직 추가
                    }
                }
                
                break;

            case "Lightning":
                // Lightning 스킬 효과
                Debug.Log("Casting Lightning!");
                // Lightning 애니메이션 및 효과 적용
                break;

            case "Poison":
                StartCoroutine(ApplyPoisonEffect(skill, characterTransform));
                break;
                
            default: // Heal 계열 스킬
                ApplyDefaultEffect(skill, characterTransform);
                break;
        }
    }

    private void ApplyDefaultEffect(CharacterSkill skill, Transform characterTransform)
    {
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
                int healAmount = CalculateHealAmount(skill.effectValue);
                GameManager.Instance.ModifyHealth(healAmount);
                Debug.Log($"Healing for {healAmount} health.");
                break;
            case CharacterSkill.EffectType.Buff:
                // 캐릭터 버프 적용 로직
                Debug.Log($"Buffing character with {skill.effectValue}.");
                break;
            default:
                break;
        }
    }

    private IEnumerator ApplyPoisonEffect(CharacterSkill skill, Transform characterTransform)
    {
        RaycastHit hit;
        if (Physics.Raycast(characterTransform.position, characterTransform.forward, out hit))
        {
            if (hit.collider.CompareTag("Enemy"))
            {
                Vector3 poisonCenter = hit.collider.transform.position;
                float duration = 5f;
                float interval = 1f;

                for (float time = 0; time < duration; time += interval)
                {
                    Collider[] hitColliders = Physics.OverlapSphere(poisonCenter, skill.effectRadius);
                    foreach (var hitCollider in hitColliders)
                    {
                        if (hitCollider.CompareTag("Enemy"))
                        {
                            Debug.Log($"Dealing {skill.effectValue} poison damage to {hitCollider.name}.");
                            // 적에게 데미지 적용 로직 추가
                        }
                    }
                    yield return new WaitForSeconds(interval);
                }
            }
        }
    }

    private int CalculateHealAmount(string effectValue)
    {
        if (effectValue.EndsWith("%"))
        {
            // 퍼센트로 해석
            int percentage = int.Parse(effectValue.TrimEnd('%'));
            return (int)(GameManager.Instance.MaxHealth * (percentage / 100f));
        }
        else
        {
            // 정수로 해석
            return int.Parse(effectValue);
        }
    }
}
