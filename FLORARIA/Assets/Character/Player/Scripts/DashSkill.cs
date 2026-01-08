using UnityEngine;

public class DashSkill : SkillBase {
    [SerializeField] private float dashSpeed = 10f;
    [SerializeField] private float dashDuration = 0.2f;
    
    private Rigidbody rb;
    private Animator animator;
    private Hydration hydration;

    void Awake() {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        hydration = GetComponent<Hydration>();
    }

    public override void Execute(Vector3 targetPos) {
        if (!canUse) return;
        
        // 대쉬 시 수분 소모
        if (hydration != null) {
            hydration.OnDash();
        }
        
        StartCoroutine(DashRoutine(targetPos));
        StartCoroutine(CooldownRoutine());
    }

    private System.Collections.IEnumerator DashRoutine(Vector3 targetPos) {
        IsActive = true;
        if (animator != null) animator.SetBool("Dash_F", true);
        
        // 이미 컨트롤러에서 계산된 안정적인 방향을 사용합니다.
        Vector3 dir = (targetPos - transform.position).normalized;
        dir.y = 0;

        rb.linearVelocity = dir * dashSpeed;

        yield return new WaitForSeconds(dashDuration);

        // 대쉬 종료 시 속도 초기화
        rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        
        if (animator != null) animator.SetBool("Dash_F", false);
        IsActive = false; 
    }
}
