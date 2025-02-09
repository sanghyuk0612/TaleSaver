using UnityEngine;

[CreateAssetMenu(fileName = "DamageMultiplier", menuName = "ScriptableObjects/DamageMultiplier", order = 1)]
public class DamageMultiplier : ScriptableObject
{
    [Header("Stage Multipliers")]
    public float[] stageMultipliers; // 스테이지별 공격력 비율

    [Header("Chapter Multipliers")]
    public float[] chapterMultipliers; // 챕터별 공격력 비율

    // 기본 공격력 비율
    public float baseDamageMultiplier = 1.0f;

    private void OnEnable()
    {
        // 스테이지별 공격력 비율 초기화
        stageMultipliers = new float[] { 1.0f, 1.0f, 1.0f, 1.05f, 1.05f, 1.1f, 1.1f, 1.15f, 1.15f, 1.25f, 1.0f }; // 스테이지 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 (0과 10은 사용하지 않음)

        // 챕터별 공격력 비율 초기화
        chapterMultipliers = new float[] { 1.0f, 1.0f, 1.44f, 1.68f }; // 챕터 0, 1, 2, 3
    }

    // 스테이지와 챕터에 따른 공격력 비율을 반환하는 메서드
    public float GetDamageMultiplier(int stage, int chapter)
    {
        float stageMultiplier = stageMultipliers[stage];
        float chapterMultiplier = chapterMultipliers[chapter];

        return stageMultiplier * chapterMultiplier;
    }
}