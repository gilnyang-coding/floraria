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

        if (distance > stoppingDistance) {
            isMoving = true;
            Vector3 direction = (targetPosXZ - currentPos).normalized;
            Vector3 velocity = direction * walkSpeed;
            
            // 물리 기반 이동 적용 (중력 유지)
            rb.linearVelocity = new Vector3(velocity.x, rb.linearVelocity.y, velocity.z);
            
            // 방향 전환
            if (direction.magnitude > 0.1f) {
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.fixedDeltaTime * 10f);
            }
        } else {
            Stop();
        }
        UpdateAnimation();
    }

    public void Stop() {
        isMoving = false;
        rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        UpdateAnimation();
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