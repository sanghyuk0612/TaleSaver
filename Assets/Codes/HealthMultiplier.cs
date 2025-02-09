using UnityEngine;

[CreateAssetMenu(fileName = "HealthMultiplier", menuName = "ScriptableObjects/HealthMultiplier", order = 1)]
public class HealthMultiplier : ScriptableObject
{
    [Header("Stage Multipliers")]
    public float[] stageMultipliers; // 스테이지별 체력 비율

    [Header("Chapter Multipliers")]
    public float[] chapterMultipliers; // 챕터별 체력 비율

    // 기본 체력 비율
    public float baseHealthMultiplier = 1.0f;

    private void OnEnable()
    {
        // 스테이지별 체력 비율 초기화
        stageMultipliers = new float[] { 1.0f, 1.0f, 1.0f, 1.2f, 1.2f, 1.4f, 1.4f, 1.6f, 1.6f, 1.8f, 1.0f }; // 스테이지 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 (0과 10은 사용하지 않음)

        // 챕터별 체력 비율 초기화
        chapterMultipliers = new float[] { 1.0f, 1.0f, 2.0f, 3.0f }; // 챕터 0, 1, 2, 3
    }

    // 스테이지와 챕터에 따른 체력 비율을 반환하는 메서드
    public float GetHealthMultiplier(int stage, int chapter)
    {
        float stageMultiplier = stageMultipliers[stage];
        float chapterMultiplier = chapterMultipliers[chapter];

        return stageMultiplier * chapterMultiplier;
    }
}