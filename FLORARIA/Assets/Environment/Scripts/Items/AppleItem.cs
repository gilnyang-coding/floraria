using UnityEngine;

/// <summary>
/// 사과 아이템
/// 생존형 소모품 - HP +10, 수분 +30
/// </summary>
public class AppleItem : ConsumableItemBase {
    [Header("Apple Settings")]
    [SerializeField] private float healAmount = 10f;
    [SerializeField] private float hydrationAmount = 30f;

    /// <summary>
    /// 사과 소비 시 HP와 수분 회복
    /// </summary>
    protected override void OnConsume(GameObject user) {
        Health health = user.GetComponent<Health>();
        Hydration hydration = user.GetComponent<Hydration>();
        
        if (health == null) {
            Debug.LogWarning($"[AppleItem] {user.name}에 Health 컴포넌트가 없습니다.");
            return;
        }

        if (health.isDead) {
            Debug.Log("[AppleItem] 사망 상태에서는 사과를 먹을 수 없습니다.");
            return;
        }

        // HP 회복
        health.Heal(healAmount);
        
        // 수분 회복
        if (hydration != null) {
            hydration.Replenish(hydrationAmount);
        } else {
            Debug.LogWarning($"[AppleItem] {user.name}에 Hydration 컴포넌트가 없습니다.");
        }

        Debug.Log($"[AppleItem] {user.name}이(가) 사과를 먹었습니다. HP +{healAmount}, 수분 +{hydrationAmount}");
    }
}

