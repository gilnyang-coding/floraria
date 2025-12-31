using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections; // 코루틴 사용을 위해 필요

public class PlayerMove : MonoBehaviour 
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 1f;
    [SerializeField] private float dashSpeed = 10f; // 대쉬는 순간적으로 빨라야 하므로 값을 높였습니다.
    [SerializeField] private float dashDuration = 0.2f; // 대쉬가 지속되는 시간 (초)
    [SerializeField] private float dashCooldown = 5f;   // 대쉬 재사용 대기시간 (초)
    [SerializeField] private float stoppingDistance = 0.1f;
    
    [Header("Raycast Settings")]
    [SerializeField] private LayerMask groundLayer = -1;
    [SerializeField] private float maxRaycastDistance = 100f;
    
    private Camera mainCamera;
    private Vector3 targetPosition;
    public bool isMoving = false;
    
    [Header("Dash States")]
    public bool dash_F = false;      // 애니메이션 파라미터용
    private bool isDashing = false;  // 현재 대쉬 중인지 체크
    private bool canDash = true;     // 쿨타임 체크용
    
    private Animator animator;
    private Rigidbody rb;

    void Start() {
        mainCamera = Camera.main;
        if (mainCamera == null) mainCamera = FindFirstObjectByType<Camera>();
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        if (rb != null) rb.freezeRotation = true;
    }

    void Update() {
        // 대쉬 입력 (한 번 눌렀을 때 실행)
        if (Keyboard.current != null && Keyboard.current.leftShiftKey.wasPressedThisFrame) {
            if (canDash && isMoving) { // 움직이고 있을 때만 대쉬 가능
                StartCoroutine(PerformDash());
            }
        }
        
        // 우클릭 이동
        if (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame) {
            MoveToMousePosition();
        }

        if (animator != null) {
            animator.SetBool("IsMoving", isMoving);
            animator.SetBool("Dash_F", dash_F); 
        }
    }

    void FixedUpdate() {
        if (isMoving && !isDashing) { // 대쉬 중이 아닐 때만 일반 이동 처리
            MoveToTarget();
        }
    }
    
    // [핵심 로직] 대쉬 실행 및 쿨타임 코루틴
    IEnumerator PerformDash() {
        canDash = false;    // 쿨타임 시작
        isDashing = true;   // 대쉬 상태 시작
        dash_F = true;      // 애니메이션 켜기

        // 대쉬 방향 결정 (현재 바라보는 방향 혹은 이동 방향)
        Vector3 dashDirection = (targetPosition - transform.position).normalized;
        dashDirection.y = 0;

        // 대쉬 속도 적용
        rb.linearVelocity = dashDirection * dashSpeed;

        yield return new WaitForSeconds(dashDuration); // 대쉬 지속 시간만큼 대기

        dash_F = false;     // 애니메이션 끄기
        isDashing = false;  // 대쉬 상태 끝
        
        // 5초 쿨타임 대기
        Debug.Log("대쉬 쿨타임 시작 (5초)");
        yield return new WaitForSeconds(dashCooldown);
        
        canDash = true;     // 다시 대쉬 가능
        Debug.Log("대쉬 준비 완료!");
    }
    
    void MoveToMousePosition() {
        if (Mouse.current == null) return;
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Ray ray = mainCamera.ScreenPointToRay(mousePosition);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, maxRaycastDistance, groundLayer)) {
            targetPosition = hit.point;
            isMoving = true;
        }
    }
    
void MoveToTarget() {
    Vector3 currentPos = transform.position;
    Vector3 targetPosXZ = new Vector3(targetPosition.x, currentPos.y, targetPosition.z);
    float distance = Vector3.Distance(currentPos, targetPosXZ);
    
    if (distance > stoppingDistance) {
        float currentSpeed = dash_F ? dashSpeed : walkSpeed;
        Vector3 direction = (targetPosXZ - currentPos).normalized;

        // [개선된 부분] 
        // 단순히 속도를 덮어씌우는 게 아니라, 
        // 캡슐 콜라이더가 돌을 타고 미끄러지도록 물리 엔진의 자연스러운 반작용을 허용합니다.
        Vector3 velocity = direction * currentSpeed;
        
        // Y축은 중력을 유지하고, XZ축은 목표 방향으로 힘을 줍니다.
        // 이때 마찰력이 0인 Physic Material이 있다면 부드럽게 비벼집니다.
        rb.linearVelocity = new Vector3(velocity.x, rb.linearVelocity.y, velocity.z);
        
        if (direction.magnitude > 0.1f) {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.fixedDeltaTime * 10f);
        }
    } else {
            isMoving = false;
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            transform.position = targetPosXZ;
        }
    }

    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip walkSound;
    [SerializeField] private AudioClip dashSound;

    public void PlayFootstep() {
        if (!isMoving || audioSource == null) return;
        AudioClip clipToPlay = isDashing ? dashSound : walkSound;
        if (clipToPlay != null) audioSource.PlayOneShot(clipToPlay);
    }
}