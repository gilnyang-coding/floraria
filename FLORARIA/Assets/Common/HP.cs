using UnityEngine;

public class Health : MonoBehaviour, IDamageable {
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;

    [Header("Animation Settings")]
    [SerializeField] private string deathTriggerName = "death1";

    private Animator animator;
    public bool isDead { get; private set; }

    void Awake() {
        animator = GetComponent<Animator>();
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount) {
        if (isDead) return;

        currentHealth -= amount;
        Debug.Log($"{gameObject.name} 데미지 발생. 남은 HP: {currentHealth}");

        if (currentHealth <= 0) {
            Die();
        }
    }

    public void Heal(float amount) {
        if (isDead) return;

        // 음수 입력을 방지하고 최대 체력을 넘지 않도록 보정
        float healValue = Mathf.Max(0f, amount);
        currentHealth = Mathf.Min(currentHealth + healValue, maxHealth);

        Debug.Log($"{gameObject.name} 체력 회복: +{healValue}, 현재 HP: {currentHealth}/{maxHealth}");
    }

    private void Die() {
        if (isDead) return;
        isDead = true;

        if (animator != null) {
            animator.SetTrigger(deathTriggerName);
        }

        //죽는 순간 다른 로직들을 끔.
        
        // 1. 플레이어 컨트롤러 비활성화 (마우스/키보드 입력 차단)
        var controller = GetComponent<PlayerController>();
        if (controller != null) {
            controller.enabled = false;
        }

        // 2. 이동 로직 정지 및 비활성화 (물리 이동 차단)
        var move = GetComponent<PlayerMove>();
        if (move != null) {
            move.Stop(); // 속도를 0으로 만듦
            move.enabled = false;
        }

        // 3. (몬스터의 경우) AI 로직 끄기
        var monsterCtrl = GetComponent<MonsterController>();
        if (monsterCtrl != null) monsterCtrl.enabled = false;

        Debug.Log($"{gameObject.name}의 모든 로직이 정지되었습니다.");
    }
}