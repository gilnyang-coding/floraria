using InventorySystem;
using UnityEngine;

/// <summary>
/// 핫바 선택 및 아이템 사용을 관리하는 컨트롤러
/// - 숫자키 1-5로 핫바 슬롯 선택
/// - 마우스 휠로 핫바 슬롯 순환 선택
/// - H키로 현재 선택된 슬롯의 아이템 사용 (핫바/인벤토리 모두)
/// </summary>
public class HotbarController : MonoBehaviour {
    [Header("Hotbar Settings")]
    [SerializeField] private string hotbarInventoryName = "Hotbar";
    [SerializeField] private int slotCount = 5;
    [SerializeField] private KeyCode useItemKey = KeyCode.H;
    
    // 핫바 내 선택된 슬롯 인덱스 (숫자키/마우스휠용)
    private int hotbarSlotIndex = 0;
    
    // 선택 변경 이벤트 (UI 업데이트용)
    public event System.Action<int> OnHotbarSlotChanged;

    private void Start() {
        // 게임 시작 시 슬롯 1번(인덱스 0) 자동 선택
        SelectHotbarSlot(0);
    }

    private void Update() {
        HandleNumberKeyInput();
        HandleScrollWheelInput();
        HandleUseInput();
    }

    /// <summary>
    /// 숫자키 1-5 입력으로 핫바 슬롯 선택
    /// </summary>
    private void HandleNumberKeyInput() {
        for (int i = 0; i < slotCount; i++) {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i)) {
                SelectHotbarSlot(i);
                return;
            }
        }
    }

    /// <summary>
    /// 마우스 휠로 핫바 슬롯 순환 선택
    /// </summary>
    private void HandleScrollWheelInput() {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        
        if (scroll > 0f) {
            // 휠 위로 → 이전 슬롯
            SelectHotbarSlot((hotbarSlotIndex - 1 + slotCount) % slotCount);
        }
        else if (scroll < 0f) {
            // 휠 아래로 → 다음 슬롯
            SelectHotbarSlot((hotbarSlotIndex + 1) % slotCount);
        }
    }

    /// <summary>
    /// H키로 현재 선택된 슬롯의 아이템 사용
    /// </summary>
    private void HandleUseInput() {
        if (Input.GetKeyDown(useItemKey)) {
            UseSelectedItem();
        }
    }

    /// <summary>
    /// 핫바 슬롯을 선택합니다.
    /// </summary>
    public void SelectHotbarSlot(int index) {
        if (index < 0 || index >= slotCount) return;
        
        hotbarSlotIndex = index;
        
        // SlotSelectionManager에 핫바 슬롯 선택 알림
        if (SlotSelectionManager.Instance != null) {
            SlotSelectionManager.Instance.SelectHotbarSlot(index);
        }
        
        OnHotbarSlotChanged?.Invoke(hotbarSlotIndex);
        
        Debug.Log($"[HotbarController] 핫바 슬롯 {hotbarSlotIndex + 1} 선택됨");
    }

    /// <summary>
    /// 현재 선택된 슬롯의 아이템을 사용합니다.
    /// SlotSelectionManager를 통해 핫바/인벤토리 모두 지원
    /// </summary>
    public void UseSelectedItem() {
        if (SlotSelectionManager.Instance == null) {
            Debug.LogWarning("[HotbarController] SlotSelectionManager가 없습니다.");
            return;
        }

        SlotSelectionManager.Instance.UseSelectedItem(gameObject);
    }

    /// <summary>
    /// 현재 핫바 슬롯 인덱스를 반환합니다.
    /// </summary>
    public int GetHotbarSlotIndex() => hotbarSlotIndex;
    
    /// <summary>
    /// 핫바 슬롯 개수를 반환합니다.
    /// </summary>
    public int GetSlotCount() => slotCount;
}
