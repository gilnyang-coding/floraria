using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour 
{
    // 아이템 설계도(ItemData)를 열쇠(Key)로 삼아 개수(Value)를 저장
    public Dictionary<ItemData, int> items = new Dictionary<ItemData, int>();

    public void AddItem(ItemData data, int amount)  {
        if (items.ContainsKey(data)) {
            items[data] += amount; // 이미 있으면 개수만 증가
        }
        else {
            items.Add(data, amount); // 없으면 새로 등록
        }

        Debug.Log($"[인벤토리] {data.itemName} 획득! (현재 총 {items[data]}개)");
    }
}