using UnityEngine;
using UnityEngine.InputSystem;

public abstract class BaseInteractable : MonoBehaviour {
    public GameObject interactionIcon;
    protected bool isPlayerInRange = false;
    protected GameObject playerRef;
    
    [Header("Icon Settings")]
    [SerializeField] protected float iconHeight = 2f; // 아이템 위 높이

    public abstract void OnInteract(GameObject player);

    protected virtual void Start() {
        if (interactionIcon != null) {
            interactionIcon.SetActive(false);
        }
    }

    protected virtual void Update() {
        if (isPlayerInRange && Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame) {
            OnInteract(playerRef);
        }
        
        if (isPlayerInRange && interactionIcon != null && Camera.main != null) {
            // 1. 위치 설정: World Up(Vector3.up)을 사용하므로 부모가 누워있어도 항상 '하늘' 방향으로 위치합니다.
            Vector3 iconPosition = transform.position + Vector3.up * iconHeight;
            interactionIcon.transform.position = iconPosition;
            
            // 2. 회전 설정: 아이콘이 카메라를 바라보도록 설정합니다.
            Vector3 directionToCamera = Camera.main.transform.position - iconPosition;
            if (directionToCamera != Vector3.zero) {
                interactionIcon.transform.rotation = Quaternion.LookRotation(directionToCamera);
            }
        }
    }

    public virtual void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {
            isPlayerInRange = true;
            playerRef = other.gameObject;
            if (interactionIcon != null) interactionIcon.SetActive(true);
        }
    }

    public virtual void OnTriggerExit(Collider other) {
        if (other.CompareTag("Player")) {
            isPlayerInRange = false;
            if (interactionIcon != null) interactionIcon.SetActive(false);
        }
    }
}