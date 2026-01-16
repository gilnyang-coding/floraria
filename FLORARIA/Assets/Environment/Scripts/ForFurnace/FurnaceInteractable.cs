using UnityEngine;
using UnityEngine.InputSystem;
using InventorySystem;

/// <summary>
/// 용광로 오브젝트 상호작용
/// - BaseInteractable 상속
/// - F키로 용광로 UI 열기/닫기
/// - F/ESC/E키로 용광로 UI 닫기
/// </summary>
public class FurnaceInteractable : BaseInteractable {
    
    [Header("Furnace Settings")]
    [Tooltip("용광로 인벤토리 이름 (InventoryController에 등록된 이름)")]
    [SerializeField] private string furnaceInventoryName = "Furnace";
    
    private GameObject furnaceUI;
    private GameObject playerInventoryUI;
    private bool isFurnaceOpen = false;
    
    protected override void Start() {
        base.Start();
        
        // Furnace UI 찾기 (약간의 딜레이 후)
        Invoke(nameof(FindFurnaceUI), 0.2f);
    }
    
    private void FindFurnaceUI() {
        if (InventoryController.instance == null) return;
        
        try {
            Inventory furnace = InventoryController.instance.GetInventory(furnaceInventoryName);
            if (furnace != null) {
                furnaceUI = furnace.GetUI();
                // 처음에는 비활성화
                if (furnaceUI != null) {
                    furnaceUI.SetActive(false);
                }
                Debug.Log($"[FurnaceInteractable] {furnaceInventoryName} UI 연결됨");
            }
        } catch {
            Debug.LogWarning($"[FurnaceInteractable] {furnaceInventoryName} 인벤토리를 찾을 수 없습니다.");
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
        // 용광로가 열려있을 때
        if (isFurnaceOpen) {
            // E키나 ESC키로 PlayerInventory가 닫히면 Furnace도 닫기
            if (playerInventoryUI != null && !playerInventoryUI.activeSelf) {
                CloseFurnaceOnly();
                return;
            }
            
            // ESC, F키로 닫기 (E키는 아무 동작도 하지 않음)
            if (Keyboard.current != null) {
                if (Keyboard.current.escapeKey.wasPressedThisFrame || 
                    (isPlayerInRange && Keyboard.current.fKey.wasPressedThisFrame)) {
                    CloseFurnace();
                    return;
                }
            }
        }
        
        // 기본 상호작용 로직 (F키로 열기)
        base.Update();
    }
    
    /// <summary>
    /// Furnace UI만 닫기 (PlayerInventory가 이미 닫힌 경우)
    /// </summary>
    private void CloseFurnaceOnly() {
        if (furnaceUI != null) {
            furnaceUI.SetActive(false);
        }
        isFurnaceOpen = false;
        
        Debug.Log("[FurnaceInteractable] 용광로 닫힘 (인벤토리 연동)");
    }
    
    /// <summary>
    /// F키 상호작용 - 용광로 열기
    /// </summary>
    public override void OnInteract(GameObject player) {
        if (isFurnaceOpen) {
            CloseFurnace();
        } else {
            OpenFurnace();
        }
    }
    
    /// <summary>
    /// 용광로 UI 열기
    /// </summary>
    private void OpenFurnace() {
        if (furnaceUI == null) {
            FindFurnaceUI();
            if (furnaceUI == null) {
                Debug.LogWarning("[FurnaceInteractable] Furnace UI를 찾을 수 없습니다.");
                return;
            }
        }
        
        // PlayerInventory를 먼저 열어야 InventoryStateEffect가 Time.timeScale을 처리함
        OpenPlayerInventory();
        
        furnaceUI.SetActive(true);
        isFurnaceOpen = true;
        
        Debug.Log("[FurnaceInteractable] 용광로 열림");
    }
    
    /// <summary>
    /// 용광로 UI 닫기
    /// </summary>
    private void CloseFurnace() {
        if (furnaceUI != null) {
            furnaceUI.SetActive(false);
        }
        isFurnaceOpen = false;
        
        // PlayerInventory도 같이 닫기 (InventoryStateEffect가 Time.timeScale을 처리)
        ClosePlayerInventory();
        
        Debug.Log("[FurnaceInteractable] 용광로 닫힘");
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
    /// 플레이어가 범위를 벗어나면 용광로 닫기
    /// </summary>
    public override void OnTriggerExit(Collider other) {
        base.OnTriggerExit(other);
        
        if (other.CompareTag("Player") && isFurnaceOpen) {
            CloseFurnace();
        }
    }
    
    /// <summary>
    /// 용광로가 열려있는지 확인
    /// </summary>
    public bool IsFurnaceOpen() => isFurnaceOpen;
}
