using InventorySystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 용광로 시스템 매니저
/// - 입력 슬롯에 광석을 넣으면 3초 후 주괴로 변환
/// - 출력 슬롯은 드래그 불가 (읽기 전용)
/// </summary>
public class FurnaceManager : MonoBehaviour {
    public static FurnaceManager Instance { get; private set; }
    
    [Header("용광로 설정")]
    [Tooltip("용광로 인벤토리 이름 (InventoryController에 등록된 이름)")]
    [SerializeField] private string furnaceInventoryName = "Furnace";
    
    [Tooltip("녹이는 시간 (초)")]
    [SerializeField] private float smeltingTime = 3f;
    
    [Header("슬롯 설정")]
    [Tooltip("입력 슬롯 인덱스 (위쪽 슬롯)")]
    [SerializeField] private int inputSlotIndex = 0;
    
    [Tooltip("출력 슬롯 인덱스 (아래쪽 슬롯)")]
    [SerializeField] private int outputSlotIndex = 1;
    
    [Header("레시피 설정")]
    [Tooltip("용광로에서 사용할 모든 레시피")]
    [SerializeField] private List<FurnaceRecipe> allRecipes = new List<FurnaceRecipe>();
    
    [Header("UI 설정")]
    [Tooltip("진행 바 (선택사항)")]
    [SerializeField] private Image progressBar;
    
    // 레시피 매핑 (입력 아이템 타입 -> 레시피)
    private Dictionary<string, FurnaceRecipe> recipeDict = new Dictionary<string, FurnaceRecipe>();
    
    // 캐시
    private Inventory furnaceInventory;
    private GameObject furnaceUI;
    private bool isInitialized = false;
    
    // 현재 녹이는 중인지
    private bool isSmelting = false;
    private Coroutine smeltingCoroutine;
    
    // 슬롯 모니터링
    private Coroutine slotMonitorCoroutine;
    private InventoryItem lastInputItem;
    private InventoryItem lastOutputItem;
    
