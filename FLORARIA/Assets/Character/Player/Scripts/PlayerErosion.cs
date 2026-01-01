using UnityEngine;
using UnityEngine.UI; // UI 연결 시 필요

public class PlayerErosion : MonoBehaviour {
    [Header("Erosion Settings")]
    [SerializeField] private float maxErosion = 100f;
    [SerializeField] private float currentErosion = 0f;
    [SerializeField] private float naturalRecoveryRate = 1f; // 안전 지대에서 초당 회복량

    private bool isInContaminatedArea = false;
    private bool isInRecoveryArea = false;

    void Update() {
        // 세계수 주위일 떄는 때는 서서히 회복
        if (isInRecoveryArea && currentErosion > 0) {
            AddErosion(-naturalRecoveryRate * Time.deltaTime);
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
        Debug.Log("플레이어가 완전히 침식되었습니다! 데미지를 입거나 디버프가 발생합니다.");
        // 여기에 HP 감소나 이동 속도 저하 로직 추가
    }

    public void SetInContaminatedArea(bool status) {
        isInContaminatedArea = status;
    }

    public float GetErosionPercent() => currentErosion / maxErosion;
}