using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;        // 따라갈 대상 (플레이어)
    public float smoothSpeed = 5f;  // 카메라 이동 부드러움 정도
    public Vector3 offset;         // 카메라와 플레이어 사이의 거리'
    void LateUpdate()
    {
        if (target == null)
            return;

        // 목표 위치 계산 (카메라는 z축 위치 유지)
        Vector3 desiredPosition = target.position + offset;
        desiredPosition.z = transform.position.z;  // 카메라 z위치 유지

        // 부드러운 이동
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.position = smoothedPosition;
    }
}
