using UnityEngine;
using System.Collections;

public class PlayerAttack : SkillBase {
    [Header("Attack Settings")]
    [SerializeField] private float damage = 20f;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float attackRadius = 1.0f; // 공격 판정 너비
    [SerializeField] private LayerMask enemyLayer;

    private Animator animator;

    void Awake() {
        animator = GetComponent<Animator>();
    }

    public override void Execute(Vector3 targetPos) {
        if (!canUse) return;

        // 1. 공격 애니메이션 실행
        IsActive = true;
        if (animator != null) {
            animator.SetTrigger("attack1");
        }

        // 2. 쿨타임 시작
        StartCoroutine(CooldownRoutine());
        
        // 3. 애니메이션이 끝날 즈음 상태 해제 (약 0.5초 후, 또는 애니메이션 길이에 맞춰 조절)
        Invoke("EndAttack", 0.5f);
    }

    private void EndAttack() {
        IsActive = false;
    }

    //애니메이션 이벤트에서 호출할 실제 데미지 판정 함수
    public void OnAttackHit() {
        // 플레이어 정면으로 구체 형태의 판정을 쏴서 적을 찾습니다.
        Collider[] hitEnemies = Physics.OverlapSphere(transform.position + transform.forward, attackRadius, enemyLayer);

        foreach (Collider enemy in hitEnemies) {
            IDamageable damageable = enemy.GetComponent<IDamageable>();
            if (damageable != null) {
                damageable.TakeDamage(damage); // 우리가 만든 인터페이스 호출
                Debug.Log($"{enemy.name}에게 {damage}의 데미지를 입혔습니다!");
            }
        }
    }
}