using UnityEngine;
using UnityEngine.InputSystem;

public abstract class BaseItem : MonoBehaviour
{
    public ItemData data;           // 연결할 아이템 데이터
    public GameObject interactionIcon; // F키 스프라이트 오브젝트
    protected bool isPlayerInRange = false;
    protected GameObject playerRef;

    // 자식 클래스에서 아이템마다 다르게 행동할 부분
    public virtual void OnInteract(GameObject player) {
        Inventory inv = player.GetComponent<Inventory>();
        if (inv != null && data != null) {
            inv.AddItem(data, 1);
            Debug.Log($"{data.itemName} 획득 및 저장 완료");
            
            // 추가적인 연출(소리, 이펙트 등)이 필요하면 여기서 호출
            // OnPickupEffect(); 
            
            Destroy(gameObject);
        }
    }

    protected virtual void Start() {
        if (interactionIcon != null) interactionIcon.SetActive(false);
    }

    protected virtual void Update() {
        if (isPlayerInRange) {
            // 1. 아이콘 빌보드 (항상 카메라 바라보기)
            if (interactionIcon != null)
                interactionIcon.transform.LookAt(Camera.main.transform);

            // 2. F키 입력 확인
            if (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame) {
                Debug.Log("F키 입력 감지됨! OnInteract 호출합니다.");
                OnInteract(playerRef);
            }
        }
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            playerRef = other.gameObject;
            if (interactionIcon != null) interactionIcon.SetActive(true);
        }
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            if (interactionIcon != null) interactionIcon.SetActive(false);
        }
    }
}