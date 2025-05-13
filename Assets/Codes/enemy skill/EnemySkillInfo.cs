using UnityEngine;

public class EnemySkillInfo : MonoBehaviour
{
    public GameObject projectilePrefab;     // 발사체 프리팹
    public float projectileSpeed = 6f;      // 발사체 속도
    public int damage = 10;                 // 데미지

    public GameObject skillEffectPrefab;    // [추가] 시전 이펙트 (예: 발사 준비 연기)
    public GameObject hitEffectPrefab;      // [추가] 명중 이펙트 (예: 폭발 이펙트)
}

