using UnityEngine;
using UnityEngine.InputSystem;
using InventorySystem;

/// <summary>
/// 공방 오브젝트 상호작용
/// - BaseInteractable 상속
/// - F키로 공방 UI 열기/닫기
/// - ESC키로 공방 UI 닫기
/// </summary>
public class BlackSmithInteractable : BaseInteractable {
    
    [Header("BlackSmith Settings")]
    [Tooltip("공방 인벤토리 이름 (InventoryController에 등록된 이름)")]
    [SerializeField] private string blackSmithName = "BlackSmith";
    
    [Header("Required Item Settings")]
    [Tooltip("공방 사용에 필요한 아이템 이름 (비어있으면 필요 없음)")]
    [SerializeField] private string requiredItemName = "";
    
    [Header("Message Icon Settings")]
    [Tooltip("필수 아이템이 없을 때 표시할 메시지 아이콘 (3초간 표시)")]
    [SerializeField] private GameObject messageIcon;
    
    private GameObject blackSmithUI;
    private GameObject playerInventoryUI;
    private bool isBlackSmithOpen = false;
    private bool isShowingMessage = false;
    private float messageTimer = 0f;
    private bool hasConsumedRequiredItem = false; // 필수 아이템을 한 번 소모했는지 추적
    
    protected override void Start() {
        base.Start();
        
        // BlackSmith UI 찾기 (약간의 딜레이 후)
        Invoke(nameof(FindBlackSmithUI), 0.2f);
        
        // 메시지 아이콘 초기화
        if (messageIcon != null) {
            messageIcon.SetActive(false);
        }
    }
    
