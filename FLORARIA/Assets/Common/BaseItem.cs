using InventorySystem;
using UnityEngine;

public abstract class BaseItem : BaseInteractable {
public ItemData data;

    [Header("Spawn Physics")]
    [SerializeField] protected float launchForce = 0.5f;
    [SerializeField] protected float launchHeight = 2f;

    protected Rigidbody rb;
    protected Collider[] colliders;
    protected bool isPhysicsPhase = true;
    
    // 플레이어 방향을 저장하는 정적 변수 (ItemDropper에서 설정)
    public static Vector3? playerForwardDirection = null;
    public static bool usePlayerDirection = false;

    protected override void Start() {
        base.Start();
        
        rb = GetComponent<Rigidbody>();
        colliders = GetComponents<Collider>();
        
        if (rb != null) {
            rb.isKinematic = false;
            rb.useGravity = true;
            
            foreach (var col in colliders) {
                col.isTrigger = false;
            }
            
            LaunchItem();
        }
    }

    protected virtual void LaunchItem() {
        if (rb == null) return;
        
        // 플레이어 방향이 설정되어 있으면 플레이어 방향 사용, 없으면 아이템의 forward 사용
        Vector3 forwardDir;
        if (usePlayerDirection && playerForwardDirection.HasValue) {
            forwardDir = (playerForwardDirection.Value + Vector3.up * 0.5f).normalized;
            Debug.Log($"[BaseItem] 플레이어 방향 사용: {forwardDir}, 원본 방향: {playerForwardDirection.Value}");//
            // 플래그는 모든 아이템 생성이 완료된 후 ItemDropper나 Gatherable에서 초기화함
            // 여기서는 초기화하지 않음 (여러 아이템을 생성할 때를 위해)
        } else {
            // 플레이어 방향이 없을 때만 아이템의 forward 사용 (채집 등 다른 경우)
            forwardDir = (transform.forward + Vector3.up * 0.5f).normalized;
            Debug.Log($"[BaseItem] 아이템 forward 사용: {forwardDir}, transform.forward: {transform.forward}");
        }
        
        rb.AddForce(forwardDir * launchForce, ForceMode.Impulse);
        
        // [수정] 회전 추가 코드(AddTorque)를 완전히 삭제했습니다.
        rb.angularVelocity = Vector3.zero; 
    }

    protected virtual void OnCollisionEnter(Collision collision) {
        if (isPhysicsPhase && collision.gameObject.CompareTag("Ground")) {
            TransitionToStatic();
        }
    }

    protected virtual void TransitionToStatic() {
        if (rb == null) return;
        
        isPhysicsPhase = false;
        
        rb.isKinematic = true;
        rb.useGravity = false;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero; // 정지 시 회전값 완전 초기화
        
        foreach (var col in colliders) {
            col.isTrigger = true;
        }
    }

    public override void OnInteract(GameObject player) {
            if (isPhysicsPhase) return;
            
            // 1. 에셋의 싱글톤 컨트롤러가 존재하는지 확인합니다.
            if (InventoryController.instance != null) {
                // 2. 먼저 핫바에 추가를 시도합니다. 핫바가 가득 차면 플레이어 인벤토리에 추가합니다.
                string targetInventory = "PlayerInventory";
                
                // 핫바가 가득 차지 않았으면 핫바에 추가, 가득 찼으면 플레이어 인벤토리에 추가
                if (!InventoryController.instance.InventoryFull("Hotbar", data.itemName)) {
                    targetInventory = "Hotbar";
                }
                
                InventoryController.instance.AddItem(targetInventory, data.itemName, 1);
                
                Debug.Log($"[에셋 시스템] {data.itemName} 획득 완료 ({targetInventory})");
                Destroy(gameObject);
            }
            else {
                Debug.LogError("씬에 InventoryController가 없습니다! 프리팹을 배치하고 Unpack 해주세요.");
            }
        }

    // Trigger는 정적 상태에서만 작동
    public override void OnTriggerEnter(Collider other) {
        if (!isPhysicsPhase) {
            base.OnTriggerEnter(other);
        }
    }

    public override void OnTriggerExit(Collider other) {
        if (!isPhysicsPhase) {
            base.OnTriggerExit(other);
        }
    }
}