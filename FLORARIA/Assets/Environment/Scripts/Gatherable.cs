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
        // 플레이어 방향 가져오기
        Vector3 playerForward = Vector3.forward; // 기본값
        if (playerRef != null) {
            playerForward = playerRef.transform.forward;
        } else {
            // 플레이어를 찾을 수 없으면 태그로 찾기
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) {
                playerForward = player.transform.forward;
            }
        }
        
        // BaseItem의 LaunchItem()이 플레이어 방향을 사용하도록 설정
        BaseItem.playerForwardDirection = playerForward;
        BaseItem.usePlayerDirection = true;
        
        for (int i = 0; i < dropCount; i++) {
            // 1. 위치: 플레이어 앞으로 약간의 위치 오차만 줌
            Vector3 spawnOffset = new Vector3(Random.Range(-0.1f, 0.1f), 0.5f, Random.Range(-0.1f, 0.1f));
            Vector3 spawnPos = transform.position + spawnOffset;
            
            GameObject item = Instantiate(itemPrefab, spawnPos, Quaternion.identity);

            // BaseItem의 Start()에서 LaunchItem()이 호출되므로 여기서는 힘을 추가하지 않음
            // LaunchItem()이 플레이어 방향을 사용하도록 이미 설정했음
        }
        
        // 모든 아이템 생성 후 플래그 초기화
        BaseItem.usePlayerDirection = false;
        BaseItem.playerForwardDirection = null;
    }

    // 시간제 방식이므로 단발성 OnInteract는 사용하지 않음
    public override void OnInteract(GameObject player) {}
}