using UnityEngine;

public class DashSkill : SkillBase {
    [SerializeField] private float dashSpeed = 10f;
    [SerializeField] private float dashDuration = 0.2f;
    
    private Rigidbody rb;
    private Animator animator;

    void Awake() {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
    }

    public override void Execute(Vector3 targetPos) {
        if (!canUse) return;
        StartCoroutine(DashRoutine(targetPos));
        StartCoroutine(CooldownRoutine());
    }

    private System.Collections.IEnumerator DashRoutine(Vector3 targetPos) {
        IsActive = true; // 상태 활성화
        if (animator != null) animator.SetBool("Dash_F", true);
        
        Vector3 dir = (targetPos - transform.position).normalized;
        dir.y = 0;
        rb.linearVelocity = dir * dashSpeed;

        yield return new WaitForSeconds(dashDuration);

        if (animator != null) animator.SetBool("Dash_F", false);
        IsActive = false; // 상태 비활성화
    }
}