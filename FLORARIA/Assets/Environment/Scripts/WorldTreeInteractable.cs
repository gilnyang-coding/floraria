using UnityEngine;
using UnityEngine.InputSystem;
using InventorySystem;

/// <summary>
/// 세계수 상호작용 스크립트
/// - BaseInteractable 상속
/// - F키를 누르고 있으면 상호작용 진행
/// - 상호작용 완료 시 특정 아이템 획득
/// - 상호작용 후에도 세계수가 사라지지 않음
/// </summary>
public class WorldTreeInteractable : BaseInteractable {
    
    [Header("World Tree Settings")]
    [Tooltip("상호작용에 걸리는 시간 (초)")]
    [SerializeField] private float interactionTime = 3.0f;
    
    [Tooltip("상호작용 완료 시 주는 아이템 이름 (InventoryController에 등록된 아이템 이름)")]
    [SerializeField] private string rewardItemName = "";
    
    [Tooltip("주는 아이템 개수")]
    [SerializeField] private int rewardItemCount = 1;
    
    private float interactionProgress = 0f;  // 현재 진행 시간
    
    protected override void Update() {
        // 기본 아이콘 빌보드 로직
        base.Update();
        
        // 시간이 멈춰있으면(인벤토리가 켜져 있으면) 상호작용 로직을 무시함
        if (Time.timeScale == 0f) {
            interactionProgress = 0f;
            return;
        }
        
        // 플레이어가 범위 내에 있고 F키를 누르고 있으면 상호작용 진행
        if (isPlayerInRange && Keyboard.current != null && Keyboard.current.fKey.isPressed) {
            UpdateInteraction();
        }
        else {
            // 누르지 않고 있으면 진행도 초기화
            interactionProgress = 0f;
        }
    }
    
    /// <summary>
    /// 상호작용 진행 업데이트
    /// </summary>
    private void UpdateInteraction() {
        interactionProgress += Time.deltaTime;
        
        // 진행률 로그 (디버그용)
        float progressPercent = Mathf.Round((interactionProgress / interactionTime) * 100f);
        Debug.Log($"[WorldTree] 상호작용 중... {progressPercent}%");
        
        if (interactionProgress >= interactionTime) {
            OnCompleteInteraction();
        }
    }
    
    /// <summary>
    /// 상호작용 완료 시 호출
    /// </summary>
    private void OnCompleteInteraction() {
        if (string.IsNullOrEmpty(rewardItemName)) {
            Debug.LogWarning("[WorldTree] 보상 아이템 이름이 설정되지 않았습니다.");
            interactionProgress = 0f;
            return;
        }
        
        // 아이템 지급
        GiveRewardItem();
        
        // 진행도 초기화 (다시 상호작용 가능하도록)
        interactionProgress = 0f;
        
        Debug.Log($"[WorldTree] 상호작용 완료! {rewardItemName} x{rewardItemCount} 획득");
    }
    
    /// <summary>
    /// 보상 아이템 지급
    /// </summary>
    private void GiveRewardItem() {
        if (InventoryController.instance == null) {
            Debug.LogError("[WorldTree] InventoryController를 찾을 수 없습니다.");
            return;
        }
        
        // 먼저 핫바에 추가 시도, 가득 차면 플레이어 인벤토리에 추가
        string targetInventory = "PlayerInventory";
        
        if (!InventoryController.instance.InventoryFull("Hotbar", rewardItemName)) {
            targetInventory = "Hotbar";
        }
        
        InventoryController.instance.AddItem(targetInventory, rewardItemName, rewardItemCount);
    }
    
    /// <summary>
    /// F키 상호작용 (단발성) - 사용하지 않음 (누르고 있기 방식 사용)
    /// </summary>
    public override void OnInteract(GameObject player) {
        // 시간제 방식이므로 단발성 OnInteract는 사용하지 않음
    }
}
