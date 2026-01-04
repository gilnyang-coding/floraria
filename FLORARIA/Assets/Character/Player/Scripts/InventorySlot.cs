using UnityEngine;
using System;

[Serializable]
public class InventorySlot {
    [SerializeField] private ItemData itemData;
    [SerializeField] private int amount;

    // 외부에서 읽기 전용으로 접근 (캡슐화)
    public ItemData ItemData => itemData;
    public int Amount => amount;

    public InventorySlot(ItemData data, int count) {
        itemData = data;
        amount = count;
    }

    public void AddAmount(int value) {
        amount += value;
    }
}