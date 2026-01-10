using UnityEngine;
using System.Collections;

/// <summary>
/// 생고기 아이템
/// 생존형 소모품 - HP +10, 이동속도 20% 감소 (일시적)
/// </summary>
public class RawSteakItem : ConsumableItemBase {
    [Header("Raw Steak Settings")]
    [SerializeField] private float healAmount = 10f;
    [SerializeField] private float speedReductionPercent = 0.2f; // 20% 감소
    [SerializeField] private float debuffDuration = 30f; // 30초 동안 지속

    /// <summary>
    /// 생고기 소비 시 HP 회복 및 이동속도 감소 디버프 적용
    /// </summary>
    protected override void OnConsume(GameObject user) {
        Health health = user.GetComponent<Health>();
        PlayerMove playerMove = user.GetComponent<PlayerMove>();
        
        if (health == null) {
            Debug.LogWarning($"[RawSteakItem] {user.name}에 Health 컴포넌트가 없습니다.");
            return;
        }

        if (health.isDead) {
            Debug.Log("[RawSteakItem] 사망 상태에서는 생고기를 먹을 수 없습니다.");
            return;
        }

        // HP 회복
        health.Heal(healAmount);
        
        // 이동속도 감소 디버프 적용
        if (playerMove != null) {
            playerMove.AddSpeedMultiplier(1f - speedReductionPercent, debuffDuration);
            Debug.Log($"[RawSteakItem] {user.name}이(가) 생고기를 먹었습니다. HP +{healAmount}, 이동속도 {speedReductionPercent * 100}% 감소 ({debuffDuration}초)");
        } else {
            Debug.LogWarning($"[RawSteakItem] {user.name}에 PlayerMove 컴포넌트가 없습니다.");
            Debug.Log($"[RawSteakItem] {user.name}이(가) 생고기를 먹었습니다. HP +{healAmount}");
        }
    }
}
