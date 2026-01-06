using InventorySystem;
using UnityEngine;

/// <summary>
/// 모든 인벤토리의 슬롯 선택을 통합 관리하는 싱글톤 매니저
/// - 핫바, 플레이어 인벤토리 등 모든 인벤토리의 선택 상태 추적
/// - H키로 현재 선택된 슬롯의 아이템 사용
/// - 인벤토리 닫을 때 핫바 1번으로 자동 리셋
/// </summary>
public class SlotSelectionManager : MonoBehaviour {
    public static SlotSelectionManager Instance { get; private set; }

    [Header("Default Settings")]
    [SerializeField] private string hotbarInventoryName = "Hotbar";
    [SerializeField] private int defaultHotbarSlot = 0; // 기본 핫바 슬롯
    
    // 현재 선택된 슬롯 정보
    private string currentInventoryName;
    private int currentSlotIndex;
    private InventoryUIManager currentUIManager;
    
    // 이벤트
    public event System.Action<string, int> OnSlotSelectionChanged; // (인벤토리 이름, 슬롯 인덱스)

    public string CurrentInventoryName => currentInventoryName;
    public int CurrentSlotIndex => currentSlotIndex;
    public bool IsHotbarSelected => currentInventoryName == hotbarInventoryName;

    private void Awake() {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject);
        }
    }

    private void Start() {
        // 게임 시작 시 핫바 1번 슬롯 선택
        SelectHotbarSlot(defaultHotbarSlot);
    }

    /// <summary>
    /// 슬롯을 선택합니다. InventoryUIManager에서 호출됨.
    /// </summary>
    public void SelectSlot(string inventoryName, int slotIndex, InventoryUIManager uiManager = null) {
        // 이전 UIManager와 다르면 이전 하이라이트 해제
        if (currentUIManager != null && currentUIManager != uiManager) {
            currentUIManager.ResetHighlight();
        }
        
        currentInventoryName = inventoryName;
        currentSlotIndex = slotIndex;
        currentUIManager = uiManager;
        
        OnSlotSelectionChanged?.Invoke(currentInventoryName, currentSlotIndex);
        
        Debug.Log($"[SlotSelectionManager] 선택: {inventoryName} 슬롯 {slotIndex + 1}");
    }

    /// <summary>
    /// 핫바의 특정 슬롯을 선택합니다. (숫자키, 마우스휠용)
    /// </summary>
    public void SelectHotbarSlot(int slotIndex) {
        if (InventoryController.instance == null) return;

        Inventory hotbarInventory = InventoryController.instance.GetInventory(hotbarInventoryName);
        if (hotbarInventory != null) {
            GameObject hotbarUI = hotbarInventory.GetUI();
            if (hotbarUI != null) {
                InventoryUIManager uiManager = hotbarUI.GetComponent<InventoryUIManager>();
                
                // 이전 UIManager와 다르면 이전 하이라이트 해제
                if (currentUIManager != null && currentUIManager != uiManager) {
                    currentUIManager.ResetHighlight();
                }
                
                SelectSlot(hotbarInventoryName, slotIndex, uiManager);
                
                // UI 하이라이트 업데이트
                uiManager?.HighlightSlotByIndex(slotIndex);
            }
        }
    }

    /// <summary>
    /// 핫바 기본 슬롯(1번)으로 리셋합니다. 인벤토리 닫을 때 호출.
    /// </summary>
    public void ResetToHotbar() {
        // 이미 핫바가 선택되어 있으면 무시
        if (IsHotbarSelected) return;
        
        SelectHotbarSlot(defaultHotbarSlot);
        Debug.Log("[SlotSelectionManager] 핫바 1번 슬롯으로 리셋");
    }

    /// <summary>
    /// 현재 선택된 슬롯의 아이템을 반환합니다.
    /// </summary>
    public InventoryItem GetSelectedItem() {
        if (InventoryController.instance == null) return null;
        if (string.IsNullOrEmpty(currentInventoryName)) return null;
        
        return InventoryController.instance.GetItem(currentInventoryName, currentSlotIndex);
    }

    /// <summary>
    /// 현재 선택된 슬롯의 아이템을 사용합니다. (H키용)
    /// </summary>
    public void UseSelectedItem(GameObject user) {
        if (ItemUsageManager.Instance == null) {
            Debug.LogWarning("[SlotSelectionManager] ItemUsageManager가 없습니다.");
            return;
        }

        InventoryItem item = GetSelectedItem();
        if (item == null || item.GetIsNull()) {
            return;
        }

        ItemUsageManager.Instance.UseItemFromHotbar(item, currentInventoryName, user);
    }
}
