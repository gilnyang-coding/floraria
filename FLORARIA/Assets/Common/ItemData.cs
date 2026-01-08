using UnityEngine;

// 아이템의 카테고리 정의
public enum ItemType { 
    Resource,           // 자원
    SurvivalConsumable, // 생존용 소모품 - 3초 쿨타임 공유
    CombatConsumable,   // 전투형 소모품
    Equipment           // 장비
}

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject 
{
    public string itemName;    // 아이템 이름
    public ItemType itemType;  // 아이템 종류
    public Sprite itemIcon;    // UI용 아이콘
    [TextArea]
    public string description; // 아이템 설명
}
