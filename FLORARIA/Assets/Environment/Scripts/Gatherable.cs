using UnityEngine;
using UnityEngine.InputSystem;

public class Gatherable : BaseInteractable {
    [Header("Gather Settings")]
    public float timeToGather = 3.0f;    // 채집에 걸리는 시간 (초)
    private float gatherProgress = 0f;  // 현재 진행 시간
    
    public GameObject itemPrefab;       // 떨어질 아이템 프리팹
    public int dropCount = 5;           // 드랍 개수

    // 부모의 Update를 덮어씌워(Override) "누르고 있기" 로직을 구현.
    protected override void Update() {
        // 1. 아이콘 빌보드 로직
        if (isPlayerInRange && interactionIcon != null) {
            interactionIcon.transform.LookAt(Camera.main.transform);
            
            // 2. F 키를 꾹 누르고 있는지 확인
            if (Keyboard.current != null && Keyboard.current.fKey.isPressed) {
                UpdateGathering();
            }
            else {
                // 누르지 않고 있으면 진행도 초기화
                gatherProgress = 0f;
            }
        }
        else {
            gatherProgress = 0f;
        }
    }

    private void UpdateGathering() {
        gatherProgress += Time.deltaTime; // 프레임 시간만큼 진행도 추가
        // 진행률 확인
        Debug.Log($"채집 중... {Mathf.Round((gatherProgress / timeToGather) * 100)}%");
        if (gatherProgress >= timeToGather) {
            OnCompleteGathering();
        }
    }

    private void OnCompleteGathering() {
        Debug.Log($"{gameObject.name} 채집 완료!");
        SpawnItems();
        Destroy(gameObject); // 나무 제거
    }

private void SpawnItems() {
    for (int i = 0; i < dropCount; i++) {
        //아주 약간의 위치 오차를 주어 겹치지 않게 생성
        Vector3 spawnPos = transform.position + new Vector3(Random.Range(-0.1f, 0.1f), 1.0f, Random.Range(-0.1f, 0.1f));
        
        //생성 시 랜덤한 회전값을 주어 더 자연스럽게 함
        Quaternion randomRot = Quaternion.Euler(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360));
        GameObject item = Instantiate(itemPrefab, spawnPos, randomRot);

        //물리 힘 가하기
        Rigidbody rb = item.GetComponent<Rigidbody>();
        if (rb != null) {
            // 위쪽(Y)으로 튀어오르면서 사방(X, Z)으로 퍼지는 힘 계산
            Vector3 forceDir = new Vector3(Random.Range(-1f, 1f), 1.5f, Random.Range(-1f, 1f)).normalized;
            float forcePower = Random.Range(4f, 7f); // 힘의 세기 조절
            
            // 순간적인 힘(Impulse)을 가함
            rb.AddForce(forceDir * forcePower, ForceMode.Impulse);

            // 무작위 회전력 추가 (떼구르르 구르는 효과)
            Vector3 torqueDir = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));
            rb.AddTorque(torqueDir * 5f, ForceMode.Impulse);
        }
    }
}

    // 시간제 방식이므로 단발성 OnInteract는 사용하지 않음
    public override void OnInteract(GameObject player) {}
}