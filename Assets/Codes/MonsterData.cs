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
    public Sprite monsterSprite; // 몬스터 기본 스프라이트
    public RuntimeAnimatorController animatorController; // 애니메이션 컨트롤러

}
