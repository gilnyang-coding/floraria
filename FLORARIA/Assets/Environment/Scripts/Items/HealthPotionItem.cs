using UnityEngine;

/// <summary>
/// 체력 회복 포션 아이템
/// ConsumableItemBase를 상속받아 소비 시 체력을 회복합니다.
/// </summary>
public class HealthPotionItem : ConsumableItemBase {
    [Header("Potion Settings")]
    [SerializeField] private float healAmount = 30f;

    /// <summary>
    /// 포션 소비 시 체력 회복
    /// </summary>
    protected override void OnConsume(GameObject user) {
        Health health = user.GetComponent<Health>();
        
        if (health == null) {
            Debug.LogWarning($"[HealthPotionItem] {user.name}에 Health 컴포넌트가 없습니다.");
            return;
        }

        if (health.isDead) {
            Debug.Log("[HealthPotionItem] 사망 상태에서는 포션을 사용할 수 없습니다.");
            return;
        }

        health.Heal(healAmount);
        Debug.Log($"[HealthPotionItem] {user.name}의 체력을 {healAmount} 회복했습니다.");
    }
}
