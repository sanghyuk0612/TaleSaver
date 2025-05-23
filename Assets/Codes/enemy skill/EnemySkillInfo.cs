using UnityEngine;

public class EnemySkillInfo : MonoBehaviour
{
    public GameObject projectilePrefab;     // 투사체 프리펩
    public float projectileSpeed = 6f;      // 투사체 속도
    public int damage = 10;                 // 데미지

    public GameObject skillEffectPrefab;    // [추가] 스킬 이펙트 (예: 투사체 발사 시 이펙트)
    public GameObject hitEffectPrefab;      // [추가] 히트 이펙트 (예: 투사체 히트 시 이펙트)
}

