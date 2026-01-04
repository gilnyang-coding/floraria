using UnityEngine;
using System.Collections;
using InventorySystem;

public class ItemDropper : MonoBehaviour {
    public static ItemDropper instance;
    private int lastDropFrame = -1; 

    [Header("설정")]
    public Transform playerTransform; 
    public float dropDistance = 1.2f; // 너무 멀지 않게 조절
    public float dropHeight = 0.8f;   // 살짝 낮은 높이에서 툭

    void Awake() {
        if (instance == null) instance = this;
        
        if (playerTransform == null) {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) playerTransform = player.transform;
        }
    }

    public void DropItem(Vector3 mousePos, InventoryItem item) {
        if (Time.frameCount == lastDropFrame) return;
        lastDropFrame = Time.frameCount;

        if (item == null || item.GetIsNull()) return;

        // [중요] 버릴 아이템의 현재 스택 개수를 저장합니다.
        int dropCount = item.GetAmount(); 
        GameObject itemPrefab = item.GetRelatedGameObject();

        if (itemPrefab != null) {
            // 플레이어 방향 가져오기 - 플레이어가 없으면 카메라 방향 사용
            Transform origin = null;
            Vector3 forwardDir = Vector3.forward;
            
            if (playerTransform != null) {
                origin = playerTransform;
                forwardDir = playerTransform.forward;
            } else {
                // 플레이어를 다시 찾아보기
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null) {
                    origin = player.transform;
                    forwardDir = player.transform.forward;
                    playerTransform = origin;
                } else if (Camera.main != null) {
                    origin = Camera.main.transform;
                    // 카메라의 forward를 수평면으로 투영 (Y 제거)
                    forwardDir = Vector3.ProjectOnPlane(Camera.main.transform.forward, Vector3.up).normalized;
                }
            }
            
            // 디버그: 방향 확인
            Debug.Log($"[ItemDropper] 플레이어 방향: {forwardDir}, 플레이어 Transform: {(origin != null ? origin.name : "null")}");
            
            Vector3 spawnPos = origin.position + (forwardDir * dropDistance) + (Vector3.up * dropHeight);

            // 1. 개수만큼 실제 오브젝트 생성
            // 각 아이템 생성 전에 플래그 설정 (Start()가 프레임 끝에 호출되므로)
            BaseItem.playerForwardDirection = forwardDir;
            BaseItem.usePlayerDirection = true;
            
            for (int i = 0; i < dropCount; i++) {
                // 플레이어 앞으로 일정한 간격을 두어 생성
                Vector3 itemOffset = forwardDir * (i * 0.1f);
                Vector3 itemPos = spawnPos + itemOffset;
                GameObject droppedItem = Instantiate(itemPrefab, itemPos, Quaternion.identity);
            }
            
            // 모든 아이템 생성 후 다음 프레임에 플래그 초기화 (Start()가 호출된 후)
            StartCoroutine(ResetPlayerDirectionFlag());

            // 2. 인벤토리에서 '전체 개수'만큼 제거
            InventoryController.instance.RemoveItem(item.GetInventory(), item, dropCount);
            
            // 3. UI 강제 갱신 명령 (중요!)
            // 아이템이 속한 인벤토리의 UI를 다시 그리라고 시킵니다.
            UpdateUI(item.GetInventory());
            
            Debug.Log($"[스택 드롭] {item.GetItemType()} {dropCount}개를 버렸습니다.");
        }
    }

    // UI를 강제로 새로고침하는 보조 함수
    private void UpdateUI(string inventoryName) {
        // 하이라키에서 해당 이름의 인벤토리 UI를 찾아 업데이트 함수를 호출합니다.
        GameObject invObj = GameObject.Find(inventoryName);
        if (invObj != null) {
            var uiManager = invObj.GetComponent<InventoryUIManager>();
            if (uiManager != null) {
                uiManager.UpdateInventoryUI(); // 전체 UI 갱신
            }
        }
    }
    
    // 모든 아이템의 Start()가 호출된 후 플래그 초기화
    private IEnumerator ResetPlayerDirectionFlag() {
        yield return null; // 다음 프레임까지 대기 (모든 Start()가 호출된 후)
        BaseItem.usePlayerDirection = false;
        BaseItem.playerForwardDirection = null;
    }
}