using UnityEngine;
using UnityEngine.UI; // UI 연결 시 필요

public class PlayerErosion : MonoBehaviour {
    [Header("Erosion Settings")]
    [SerializeField] private float maxErosion = 100f;
    [SerializeField] private float currentErosion = 0f;
    [SerializeField] private float naturalRecoveryRate = 1f; // 안전 지대에서 초당 회복량
    
    [Header("Max Erosion Damage Settings")]
    [SerializeField] private float damagePerSecond = 1f; // 침식도 100%일 때 초당 데미지

    private bool isInContaminatedArea = false;
    private bool isInRecoveryArea = false;
    private Health healthComponent;
    private float damageTimer = 0f;
    private const float DAMAGE_INTERVAL = 1f; // 1초 간격으로 데미지

    void Awake() {
        healthComponent = GetComponent<Health>();
    }

    void Update() {
        // 세계수 주위일 떄는 때는 서서히 회복
        if (isInRecoveryArea && currentErosion > 0) {
            AddErosion(-naturalRecoveryRate * Time.deltaTime);
        }

        // 침식도가 100%일 때 초당 1씩 HP 감소
        if (IsErosionMaxed()) {
            ApplyMaxErosionDamage();
        } else {
            damageTimer = 0f; // 침식도가 100% 미만이면 타이머 리셋
        }
    }

    // 침식 수치를 변경하는 공용 함수 (몬스터 공격, 지역 침식 모두 사용)
    public void AddErosion(float amount) {
        currentErosion = Mathf.Clamp(currentErosion + amount, 0, maxErosion);
        
        if (currentErosion >= maxErosion) {
            OnErosionMax();
        }
    }

    public void SetInRecoveryArea(bool status) {
        isInRecoveryArea = status;
    }

    private void OnErosionMax() {
        Debug.Log("플레이어가 완전히 침식되었습니다! 초당 1씩 HP가 감소합니다.");
    }

    private bool IsErosionMaxed() {
        return currentErosion >= maxErosion;
    }

    private void ApplyMaxErosionDamage() {
        if (healthComponent == null) return;

        damageTimer += Time.deltaTime;
        
        // 1초마다 데미지 적용
        if (damageTimer >= DAMAGE_INTERVAL) {
            healthComponent.TakeDamage(damagePerSecond);
            damageTimer = 0f;
        }
    }

    public void SetInContaminatedArea(bool status) {
        isInContaminatedArea = status;
    }

    public float GetErosionPercent() => currentErosion / maxErosion;
}