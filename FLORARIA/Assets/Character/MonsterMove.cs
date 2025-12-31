using UnityEngine;
using UnityEngine.AI; // 내비게이션 기능을 쓰기 위해 반드시 필요합니다.

public class MonsterMove : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField] private string playerTag = "Player";
    private Transform playerTransform;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 2f; // NavMeshAgent의 속도로 제어됩니다.
    [SerializeField] private float stoppingDistance = 1.5f;
    
    private Rigidbody rb;
    private Animator animator;
    private NavMeshAgent agent; // 길찾기 AI 컴포넌트

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();

        // 물리 설정: 길찾기 AI가 이동을 주도하므로 Rigidbody의 직접 제어는 최소화합니다.
        if (rb != null) 
        {
            rb.freezeRotation = true;
            // NavMesh와 Rigidbody가 충돌하지 않도록 IsKinematic을 켜주는 것이 안정적입니다.
            rb.isKinematic = true; 
        }

        // NavMeshAgent 초기 설정
        if (agent != null)
        {
            agent.speed = moveSpeed;
            agent.stoppingDistance = stoppingDistance;
            // 에이전트가 직접 회전하게 할지, 코드로 할지 결정 (여기선 코드로 부드럽게 처리)
            agent.updateRotation = false; 
        }

        GameObject player = GameObject.FindGameObjectWithTag(playerTag);
        if (player != null) playerTransform = player.transform;
    }

    void Update()
    {
        if (playerTransform == null || agent == null) return;

        // 매 프레임 플레이어의 위치를 목적지로 갱신 (가장 빠른 길 계산)
        agent.SetDestination(playerTransform.position);

        // 현재 실제로 움직이고 있는지 체크 (남은 거리가 정지 거리보다 클 때)
        bool isActuallyMoving = agent.remainingDistance > agent.stoppingDistance;

        if (isActuallyMoving)
        {
            RotateTowardsPath();
            UpdateAnimation(true);
        }
        else
        {
            UpdateAnimation(false);
        }
    }

    void RotateTowardsPath()
    {
        // agent.desiredVelocity는 AI가 다음에 가야 할 길의 방향을 알려줍니다.
        Vector3 direction = agent.desiredVelocity.normalized;
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 8f);
        }
    }

    void UpdateAnimation(bool isMoving)
    {
        if (animator != null)
        {
            // 거미 애니메이터의 파라미터 이름에 맞춰 수정하세요. (예: "walk")
            animator.SetBool("IsMoving", isMoving);
        }
    }
}