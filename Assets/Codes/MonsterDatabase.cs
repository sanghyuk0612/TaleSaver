using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "MonsterDatabase", menuName = "Monster/Monster Database")]
public class MonsterDatabase : ScriptableObject
{
    [System.Serializable]
    public class MonsterSpawnData
    {
        public string mapName; // 맵 이름
        public List<MonsterData> monsters; // 해당 맵에서 등장하는 몬스터 리스트
    }

    public List<MonsterSpawnData> spawnDataList; // 맵별 몬스터 스폰 데이터 리스트

    public List<MonsterData> GetMonstersForMap(string mapName)
    {
        foreach (var data in spawnDataList)
        {
            if (data.mapName == mapName)
            {
                return data.monsters; // 해당 맵의 몬스터 리스트 반환
            }
        }
        return new List<MonsterData>(); // 해당 맵에 대한 정보가 없으면 빈 리스트 반환
    }
}