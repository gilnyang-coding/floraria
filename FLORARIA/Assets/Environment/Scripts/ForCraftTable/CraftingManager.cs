using InventorySystem;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 제작 시스템 매니저
/// - InventoryController의 CraftTable 인벤토리를 사용
/// - 레시피에 따라 슬롯에 아이템 아이콘 표시
/// - 클릭 시 제작 실행
/// </summary>
public class CraftingManager : MonoBehaviour {
    public static CraftingManager Instance { get; private set; }
    
    [Header("레시피 설정")]
    [Tooltip("게임에서 사용할 모든 제작 레시피")]
    [SerializeField] private List<CraftingRecipe> allRecipes = new List<CraftingRecipe>();
    
    [Header("인벤토리 설정")]
    [Tooltip("제작대 인벤토리 이름 (InventoryController에 등록된 이름)")]
    [SerializeField] private string craftTableName = "CraftTable";
    
    [Tooltip("플레이어 인벤토리 이름")]
    [SerializeField] private string playerInventoryName = "PlayerInventory";
    
    [Tooltip("핫바 인벤토리 이름")]
    [SerializeField] private string hotbarInventoryName = "Hotbar";
    
    [Header("툴팁 설정")]
    [SerializeField] private CraftingTooltip tooltip;
    
    [Header("메시지 설정")]
    [SerializeField] private float messageDuration = 2f;
    
    // 이벤트
    public event System.Action<string> OnCraftingMessage;
    
    // 슬롯별 레시피 매핑
    private Dictionary<int, CraftingRecipe> slotRecipes = new Dictionary<int, CraftingRecipe>();
    
    // 캐시
    private Inventory craftTableInventory;
    private GameObject craftTableUI;
    private bool isInitialized = false;
    
