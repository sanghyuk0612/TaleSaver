using UnityEngine;

public class StorePlayerSpawner : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab; // 플레이어 프리팹
    [SerializeField] private CameraFollow cameraFollow; // CameraFollow 스크립트 참조

    private void Start()
    {
        // 플레이어 오브젝트 생성
        GameObject playerObject = Instantiate(playerPrefab, new Vector3(-5, 0, 0), Quaternion.identity);
        
        // CameraFollow의 target을 소환된 플레이어로 설정
        if (cameraFollow != null)
        {
            cameraFollow.target = playerObject.transform; // 카메라가 플레이어를 따라가도록 설정
        }
        
        // GameManager에서 플레이어 상태 복원
        IDamageable playerController = playerObject.GetComponent<IDamageable>();
        GameManager.Instance.RestorePlayerState(playerController);
    }
}