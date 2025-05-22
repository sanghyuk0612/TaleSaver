using UnityEngine;
using UnityEngine.UI;

public class SkillUIManager : MonoBehaviour
{
    public static SkillUIManager Instance { get; private set; }

    [System.Serializable]
    public class SkillSlot
    {
        public Image iconImage;         // 스킬 아이콘
        public Image cooldownMask;      // 어두워지는 이미지 (Image Type: Filled)
        public float cooldownTime;      // 쿨타임 설정
        [HideInInspector] public float currentCooldown;
        [HideInInspector] public bool isCoolingDown;
        [HideInInspector] public CharacterSkill assignedSkill; // ★ 스킬 참조 저장
    }

    public SkillSlot[] skillSlots = new SkillSlot[4]; // 4개의 슬롯 연결
    public CharacterData currentCharacter;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬 전환에도 유지
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (currentCharacter != null)
            SetCharacterSkills(currentCharacter);
        else
            Debug.Log("current character is null");
    }
    
    public void SetCharacterSkills(CharacterData character)
    {
        currentCharacter = character;

        for (int i = 0; i < skillSlots.Length; i++)
        {
            if (i >= character.skills.Length) continue;

            CharacterSkill skill = character.skills[i];
            SkillSlot slot = skillSlots[i];

            slot.cooldownTime = skill.skillCooldown;
            slot.currentCooldown = 0f;
            slot.cooldownMask.fillAmount = 0f;
            slot.isCoolingDown = false;
            slot.assignedSkill = skill; // ✅ 슬롯에 스킬 저장

            if (slot.iconImage != null)
                slot.iconImage.sprite = skill.skillImage;
            if (slot.cooldownMask != null)
                slot.cooldownMask.sprite = skill.skillImage;
        }
    }



    public void TriggerSkillCooldown(CharacterSkill skill)
    {
        for (int i = 0; i < skillSlots.Length; i++)
        {
            SkillSlot slot = skillSlots[i];

            // ✅ 슬롯에 저장된 스킬과 일치할 때만 쿨타임 시작
            if (slot.assignedSkill == skill)
            {
                slot.currentCooldown = slot.cooldownTime;
                slot.cooldownMask.fillAmount = 1f;
                slot.isCoolingDown = true;

                Debug.Log($"쿨타임 시작: {skill.skillName} (Slot {i})");
                return;
            }
        }

        Debug.LogWarning($"쿨다운 실패: {skill.skillName} 슬롯을 못 찾음");
    }


    public void UpdateCooldown(int index)
    {
        SkillSlot slot = skillSlots[index];
        if (!slot.isCoolingDown) return;

        slot.currentCooldown -= Time.deltaTime;
        float ratio = slot.currentCooldown / slot.cooldownTime;
        slot.cooldownMask.fillAmount = Mathf.Clamp01(ratio);

        if (slot.currentCooldown <= 0f)
        {
            slot.cooldownMask.fillAmount = 0f;
            slot.isCoolingDown = false;
        }
    }
}