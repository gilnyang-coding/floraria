using InventorySystem;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
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
    [Tooltip("입력 슬롯 인덱스 (아래쪽 슬롯)")]
    [SerializeField] private int inputSlotIndex = 1;
    
    [Tooltip("출력 슬롯 인덱스 (위쪽 슬롯)")]
    [SerializeField] private int outputSlotIndex = 0;
    
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
        
        // 초기 슬롯 상태 저장
        lastInputItem = furnaceInventory.InventoryGetItem(inputSlotIndex);
        lastOutputItem = furnaceInventory.InventoryGetItem(outputSlotIndex);
        
        // 슬롯 모니터링 시작
        slotMonitorCoroutine = StartCoroutine(MonitorSlots());
        
        isInitialized = true;
    }
    
    /// <summary>
    /// 입력 슬롯에 아이템을 넣을 수 있는지 확인 (레시피에 등록된 아이템만)
    /// </summary>
    public bool CanAcceptItemInInputSlot(string itemType) {
        return recipeDict.ContainsKey(itemType);
    }
    
    /// <summary>
    /// 입력 슬롯 인덱스 가져오기 (DragItem에서 사용)
    /// </summary>
    public int GetInputSlotIndex() {
        return inputSlotIndex;
    }
    
    /// <summary>
    /// 슬롯 변경 감지 모니터링 (더 빠른 감지)
    /// </summary>
    private IEnumerator MonitorSlots() {
        while (true) {
            yield return new WaitForSeconds(0.1f);
            
            if (!isInitialized || furnaceInventory == null) continue;
            
            // 입력 슬롯 체크
            InventoryItem currentInputItem = furnaceInventory.InventoryGetItem(inputSlotIndex);
            bool inputChanged = false;
            
            // null 체크
            if (currentInputItem == null) {
                if (lastInputItem != null) {
                    inputChanged = true;
                }
            } else if (lastInputItem == null) {
                inputChanged = true;
            } else {
                // 둘 다 null이 아닌 경우
                bool currentIsNull = currentInputItem.GetIsNull();
                bool lastIsNull = lastInputItem.GetIsNull();
                
                if (currentIsNull != lastIsNull) {
                    inputChanged = true;
                } else if (!currentIsNull && !lastIsNull) {
                    // 둘 다 아이템이 있는 경우 타입 비교
                    if (currentInputItem.GetItemType() != lastInputItem.GetItemType()) {
                        inputChanged = true;
                    }
                }
            }
            
            if (inputChanged) {
                lastInputItem = currentInputItem;
                OnInputSlotChanged();
            }
            
            // 출력 슬롯 체크
            InventoryItem currentOutputItem = furnaceInventory.InventoryGetItem(outputSlotIndex);
            bool outputChanged = false;
            
            if (currentOutputItem == null) {
                if (lastOutputItem != null) {
                    outputChanged = true;
                }
            } else if (lastOutputItem == null) {
                outputChanged = true;
            } else {
                bool currentIsNull = currentOutputItem.GetIsNull();
                bool lastIsNull = lastOutputItem.GetIsNull();
                
                if (currentIsNull != lastIsNull) {
                    outputChanged = true;
                } else if (!currentIsNull && !lastIsNull) {
                    if (currentOutputItem.GetItemType() != lastOutputItem.GetItemType()) {
                        outputChanged = true;
                    }
                }
            }
            
            if (outputChanged) {
                lastOutputItem = currentOutputItem;
                OnOutputSlotChanged();
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
    /// 슬롯 업데이트 시 호출 (InventoryUIManager에서 호출)
    /// </summary>
    public void OnSlotUpdated(int slotIndex) {
        if (!isInitialized) return;
        
        if (slotIndex == inputSlotIndex) {
            // 입력 슬롯 변경
            OnInputSlotChanged();
        } else if (slotIndex == outputSlotIndex) {
            // 출력 슬롯 변경
            OnOutputSlotChanged();
        }
    }
    
    /// <summary>
    /// 입력 슬롯 아이템 변경 감지 및 녹이기 시작
    /// </summary>
    public void OnInputSlotChanged() {
        if (!isInitialized) return;
        
        if (isSmelting) return;
        
        InventoryItem inputItem = furnaceInventory.InventoryGetItem(inputSlotIndex);
        if (inputItem == null || inputItem.GetIsNull()) {
            // 입력 슬롯이 비워지면 녹이기 중단
            if (isSmelting && smeltingCoroutine != null) {
                StopCoroutine(smeltingCoroutine);
                isSmelting = false;
                if (progressBar != null) {
                    progressBar.fillAmount = 0f;
                }
            }
            return;
        }
        
        string itemType = inputItem.GetItemType();
        if (!recipeDict.ContainsKey(itemType)) {
            return;
        }
        
        // 출력 슬롯이 비어있는지 확인
        InventoryItem outputItem = furnaceInventory.InventoryGetItem(outputSlotIndex);
        if (outputItem != null && !outputItem.GetIsNull()) {
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
    /// Time.timeScale에 영향받지 않도록 Time.unscaledDeltaTime 사용
    /// </summary>
    private IEnumerator SmeltingCoroutine(FurnaceRecipe recipe) {
        float elapsed = 0f;
        
        while (elapsed < smeltingTime) {
            // Time.timeScale에 영향받지 않는 실제 시간 사용
            elapsed += Time.unscaledDeltaTime;
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
        } else {
            Debug.LogError("[FurnaceManager] InventoryController.instance가 null입니다!");
            isSmelting = false;
            yield break;
        }
        
        // 출력 아이템 추가
        AddOutputItem(recipe);
        
        // 진행 바 리셋
        if (progressBar != null) {
            progressBar.fillAmount = 0f;
        }
        
        isSmelting = false;
    }
    
    /// <summary>
    /// 출력 슬롯에 아이템 추가
    /// </summary>
    private void AddOutputItem(FurnaceRecipe recipe) {
        if (InventoryController.instance == null) {
            Debug.LogError("[FurnaceManager] InventoryController.instance가 null입니다.");
            return;
        }
        
        if (string.IsNullOrEmpty(recipe.outputItemType)) {
            Debug.LogError($"[FurnaceManager] 레시피의 outputItemType이 비어있습니다. 레시피: {recipe.name}");
            return;
        }
        
        // 출력 슬롯이 비어있는지 다시 확인
        InventoryItem currentOutput = furnaceInventory.InventoryGetItem(outputSlotIndex);
        if (currentOutput != null && !currentOutput.GetIsNull()) {
            return;
        }
        
        // TestItemDict 확인 (아이템이 등록되어 있는지 확인)
        MethodInfo testItemDictMethod = typeof(InventoryController).GetMethod("TestItemDict", BindingFlags.NonPublic | BindingFlags.Instance);
        bool itemExists = false;
        if (testItemDictMethod != null) {
            itemExists = (bool)testItemDictMethod.Invoke(InventoryController.instance, new object[] { recipe.outputItemType });
        }
        
        bool success = false;
        
        // 방법 1: InventoryController.AddItemPos(string, string, int, int) 사용
        if (itemExists) {
            try {
                InventoryController.instance.AddItemPos(furnaceInventoryName, recipe.outputItemType, outputSlotIndex, recipe.outputAmount);
                success = true;
            } catch (System.Exception e) {
                Debug.LogError($"[FurnaceManager] AddItemPos 예외 발생: {e.Message}");
            }
        }
        
        // 방법 2: itemManager에서 직접 InventoryItem을 가져와서 추가 (방법 1이 실패한 경우)
        if (!success) {
            try {
                // 리플렉션으로 itemManager 접근
                FieldInfo itemManagerField = typeof(InventoryController).GetField("itemManager", BindingFlags.NonPublic | BindingFlags.Instance);
                if (itemManagerField != null) {
                    Dictionary<string, InventoryItem> itemManager = itemManagerField.GetValue(InventoryController.instance) as Dictionary<string, InventoryItem>;
                    
                    if (itemManager != null && itemManager.ContainsKey(recipe.outputItemType)) {
                        // itemManager에서 InventoryItem 가져오기
                        InventoryItem templateItem = itemManager[recipe.outputItemType];
                        InventoryItem newItem = new InventoryItem(templateItem, recipe.outputAmount);
                        
                        // InventoryController.AddItemPos(string, InventoryItem, int) 사용
                        InventoryController.instance.AddItemPos(furnaceInventoryName, newItem, outputSlotIndex);
                        success = true;
                    } else {
                        Debug.LogError($"[FurnaceManager] itemManager에 '{recipe.outputItemType}' 아이템이 없습니다.");
                    }
                }
            } catch (System.Exception e) {
                Debug.LogError($"[FurnaceManager] 리플렉션을 통한 아이템 추가 실패: {e.Message}");
            }
        }
        
        // UI 업데이트 강제 호출 (AddItemPos 후 즉시 반영)
        if (success && furnaceUI != null) {
            InventoryUIManager uiManager = furnaceUI.GetComponent<InventoryUIManager>();
            if (uiManager != null) {
                uiManager.UpdateSlot(outputSlotIndex);
            }
        }
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
