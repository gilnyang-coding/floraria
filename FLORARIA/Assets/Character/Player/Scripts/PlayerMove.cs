using UnityEngine;
using System.Collections;

public class PlayerMove : MonoBehaviour {
    [SerializeField] private float walkSpeed = 1f;
    [SerializeField] private float stoppingDistance = 0.1f;
    
    private Rigidbody rb;
    private Animator animator;
    private Hydration hydration;
    private bool isMoving = false;
    
    // 일시적인 속도 배율 (디버프/버프용)
    private float temporarySpeedMultiplier = 1f;
    private Coroutine speedMultiplierCoroutine;

    void Awake() {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        hydration = GetComponent<Hydration>();
        if (rb != null) rb.freezeRotation = true;
    }

    /// <summary>
    /// 현재 실제 이동속도 (수분 상태 및 일시적 배율 반영)
    /// </summary>
    private float CurrentSpeed {
        get {
            float speed = walkSpeed;
            // 수분 부족 시 이동속도 감소 적용
            if (hydration != null) {
                speed *= hydration.SpeedMultiplier;
            }
            // 일시적인 속도 배율 적용
            speed *= temporarySpeedMultiplier;
            return speed;
        }
    }
    
    /// <summary>
    /// 일시적인 속도 배율 추가 (디버프/버프용)
    /// </summary>
    /// <param name="multiplier">속도 배율 (예: 0.8 = 20% 감소)</param>
    /// <param name="duration">지속 시간 (초)</param>
    public void AddSpeedMultiplier(float multiplier, float duration) {
        // 기존 코루틴이 있으면 중지
        if (speedMultiplierCoroutine != null) {
            StopCoroutine(speedMultiplierCoroutine);
        }
        
        // 배율 적용
        temporarySpeedMultiplier = multiplier;
        
        // 일정 시간 후 원래대로 복구
        speedMultiplierCoroutine = StartCoroutine(ResetSpeedMultiplierAfterDelay(duration));
    }
    
    /// <summary>
    /// 일정 시간 후 속도 배율을 원래대로 복구
    /// </summary>
    private IEnumerator ResetSpeedMultiplierAfterDelay(float delay) {
        yield return new WaitForSeconds(delay);
        temporarySpeedMultiplier = 1f;
        speedMultiplierCoroutine = null;
        Debug.Log("[PlayerMove] 일시적 속도 배율이 원래대로 복구되었습니다.");
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
                float speed = CurrentSpeed; // 수분 상태 반영된 속도
                rb.linearVelocity = new Vector3(direction.x * speed, rb.linearVelocity.y, direction.z * speed);
                
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
