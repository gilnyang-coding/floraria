using UnityEngine;

public class MonsterAttack : MonoBehaviour {
    private Animator animator;
    [SerializeField] private float attackCooldown = 2f;
    private float lastAttackTime;

    void Awake() => animator = GetComponent<Animator>();

    public bool CanAttack() => Time.time >= lastAttackTime + attackCooldown;

    public void ExecuteAttack() {
        lastAttackTime = Time.time;
        // Any State에서 연결된 attack1 트리거 실행
        animator.SetTrigger("attack1"); 
    }
}