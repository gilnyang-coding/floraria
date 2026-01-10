using UnityEngine;

/// <summary>
/// 익힌 고기 아이템
/// 생존형 소모품 - HP +20
/// </summary>
public class CookedSteakItem : ConsumableItemBase {
    [Header("Cooked Steak Settings")]
    [SerializeField] private float healAmount = 20f;

    /// <summary>
    /// 익힌 고기 소비 시 HP 회복
    /// </summary>
    protected override void OnConsume(GameObject user) {
        Health health = user.GetComponent<Health>();
        
        if (health == null) {
            Debug.LogWarning($"[CookedSteakItem] {user.name}에 Health 컴포넌트가 없습니다.");
            return;
        }

        if (health.isDead) {
            Debug.Log("[CookedSteakItem] 사망 상태에서는 익힌 고기를 먹을 수 없습니다.");
            return;
        }

        // HP 회복
        health.Heal(healAmount);
        
        Debug.Log($"[CookedSteakItem] {user.name}이(가) 익힌 고기를 먹었습니다. HP +{healAmount}");
    }
}
