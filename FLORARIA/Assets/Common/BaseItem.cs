using UnityEngine;

public abstract class BaseItem : BaseInteractable {
    public ItemData data;

    [Header("Spawn Physics")]
    [SerializeField] protected float launchForce = 1f;
    [SerializeField] protected float launchHeight = 1f;
    [SerializeField] protected float spinTorque = 1f;

    protected Rigidbody rb;
    protected Collider[] colliders;
    protected bool isPhysicsPhase = true;

    protected override void Start() {
        base.Start();
        
        rb = GetComponent<Rigidbody>();
        colliders = GetComponents<Collider>();
        
        if (rb != null) {
            // 초기 물리 설정
            rb.isKinematic = false;
            rb.useGravity = true;
            
            // Trigger 비활성화 (물리 충돌 허용)
            foreach (var col in colliders) {
                col.isTrigger = false;
            }
            
            // 튀어나가는 효과
            LaunchItem();
        }
    }

    protected virtual void LaunchItem() {
        if (rb == null) return;
        
        // 위쪽+랜덤 방향으로 발사
        Vector3 randomDir = new Vector3(
            Random.Range(-1f, 1f), 
            launchHeight, 
            Random.Range(-1f, 1f)
        ).normalized;
        
        rb.AddForce(randomDir * launchForce, ForceMode.Impulse);
        
        // 회전 추가
        rb.AddTorque(Random.insideUnitSphere * spinTorque);
    }

    protected virtual void OnCollisionEnter(Collision collision) {
        // 땅에 닿으면 정적 상태로 전환
        if (isPhysicsPhase && collision.gameObject.CompareTag("Ground")) {
            TransitionToStatic();
        }
    }

    protected virtual void TransitionToStatic() {
        if (rb == null) return;
        
        isPhysicsPhase = false;
        
        // 정적 상태로 전환
        rb.isKinematic = true;
        rb.useGravity = false;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        
        // Trigger 활성화 (플레이어와 상호작용 가능)
        foreach (var col in colliders) {
            col.isTrigger = true;
        }
        
        Debug.Log($"{gameObject.name}이(가) 정적 상태로 전환되었습니다.");
    }

    public override void OnInteract(GameObject player) {
        // 물리 단계에서는 상호작용 불가
        if (isPhysicsPhase) return;
        
        Inventory inv = player.GetComponent<Inventory>();
        if (inv != null) {
            inv.AddItem(data, 1);
            Destroy(gameObject);
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