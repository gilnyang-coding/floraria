using UnityEngine;
using UnityEngine.InputSystem;
using InventorySystem;

/// <summary>
/// 제작대 오브젝트 상호작용
/// - BaseInteractable 상속
/// - F키로 제작대 UI 열기/닫기
/// - ESC키로 제작대 UI 닫기
/// </summary>
public class CraftTableInteractable : BaseInteractable {
    
    [Header("CraftTable Settings")]
    [Tooltip("제작대 인벤토리 이름 (InventoryController에 등록된 이름)")]
    [SerializeField] private string craftTableName = "CraftTable";
    
    [Header("Required Item Settings")]
    [Tooltip("제작대 사용에 필요한 아이템 이름 (비어있으면 필요 없음)")]
    [SerializeField] private string requiredItemName = "";
    
    [Header("Message Icon Settings")]
    [Tooltip("필수 아이템이 없을 때 표시할 메시지 아이콘 (3초간 표시)")]
    [SerializeField] private GameObject messageIcon;
    
    private GameObject craftTableUI;
    private GameObject playerInventoryUI;
    private bool isCraftTableOpen = false;
    private bool isShowingMessage = false;
    private float messageTimer = 0f;
    
    protected override void Start() {
        base.Start();
        
        // CraftTable UI 찾기 (약간의 딜레이 후)
        Invoke(nameof(FindCraftTableUI), 0.2f);
        
        // 메시지 아이콘 초기화
        if (messageIcon != null) {
            messageIcon.SetActive(false);
        }
    }
    
