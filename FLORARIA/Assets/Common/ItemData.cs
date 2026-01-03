using UnityEngine;

// 아이템의 카테고리 정의
public enum ItemType { Resource, Consumable, Equipment }

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject 
{
    public string itemName;    // 아이템 이름
    public ItemType itemType;  // 아이템 종류
    public Sprite itemIcon;    // UI용 아이콘
    [TextArea]
    public string description; // 아이템 설명
}