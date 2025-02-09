using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CharacterManager : MonoBehaviour
{
    [Header("Character Data Info")]
    public CharacterData[] characters;        // 모든 캐릭터 데이터 배열
    private string[] characterNames = { "Character1", "Gyeonu", "Character3", "Character4", "Character5", "Character6", "Character7" }; // 캐릭터 이름 배열
    public Image characterImage;              // 캐릭터 이미지
    public Text characterNameText;            // 캐릭터 이름 텍스트
    public Text descriptionText;              // 캐릭터 설명 텍스트
    public Text levelText;                    // 캐릭터 레벨 텍스트

    // 현재 선택된 캐릭터의 스프라이트를 저장할 변수
    private Sprite characterSprite;

    [Header("Panel Info")]
    public GameObject characterInfoPanel;     // 캐릭터 정보 패널
    public GameObject upgradePanel;           // 업그레이드 패널

    [Header("Upgrade Info")]
    public Image upgradeCharacterImage;
    public Text upgradeNameText;
    public Text upgradeLevelText;
    public Text vitalityText;
    public Text powerText;
    public Text agilityText;
    public Text luckText;

    [Header("Upgrade Buttons")]
    public Button vitalityUpgradeButton; // Vitality 업그레이드 버튼
    public Button powerUpgradeButton;     // Power 업그레이드 버튼
    public Button agilityUpgradeButton;   // Agility 업그레이드 버튼
    public Button luckUpgradeButton;      // Luck 업그레이드 버튼

    [Header("Buttons")]
    public Button characterButton1;
    public Button characterButton2;
    public Button characterButton3;
    public Button characterButton4;
    public Button characterButton5;
    public Button characterButton6;
    public Button characterButton7;
    public Button backButton;
    public Button selectButton;
    public Button upgradeButton;
    public Button closeUpgradeButton;         // 업그레이드 창 닫기 버튼

    private int currentCharacterIndex = 0;
    private float[] skillCooldownTimers; // 스킬 쿨타임 타이머

    private SkillManager skillManager;
    private int currentHealth;
    private int maxHealth;

    // 각 캐릭터의 최대 체력을 저장하는 배열
    private int[] maxHealthArray = new int[7] { 100, 120, 110, 130, 140, 150, 160 };

    private void Start()
    {
        characterInfoPanel.SetActive(false);
        upgradePanel.SetActive(false);

        // 캐릭터 데이터가 올바르게 로드되었는지 확인
        if (characters == null || characters.Length == 0)
        {
            Debug.LogError("No character data found!");
            return;
        }

        // 초기 캐릭터 로드
        LoadCharacter(0);

        // 버튼 리스너 설정
        characterButton1.onClick.AddListener(() => ShowCharacterInfo(0));
        characterButton2.onClick.AddListener(() => ShowCharacterInfo(1));
        characterButton3.onClick.AddListener(() => ShowCharacterInfo(2));
        characterButton4.onClick.AddListener(() => ShowCharacterInfo(3));
        characterButton5.onClick.AddListener(() => ShowCharacterInfo(4));
        characterButton6.onClick.AddListener(() => ShowCharacterInfo(5));
        characterButton7.onClick.AddListener(() => ShowCharacterInfo(6));


        backButton.onClick.AddListener(HideCharacterInfo);
        selectButton.onClick.AddListener(OnSelectButtonClick);
        upgradeButton.onClick.AddListener(ShowUpgradePanel);

        closeUpgradeButton.onClick.AddListener(CloseUpgradePanel); // 업그레이드 창 닫기 버튼에 이벤트 추가

        // CharacterSelectionData에 스프라이트 설정 요청
        CharacterSelectionData.Instance.SetDefaultCharacterSprite(this);

        // 스킬 쿨타임 타이머 초기화
        skillCooldownTimers = new float[4];

        // SkillManager를 GameObject에 추가
        skillManager = gameObject.AddComponent<SkillManager>();

        maxHealth = GameManager.Instance.GetCurrentMaxHealth();
        currentHealth = maxHealth;

        // GameManager에 현재 캐릭터 설정
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetCurrentCharacter(characters[currentCharacterIndex]);
        }
    }

    private void Update()
    {
        // 키보드의 `키`를 눌렀을 때 현재 선택된 캐릭터의 레벨을 증가
        if (Input.GetKeyDown(KeyCode.BackQuote)) IncreaseCharacterLevel(); // `키는 BackQuote로 표현
        // 스킬 쿨타임 타이머 업데이트
        for (int i = 0; i < skillCooldownTimers.Length; i++)
        {
            if (skillCooldownTimers[i] > 0)
            {
                skillCooldownTimers[i] -= Time.deltaTime;
            }
        }

        // 키 입력 감지 및 스킬 사용
        if (Input.GetKeyDown(KeyCode.T))
        {
            UseSkill(0);
        }
        if (Input.GetKeyDown(KeyCode.Y))
        {
            UseSkill(1);
        }
        if (Input.GetKeyDown(KeyCode.U))
        {
            UseSkill(2);
        }
        if (Input.GetKeyDown(KeyCode.I))
        {
            UseSkill(3);
        }
    }

    private void UseSkill(int skillIndex)
    {
        CharacterData character = GameManager.Instance.CurrentCharacter;
        if (character.skills != null && skillIndex >= 0 && skillIndex < character.skills.Length)
        {
            CharacterSkill skill = character.skills[skillIndex];

            // 스킬 쿨타임 확인
            if (skillCooldownTimers[skillIndex] <= 0)
            {
                skillManager.UseSkill(skill, transform);

                // 쿨타임 설정
                skillCooldownTimers[skillIndex] = skill.skillCooldown;
            }
            else
            {
                Debug.Log($"Skill {skill.skillName} is on cooldown for {skillCooldownTimers[skillIndex]:F1} more seconds.");
            }
        }
        else
        {
            Debug.LogWarning("Invalid skill index!");
        }
    }

    private void IncreaseCharacterLevel()
    {
        CharacterData character = characters[currentCharacterIndex];
        character.level++; // 레벨 증가
        SaveCharacterStats(); // 변경된 레벨 저장
        LoadCharacter(currentCharacterIndex); // 캐릭터 정보 다시 로드

        LoadUpgradePanel();
    }

    // Select 버튼 클릭
    public void OnSelectButtonClick()
    {
        // 선택된 캐릭터의 데이터를 CharacterSelectionData에 저장
        CharacterData selectedCharacter = characters[currentCharacterIndex];
        if (selectedCharacter == null)
        {
            Debug.LogError("Selected character is null!");
            return; // 캐릭터가 null인 경우 메서드 종료
        }

        if (characterImage.sprite == null)
        {
            Debug.LogError("Selected character sprite is missing!"); // 스프라이트가 null인 경우 오류 로그
            return; // 스프라이트가 null인 경우 메서드 종료
        }

        CharacterSelectionData.Instance.selectedCharacterSprite = characterImage.sprite;
        CharacterSelectionData.Instance.selectedCharacterData = selectedCharacter; // 선택된 캐릭터 데이터 저장
        SceneManager.LoadScene("GameScene");
    }

    private void UseCharacterSkills(CharacterData character)
    {
        foreach (var skill in character.skills)
        {
            // 각 스킬 사용 로직
            Debug.Log($"Using skill: {skill.skillName} with damage: {skill.skillDamage}");
            // 여기서 각 스킬을 실제로 사용하는 로직을 구현할 수 있습니다.
        }
    }

    public void ShowCharacterInfo(int index)
    {
        characterInfoPanel.SetActive(true);
        characterButton1.interactable = false;
        characterButton2.interactable = false;
        characterButton3.interactable = false;
        characterButton4.interactable = false;
        characterButton5.interactable = false;
        characterButton6.interactable = false;
        characterButton7.interactable = false;

        LoadCharacter(index);
    }

    public void HideCharacterInfo()
    {
        characterInfoPanel.SetActive(false);
        characterButton1.interactable = true;
        characterButton2.interactable = true;
        characterButton3.interactable = true;
        characterButton4.interactable = true;
        characterButton5.interactable = true;
        characterButton6.interactable = true;
        characterButton7.interactable = true;
    }

    public void LoadCharacter(int index)
    {
        if (index < 0 || index >= characters.Length)
        {
            Debug.LogError("Character index out of range!");
            return;
        }

        currentCharacterIndex = index;
        CharacterData character = characters[index];

        if (character == null)
        {
            Debug.LogError("CharacterData is null for index: " + index);
            return;
        }

        // PlayerPrefs에서 캐릭터 속성 로드
        character.maxHealth = 
        character.level = PlayerPrefs.GetInt("CharacterLevel_" + currentCharacterIndex, 1);
        character.vitality = PlayerPrefs.GetInt("CharacterVitality_" + currentCharacterIndex, 0);
        character.power = PlayerPrefs.GetInt("CharacterPower_" + currentCharacterIndex, 0);
        character.agility = PlayerPrefs.GetInt("CharacterAgility_" + currentCharacterIndex, 0);
        character.luck = PlayerPrefs.GetInt("CharacterLuck_" + currentCharacterIndex, 0);

        // 스킬 로드
        LoadCharacterSkills(character);

        CharacterSelectionData.Instance.selectedCharacterSprite = character.characterSprite;
        characterImage.sprite = character.characterSprite;
        characterNameText.text = character.characterName;
        descriptionText.text = character.description;
        levelText.text = "Level: " + character.level;

        // 업그레이드 창에 동일하게 표시
        upgradeNameText.text = character.characterName;
        upgradeLevelText.text = "Level: " + character.level;
        vitalityText.text = "VIT: " + character.vitality;
        powerText.text = "POW: " + character.power;
        agilityText.text = "AGI: " + character.agility;
        luckText.text = "LUK: " + character.luck;
        currentCharacterIndex = index;

        // 레벨 값의 -1과 비교
        int totalIncrease = character.vitality + character.power + character.agility + character.luck;

        if (totalIncrease == character.level - 1)
        {
            // 모든 버튼 비활성화
            vitalityUpgradeButton.interactable = false;
            powerUpgradeButton.interactable = false;
            agilityUpgradeButton.interactable = false;
            luckUpgradeButton.interactable = false;
        }
        else if (totalIncrease < character.level - 1)
        {
            // 각 속성의 증가량이 5인 경우 해당 버튼 비활성화
            vitalityUpgradeButton.interactable = character.vitality < 5;
            powerUpgradeButton.interactable = character.power < 5;
            agilityUpgradeButton.interactable = character.agility < 5;
            luckUpgradeButton.interactable = character.luck < 5;
        }

        Debug.Log($"Total Increase: {totalIncrease}, Level: {character.level}, Vitality: {character.vitality}, Power: {character.power}, Agility: {character.agility}, Luck: {character.luck}");
    }

    private void LoadCharacterSkills(CharacterData character)
    {
        if (character.skills != null && character.skills.Length > 0)
        {
            foreach (var skill in character.skills)
            {
                if (skill == null || skill.skillName == "none") // 스킬이 null이거나 "none"인 경우
                {
                    Debug.LogWarning("Skill is null or not implemented, setting to null.");
                    // 스킬을 null로 설정
                    continue; // 다음 스킬로 넘어감
                }

                Debug.Log($"Loaded skill: {skill.skillName}");
            }
        }
        else
        {
            Debug.LogWarning("No skills found for this character.");
        }
    }

    public void ShowUpgradePanel()
    {
        characterInfoPanel.SetActive(false); // 캐릭터 정보 창 숨기기
        upgradePanel.SetActive(true);
        LoadUpgradePanel();
    }

    public void LoadUpgradePanel()
    {
        Debug.Log($"Current Character Index: {currentCharacterIndex}");
        Debug.Log($"Characters Array Length: {characters.Length}");

        if (currentCharacterIndex < 0 || currentCharacterIndex >= characters.Length)
        {
            Debug.LogError("Current character index is out of bounds!");
            return;
        }

        CharacterData character = characters[currentCharacterIndex];

        if (character == null)
        {
            Debug.LogError("Character data is null!");
            return;
        }

        // UI 요소가 null인지 확인
        if (upgradeCharacterImage == null || upgradeNameText == null || upgradeLevelText == null ||
            vitalityText == null || powerText == null || agilityText == null || luckText == null)
        {
            Debug.LogError("One or more UI elements are not assigned!");
            return;
        }

        // 캐릭터 정보 로드
        upgradeCharacterImage.sprite = character.characterSprite;
        upgradeNameText.text = character.characterName;
        upgradeLevelText.text = "Level: " + character.level;
        vitalityText.text = "VIT: " + character.vitality;
        powerText.text = "POW: " + character.power;
        agilityText.text = "AGI: " + character.agility;
        luckText.text = "LUK: " + character.luck;

        // 레벨 값의 -1과 비교
        int totalIncrease = character.vitality + character.power + character.agility + character.luck;

        if (totalIncrease == character.level - 1)
        {
            // 모든 버튼 비활성화
            vitalityUpgradeButton.interactable = false;
            powerUpgradeButton.interactable = false;
            agilityUpgradeButton.interactable = false;
            luckUpgradeButton.interactable = false;
        }
        else if (totalIncrease < character.level - 1)
        {
            // 각 속성의 증가량이 5인 경우 해당 버튼 비활성화
            vitalityUpgradeButton.interactable = character.vitality < 5;
            powerUpgradeButton.interactable = character.power < 5;
            agilityUpgradeButton.interactable = character.agility < 5;
            luckUpgradeButton.interactable = character.luck < 5;
        }

        Debug.Log($"Total Increase: {totalIncrease}, Level: {character.level}, Vitality: {character.vitality}, Power: {character.power}, Agility: {character.agility}, Luck: {character.luck}");
    }

    public void IncreaseVitality()
    {
        CharacterData character = characters[currentCharacterIndex];
        if (character.vitality < 5)
        {
            character.vitality++;
            vitalityText.text = "VIT: " + character.vitality;
            SaveCharacterStats(); // 캐릭터 속성 저장
        }

        LoadUpgradePanel();
    }

    public void IncreasePower()
    {
        CharacterData character = characters[currentCharacterIndex];
        if (character.power < 5)
        {
            character.power++;
            powerText.text = "POW: " + character.power;
            SaveCharacterStats(); // 캐릭터 속성 저장
        }

        LoadUpgradePanel();
    }

    public void IncreaseAgility()
    {
        CharacterData character = characters[currentCharacterIndex];
        if (character.agility < 5)
        {
            character.agility++;
            agilityText.text = "AGI: " + character.agility;
            SaveCharacterStats(); // 캐릭터 속성 저장
        }

        LoadUpgradePanel();
    }

    public void IncreaseLuck()
    {
        CharacterData character = characters[currentCharacterIndex];
        if (character.luck < 5)
        {
            character.luck++;
            luckText.text = "LUK: " + character.luck;
            SaveCharacterStats(); // 캐릭터 속성 저장
        }

        LoadUpgradePanel();
    }

    public void CloseUpgradePanel()
    {
        upgradePanel.SetActive(false);       // 업그레이드 창 숨기기
        characterInfoPanel.SetActive(true);  // 캐릭터 정보 창 다시 표시
        LoadCharacter(currentCharacterIndex); // 현재 선택된 캐릭터 정보 다시 로드
    }

    // 캐릭터 속성을 PlayerPrefs에 저장하는 메서드
    private void SaveCharacterStats()
    {
        CharacterData character = characters[currentCharacterIndex];
        PlayerPrefs.SetInt("CharacterLevel_" + currentCharacterIndex, character.level);
        PlayerPrefs.SetInt("CharacterVitality_" + currentCharacterIndex, character.vitality);
        PlayerPrefs.SetInt("CharacterPower_" + currentCharacterIndex, character.power);
        PlayerPrefs.SetInt("CharacterAgility_" + currentCharacterIndex, character.agility);
        PlayerPrefs.SetInt("CharacterLuck_" + currentCharacterIndex, character.luck);
        PlayerPrefs.Save(); // 변경 사항 저장
    }

    /*private void SetCharacterSkills(CharacterData character)
    {
        if (character.characterName == "Gyeonu")
        {
            character.skills = new CharacterSkill[4];

            character.skills[0] = new CharacterSkill
            {
                skillName = "Fireball",
                skillDamage = 20,
                skillCooldown = 5,
                effectRadius = 3.0f,
                effectType = CharacterSkill.EffectType.Damage,
                effectValue = 20
            };

            character.skills[1] = new CharacterSkill
            {
                skillName = "Heal",
                skillDamage = 0,
                skillCooldown = 10,
                effectType = CharacterSkill.EffectType.Heal,
                effectValue = 15
            };

            character.skills[2] = new CharacterSkill
            {
                skillName = "Speed Boost",
                skillDamage = 0,
                skillCooldown = 8,
                effectType = CharacterSkill.EffectType.Buff,
                effectValue = 5
            };

            character.skills[3] = new CharacterSkill
            {
                skillName = "Poison",
                skillDamage = 10,
                skillCooldown = 6,
                effectType = CharacterSkill.EffectType.Debuff,
                effectValue = 10
            };
        }
    }*/
}
