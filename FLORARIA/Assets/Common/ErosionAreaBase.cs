using UnityEngine;

public abstract class ErosionAreaBase : MonoBehaviour {
    [SerializeField] protected float amountPerSecond = 5f; // 초당 변화량

    // 플레이어가 트리거 안에 있을 때 공통적으로 실행될 로직
    protected virtual void OnTriggerStay(Collider other) {
        if (other.CompareTag("Player")) {
            PlayerErosion erosion = other.GetComponent<PlayerErosion>();
            if (erosion != null) {
                ApplyEffect(erosion);
            }
        }
    }

    // 자식 클래스에서 "수치를 올릴지 내릴지" 구체적으로 정의할 함수
    protected abstract void ApplyEffect(PlayerErosion erosion);

    // 트리거에서 나갈 때의 공통 로직
    protected virtual void OnTriggerExit(Collider other) {
        if (other.CompareTag("Player")) {
            PlayerErosion erosion = other.GetComponent<PlayerErosion>();
            if (erosion != null) {
                ResetStatus(erosion);
            }
        }
    }

    protected abstract void ResetStatus(PlayerErosion erosion);
}