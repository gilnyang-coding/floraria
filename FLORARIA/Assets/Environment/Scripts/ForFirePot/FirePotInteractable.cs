using UnityEngine;
using UnityEngine.InputSystem;
using InventorySystem;

/// <summary>
/// 화로 오브젝트 상호작용
/// - BaseInteractable 상속
/// - F키로 화로 UI 열기/닫기
/// - F/ESC/E키로 화로 UI 닫기
/// </summary>
public class FirePotInteractable : BaseInteractable {
    
    [Header("FirePot Settings")]
    [Tooltip("화로 인벤토리 이름 (InventoryController에 등록된 이름)")]
    [SerializeField] private string firePotInventoryName = "FirePot";
    
    private GameObject firePotUI;
    private GameObject playerInventoryUI;
    private bool isFirePotOpen = false;
    
    protected override void Start() {
        base.Start();
        
        // FirePot UI 찾기 (약간의 딜레이 후)
        Invoke(nameof(FindFirePotUI), 0.2f);
    }
    
    private void FindFirePotUI() {
        if (InventoryController.instance == null) return;
        
        try {
            Inventory firePot = InventoryController.instance.GetInventory(firePotInventoryName);
            if (firePot != null) {
                firePotUI = firePot.GetUI();
                // 처음에는 비활성화
                if (firePotUI != null) {
                    firePotUI.SetActive(false);
                }
                Debug.Log($"[FirePotInteractable] {firePotInventoryName} UI 연결됨");
            }
        } catch {
            Debug.LogWarning($"[FirePotInteractable] {firePotInventoryName} 인벤토리를 찾을 수 없습니다.");
        }
        
        // PlayerInventory UI도 찾기
        try {
            Inventory playerInv = InventoryController.instance.GetInventory(InventoryNames.PlayerInventory);
            if (playerInv != null) {
                playerInventoryUI = playerInv.GetUI();
            }
        } catch { }
    }
    
    protected override void Update() {
        // 화로가 열려있을 때
        if (isFirePotOpen) {
            // E키나 ESC키로 PlayerInventory가 닫히면 FirePot도 닫기
            if (playerInventoryUI != null && !playerInventoryUI.activeSelf) {
                CloseFirePotOnly();
                return;
            }
            
            // ESC, F키로 닫기 (E키는 아무 동작도 하지 않음)
            if (Keyboard.current != null) {
                if (Keyboard.current.escapeKey.wasPressedThisFrame || 
                    (isPlayerInRange && Keyboard.current.fKey.wasPressedThisFrame)) {
                    CloseFirePot();
                    return;
                }
            }
        }
        
        // 기본 상호작용 로직 (F키로 열기)
        base.Update();
    }
    
    /// <summary>
    /// FirePot UI만 닫기 (PlayerInventory가 이미 닫힌 경우)
    /// </summary>
    private void CloseFirePotOnly() {
        if (firePotUI != null) {
            firePotUI.SetActive(false);
        }
        isFirePotOpen = false;
        
        // FirePotManager 툴팁 숨기기
        if (FirePotManager.Instance != null) {
            FirePotManager.Instance.HideTooltip();
        }
        
        Debug.Log("[FirePotInteractable] 화로 닫힘 (인벤토리 연동)");
    }
    
    /// <summary>
    /// F키 상호작용 - 화로 열기
    /// </summary>
    public override void OnInteract(GameObject player) {
        if (isFirePotOpen) {
            CloseFirePot();
        } else {
            OpenFirePot();
        }
    }
    
    /// <summary>
    /// 화로 UI 열기
    /// </summary>
    private void OpenFirePot() {
        if (firePotUI == null) {
            FindFirePotUI();
            if (firePotUI == null) {
                Debug.LogWarning("[FirePotInteractable] FirePot UI를 찾을 수 없습니다.");
                return;
            }
        }
        
        // PlayerInventory를 먼저 열어야 InventoryStateEffect가 Time.timeScale을 처리함
        OpenPlayerInventory();
        
        firePotUI.SetActive(true);
        isFirePotOpen = true;
        
        // FirePotManager에서 슬롯 상태 갱신
        if (FirePotManager.Instance != null) {
            FirePotManager.Instance.RefreshAllSlots();
        }
        
        Debug.Log("[FirePotInteractable] 화로 열림");
    }
    
    /// <summary>
    /// 화로 UI 닫기
    /// </summary>
    private void CloseFirePot() {
        if (firePotUI != null) {
            firePotUI.SetActive(false);
        }
        isFirePotOpen = false;
        
        // FirePotManager 툴팁 숨기기
        if (FirePotManager.Instance != null) {
            FirePotManager.Instance.HideTooltip();
        }
        
        // PlayerInventory도 같이 닫기 (InventoryStateEffect가 Time.timeScale을 처리)
        ClosePlayerInventory();
        
        Debug.Log("[FirePotInteractable] 화로 닫힘");
    }
    
    /// <summary>
    /// PlayerInventory 열기
    /// </summary>
    private void OpenPlayerInventory() {
        if (InventoryController.instance == null) return;
        
        try {
            Inventory playerInventory = InventoryController.instance.GetInventory(InventoryNames.PlayerInventory);
            if (playerInventory != null) {
                GameObject playerInvUI = playerInventory.GetUI();
                if (playerInvUI != null) {
                    playerInvUI.SetActive(true);
                }
            }
        } catch { }
    }
    
    /// <summary>
    /// PlayerInventory 닫기
    /// </summary>
    private void ClosePlayerInventory() {
        if (InventoryController.instance == null) return;
        
        try {
            Inventory playerInventory = InventoryController.instance.GetInventory(InventoryNames.PlayerInventory);
            if (playerInventory != null) {
                GameObject playerInvUI = playerInventory.GetUI();
                if (playerInvUI != null) {
                    playerInvUI.SetActive(false);
                }
            }
        } catch { }
    }
    
    /// <summary>
    /// 플레이어가 범위를 벗어나면 화로 닫기
    /// </summary>
    public override void OnTriggerExit(Collider other) {
        base.OnTriggerExit(other);
        
        if (other.CompareTag("Player") && isFirePotOpen) {
            CloseFirePot();
        }
    }
    
    /// <summary>
    /// 화로가 열려있는지 확인
    /// </summary>
    public bool IsFirePotOpen() => isFirePotOpen;
}
