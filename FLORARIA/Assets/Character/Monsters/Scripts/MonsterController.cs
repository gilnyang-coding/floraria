using UnityEngine;

public class MonsterController : MonoBehaviour
{
    private enum State { Chase, Attack }
    [SerializeField] private State currentState = State.Chase;

    private MonsterMove moveAction;
    private MonsterAttack attackAction;
    private Transform player;

    [SerializeField] private float attackRange = 2f; // 이 거리 안으로 들어오면 공격

    void Start() {
        moveAction = GetComponent<MonsterMove>();
        attackAction = GetComponent<MonsterAttack>();
        // "Player" 태그를 가진 오브젝트를 무조건 타겟팅합니다.
        player = GameObject.FindGameObjectWithTag("Player").transform; 
    }

    void Update() {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        // 거리 기반 탐지 로직을 제거하고, 공격 사거리만 체크합니다.
        if (distance <= attackRange) {
            currentState = State.Attack;
        }
        else {
            currentState = State.Chase;
        }

        // 상태별 실행 로직
        switch (currentState) {
            case State.Chase:
                moveAction.MoveTo(player.position); 
                break;
                
            case State.Attack:
                moveAction.Stop(); // 공격 시에는 이동 중지
                if (attackAction.CanAttack()) {
                    attackAction.ExecuteAttack(); 
                }
                break;
        }
    }
}