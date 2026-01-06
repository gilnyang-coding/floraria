using InventorySystem;
using System.Collections;
using UnityEngine;

/// <summary>
/// 아이템 사용을 중앙에서 관리하는 싱글톤 매니저
/// - 캐스팅 타임 (버튼을 꾹 누르고 있어야 사용됨)
/// - 생존용 소모품 쿨타임 (3초 공유)
/// - 핫바/인벤토리 양쪽에서 사용 요청 처리
/// </summary>
public class ItemUsageManager : MonoBehaviour {
    public static ItemUsageManager Instance { get; private set; }

    [Header("Usage Settings")]
    [SerializeField] private float castingTime = 1f; // 사용 대기 시간 (꾹 누르고 있어야 함)
    [SerializeField] private KeyCode useItemKey = KeyCode.H; // 핫바 아이템 사용 키
    
    [Header("Cooldown Settings")]
    [SerializeField] private float survivalCooldown = 3f; // 생존용 소모품 공유 쿨타임
    
    // 현재 캐스팅 중인지
    private bool isCasting = false;
    private Coroutine castingCoroutine;
    
    // 캐스팅 입력 타입
    private enum InputType { None, Keyboard, Mouse }
    private InputType currentInputType = InputType.None;
    
    // 생존용 소모품 쿨타임 추적
    private float survivalCooldownRemaining = 0f;
    
    // 이벤트
    public event System.Action<float> OnCastingStarted; // 캐스팅 시간 전달
    public event System.Action OnCastingCompleted;
    public event System.Action OnCastingCancelled;
    public event System.Action<float> OnSurvivalCooldownStarted; // 쿨다운 시간 전달
    public event System.Action OnSurvivalCooldownEnded;

