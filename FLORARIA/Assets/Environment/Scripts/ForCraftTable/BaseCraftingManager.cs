using InventorySystem;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 제작 시스템의 베이스 클래스
/// CraftingManager와 FirePotManager의 공통 로직을 추출
/// </summary>
public abstract class BaseCraftingManager : MonoBehaviour {
    
    [Header("인벤토리 설정")]
    [Tooltip("플레이어 인벤토리 이름")]
    [SerializeField] protected string playerInventoryName = "PlayerInventory";
    
    [Tooltip("핫바 인벤토리 이름")]
    [SerializeField] protected string hotbarInventoryName = "Hotbar";
    
    [Header("툴팁 설정")]
    [SerializeField] protected BaseCraftingTooltip tooltip;
    
    [Header("메시지 설정")]
    [SerializeField] protected float messageDuration = 2f;
    
    // 이벤트
    public event System.Action<string> OnCraftingMessage;
    
    // 슬롯별 레시피 매핑
    protected Dictionary<int, CraftingRecipe> slotRecipes = new Dictionary<int, CraftingRecipe>();
    
    // 캐시
    protected Inventory craftingInventory;
    protected GameObject craftingUI;
    protected bool isInitialized = false;
    
    // 슬롯들을 UI 위치 기준으로 정렬한 캐시
    protected Slot[] sortedSlots;
    
    /// <summary>
    /// 인벤토리 이름 가져오기 (하위 클래스에서 구현)
    /// </summary>
    protected abstract string GetInventoryName();
    
    /// <summary>
    /// 레시피 목록 가져오기 (하위 클래스에서 구현)
    /// </summary>
    protected abstract List<CraftingRecipe> GetAllRecipes();
    
    protected virtual void Start() {
        // 약간의 딜레이 후 초기화 (InventoryController가 먼저 초기화되도록)
        Invoke(nameof(Initialize), 0.1f);
    }
    
    protected virtual void Initialize() {
        if (InventoryController.instance == null) {
            Debug.LogError($"[{GetType().Name}] InventoryController를 찾을 수 없습니다.");
            return;
        }
        
        string inventoryName = GetInventoryName();
        
        // 인벤토리 찾기
        try {
            craftingInventory = InventoryController.instance.GetInventory(inventoryName);
            if (craftingInventory != null) {
                craftingUI = craftingInventory.GetUI();
                Debug.Log($"[{GetType().Name}] {inventoryName} 인벤토리 연결됨");
            }
        } catch {
            Debug.LogError($"[{GetType().Name}] {inventoryName} 인벤토리를 찾을 수 없습니다.");
            return;
        }
        
        // 레시피를 슬롯에 배치
        SetupRecipeSlots();
        isInitialized = true;
    }
    
    /// <summary>
    /// 레시피를 슬롯에 배치
    /// </summary>
    protected virtual void SetupRecipeSlots() {
        if (craftingUI == null) return;
        
        slotRecipes.Clear();
        
        // 1. 슬롯들을 UI 위치 기준으로 정렬 (위에서 아래, 왼쪽에서 오른쪽)
        CacheAndSortSlots();
        
        // 2. 먼저 모든 슬롯의 아이템 이미지를 비활성화
        DisableAllSlotIcons();
        
        // 3. 필터링된 레시피를 정렬된 슬롯 순서대로 배치
        var filteredRecipes = GetFilteredRecipes();
        for (int i = 0; i < filteredRecipes.Count && i < sortedSlots.Length; i++) {
            var recipe = filteredRecipes[i];
            if (recipe == null) continue;
            
            Slot targetSlot = sortedSlots[i];
            int slotPosition = targetSlot.GetPosition();
            
            // 슬롯에 레시피 매핑
            slotRecipes[slotPosition] = recipe;
            
            // 슬롯 UI를 직접 업데이트 (아이콘 표시)
            SetupSlotIconDirect(recipe, targetSlot);
            
            Debug.Log($"[{GetType().Name}] 슬롯 {i}번째 (position: {slotPosition})에 {recipe.resultDisplayName} 레시피 배치");
        }
        
        // 모든 슬롯 상태 업데이트
        RefreshAllSlots();
    }
    
    /// <summary>
    /// 레시피 목록 가져오기 (필터링 없이 모든 레시피 반환)
    /// </summary>
    protected virtual List<CraftingRecipe> GetFilteredRecipes() {
        var allRecipes = GetAllRecipes();
        var filtered = new List<CraftingRecipe>();
        
        foreach (var recipe in allRecipes) {
            if (recipe != null) {
                filtered.Add(recipe);
            }
        }
        
        return filtered;
    }
    
    /// <summary>
    /// 슬롯들을 UI 위치 기준으로 정렬해서 캐시
    /// (위에서 아래, 왼쪽에서 오른쪽 순서)
    /// </summary>
    protected virtual void CacheAndSortSlots() {
        if (craftingUI == null) return;
        
        var slots = craftingUI.GetComponentsInChildren<Slot>();
        
        // UI 위치 기준으로 정렬: Y가 높을수록 위, X가 낮을수록 왼쪽
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
        Debug.Log($"[{GetType().Name}] 총 슬롯 수: {sortedSlots.Length}");
    }
    
