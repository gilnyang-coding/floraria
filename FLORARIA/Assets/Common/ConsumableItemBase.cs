using InventorySystem;
using UnityEngine;

/// <summary>
/// 소모품 타입 정의
/// </summary>
public enum ConsumableType {
    Survival,   // 생존용 - 3초 쿨타임 공유
    Combat      // 전투형
}

/// <summary>
/// 소비 아이템의 추상 베이스 클래스
/// 모든 소비 아이템은 이 클래스를 상속받아 OnConsume을 구현합니다.
/// </summary>
public abstract class ConsumableItemBase : BaseItem, IConsumable {
    
    [Header("Consumable Settings")]
    [SerializeField] private ConsumableType consumableType = ConsumableType.Survival;
    
    /// <summary>
    /// 이 소모품의 타입 반환
    /// </summary>
    public ConsumableType GetConsumableType() => consumableType;
    
    /// <summary>
    /// IConsumable 구현 - 공통 소비 로직 처리 후 OnConsume 호출
    /// </summary>
    public void Consume(GameObject user) {
        if (user == null) {
            Debug.LogWarning($"[{GetType().Name}] 사용자가 null입니다.");
            return;
        }

        // 자식 클래스의 실제 소비 효과 적용
        OnConsume(user);
    }

    /// <summary>
    /// 인벤토리에서 좌클릭 시 호출되는 메서드
    /// ItemInitializer의 itemAction UnityEvent에 연결
    /// </summary>
    public void UseFromInventory(InventoryItem item) {
        if (item == null || item.GetIsNull()) return;

        if (ItemUsageManager.Instance == null) {
            Debug.LogWarning($"[{GetType().Name}] ItemUsageManager가 없습니다.");
            return;
        }

        // ItemUsageManager에 사용 요청 (캐스팅 타임 + 쿨타임 적용됨)
        ItemUsageManager.Instance.UseItemFromInventory(item);
    }

    /// <summary>
    /// 실제 소비 효과를 구현하는 추상 메서드
    /// 자식 클래스에서 오버라이드하여 구체적인 효과 구현
    /// </summary>
    /// <param name="user">아이템을 사용하는 주체</param>
    protected abstract void OnConsume(GameObject user);
}
