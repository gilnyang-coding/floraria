using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMove : MonoBehaviour 
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 1f;
    [SerializeField] private float runSpeed = 3f;
    [SerializeField] private float stoppingDistance = 0.1f;
    
    [Header("Raycast Settings")]
    [SerializeField] private LayerMask groundLayer = -1;
    [SerializeField] private float maxRaycastDistance = 100f;
    
    private Camera mainCamera;
    private Vector3 targetPosition;
    public bool isMoving = false;
    public bool isRunning = false;
    private Animator animator;

    void Start() {
        mainCamera = Camera.main;
        if (mainCamera == null) {
            mainCamera = FindFirstObjectByType<Camera>();
        }

        animator = GetComponent<Animator>();
    }
    
    void Update() {
        if (Keyboard.current != null) {
            isRunning = Keyboard.current.leftShiftKey.isPressed;
        }
        
        // 우클릭 감지=> 1초에 수 십번 호출되는 이벤트
        if (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame) {
            MoveToMousePosition();
        }
        
        // 이동 처리
        if (isMoving) {
            MoveToTarget();
        }

        if (animator != null) {
            animator.SetBool("IsMoving", isMoving);
            animator.SetBool("IsRunning", isRunning && isMoving);
        }
    }
    
    void MoveToMousePosition() {
        // 마우스 위치에서 레이캐스트
        if (Mouse.current == null) return;
        
        //Mouse.current: 현재 활성화된 마우스
        //.position.ReadValue(): 그 마우스가 찍은 위치의 값을 읽는다.
        //Vector2: 2차원 벡터
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        //ScreenPointToRay: 카메라의 시점에서 마우스가 찍은 위치를 향하는 레이를 만든다.
        Ray ray = mainCamera.ScreenPointToRay(mousePosition);
        //RaycastHit: 레이가 충돌한 객체의 정보를 담고 있다.
        RaycastHit hit;
        
        //maxRaycastDistance: 레이가 충돌할 수 있는 최대 거리
        //groundLayer: 레이가 충돌할 수 있는 레이어
        if (Physics.Raycast(ray, out hit, maxRaycastDistance, groundLayer)) {
            targetPosition = hit.point;
            isMoving = true;
        }
    }
    
    void MoveToTarget() {
        // 목표 위치까지의 거리 계산 Distance(현재, 목표)
        float distance = Vector3.Distance(transform.position, targetPosition);
        
        if (distance > stoppingDistance) {
            float speed = isRunning ? runSpeed : walkSpeed;
            // 목표 위치로 이동
            //(targetPosition - transform.position): 현재에서 목표까지의 거리와 방향을 나타내는 벡터가 생성됨(물리학에서 많이 보는 거)
            //.normalized: 벡터의 크기를 1로 만든다.(물리학의 i, j, k 벡터)
            Vector3 direction = (targetPosition - transform.position).normalized;
            // s = v*t (s, v는 벡터량) 시간은 프레임 단위로 계산되기 때문에 Time.deltaTime을 곱해줘 유니티가 알아서 잘 처리하게 해준다.
            transform.position += direction * speed * Time.deltaTime;
            
            // 이동 방향으로 회전 (Y축만)
            //direction.magnitude > 0.1f: 벡터의 크기를 계산해서 0.1보다 크면 회전
            if (direction.magnitude > 0.1f) {
                //y축 고정, x,z축만 회전하면 좌우로만 회전할 수 있음.
                Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
                //Slerp: 현재 회전에서 목표 회전까지 부드럽게 회전하게 해주는 함수
                //Time.deltaTime * 10f: 돌아가는 빠르기를 정한다.
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 10f);
            }
        }
        else {
            // 목표 위치에 도달
            isMoving = false;
            transform.position = targetPosition;
        }
    }

    [Header("Audio Settings")]
[SerializeField] private AudioSource audioSource;
[SerializeField] private AudioClip walkSound;
[SerializeField] private AudioClip runSound;

public void PlayFootstep() 
{
    // 로그를 찍어서 이벤트가 오는지 먼저 확인하세요!
    Debug.Log("발소리 이벤트 신호 들어옴!");

    if (!isMoving || audioSource == null) return;

    // clip을 변경하지 않고 바로 OneShot으로 재생 (더 안전함)
    AudioClip clipToPlay = isRunning ? runSound : walkSound;
    
    if (clipToPlay != null)
    {
        audioSource.PlayOneShot(clipToPlay);
    }
}
    
    // 디버그용: Scene 뷰에서 목표 위치 표시
    void OnDrawGizmos() {
        if (isMoving) {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(targetPosition, 0.5f);
            Gizmos.DrawLine(transform.position, targetPosition);
        }
    }
}
