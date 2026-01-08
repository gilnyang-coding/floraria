using UnityEngine;

public class MonsterAttack : MonoBehaviour {
    private Animator animator;
    [Header("Attack Settings")]
    [SerializeField] private float attackDamage = 10f; // 공격력
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float erosionAmount = 10f; // 공격 한 번당 침식도 10 증가

    private float lastAttackTime;
    private Transform player;

    void Awake() {
        animator = GetComponent<Animator>();
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    public bool CanAttack() {
        return Time.time >= lastAttackTime + attackCooldown;
    }

    public void ExecuteAttack() {
        lastAttackTime = Time.time;
        animator.SetTrigger("attack1");

        // 실제 데미지 판정 로직
        if (player != null && Vector3.Distance(transform.position, player.position) <= attackRange) {
            //플레이어의 구체적인 클래스를 몰라도 인터페이스로 접근 가능
            IDamageable target = player.GetComponent<IDamageable>();
            if (target != null) {
                target.TakeDamage(attackDamage);
            }

            PlayerErosion erosion = player.GetComponent<PlayerErosion>();
            if (erosion != null) {
                erosion.AddErosion(erosionAmount);
            }
        }
    }
}