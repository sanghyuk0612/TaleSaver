using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewMonsterData", menuName = "Monster/Monster Data")]
public class MonsterData : ScriptableObject
{
    [Header("Basic Info")]
    public string monsterName; // 몬스터 이름
    public int health;
    public int damage;
    public float moveSpeed;
    public bool isRanged;

    [Header("Visuals")]
    public GameObject MonsterPrefab; // 몬스터 프리팹
    public Sprite monsterSprite; // 몬스터 기본 스프라이트
    public RuntimeAnimatorController animatorController; // 애니메이션 컨트롤러

    public Sprite GetMonsterSprite()
    {
        if (MonsterPrefab != null)
        {
            var renderer = MonsterPrefab.GetComponentInChildren<SpriteRenderer>();
            return renderer != null ? renderer.sprite : null;
        }
        return null;
    }

    public RuntimeAnimatorController GetAnimatorController()
    {
        if (MonsterPrefab != null)
        {
            var animator = MonsterPrefab.GetComponentInChildren<Animator>();
            return animator != null ? animator.runtimeAnimatorController : null;
        }
        return null;
    }

}