    private void FindBlackSmithUI() {
        if (InventoryController.instance == null) return;
        
        try {
            Inventory blackSmith = InventoryController.instance.GetInventory(blackSmithName);
            if (blackSmith != null) {
                blackSmithUI = blackSmith.GetUI();
                // 처음에는 비활성화
                if (blackSmithUI != null) {
                    blackSmithUI.SetActive(false);
                }
                Debug.Log($"[BlackSmithInteractable] {blackSmithName} UI 연결됨");
            }
        } catch {
            Debug.LogWarning($"[BlackSmithInteractable] {blackSmithName} 인벤토리를 찾을 수 없습니다.");
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
        
        // 공방이 열려있을 때
        if (isBlackSmithOpen) {
            // E키나 ESC키로 PlayerInventory가 닫히면 BlackSmith도 닫기
            if (playerInventoryUI != null && !playerInventoryUI.activeSelf) {
                CloseBlackSmithOnly();
                return;
            }
            
            // ESC 또는 F키로 닫기
            if (Keyboard.current != null) {
                if (Keyboard.current.escapeKey.wasPressedThisFrame || 
                    (isPlayerInRange && Keyboard.current.fKey.wasPressedThisFrame)) {
                    CloseBlackSmith();
                    return;
                }
            }
        }
        
        // 기본 상호작용 로직 (F키로 열기)
        base.Update();
    }
    
    /// <summary>
    /// BlackSmith UI만 닫기 (PlayerInventory가 이미 닫힌 경우)
    /// </summary>
    private void CloseBlackSmithOnly() {
        if (blackSmithUI != null) {
            blackSmithUI.SetActive(false);
        }
        isBlackSmithOpen = false;
        
        // BlackSmithManager 툴팁 숨기기
        if (BlackSmithManager.Instance != null) {
            BlackSmithManager.Instance.HideTooltip();
        }
        
        Debug.Log("[BlackSmithInteractable] 공방 닫힘 (인벤토리 연동)");
    }
    
    /// <summary>
    /// F키 상호작용 - 공방 열기
    /// </summary>
    public override void OnInteract(GameObject player) {
        if (isBlackSmithOpen) {
            CloseBlackSmith();
        } else {
            // 필수 아이템이 설정되어 있고, 플레이어가 그 아이템을 들고 있지 않으면 메시지 표시
            if (!string.IsNullOrEmpty(requiredItemName) && !HasRequiredItem()) {
                ShowMessageIcon();
                return;
            }
            
            OpenBlackSmith();
        }
    }
    
    /// <summary>
    /// 플레이어가 필수 아이템을 들고 있는지 확인
    /// </summary>
    private bool HasRequiredItem() {
        // 이미 한 번 소모했다면 항상 true 반환
        if (hasConsumedRequiredItem) {
            return true;
        }
        
        if (SlotSelectionManager.Instance == null) {
            Debug.LogWarning("[BlackSmithInteractable] SlotSelectionManager를 찾을 수 없습니다.");
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
    /// 필수 아이템 소모 (선택된 아이템에서 1개 제거)
    /// </summary>
    private void ConsumeRequiredItem() {
        if (SlotSelectionManager.Instance == null) {
            Debug.LogWarning("[BlackSmithInteractable] SlotSelectionManager를 찾을 수 없습니다.");
            return;
        }
        
        InventoryItem selectedItem = SlotSelectionManager.Instance.GetSelectedItem();
        if (selectedItem == null || selectedItem.GetIsNull()) {
            Debug.LogWarning("[BlackSmithInteractable] 선택된 아이템이 없습니다.");
            return;
        }
        
        // 선택된 아이템의 인벤토리 이름과 위치 가져오기
        string inventoryName = SlotSelectionManager.Instance.CurrentInventoryName;
        int position = SlotSelectionManager.Instance.CurrentSlotIndex;
        
        if (string.IsNullOrEmpty(inventoryName) || position < 0) {
            Debug.LogWarning("[BlackSmithInteractable] 선택된 아이템의 인벤토리 정보를 가져올 수 없습니다.");
            return;
        } 
        
        // 아이템 1개 제거
        if (InventoryController.instance != null) {
            InventoryController.instance.RemoveItemPos(inventoryName, position, 1);
            hasConsumedRequiredItem = true;
            Debug.Log($"[BlackSmithInteractable] {requiredItemName} 1개 소모됨");
        }
    }
    
    /// <summary>
    /// 메시지 아이콘 표시 (3초간)
    /// </summary>
    private void ShowMessageIcon() {
        if (messageIcon == null) {
            Debug.LogWarning("[BlackSmithInteractable] 메시지 아이콘이 설정되지 않았습니다.");
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
        
        Debug.Log($"[BlackSmithInteractable] {requiredItemName} 아이템이 필요합니다.");
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
    /// 공방 UI 열기
    /// </summary>
    private void OpenBlackSmith() {
        if (blackSmithUI == null) {
            FindBlackSmithUI();
            if (blackSmithUI == null) {
                Debug.LogWarning("[BlackSmithInteractable] BlackSmith UI를 찾을 수 없습니다.");
                return;
            }
        }
        
        // 필수 아이템이 설정되어 있고, 아직 소모하지 않았다면 소모
        if (!string.IsNullOrEmpty(requiredItemName) && !hasConsumedRequiredItem && HasRequiredItem()) {
            ConsumeRequiredItem();
        }
        
        // PlayerInventory를 먼저 열어야 InventoryStateEffect가 Time.timeScale을 처리함
        OpenPlayerInventory();
        
        blackSmithUI.SetActive(true);
        isBlackSmithOpen = true;
        
        // BlackSmithManager에서 슬롯 상태 갱신
        if (BlackSmithManager.Instance != null) {
            BlackSmithManager.Instance.RefreshAllSlots();
        }
        
        Debug.Log("[BlackSmithInteractable] 공방 열림");
    }
    
    /// <summary>
    /// 공방 UI 닫기
    /// </summary>
    private void CloseBlackSmith() {
        if (blackSmithUI != null) {
            blackSmithUI.SetActive(false);
        }
        isBlackSmithOpen = false;
        
        // BlackSmithManager 툴팁 숨기기
        if (BlackSmithManager.Instance != null) {
            BlackSmithManager.Instance.HideTooltip();
        }
        
        // PlayerInventory도 같이 닫기 (InventoryStateEffect가 Time.timeScale을 처리)
        ClosePlayerInventory();
        
        Debug.Log("[BlackSmithInteractable] 공방 닫힘");
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
    /// 플레이어가 범위를 벗어나면 공방 닫기 및 메시지 아이콘 숨기기
    /// </summary>
    public override void OnTriggerExit(Collider other) {
        base.OnTriggerExit(other);
        
        if (other.CompareTag("Player")) {
            if (isBlackSmithOpen) {
                CloseBlackSmith();
            }
            
            // 메시지 아이콘도 숨기기
            HideMessageIcon();
        }
    }
    
    /// <summary>
    /// 공방이 열려있는지 확인
    /// </summary>
    public bool IsBlackSmithOpen() => isBlackSmithOpen;
}
