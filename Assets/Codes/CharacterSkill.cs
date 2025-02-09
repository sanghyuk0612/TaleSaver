using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewSkill", menuName = "Character/Skill")]
public class CharacterSkill : ScriptableObject
{
    public string skillName;
    public int skillDamage;
    public float skillCooldown;
    public float effectRadius;
    public EffectType effectType;
    public string effectValue;

    public enum EffectType
    {
        Damage,
        Heal,
        Buff,
        Debuff
    }
}