    private void Awake() {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject);
        }
    }
    
    private void Start() {
        // 약간의 딜레이 후 초기화 (InventoryController 초기화 대기)
        Invoke(nameof(Initialize), 0.2f);
    }
    
    private void Initialize() {
        if (InventoryController.instance == null) {
            Debug.LogError("[FurnaceManager] InventoryController를 찾을 수 없습니다.");
            // 재시도
            Invoke(nameof(Initialize), 0.5f);
            return;
        }
        
        // Furnace 인벤토리 찾기
        try {
            furnaceInventory = InventoryController.instance.GetInventory(furnaceInventoryName);
            if (furnaceInventory != null) {
                furnaceUI = furnaceInventory.GetUI();
                Debug.Log($"[FurnaceManager] {furnaceInventoryName} 인벤토리 연결됨");
            } else {
                Debug.LogError($"[FurnaceManager] {furnaceInventoryName} 인벤토리가 null입니다.");
                // 재시도
                Invoke(nameof(Initialize), 0.5f);
                return;
            }
        } catch (System.Exception e) {
            Debug.LogError($"[FurnaceManager] {furnaceInventoryName} 인벤토리를 찾을 수 없습니다. 오류: {e.Message}");
            Debug.LogWarning($"[FurnaceManager] InventoryController에 '{furnaceInventoryName}' 인벤토리가 등록되어 있는지 확인하세요.");
            // 재시도
            Invoke(nameof(Initialize), 0.5f);
            return;
        }
        
        // 레시피 딕셔너리 초기화
        foreach (var recipe in allRecipes) {
            if (recipe != null && !string.IsNullOrEmpty(recipe.inputItemType)) {
                recipeDict[recipe.inputItemType] = recipe;
            }
        }
        
        // 출력 슬롯 드래그 방지 설정
        SetupOutputSlot();
        
        // 슬롯 모니터링 시작
        slotMonitorCoroutine = StartCoroutine(MonitorSlots());
        
        isInitialized = true;
        Debug.Log($"[FurnaceManager] 초기화 완료. 레시피 수: {recipeDict.Count}");
    }
    
    /// <summary>
    /// 슬롯 변경 감지 모니터링
    /// </summary>
    private IEnumerator MonitorSlots() {
        while (true) {
            yield return new WaitForSeconds(0.1f);
            
            if (!isInitialized || furnaceInventory == null) continue;
            
            // 입력 슬롯 체크
            InventoryItem currentInputItem = furnaceInventory.InventoryGetItem(inputSlotIndex);
            if (currentInputItem != lastInputItem) {
                lastInputItem = currentInputItem;
                OnInputSlotChanged();
            }
            
            // 출력 슬롯 체크
            InventoryItem currentOutputItem = furnaceInventory.InventoryGetItem(outputSlotIndex);
            if (currentOutputItem != lastOutputItem) {
                lastOutputItem = currentOutputItem;
                OnOutputSlotChanged();
            }
        }
    }
    
    /// <summary>
    /// 출력 슬롯 드래그 방지 설정
    /// </summary>
    private void SetupOutputSlot() {
        if (furnaceUI == null) return;
        
        Slot outputSlot = GetSlotByIndex(outputSlotIndex);
        if (outputSlot != null) {
            // 출력 슬롯에 특별한 태그나 컴포넌트 추가하여 드래그 방지
            // 실제로는 슬롯 업데이트 시마다 체크하여 드래그를 막음
            StartCoroutine(MonitorOutputSlot());
        }
    }
    
    /// <summary>
    /// 출력 슬롯 모니터링 (드래그 방지)
    /// </summary>
    private IEnumerator MonitorOutputSlot() {
        while (true) {
            yield return new WaitForSeconds(0.05f);
            
            Slot outputSlot = GetSlotByIndex(outputSlotIndex);
            if (outputSlot != null) {
                var itemHolder = outputSlot.transform.Find("SlotItemHolder");
                if (itemHolder == null && outputSlot.transform.childCount > 0) {
                    itemHolder = outputSlot.transform.GetChild(0);
                }
                
                if (itemHolder != null && itemHolder.gameObject.activeSelf) {
                    var dragItem = itemHolder.GetComponent<DragItem>();
                    if (dragItem != null && dragItem.enabled) {
                        // 출력 슬롯의 드래그 비활성화
                        dragItem.enabled = false;
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// 슬롯 인덱스로 Slot 컴포넌트 찾기
    /// </summary>
    private Slot GetSlotByIndex(int index) {
        if (furnaceUI == null) return null;
        
        var slots = furnaceUI.GetComponentsInChildren<Slot>();
        foreach (var slot in slots) {
            if (slot.GetPosition() == index) {
                return slot;
            }
        }
        return null;
    }
    
    /// <summary>
    /// 입력 슬롯 아이템 변경 감지 및 녹이기 시작
    /// </summary>
    public void OnInputSlotChanged() {
        if (!isInitialized || isSmelting) return;
        
        InventoryItem inputItem = furnaceInventory.InventoryGetItem(inputSlotIndex);
        if (inputItem == null || inputItem.GetIsNull()) {
            return;
        }
        
        string itemType = inputItem.GetItemType();
        if (!recipeDict.ContainsKey(itemType)) {
            Debug.Log($"[FurnaceManager] {itemType}에 대한 레시피가 없습니다.");
            return;
        }
        
        // 출력 슬롯이 비어있는지 확인
        InventoryItem outputItem = furnaceInventory.InventoryGetItem(outputSlotIndex);
        if (outputItem != null && !outputItem.GetIsNull()) {
            Debug.Log("[FurnaceManager] 출력 슬롯이 비어있지 않습니다.");
            return;
        }
        
        // 녹이기 시작
        StartSmelting(recipeDict[itemType]);
    }
    
    /// <summary>
    /// 녹이기 시작
    /// </summary>
    private void StartSmelting(FurnaceRecipe recipe) {
        if (isSmelting) return;
        
        isSmelting = true;
        smeltingCoroutine = StartCoroutine(SmeltingCoroutine(recipe));
    }
    
    /// <summary>
    /// 녹이기 코루틴 (3초 대기 후 변환)
    /// </summary>
    private IEnumerator SmeltingCoroutine(FurnaceRecipe recipe) {
        float elapsed = 0f;
        
        while (elapsed < smeltingTime) {
            elapsed += Time.deltaTime;
            float progress = elapsed / smeltingTime;
            
            // 진행 바 업데이트
            if (progressBar != null) {
                progressBar.fillAmount = progress;
            }
            
            yield return null;
        }
        
        // 입력 아이템 소모
        if (InventoryController.instance != null) {
            InventoryController.instance.RemoveItemPos(furnaceInventoryName, inputSlotIndex, 1);
        }
        
        // 출력 아이템 추가
        AddOutputItem(recipe);
        
        // 진행 바 리셋
        if (progressBar != null) {
            progressBar.fillAmount = 0f;
        }
        
        isSmelting = false;
        Debug.Log($"[FurnaceManager] {recipe.outputDisplayName} 제작 완료!");
    }
    
    /// <summary>
    /// 출력 슬롯에 아이템 추가
    /// </summary>
    private void AddOutputItem(FurnaceRecipe recipe) {
        if (InventoryController.instance == null) return;
        
        // 출력 슬롯에 직접 추가
        InventoryController.instance.AddItemPos(furnaceInventoryName, recipe.outputItemType, outputSlotIndex, recipe.outputAmount);
    }
    
    /// <summary>
    /// 출력 슬롯 아이템 제거 시 처리 (플레이어가 가져간 경우)
    /// </summary>
    public void OnOutputSlotChanged() {
        if (!isInitialized) return;
        
        InventoryItem outputItem = furnaceInventory.InventoryGetItem(outputSlotIndex);
        if (outputItem == null || outputItem.GetIsNull()) {
            // 출력 슬롯이 비워지면 다시 녹이기 가능
            if (isSmelting && smeltingCoroutine != null) {
                StopCoroutine(smeltingCoroutine);
                isSmelting = false;
                if (progressBar != null) {
                    progressBar.fillAmount = 0f;
                }
            }
            
            // 입력 슬롯에 아이템이 있으면 다시 녹이기 시작
            OnInputSlotChanged();
        }
    }
    
    /// <summary>
    /// 특정 아이템 타입에 대한 레시피 가져오기
    /// </summary>
    public FurnaceRecipe GetRecipe(string inputItemType) {
        return recipeDict.ContainsKey(inputItemType) ? recipeDict[inputItemType] : null;
    }
    
    /// <summary>
    /// 모든 레시피 가져오기
    /// </summary>
    public List<FurnaceRecipe> GetAllRecipes() {
        return new List<FurnaceRecipe>(allRecipes);
    }
}
