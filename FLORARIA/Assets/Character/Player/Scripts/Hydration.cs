using UnityEngine;

/// <summary>
/// 플레이어의 수분 상태를 관리하는 컴포넌트
/// - 시간에 따라 자동 감소 (분당 1)
/// - 대쉬 등 행동에 따른 감소
/// - 수분 30 이하: 이동속도 20% 감소
/// - 수분 0: 초당 1씩 HP 감소
/// </summary>
public class Hydration : MonoBehaviour {
    [Header("Hydration Settings")]
    [SerializeField] private float maxHydration = 100f;
    [SerializeField] private float currentHydration;
    
    [Header("Decay Settings")]
    [SerializeField] private float decayPerMinute = 1f; // 분당 감소량
    
    [Header("Action Costs")]
    [SerializeField] private float dashCost = 3f; // 대쉬 1회당 소모량
    
    [Header("Debuff Settings")]
    [SerializeField] private float lowHydrationThreshold = 30f; // 이 이하면 이동속도 감소
    [SerializeField] private float speedReductionPercent = 0.2f; // 20% 감소
    [SerializeField] private float dehydrationDamagePerSecond = 1f; // 탈수 시 초당 HP 감소
    
    // 컴포넌트 캐싱
    private Health health;
    
    // 이벤트
    public event System.Action<float, float> OnHydrationChanged; // (현재, 최대)
    public event System.Action OnDehydrated; // 수분 고갈 시
    public event System.Action OnLowHydration; // 수분 부족 상태 진입 시
    public event System.Action OnHydrationRestored; // 수분 부족 상태 해제 시
    
    // 상태 속성
    public bool IsDehydrated => currentHydration <= 0f;
    public bool IsLowHydration => currentHydration <= lowHydrationThreshold;
    public float CurrentHydration => currentHydration;
    public float MaxHydration => maxHydration;
    public float HydrationPercent => currentHydration / maxHydration;
    
    /// <summary>
    /// 이동속도 배율 반환 (수분 부족 시 0.8, 정상 시 1.0)
    /// </summary>
    public float SpeedMultiplier => IsLowHydration ? (1f - speedReductionPercent) : 1f;

    private bool wasLowHydration = false;

    private void Awake() {
        currentHydration = maxHydration;
        health = GetComponent<Health>();
    }

    private void Update() {
        // 시간에 따른 자연 감소 (분당 decayPerMinute)
        float decayPerSecond = decayPerMinute / 60f;
        DecreaseHydration(decayPerSecond * Time.deltaTime);
        
        // 탈수 상태일 때 HP 감소
        if (IsDehydrated && health != null && !health.isDead) {
            health.TakeDamage(dehydrationDamagePerSecond * Time.deltaTime);
        }
        
        // 수분 부족 상태 변화 감지
        CheckLowHydrationState();
    }

    /// <summary>
    /// 수분 부족 상태 변화 감지 및 이벤트 발생
    /// </summary>
    private void CheckLowHydrationState() {
        if (IsLowHydration && !wasLowHydration) {
            wasLowHydration = true;
            OnLowHydration?.Invoke();
            Debug.Log($"[Hydration] 수분 부족! 이동속도 {speedReductionPercent * 100}% 감소");
        }
        else if (!IsLowHydration && wasLowHydration) {
            wasLowHydration = false;
            OnHydrationRestored?.Invoke();
            Debug.Log("[Hydration] 수분 회복! 이동속도 정상화");
        }
    }

    /// <summary>
    /// 수분 감소 (내부용)
    /// </summary>
    private void DecreaseHydration(float amount) {
        float previousHydration = currentHydration;
        currentHydration = Mathf.Max(0f, currentHydration - amount);
        
        // 변화가 있을 때만 이벤트 발생
        if (Mathf.Abs(previousHydration - currentHydration) > 0.01f) {
            OnHydrationChanged?.Invoke(currentHydration, maxHydration);
        }
        
        if (currentHydration <= 0f && previousHydration > 0f) {
            OnDehydrated?.Invoke();
            Debug.Log($"[Hydration] {gameObject.name} 탈수 상태! 초당 {dehydrationDamagePerSecond} HP 감소");
        }
    }

    /// <summary>
    /// 대쉬 시 수분 소모
    /// </summary>
    public void OnDash() {
        DecreaseHydration(dashCost);
        Debug.Log($"[Hydration] 대쉬로 수분 -{dashCost}, 현재: {currentHydration:F1}/{maxHydration}");
    }

    /// <summary>
    /// 특정 행동으로 수분 소모 (범용)
    /// </summary>
    public void Consume(float amount) {
        DecreaseHydration(Mathf.Max(0f, amount));
    }

    /// <summary>
    /// 수분 보충
    /// </summary>
    public void Replenish(float amount) {
        if (amount <= 0f) return;
        
        float previousHydration = currentHydration;
        currentHydration = Mathf.Min(currentHydration + amount, maxHydration);
        
        if (currentHydration != previousHydration) {
            OnHydrationChanged?.Invoke(currentHydration, maxHydration);
            Debug.Log($"[Hydration] 수분 보충 +{amount}, 현재: {currentHydration:F1}/{maxHydration}");
        }
    }

    /// <summary>
    /// 수분 완전 회복
    /// </summary>
    public void ReplenishFull() {
        Replenish(maxHydration - currentHydration);
    }
}
