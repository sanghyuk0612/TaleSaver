using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreBoardBootstrap : MonoBehaviour
{
    void Start()
    {
        if (RankingManager.Instance != null)
        {
            RankingManager.Instance.TryReconnectRankingUI(); // ? ScoreBoard 진입 시 UI 재연결
        }
        else
        {
            Debug.LogWarning("? RankingManager.Instance가 존재하지 않습니다.");
        }
    }
}