    /// <summary>
    /// 모든 슬롯의 아이템 이미지 비활성화
    /// </summary>
    protected virtual void DisableAllSlotIcons() {
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
    protected virtual void SetupSlotIconDirect(CraftingRecipe recipe, Slot slot) {
        if (recipe == null || slot == null) return;
        
        // 아이콘 가져오기 (레시피에 설정된 것 우선, 없으면 InventoryController에서)
        Sprite icon = GetRecipeIcon(recipe);
        if (icon == null) {
            Debug.LogWarning($"[{GetType().Name}] {recipe.resultDisplayName} 아이콘을 찾을 수 없습니다.");
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
            
            // DragItem의 수량 텍스트 비활성화
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
    protected virtual void DisableRaycastTargetsInSlot(Transform itemHolder) {
        var graphics = itemHolder.GetComponentsInChildren<UnityEngine.UI.Graphic>(true);
        foreach (var graphic in graphics) {
            graphic.raycastTarget = false;
        }
    }
    
    /// <summary>
    /// 레시피 아이콘 가져오기
    /// </summary>
    protected virtual Sprite GetRecipeIcon(CraftingRecipe recipe) {
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
    public virtual void RefreshAllSlots() {
        if (craftingUI == null) return;
        
        var uiManager = craftingUI.GetComponent<InventoryUIManager>();
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
    protected virtual void UpdateSlotVisual(int slotIndex, bool canCraft) {
        if (craftingUI == null) return;
        
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
    protected virtual Transform FindSlotByIndex(int index) {
        if (craftingUI == null) return null;
        
        // InventoryUIManager의 자식들 중에서 Slot 컴포넌트를 가진 것 찾기
        var slots = craftingUI.GetComponentsInChildren<Slot>();
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
    public virtual bool CanCraft(CraftingRecipe recipe) {
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
    public virtual int GetTotalItemCount(string itemType) {
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
    public virtual Dictionary<string, bool> GetIngredientAvailability(CraftingRecipe recipe) {
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
    public virtual void OnSlotClicked(int slotIndex) {
        if (!slotRecipes.ContainsKey(slotIndex)) return;
        
        CraftingRecipe recipe = slotRecipes[slotIndex];
        TryCraft(recipe);
    }
    
    /// <summary>
    /// 제작 실행
    /// </summary>
    public virtual bool TryCraft(CraftingRecipe recipe) {
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
    
    protected virtual bool HasSpaceForResult(string itemType) {
        if (InventoryController.instance == null) return false;
        
        if (!InventoryController.instance.InventoryFull(playerInventoryName, itemType))
            return true;
        if (!InventoryController.instance.InventoryFull(hotbarInventoryName, itemType))
            return true;
        
        return false;
    }
    
    protected virtual void ConsumeIngredient(string itemType, int amount) {
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
    
    protected virtual void AddResultItem(string itemType, int amount) {
        if (InventoryController.instance == null) return;
        
        // 플레이어 인벤토리에 먼저
        if (!InventoryController.instance.InventoryFull(playerInventoryName, itemType)) {
            InventoryController.instance.AddItem(playerInventoryName, itemType, amount);
            return;
        }
        
        // 핫바에 추가
        InventoryController.instance.AddItem(hotbarInventoryName, itemType, amount);
    }
    
    public virtual void ShowMessage(string message) {
        Debug.Log($"[{GetType().Name}] {message}");
        OnCraftingMessage?.Invoke(message);
    }
    
    /// <summary>
    /// 슬롯 호버 시 툴팁 표시
    /// </summary>
    public virtual void ShowTooltip(int slotIndex, Vector3 position) {
        if (!slotRecipes.ContainsKey(slotIndex)) {
            return;
        }
        if (tooltip == null) {
            return;
        }
        
        CraftingRecipe recipe = slotRecipes[slotIndex];
        bool canCraft = CanCraft(recipe);
        var availability = GetIngredientAvailability(recipe);
        
        if (tooltip is BaseCraftingTooltip baseTooltip) {
            baseTooltip.Show(recipe, canCraft, position, availability);
        } else {
            // 호환성을 위한 폴백 (CraftingTooltip의 Show 메서드)
            if (tooltip is CraftingTooltip craftingTooltip) {
                craftingTooltip.Show(recipe, canCraft, position);
            }
        }
    }
    
    public virtual void HideTooltip() {
        if (tooltip != null) {
            tooltip.Hide();
        }
    }
    
    /// <summary>
    /// 슬롯 인덱스로 레시피 가져오기
    /// </summary>
    public virtual CraftingRecipe GetRecipeBySlot(int slotIndex) {
        return slotRecipes.ContainsKey(slotIndex) ? slotRecipes[slotIndex] : null;
    }
    
    /// <summary>
    /// 특정 슬롯의 아이콘을 다시 설정 (Slot.UpdateSlot에서 호출)
    /// </summary>
    public virtual void RefreshSlotIcon(int slotIndex, Slot slot = null) {
        if (!slotRecipes.ContainsKey(slotIndex)) return;
        
        CraftingRecipe recipe = slotRecipes[slotIndex];
        
        // Slot이 직접 전달되면 사용, 없으면 찾기
        if (slot == null) {
            if (craftingUI == null) return;
            
            var slots = craftingUI.GetComponentsInChildren<Slot>();
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
    public virtual Sprite GetRecipeIconForSlot(int slotIndex) {
        if (!slotRecipes.ContainsKey(slotIndex)) return null;
        return GetRecipeIcon(slotRecipes[slotIndex]);
    }
}
