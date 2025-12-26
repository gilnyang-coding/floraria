using UnityEngine;

public class PlayerMove : MonoBehaviour 
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float stoppingDistance = 0.1f;
    
    [Header("Raycast Settings")]
    [SerializeField] private LayerMask groundLayer = -1;
    [SerializeField] private float maxRaycastDistance = 100f;
    
    private Camera mainCamera;
    private Vector3 targetPosition;
    private bool isMoving = false;
    
    void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            mainCamera = FindObjectOfType<Camera>();
        }
    }
    
    void Update()
    {
        // 우클릭 감지
        if (Input.GetMouseButtonDown(1))
        {
            MoveToMousePosition();
        }
        
        // 이동 처리
        if (isMoving)
        {
            MoveToTarget();
        }
    }
    
    void MoveToMousePosition()
    {
        // 마우스 위치에서 레이캐스트
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, maxRaycastDistance, groundLayer))
        {
            targetPosition = hit.point;
            isMoving = true;
        }
    }
    
    void MoveToTarget()
    {
        // 목표 위치까지의 거리 계산
        float distance = Vector3.Distance(transform.position, targetPosition);
        
        if (distance > stoppingDistance)
        {
            // 목표 위치로 이동
            Vector3 direction = (targetPosition - transform.position).normalized;
            transform.position += direction * moveSpeed * Time.deltaTime;
            
            // 이동 방향으로 회전 (Y축만)
            if (direction.magnitude > 0.1f)
            {
                Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 10f);
            }
        }
        else
        {
            // 목표 위치에 도달
            isMoving = false;
            transform.position = targetPosition;
        }
    }
    
    // 디버그용: Scene 뷰에서 목표 위치 표시
    void OnDrawGizmos()
    {
        if (isMoving)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(targetPosition, 0.5f);
            Gizmos.DrawLine(transform.position, targetPosition);
        }
    }
}
