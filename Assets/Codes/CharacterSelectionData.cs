using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSelectionData : MonoBehaviour
{
    public static CharacterSelectionData Instance { get; private set; }

    public Sprite selectedCharacterSprite; // 선택된 캐릭터의 스프라이트
    public CharacterData selectedCharacterData; // 선택된 캐릭터의 데이터
    public RuntimeAnimatorController selectedCharacterAnimator;  // 선택된 캐릭터 애니메이터

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬 전환 시 오브젝트 유지
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        StartCoroutine(WaitForCharacterManager());
    }

    private IEnumerator WaitForCharacterManager()
    {
        // CharacterManager가 준비될 때까지 대기
        CharacterManager characterManager = null;
        while (characterManager == null)
        {
            characterManager = FindObjectOfType<CharacterManager>();
            yield return null; // 다음 프레임까지 대기
        }

        // CharacterManager가 준비되면 스프라이트 설정
        SetDefaultCharacterSprite(characterManager);
    }


    public void SetDefaultCharacterSprite(CharacterManager characterManager)
    {
        if (characterManager.characters.Length > 0)
        {
            selectedCharacterSprite = characterManager.characters[0].characterSprite;
            Debug.Log("Default character sprite set to: " + selectedCharacterSprite.name);
        }
        else
        {
            Debug.LogWarning("No characters available in CharacterManager!");
        }
    }

}

