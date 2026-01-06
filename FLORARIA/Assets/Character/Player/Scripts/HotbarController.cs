using InventorySystem;
using UnityEngine;

/// <summary>
/// 핫바 선택 및 아이템 사용을 관리하는 컨트롤러
/// - 숫자키 1-5로 슬롯 선택
/// - 마우스 휠로 슬롯 순환 선택
/// - H키로 Consumable 아이템 사용 (ItemUsageManager 통해)
/// </summary>
public class HotbarController : MonoBehaviour {
    [Header("Hotbar Settings")]
    [SerializeField] private string hotbarInventoryName = "Hotbar";
    [SerializeField] private int slotCount = 5;
    [SerializeField] private KeyCode useItemKey = KeyCode.H;
    
    // 현재 선택된 슬롯 인덱스 (0부터 시작)
    private int selectedSlotIndex = 0;
    
    // 핫바 UI 매니저 캐싱
    private InventoryUIManager hotbarUIManager;
    
    // 선택 변경 이벤트 (UI 업데이트용)
    public event System.Action<int> OnSlotSelected;

    private void Start() {
        // 게임 시작 시 핫바 UI 매니저 찾기
        FindHotbarUIManager();
        
        // 게임 시작 시 슬롯 1번(인덱스 0) 자동 선택 (고정)
        ForceSelectSlot(0);
    }

    private void Update() {
        HandleNumberKeyInput();
        HandleScrollWheelInput();
        HandleUseInput();
    }

    /// <summary>
    /// 핫바의 InventoryUIManager를 찾아서 캐싱합니다.
    /// </summary>
    private void FindHotbarUIManager() {
        if (InventoryController.instance == null) return;

        // 핫바 인벤토리의 UI 매니저 찾기
        Inventory hotbarInventory = InventoryController.instance.GetInventory(hotbarInventoryName);
        if (hotbarInventory != null) {
            GameObject hotbarUI = hotbarInventory.GetUI();
            if (hotbarUI != null) {
                hotbarUIManager = hotbarUI.GetComponent<InventoryUIManager>();
            }
        }

        if (hotbarUIManager == null) {
            Debug.LogWarning($"[HotbarController] '{hotbarInventoryName}' 핫바의 InventoryUIManager를 찾을 수 없습니다.");
        }
    }

    /// <summary>
    /// 숫자키 1-5 입력으로 슬롯 선택
    /// </summary>
    private void HandleNumberKeyInput() {
        for (int i = 0; i < slotCount; i++) {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i)) {
                SelectSlot(i);
                return;
            }
        }
    }

    /// <summary>
    /// 마우스 휠로 슬롯 순환 선택
    /// </summary>
    private void HandleScrollWheelInput() {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        
        if (scroll > 0f) {
            // 휠 위로 → 이전 슬롯
            SelectSlot((selectedSlotIndex - 1 + slotCount) % slotCount);
        }
        else if (scroll < 0f) {
            // 휠 아래로 → 다음 슬롯
            SelectSlot((selectedSlotIndex + 1) % slotCount);
        }
    }

    /// <summary>
    /// H키로 선택된 슬롯의 Consumable 아이템 사용
    /// </summary>
    private void HandleUseInput() {
        if (Input.GetKeyDown(useItemKey)) {
            UseSelectedItem();
        }
    }

    /// <summary>
    /// 특정 슬롯을 선택합니다.
    /// </summary>
    public void SelectSlot(int index) {
        if (index < 0 || index >= slotCount) return;
        if (index == selectedSlotIndex) return; // 같은 슬롯이면 무시
        
        ForceSelectSlot(index);
    }

    /// <summary>
    /// 같은 슬롯이어도 강제로 선택합니다. (초기화용)
    /// </summary>
    private void ForceSelectSlot(int index) {
        if (index < 0 || index >= slotCount) return;
        
        selectedSlotIndex = index;
        
        // 슬롯 하이라이트 업데이트
        UpdateSlotHighlight();
        
        OnSlotSelected?.Invoke(selectedSlotIndex);
        
        Debug.Log($"[HotbarController] 슬롯 {selectedSlotIndex + 1} 선택됨");
    }

    /// <summary>
    /// 현재 선택된 슬롯을 하이라이트합니다.
    /// </summary>
    private void UpdateSlotHighlight() {
        if (hotbarUIManager == null) {
            FindHotbarUIManager();
            if (hotbarUIManager == null) return;
        }

        // 선택된 슬롯 하이라이트 (기존 하이라이트는 자동으로 해제됨)
        hotbarUIManager.HighlightSlotByIndex(selectedSlotIndex);
    }

    /// <summary>
    /// 현재 선택된 슬롯의 아이템을 사용합니다.
    /// ItemUsageManager를 통해 캐스팅 타임 적용
    /// </summary>
    public void UseSelectedItem() {
        if (InventoryController.instance == null) {
            Debug.LogWarning("[HotbarController] InventoryController가 없습니다.");
            return;
        }

        if (ItemUsageManager.Instance == null) {
            Debug.LogWarning("[HotbarController] ItemUsageManager가 없습니다. 씬에 추가해주세요.");
            return;
        }

        // 핫바에서 선택된 슬롯의 아이템 가져오기
        InventoryItem inventoryItem = InventoryController.instance.GetItem(hotbarInventoryName, selectedSlotIndex);
        
        if (inventoryItem == null || inventoryItem.GetIsNull()) {
            // 빈 슬롯 - 아무것도 안 함
            return;
        }

        // ItemUsageManager에 사용 요청 (캐스팅 타임 적용됨)
        ItemUsageManager.Instance.UseItemFromHotbar(inventoryItem, hotbarInventoryName, gameObject);
    }

    /// <summary>
    /// 현재 선택된 슬롯 인덱스를 반환합니다.
    /// </summary>
    public int GetSelectedSlotIndex() => selectedSlotIndex;
    
    /// <summary>
    /// 핫바 슬롯 개수를 반환합니다.
    /// </summary>
    public int GetSlotCount() => slotCount;
}
