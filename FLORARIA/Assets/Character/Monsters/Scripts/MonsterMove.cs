using UnityEngine;
using UnityEngine.AI;

public class MonsterMove : MonoBehaviour
{
    private NavMeshAgent agent;
    private Animator animator;
    private float currentLoco, currentTurn;

    void Awake() {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        // NavMeshAgent가 자동으로 회전하지 않게 하고 스크립트에서 제어합니다.
        agent.updateRotation = false; 
    }

    public void MoveTo(Vector3 target) {
        agent.isStopped = false;
        agent.SetDestination(target);
        
        Vector3 velocity = agent.desiredVelocity;
        if (velocity.magnitude > 0.1f) {
            // 부드러운 회전 처리
            Quaternion targetRot = Quaternion.LookRotation(velocity.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 8f);
            
            // 블렌드 트리용 turning 값 계산(블랜드에서 좀 더 자연스럽게 모션을 만들어주는 역할)
            float turnValue = transform.InverseTransformDirection(velocity.normalized).x;
            UpdateAnimation(1.0f, turnValue);
        }
    }

    public void Stop() {
        agent.isStopped = true;
        UpdateAnimation(0.0f, 0.0f);
    }

    private void UpdateAnimation(float loco, float turn) {
        currentLoco = Mathf.Lerp(currentLoco, loco, Time.deltaTime * 5f);
        currentTurn = Mathf.Lerp(currentTurn, turn, Time.deltaTime * 5f);
        
        // 애니메이터 파라미터 이름과 정확히 매칭
        animator.SetFloat("locomotion", currentLoco); 
        animator.SetFloat("turning", currentTurn);
    }
}