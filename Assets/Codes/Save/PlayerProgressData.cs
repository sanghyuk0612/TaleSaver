using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine;

[System.Serializable]
public class PlayerProgressData
{
    public float playTime;   // 플레이 시간 (초 단위)
    public int stageIndex;   // 현재 스테이지 인덱스

    public PlayerProgressData(float playTime, int stageIndex)
    {
        this.playTime = playTime;
        this.stageIndex = stageIndex;
    }
}
