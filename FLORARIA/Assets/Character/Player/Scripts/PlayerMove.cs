using UnityEngine;

public class PlayerMove : MonoBehaviour {
    [SerializeField] private float walkSpeed = 1f;
    [SerializeField] private float stoppingDistance = 0.1f;
    
    private Rigidbody rb;
    private Animator animator;
    private bool isMoving = false;

    void Awake() {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        if (rb != null) rb.freezeRotation = true;
    }

    public void MoveTo(Vector3 targetPos) {
        Vector3 currentPos = transform.position;
        Vector3 targetPosXZ = new Vector3(targetPos.x, currentPos.y, targetPos.z);
        float distance = Vector3.Distance(currentPos, targetPosXZ);

        // 1. 최소 정지 거리보다 멀 때만 이동 로직 실행
        if (distance > stoppingDistance) {
            isMoving = true;
            Vector3 direction = (targetPosXZ - currentPos).normalized;
            
            // [핵심] 방향 벡터가 충분히 크지 않으면(0.01 이상) 연산을 무시합니다.
            // 90도 돌아가는 현상은 direction이 (0.00001, 0, 0) 같은 값을 가질 때 발생하기 때문입니다.
            if (direction.sqrMagnitude > 0.01f) {
                rb.linearVelocity = new Vector3(direction.x * walkSpeed, rb.linearVelocity.y, direction.z * walkSpeed);
                
                // [추가 안전장치] 회전할 방향(LookRotation)이 Zero Vector가 아닐 때만 회전 수행
                if (direction != Vector3.zero) {
                    Quaternion lookRotation = Quaternion.LookRotation(direction);
                    transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.fixedDeltaTime * 10f);
                }
            }
        } else {
            Stop();
        }
        UpdateAnimation();
    }

    public void Stop() {
        isMoving = false;
        if (rb != null) {
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        }
        UpdateAnimation(); // IsMoving을 false로 전달
    }

    private void UpdateAnimation() {
        if (animator != null) {
            animator.SetBool("IsMoving", isMoving);
        }
    }

    public bool GetIsMoving() {
        return isMoving;
    }
}