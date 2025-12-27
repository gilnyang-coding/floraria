using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;        // 따라갈 대상 (캐릭터)
    public Vector3 offset;         // 캐릭터와 카메라 사이의 거리
    public float smoothSpeed = 5f; // 따라가는 부드러운 정도

    void LateUpdate()
    {
        if (target == null) return;

        // 카메라가 가야 할 목표 위치 계산
        Vector3 desiredPosition = target.position + offset;
        
        // 목표 위치로 부드럽게 이동 (Lerp)
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        
        transform.position = smoothedPosition;
    }
}