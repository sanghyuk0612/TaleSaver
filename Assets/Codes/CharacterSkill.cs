using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "NewSkill", menuName = "Character/Skill")]
public class CharacterSkill : ScriptableObject
{
    public string skillName;
    public int skillDamage;
    public float skillCooldown;
    public float effectRadius;
    public EffectType effectType;
    public string effectValue;
    public Sprite skillImage;

    public enum EffectType
    {
        Damage,
        Heal,
        Buff,
        Debuff
    }
    
    // STR(power) 레벨에 따른 실제 데미지 계산
    public int CalculateActualDamage(int powerLevel)
    {
        // 공식: 기본 데미지 * (1 + (STR 레벨 * 0.1)) * (1 + (롱소드 보유 시 0.15))
        float damageMultiplier = 1 + (powerLevel * 0.1f);
        
        // 롱소드 아이템 효과 적용 (ID: 2)
        bool hasLongSword = InventoryManager.Instance != null && 
                          InventoryManager.Instance.inventory != null && 
                          InventoryManager.Instance.inventory.items != null && 
                          InventoryManager.Instance.inventory.items.Contains(2);
        
        if (hasLongSword)
        {
            damageMultiplier *= 1.15f;
        }
        
        int actualDamage = Mathf.RoundToInt(skillDamage * damageMultiplier);
        
        Debug.Log($"스킬 '{skillName}' 데미지 계산: 기본({skillDamage}) * 배율({damageMultiplier}) = {actualDamage}");
        return actualDamage;
    }
}