    private void Awake() {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject);
        }
    }
    
    private void Start() {
        // 약간의 딜레이 후 초기화 (InventoryController가 먼저 초기화되도록)
        Invoke(nameof(Initialize), 0.1f);
    }
    
    private void Initialize() {
        if (InventoryController.instance == null) {
            Debug.LogError("[CraftingManager] InventoryController를 찾을 수 없습니다.");
            return;
        }
        
        // CraftTable 인벤토리 찾기
        try {
            craftTableInventory = InventoryController.instance.GetInventory(craftTableName);
            if (craftTableInventory != null) {
                craftTableUI = craftTableInventory.GetUI();
                Debug.Log($"[CraftingManager] {craftTableName} 인벤토리 연결됨");
            }
        } catch {
            Debug.LogError($"[CraftingManager] {craftTableName} 인벤토리를 찾을 수 없습니다.");
            return;
        }
        
        // 레시피를 슬롯에 배치
        SetupRecipeSlots();
        isInitialized = true;
    }
    
    // 슬롯들을 UI 위치 기준으로 정렬한 캐시
    private Slot[] sortedSlots;
    
    /// <summary>
    /// 레시피를 CraftTable 슬롯에 배치
    /// </summary>
    private void SetupRecipeSlots() {
        if (craftTableUI == null) return;
        
        slotRecipes.Clear();
        
        // 1. 슬롯들을 UI 위치 기준으로 정렬 (위에서 아래, 왼쪽에서 오른쪽)
        CacheAndSortSlots();
        
        // 2. 먼저 모든 슬롯의 아이템 이미지를 비활성화
        DisableAllSlotIcons();
        
        // 3. 레시피를 정렬된 슬롯 순서대로 배치
        for (int i = 0; i < allRecipes.Count && i < sortedSlots.Length; i++) {
            var recipe = allRecipes[i];
            if (recipe == null) continue;
            
            Slot targetSlot = sortedSlots[i];
            int slotPosition = targetSlot.GetPosition();
            
            // 슬롯에 레시피 매핑
            slotRecipes[slotPosition] = recipe;
            
            // 슬롯 UI를 직접 업데이트 (아이콘 표시)
            SetupSlotIconDirect(recipe, targetSlot);
            
            Debug.Log($"[CraftingManager] 슬롯 {i}번째 (position: {slotPosition})에 {recipe.resultDisplayName} 레시피 배치");
        }
        
        // 모든 슬롯 상태 업데이트
        RefreshAllSlots();
    }
    
    /// <summary>
    /// 슬롯들을 UI 위치 기준으로 정렬해서 캐시
    /// (위에서 아래, 왼쪽에서 오른쪽 순서)
    /// </summary>
    private void CacheAndSortSlots() {
        if (craftTableUI == null) return;
        
        var slots = craftTableUI.GetComponentsInChildren<Slot>();
        
        // UI 위치 기준으로 정렬: Y가 높을수록 위, X가 낮을수록 왼쪽
        // RectTransform의 position 사용 (Y 내림차순, X 오름차순)
        System.Array.Sort(slots, (a, b) => {
            float yA = a.transform.position.y;
            float yB = b.transform.position.y;
            float xA = a.transform.position.x;
            float xB = b.transform.position.x;
            
            // Y 비교 (높은 게 먼저 = 위쪽이 먼저)
            if (Mathf.Abs(yA - yB) > 0.1f) {
                return yB.CompareTo(yA); // 내림차순
            }
            // Y가 같으면 X 비교 (낮은 게 먼저 = 왼쪽이 먼저)
            return xA.CompareTo(xB); // 오름차순
        });
        
        sortedSlots = slots;
        Debug.Log($"[CraftingManager] 총 슬롯 수: {sortedSlots.Length}");
    }
    
    /// <summary>
    /// 모든 슬롯의 아이템 이미지 비활성화
    /// </summary>
    private void DisableAllSlotIcons() {
        if (sortedSlots == null) return;
        
        foreach (var slot in sortedSlots) {
            var itemHolder = slot.transform.Find("SlotItemHolder");
            if (itemHolder == null && slot.transform.childCount > 0) {
                itemHolder = slot.transform.GetChild(0);
            }
            
            if (itemHolder != null) {
                itemHolder.gameObject.SetActive(false);
            } 
        }
    }
    
    /// <summary>
    /// 슬롯에 레시피 아이콘 설정 (Slot 직접 전달)
    /// </summary>
    private void SetupSlotIconDirect(CraftingRecipe recipe, Slot slot) {
        if (recipe == null || slot == null) return;
        
        // 아이콘 가져오기 (레시피에 설정된 것 우선, 없으면 InventoryController에서)
        Sprite icon = GetRecipeIcon(recipe);
        if (icon == null) {
            Debug.LogWarning($"[CraftingManager] {recipe.resultDisplayName} 아이콘을 찾을 수 없습니다.");
            return;
        }
        
        // SlotItemHolder의 Image에 아이콘 설정
        var itemHolder = slot.transform.Find("SlotItemHolder");
        if (itemHolder == null && slot.transform.childCount > 0) {
            itemHolder = slot.transform.GetChild(0);
        }
        
        if (itemHolder != null) {
            var image = itemHolder.GetComponent<UnityEngine.UI.Image>();
            if (image != null) {
                image.sprite = icon;
                image.enabled = true;
                image.raycastTarget = false; // 깜빡거림 방지: 아이콘이 레이캐스트 받지 않음
                itemHolder.gameObject.SetActive(true);
            }
            
            // DragItem의 수량 텍스트 비활성화 (제작대는 수량 표시 안 함)
            var dragItem = itemHolder.GetComponent<DragItem>();
            if (dragItem != null) {
                dragItem.HideText();
            }
            
            // 모든 자식의 Raycast Target 끄기 (깜빡거림 방지)
            DisableRaycastTargetsInSlot(itemHolder);
        }
    }
    
    /// <summary>
    /// 슬롯 내 모든 Graphic의 Raycast Target 비활성화
    /// </summary>
    private void DisableRaycastTargetsInSlot(Transform itemHolder) {
        var graphics = itemHolder.GetComponentsInChildren<UnityEngine.UI.Graphic>(true);
        foreach (var graphic in graphics) {
            graphic.raycastTarget = false;
        }
    }
    
    /// <summary>
    /// 레시피 아이콘 가져오기
    /// </summary>
    private Sprite GetRecipeIcon(CraftingRecipe recipe) {
        // 레시피에 직접 설정된 아이콘이 있으면 사용
        if (recipe.resultIcon != null) {
            return recipe.resultIcon;
        }
        
        // 없으면 InventoryController에서 찾기
        if (InventoryController.instance == null) return null;
        
        var items = InventoryController.instance.GetItems();
        foreach (var item in items) {
            if (item.GetItemType() == recipe.resultItemType) {
                return item.GetItemImage();
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// 모든 슬롯의 제작 가능 상태 업데이트
    /// </summary>
    public void RefreshAllSlots() {
        if (craftTableUI == null) return;
        
        var uiManager = craftTableUI.GetComponent<InventoryUIManager>();
        if (uiManager == null) return;
        
        foreach (var kvp in slotRecipes) {
            int slotIndex = kvp.Key;
            CraftingRecipe recipe = kvp.Value;
            bool canCraft = CanCraft(recipe);
            
            // 슬롯 아이템의 알파값 조정
            UpdateSlotVisual(slotIndex, canCraft);
        }
    }
    
    /// <summary>
    /// 슬롯 시각적 상태 업데이트 (제작 가능: 알파 1.0, 불가: 알파 0.7)
    /// </summary>
    private void UpdateSlotVisual(int slotIndex, bool canCraft) {
        if (craftTableUI == null) return;
        
        Transform slotTransform = FindSlotByIndex(slotIndex);
        if (slotTransform == null) return;
        
        // 아이템 이미지의 알파값 조정
        var itemHolder = slotTransform.Find("SlotItemHolder");
        if (itemHolder == null && slotTransform.childCount > 0) {
            itemHolder = slotTransform.GetChild(0);
        }
        
        if (itemHolder != null) {
            var image = itemHolder.GetComponent<UnityEngine.UI.Image>();
            if (image != null) {
                Color color = image.color;
                color.a = canCraft ? 1f : 0.7f;
                image.color = color;
            }
        }
    }
    
    /// <summary>
    /// 슬롯 인덱스로 슬롯 Transform 찾기
    /// </summary>
    private Transform FindSlotByIndex(int index) {
        if (craftTableUI == null) return null;
        
        // InventoryUIManager의 자식들 중에서 Slot 컴포넌트를 가진 것 찾기
        var slots = craftTableUI.GetComponentsInChildren<Slot>();
        foreach (var slot in slots) {
            if (slot.GetPosition() == index) {
                return slot.transform;
            }
        }
        return null;
    }
    
    /// <summary>
    /// 특정 레시피 제작 가능 여부 확인
    /// </summary>
    public bool CanCraft(CraftingRecipe recipe) {
        if (recipe == null) return false;
        if (InventoryController.instance == null) return false;
        
        foreach (var ingredient in recipe.ingredients) {
            int count = GetTotalItemCount(ingredient.itemType);
            if (count < ingredient.amount) {
                return false;
            }
        }
        return true;
    }
    
    /// <summary>
    /// 플레이어 인벤토리 + 핫바에서 아이템 총 개수
    /// </summary>
    public int GetTotalItemCount(string itemType) {
        if (InventoryController.instance == null) return 0;
        
        int count = 0;
        try {
            count += InventoryController.instance.CountItems(playerInventoryName, itemType);
        } catch { }
        try {
            count += InventoryController.instance.CountItems(hotbarInventoryName, itemType);
        } catch { }
        
        return count;
    }
    
    /// <summary>
    /// 각 재료별 보유 여부 확인 (툴팁 색상용)
    /// </summary>
    public Dictionary<string, bool> GetIngredientAvailability(CraftingRecipe recipe) {
        var result = new Dictionary<string, bool>();
        if (recipe?.ingredients == null) return result;
        
        foreach (var ingredient in recipe.ingredients) {
            int count = GetTotalItemCount(ingredient.itemType);
            result[ingredient.itemType] = count >= ingredient.amount;
        }
        return result;
    }
    
    /// <summary>
    /// 슬롯 클릭 시 호출 - 제작 시도
    /// </summary>
    public void OnSlotClicked(int slotIndex) {
        if (!slotRecipes.ContainsKey(slotIndex)) return;
        
        CraftingRecipe recipe = slotRecipes[slotIndex];
        TryCraft(recipe);
    }
    
    /// <summary>
    /// 제작 실행
    /// </summary>
    public bool TryCraft(CraftingRecipe recipe) {
        if (recipe == null) return false;
        
        if (!CanCraft(recipe)) {
            ShowMessage("재료가 부족합니다.");
            return false;
        }
        
        // 결과물 넣을 공간 확인
        if (!HasSpaceForResult(recipe.resultItemType)) {
            ShowMessage("인벤토리가 가득 찼습니다.");
            return false;
        }
        
        // 재료 소모
        foreach (var ingredient in recipe.ingredients) {
            ConsumeIngredient(ingredient.itemType, ingredient.amount);
        }
        
        // 결과물 지급 (플레이어 인벤토리 우선)
        AddResultItem(recipe.resultItemType, recipe.resultAmount);
        
        string name = string.IsNullOrEmpty(recipe.resultDisplayName) 
            ? recipe.resultItemType : recipe.resultDisplayName;
        ShowMessage($"{name} 제작 완료!");
        
        // 슬롯 상태 갱신
        RefreshAllSlots();
        
        return true;
    }
    
    private bool HasSpaceForResult(string itemType) {
        if (InventoryController.instance == null) return false;
        
        if (!InventoryController.instance.InventoryFull(playerInventoryName, itemType))
            return true;
        if (!InventoryController.instance.InventoryFull(hotbarInventoryName, itemType))
            return true;
        
        return false;
    }
    
    private void ConsumeIngredient(string itemType, int amount) {
        if (InventoryController.instance == null) return;
        
        int remaining = amount;
        
        // 플레이어 인벤토리에서 먼저
        try {
            int playerCount = InventoryController.instance.CountItems(playerInventoryName, itemType);
            if (playerCount > 0) {
                int toRemove = Mathf.Min(playerCount, remaining);
                InventoryController.instance.RemoveItem(playerInventoryName, itemType, toRemove);
                remaining -= toRemove;
            }
        } catch { }
        
        // 부족하면 핫바에서
        if (remaining > 0) {
            try {
                InventoryController.instance.RemoveItem(hotbarInventoryName, itemType, remaining);
            } catch { }
        }
    }
    
    private void AddResultItem(string itemType, int amount) {
        if (InventoryController.instance == null) return;
        
        // 플레이어 인벤토리에 먼저
        if (!InventoryController.instance.InventoryFull(playerInventoryName, itemType)) {
            InventoryController.instance.AddItem(playerInventoryName, itemType, amount);
            return;
        }
        
        // 핫바에 추가
        InventoryController.instance.AddItem(hotbarInventoryName, itemType, amount);
    }
    
    public void ShowMessage(string message) {
        Debug.Log($"[CraftingManager] {message}");
        OnCraftingMessage?.Invoke(message);
    }
    
    /// <summary>
    /// 슬롯 호버 시 툴팁 표시
    /// </summary>
    public void ShowTooltip(int slotIndex, Vector3 position) {
        Debug.Log($"[CraftingManager] ShowTooltip 호출 - slotIndex: {slotIndex}, slotRecipes 키: {string.Join(", ", slotRecipes.Keys)}");
        
        if (!slotRecipes.ContainsKey(slotIndex)) {
            Debug.Log($"[CraftingManager] slotRecipes에 슬롯 {slotIndex}이 없습니다.");
            return;
        }
        if (tooltip == null) {
            Debug.LogWarning("[CraftingManager] tooltip이 연결되어 있지 않습니다! Inspector에서 연결해주세요.");
            return;
        }
        
        CraftingRecipe recipe = slotRecipes[slotIndex];
        bool canCraft = CanCraft(recipe);
        Debug.Log($"[CraftingManager] 툴팁 표시: {recipe.resultDisplayName}, canCraft: {canCraft}");
        tooltip.Show(recipe, canCraft, position);
    }
    
    public void HideTooltip() {
        if (tooltip != null) {
            tooltip.Hide();
        }
    }
    
    /// <summary>
    /// 슬롯 인덱스로 레시피 가져오기
    /// </summary>
    public CraftingRecipe GetRecipeBySlot(int slotIndex) {
        return slotRecipes.ContainsKey(slotIndex) ? slotRecipes[slotIndex] : null;
    }
    
    /// <summary>
    /// 특정 슬롯의 아이콘을 다시 설정 (Slot.UpdateSlot에서 호출)
    /// </summary>
    public void RefreshSlotIcon(int slotIndex, Slot slot = null) {
        if (!slotRecipes.ContainsKey(slotIndex)) return;
        
        CraftingRecipe recipe = slotRecipes[slotIndex];
        
        // Slot이 직접 전달되면 사용, 없으면 찾기
        if (slot == null) {
            if (craftTableUI == null) return;
            
            var slots = craftTableUI.GetComponentsInChildren<Slot>();
            foreach (var s in slots) {
                if (s.GetPosition() == slotIndex) {
                    slot = s;
                    break;
                }
            }
        }
        
        if (slot != null) {
            SetupSlotIconDirect(recipe, slot);
        }
    }
    
    /// <summary>
    /// 레시피 아이콘 가져오기 (public)
    /// </summary>
    public Sprite GetRecipeIconForSlot(int slotIndex) {
        if (!slotRecipes.ContainsKey(slotIndex)) return null;
        return GetRecipeIcon(slotRecipes[slotIndex]);
    }
    
    public List<CraftingRecipe> AllRecipes => allRecipes;
}