    private void FindCraftTableUI() {
        if (InventoryController.instance == null) return;
        
        try {
            Inventory craftTable = InventoryController.instance.GetInventory(craftTableName);
            if (craftTable != null) {
                craftTableUI = craftTable.GetUI();
                // 처음에는 비활성화
                if (craftTableUI != null) {
                    craftTableUI.SetActive(false);
                }
                Debug.Log($"[CraftTableInteractable] {craftTableName} UI 연결됨");
            }
        } catch {
            Debug.LogWarning($"[CraftTableInteractable] {craftTableName} 인벤토리를 찾을 수 없습니다.");
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
        // 메시지 아이콘 타이머 업데이트 및 빌보드 처리
        if (isShowingMessage && messageIcon != null) {
            messageTimer -= Time.deltaTime;
            if (messageTimer <= 0f) {
                HideMessageIcon();
            } else if (isPlayerInRange && Camera.main != null) {
                // 메시지 아이콘 빌보드 처리 (매 프레임 업데이트)
                Vector3 iconPosition = transform.position + Vector3.up * iconHeight;
                messageIcon.transform.position = iconPosition;
                
                Vector3 directionToCamera = Camera.main.transform.position - iconPosition;
                if (directionToCamera != Vector3.zero) {
                    messageIcon.transform.rotation = Quaternion.LookRotation(directionToCamera);
                }
            }
        }
        
        // 제작대가 열려있을 때
        if (isCraftTableOpen) {
            // E키나 ESC키로 PlayerInventory가 닫히면 CraftTable도 닫기
            if (playerInventoryUI != null && !playerInventoryUI.activeSelf) {
                CloseCraftTableOnly();
                return;
            }
            
            // ESC 또는 F키로 닫기
            if (Keyboard.current != null) {
                if (Keyboard.current.escapeKey.wasPressedThisFrame || 
                    (isPlayerInRange && Keyboard.current.fKey.wasPressedThisFrame)) {
                    CloseCraftTable();
                    return;
                }
            }
        }
        
        // 기본 상호작용 로직 (F키로 열기)
        base.Update();
    }
    
    /// <summary>
    /// CraftTable UI만 닫기 (PlayerInventory가 이미 닫힌 경우)
    /// </summary>
    private void CloseCraftTableOnly() {
        if (craftTableUI != null) {
            craftTableUI.SetActive(false);
        }
        isCraftTableOpen = false;
        
        // CraftingManager 툴팁 숨기기
        if (CraftingManager.Instance != null) {
            CraftingManager.Instance.HideTooltip();
        }
        
        Debug.Log("[CraftTableInteractable] 제작대 닫힘 (인벤토리 연동)");
    }
    
    /// <summary>
    /// F키 상호작용 - 제작대 열기
    /// </summary>
    public override void OnInteract(GameObject player) {
        if (isCraftTableOpen) {
            CloseCraftTable();
        } else {
            // 필수 아이템이 설정되어 있고, 플레이어가 그 아이템을 들고 있지 않으면 메시지 표시
            if (!string.IsNullOrEmpty(requiredItemName) && !HasRequiredItem()) {
                ShowMessageIcon();
                return;
            }
            
            OpenCraftTable();
        }
    }
    
    /// <summary>
    /// 플레이어가 필수 아이템을 들고 있는지 확인
    /// </summary>
    private bool HasRequiredItem() {
        if (SlotSelectionManager.Instance == null) {
            Debug.LogWarning("[CraftTableInteractable] SlotSelectionManager를 찾을 수 없습니다.");
            return false;
        }
        
        InventoryItem selectedItem = SlotSelectionManager.Instance.GetSelectedItem();
        if (selectedItem == null || selectedItem.GetIsNull()) {
            return false;
        }
        
        // 현재 선택된 아이템의 타입이 필수 아이템 이름과 일치하는지 확인
        return selectedItem.GetItemType() == requiredItemName;
    }
    
    /// <summary>
    /// 메시지 아이콘 표시 (3초간)
    /// </summary>
    private void ShowMessageIcon() {
        if (messageIcon == null) {
            Debug.LogWarning("[CraftTableInteractable] 메시지 아이콘이 설정되지 않았습니다.");
            return;
        }
        
        // 기본 상호작용 아이콘 숨기기
        if (interactionIcon != null) {
            interactionIcon.SetActive(false);
        }
        
        // 메시지 아이콘 위치 설정 (상호작용 아이콘과 같은 위치)
        if (Camera.main != null) {
            Vector3 iconPosition = transform.position + Vector3.up * iconHeight;
            messageIcon.transform.position = iconPosition;
            
            // 빌보드 처리
            Vector3 directionToCamera = Camera.main.transform.position - iconPosition;
            if (directionToCamera != Vector3.zero) {
                messageIcon.transform.rotation = Quaternion.LookRotation(directionToCamera);
            }
        }
        
        // 메시지 아이콘 표시
        messageIcon.SetActive(true);
        isShowingMessage = true;
        messageTimer = 3f;
        
        Debug.Log($"[CraftTableInteractable] {requiredItemName} 아이템이 필요합니다.");
    }
    
    /// <summary>
    /// 메시지 아이콘 숨기기
    /// </summary>
    private void HideMessageIcon() {
        if (messageIcon != null) {
            messageIcon.SetActive(false);
        }
        
        // 기본 상호작용 아이콘 다시 표시
        if (interactionIcon != null && isPlayerInRange) {
            interactionIcon.SetActive(true);
        }
        
        isShowingMessage = false;
        messageTimer = 0f;
    }
    
    /// <summary>
    /// 제작대 UI 열기
    /// </summary>
    private void OpenCraftTable() {
        if (craftTableUI == null) {
            FindCraftTableUI();
            if (craftTableUI == null) {
                Debug.LogWarning("[CraftTableInteractable] CraftTable UI를 찾을 수 없습니다.");
                return;
            }
        }
        
        // PlayerInventory를 먼저 열어야 InventoryStateEffect가 Time.timeScale을 처리함
        OpenPlayerInventory();
        
        craftTableUI.SetActive(true);
        isCraftTableOpen = true;
        
        // CraftingManager에서 슬롯 상태 갱신
        if (CraftingManager.Instance != null) {
            CraftingManager.Instance.RefreshAllSlots();
        }
        
        Debug.Log("[CraftTableInteractable] 제작대 열림");
    }
    
    /// <summary>
    /// 제작대 UI 닫기
    /// </summary>
    private void CloseCraftTable() {
        if (craftTableUI != null) {
            craftTableUI.SetActive(false);
        }
        isCraftTableOpen = false;
        
        // CraftingManager 툴팁 숨기기
        if (CraftingManager.Instance != null) {
            CraftingManager.Instance.HideTooltip();
        }
        
        // PlayerInventory도 같이 닫기 (InventoryStateEffect가 Time.timeScale을 처리)
        ClosePlayerInventory();
        
        Debug.Log("[CraftTableInteractable] 제작대 닫힘");
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
    /// 플레이어가 범위를 벗어나면 제작대 닫기 및 메시지 아이콘 숨기기
    /// </summary>
    public override void OnTriggerExit(Collider other) {
        base.OnTriggerExit(other);
        
        if (other.CompareTag("Player")) {
            if (isCraftTableOpen) {
                CloseCraftTable();
            }
            
            // 메시지 아이콘도 숨기기
            HideMessageIcon();
        }
    }
    
    /// <summary>
    /// 제작대가 열려있는지 확인
    /// </summary>
    public bool IsCraftTableOpen() => isCraftTableOpen;
}

