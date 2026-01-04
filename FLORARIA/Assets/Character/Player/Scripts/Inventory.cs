using System;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour {
    // 인스펙터에서 실시간 확인을 위한 리스트
    [SerializeField] 
    private List<InventorySlot> inventorySlots = new List<InventorySlot>();

    // 데이터 변경 시 호출될 이벤트
    public event Action OnInventoryChanged;

    public void AddItem(ItemData data, int amount) {
        if (data == null) return;

        // 1. 기존 아이템이 있는지 검색
        InventorySlot slot = inventorySlots.Find(s => s.ItemData == data);

        if (slot != null) {
            // 이미 존재하면 개수 추가
            slot.AddAmount(amount);
        }
        else {
            // 없으면 새로 생성하여 리스트에 추가
            inventorySlots.Add(new InventorySlot(data, amount));
        }

        // 2. 데이터가 변경되었음을 구독자들에게 알림
        // 이 코드가 실행되면 이 이벤트를 듣고 있는 UI 등이 알아서 갱신됨됨.
        OnInventoryChanged?.Invoke();

        Debug.Log($"[인벤토리] {data.itemName} {amount}개 추가됨.");
    }

    // 인벤토리 목록을 안전하게 가져오기 위한 속성
    public IReadOnlyList<InventorySlot> GetSlots() {
        return inventorySlots.AsReadOnly();
    }
}