using UnityEngine;

public class CameraFollow : MonoBehaviour {
    public Transform target;
    public Vector3 offset;
    public float smoothSpeed = 5f;

    [Header("Zoom Settings")]
    public float normalFOV = 55f;
    public float dashFOV = 65f;
    public float zoomSpeed = 5f;

    private Camera mainCamera;
    private DashSkill dashSkill; // PlayerMove 대신 DashSkill 참조
    //카메라는 이동을 알 필요는 없음. 대쉬의 여부만 알면 됨

    void Start() {
        mainCamera = GetComponent<Camera>();
        if (target != null) {
            // 타겟에서 DashSkill 컴포넌트를 찾음
            dashSkill = target.GetComponent<DashSkill>();
        }
    }

    void LateUpdate() {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.position = smoothedPosition;
        HandleZoom();
    }

    void HandleZoom() {
        if (mainCamera == null) return;

        // dashSkill이 존재하고, 현재 실행 중(IsActive)인지 확인
        bool isDashing = (dashSkill != null && dashSkill.IsActive);
        
        float targetFov = isDashing ? dashFOV : normalFOV;
        mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, targetFov, zoomSpeed * Time.deltaTime);
    }
}