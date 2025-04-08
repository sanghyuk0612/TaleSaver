using UnityEngine;
using UnityEngine.UI;

public class StageUIController : MonoBehaviour
{
    public Text NowStage;
    public Text PlayTime;

    private float playTime = 0f;

    private readonly string[] stages = {
        "0", "Stage 1", "Stage 2", "Stage 3", "Stage 4", 
        "Stage 5", "Stage 6", "Stage 7", "Stage 8", 
        "Stage 9", "Boss"
    };

    void Start()
    {
        var progress = SaveManager.Instance.LoadProgressData();
        if (progress != null)
        {
            playTime = progress.playTime;
            GameManager.Instance.Stage = progress.stageIndex;
        }

        UpdateStageText();
    }

    void Update()
    {
        playTime += Time.deltaTime;

        int minutes = Mathf.FloorToInt(playTime / 60f);
        int seconds = Mathf.FloorToInt(playTime % 60f);
        PlayTime.text = $"Time: {minutes:00}:{seconds:00}";

        UpdateStageText(); // 매 프레임마다 스테이지 이름 업데이트
    }

    public void NextStage()
    {
        GameManager.Instance.Stage++;
        SaveProgress();
    }

    private void UpdateStageText()
    {
        int stageIndex = GameManager.Instance.Stage;

        if (stageIndex >= 0 && stageIndex < stages.Length)
        {
            NowStage.text = stages[stageIndex];
        }
        else
        {
            NowStage.text = "Unknown";
        }
    }

    private void SaveProgress()
    {
        int stageIndex = GameManager.Instance.Stage;
        PlayerProgressData data = new PlayerProgressData(playTime, stageIndex);
        SaveManager.Instance.SaveProgressData(data);
    }
}