    private void Awake() {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject);
        }
    }

    private void Update() {
        // 생존용 소모품 쿨타임 감소 (Time.timeScale 영향 안 받음)
        if (survivalCooldownRemaining > 0f) {
            survivalCooldownRemaining -= Time.unscaledDeltaTime;
            if (survivalCooldownRemaining <= 0f) {
                survivalCooldownRemaining = 0f;
                OnSurvivalCooldownEnded?.Invoke();
            }
        }
    }

    /// <summary>
    /// 핫바에서 아이템 사용 요청 (H키)
    /// </summary>
    public void UseItemFromHotbar(InventoryItem item, string inventoryName, GameObject user) {
        if (item == null || item.GetIsNull()) return;
        
        TryUseItem(item, inventoryName, user, InputType.Keyboard);
    }

    /// <summary>
    /// 인벤토리에서 아이템 사용 요청 (좌클릭)
    /// </summary>
    public void UseItemFromInventory(InventoryItem item) {
        if (item == null || item.GetIsNull()) return;
        
        // 플레이어 찾기
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) {
            Debug.LogWarning("[ItemUsageManager] Player를 찾을 수 없습니다.");
            return;
        }
        
        TryUseItem(item, item.GetInventory(), player, InputType.Mouse);
    }

    /// <summary>
    /// 아이템 사용 시도 (캐스팅 타임 + 쿨타임 적용)
    /// </summary>
    private void TryUseItem(InventoryItem item, string inventoryName, GameObject user, InputType inputType) {
        // 이미 캐스팅 중이면 무시
        if (isCasting) {
            Debug.Log("[ItemUsageManager] 이미 아이템을 사용 중입니다.");
            return;
        }

        // Consumable인지 확인
        GameObject relatedObj = item.GetRelatedGameObject();
        if (relatedObj == null) {
            Debug.Log($"[ItemUsageManager] {item.GetItemType()}은 사용할 수 없는 아이템입니다.");
            return;
        }

        // IConsumable + ConsumableItemBase 확인
        ConsumableItemBase consumableBase = relatedObj.GetComponent<ConsumableItemBase>();
        if (consumableBase == null) {
            Debug.Log($"[ItemUsageManager] {item.GetItemType()}은 소비 아이템이 아닙니다.");
            return;
        }

        // 생존용 소모품 쿨타임 체크
        if (consumableBase.GetConsumableType() == ConsumableType.Survival) {
            if (survivalCooldownRemaining > 0f) {
                Debug.Log($"[ItemUsageManager] 생존용 소모품 쿨타임 중입니다. ({survivalCooldownRemaining:F1}초 남음)");
                return;
            }
        }

        // 캐스팅 시작
        currentInputType = inputType;
        castingCoroutine = StartCoroutine(CastingCoroutine(item, inventoryName, user, consumableBase));
    }

    /// <summary>
    /// 캐스팅 코루틴 - 버튼을 꾹 누르고 있어야 사용됨
    /// </summary>
    private IEnumerator CastingCoroutine(InventoryItem item, string inventoryName, GameObject user, ConsumableItemBase consumable) {
        isCasting = true;
        OnCastingStarted?.Invoke(castingTime);
        
        Debug.Log($"[ItemUsageManager] {item.GetItemType()} 사용 준비 중... ({castingTime}초 동안 누르고 있으세요)");
        
        // 마우스 입력의 경우: OnPointerClick이 버튼 뗄 때 발생하므로
        // 다시 버튼을 누를 때까지 대기
        if (currentInputType == InputType.Mouse) {
            // 버튼이 눌릴 때까지 대기
            while (!Input.GetMouseButton(0)) {
                yield return null;
            }
        }
        
        float elapsedTime = 0f;
        
        // 캐스팅 시간 동안 버튼이 눌려있는지 체크
        while (elapsedTime < castingTime) {
            // 입력이 풀렸는지 체크
            if (!IsInputHeld()) {
                // 버튼을 뗐으면 캐스팅 취소
                Debug.Log("[ItemUsageManager] 버튼을 떼서 사용이 취소되었습니다.");
                isCasting = false;
                currentInputType = InputType.None;
                OnCastingCancelled?.Invoke();
                castingCoroutine = null;
                yield break;
            }
            
            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }
        
        // 캐스팅 완료 - 아이템 사용
        consumable.Consume(user);
        
        // 인벤토리에서 아이템 제거
        if (InventoryController.instance != null) {
            InventoryController.instance.RemoveItem(inventoryName, item, 1);
        }
        
        // 생존용 소모품이면 쿨타임 시작
        if (consumable.GetConsumableType() == ConsumableType.Survival) {
            StartSurvivalCooldown();
        }
        
        Debug.Log($"[ItemUsageManager] {item.GetItemType()} 사용 완료!");
        
        isCasting = false;
        currentInputType = InputType.None;
        OnCastingCompleted?.Invoke();
        castingCoroutine = null;
    }

    /// <summary>
    /// 현재 입력이 계속 눌려있는지 체크
    /// </summary>
    private bool IsInputHeld() {
        switch (currentInputType) {
            case InputType.Keyboard:
                return Input.GetKey(useItemKey);
            case InputType.Mouse:
                return Input.GetMouseButton(0); // 좌클릭
            default:
                return false;
        }
    }

    /// <summary>
    /// 생존용 소모품 쿨타임 시작
    /// </summary>
    private void StartSurvivalCooldown() {
        survivalCooldownRemaining = survivalCooldown;
        OnSurvivalCooldownStarted?.Invoke(survivalCooldown);
        Debug.Log($"[ItemUsageManager] 생존용 소모품 쿨타임 시작 ({survivalCooldown}초)");
    }

    /// <summary>
    /// 캐스팅 취소
    /// </summary>
    public void CancelCasting() {
        if (castingCoroutine != null) {
            StopCoroutine(castingCoroutine);
            castingCoroutine = null;
            isCasting = false;
            currentInputType = InputType.None;
            OnCastingCancelled?.Invoke();
            Debug.Log("[ItemUsageManager] 아이템 사용이 취소되었습니다.");
        }
    }

    /// <summary>
    /// 현재 캐스팅 중인지 반환
    /// </summary>
    public bool IsCasting() => isCasting;
    
    /// <summary>
    /// 생존용 소모품 쿨타임 중인지 반환
    /// </summary>
    public bool IsSurvivalOnCooldown() => survivalCooldownRemaining > 0f;
    
    /// <summary>
    /// 생존용 소모품 남은 쿨타임 반환
    /// </summary>
    public float GetSurvivalCooldownRemaining() => survivalCooldownRemaining;
    
    /// <summary>
    /// 캐스팅 시간 설정
    /// </summary>
    public void SetCastingTime(float time) => castingTime = Mathf.Max(0f, time);
    
    /// <summary>
    /// 생존용 소모품 쿨타임 설정
    /// </summary>
    public void SetSurvivalCooldown(float time) => survivalCooldown = Mathf.Max(0f, time);
}
