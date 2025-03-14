using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewCharacterData", menuName = "Character/Character Data")]
public class CharacterData : ScriptableObject
{
    public string characterName;  // 캐릭터 이름
    public Sprite characterSprite; // 캐릭터 이미지
    public RuntimeAnimatorController animatorController;
    public int level = 1;         // 레벨 (초기값을 1로 설정)
    public string description;    // 설명
    public int vitality;          // 생명력
    public int power;             // 파워
    public int agility;           // 민첩
    public int luck;              // 행운
    public int maxHealth;         // 캐릭터의 최대 체력
    public bool isUnlocked;       // 해금 여부
    public int requiredSteelPieces = 1;// 해금 조건 - screw 개수
    public int requiredBookPages = 1; // 해금 조건 - page 개수

    // 스킬 배열 추가
    public CharacterSkill[] skills; // 스킬 배열

    // 스킬 초기화 메서드
    public void InitializeSkills()
    {
        // 기본 스킬 초기화 (이 부분은 CharacterManager에서 각 캐릭터에 맞게 수정)
        skills = new CharacterSkill[4];
    }

    public void SetSkills(CharacterSkill[] newSkills)
    {
        if (newSkills.Length == 4)
        {
            skills = newSkills;
        }
        else
        {
            Debug.LogWarning("스킬 배열의 길이는 4여야 합니다.");
        }
    }
}
