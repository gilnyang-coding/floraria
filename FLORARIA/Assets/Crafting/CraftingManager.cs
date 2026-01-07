using InventorySystem;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 제작 시스템 핵심 매니저
/// - 레시피 관리
/// - 제작 가능 여부 확인
/// - 아이템 제작 실행
/// </summary>
public class CraftingManager : MonoBehaviour {
    public static CraftingManager Instance { get; private set; }
    
    [Header("레시피 설정")]
    [Tooltip("게임에서 사용할 모든 제작 레시피")]
    [SerializeField] private List<CraftingRecipe> allRecipes = new List<CraftingRecipe>();
    
    [Header("인벤토리 설정")]
    [Tooltip("플레이어 인벤토리 이름")]
    [SerializeField] private string playerInventoryName = "PlayerInventory";
    
    [Tooltip("핫바 인벤토리 이름")]
    [SerializeField] private string hotbarInventoryName = "Hotbar";
    
    [Header("메시지 설정")]
    [Tooltip("메시지 표시 시간")]
    [SerializeField] private float messageDuration = 2f;
    
    // 이벤트
    public event System.Action<string> OnCraftingMessage; // 제작 결과 메시지
    public event System.Action OnInventoryChanged; // 인벤토리 변경 시 (UI 갱신용)
    
    public List<CraftingRecipe> AllRecipes => allRecipes;
    public string PlayerInventoryName => playerInventoryName;
    public string HotbarInventoryName => hotbarInventoryName;
    
    private void Awake() {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// 특정 레시피를 제작할 수 있는지 확인
    /// </summary>
    public bool CanCraft(CraftingRecipe recipe) {
        if (recipe == null || !recipe.isUnlocked) return false;
        if (InventoryController.instance == null) return false;
        
        // 모든 재료가 충분한지 확인
        foreach (var ingredient in recipe.ingredients) {
            int playerCount = GetTotalItemCount(ingredient.itemType);
            if (playerCount < ingredient.amount) {
                return false;
            }
        }
        
        return true;
    }
    
    /// <summary>
    /// 플레이어 인벤토리 + 핫바에서 특정 아이템의 총 개수 확인
    /// </summary>
    public int GetTotalItemCount(string itemType) {
        if (InventoryController.instance == null) return 0;
        
        int count = 0;
        
        // 플레이어 인벤토리에서 확인
        try {
            count += InventoryController.instance.CountItems(playerInventoryName, itemType);
        } catch { }
        
        // 핫바에서 확인
        try {
            count += InventoryController.instance.CountItems(hotbarInventoryName, itemType);
        } catch { }
        
        return count;
    }
    
    /// <summary>
    /// 각 재료별로 제작 가능 여부 확인 (UI 표시용)
    /// </summary>
    public Dictionary<string, bool> GetIngredientAvailability(CraftingRecipe recipe) {
        var result = new Dictionary<string, bool>();
        
        if (recipe == null || recipe.ingredients == null) return result;
        
        foreach (var ingredient in recipe.ingredients) {
            int playerCount = GetTotalItemCount(ingredient.itemType);
            result[ingredient.itemType] = playerCount >= ingredient.amount;
        }
        
        return result;
    }
    
    /// <summary>
    /// 아이템 제작 실행
    /// </summary>
    public bool TryCraft(CraftingRecipe recipe) {
        if (recipe == null) return false;
        
        // 제작 가능 여부 확인
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
        
        // 결과물 지급
        AddResultItem(recipe.resultItemType, recipe.resultAmount);
        
        string resultName = string.IsNullOrEmpty(recipe.resultDisplayName) 
            ? recipe.resultItemType 
            : recipe.resultDisplayName;
        ShowMessage($"{resultName} 제작 완료!");
        
        // 인벤토리 변경 알림
        OnInventoryChanged?.Invoke();
        
        return true;
    }
    
    /// <summary>
    /// 결과물을 넣을 공간이 있는지 확인
    /// </summary>
    private bool HasSpaceForResult(string itemType) {
        if (InventoryController.instance == null) return false;
        
        // 플레이어 인벤토리에 공간 확인
        if (!InventoryController.instance.InventoryFull(playerInventoryName, itemType)) {
            return true;
        }
        
        // 핫바에 공간 확인
        if (!InventoryController.instance.InventoryFull(hotbarInventoryName, itemType)) {
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// 재료 소모 (플레이어 인벤토리 우선, 부족하면 핫바에서)
    /// </summary>
    private void ConsumeIngredient(string itemType, int amount) {
        if (InventoryController.instance == null) return;
        
        int remaining = amount;
        
        // 플레이어 인벤토리에서 먼저 제거
        int playerCount = 0;
        try {
            playerCount = InventoryController.instance.CountItems(playerInventoryName, itemType);
        } catch { }
        
        if (playerCount > 0) {
            int toRemove = Mathf.Min(playerCount, remaining);
            InventoryController.instance.RemoveItem(playerInventoryName, itemType, toRemove);
            remaining -= toRemove;
        }
        
        // 부족하면 핫바에서 제거
        if (remaining > 0) {
            InventoryController.instance.RemoveItem(hotbarInventoryName, itemType, remaining);
        }
    }
    
    /// <summary>
    /// 결과물 지급 (플레이어 인벤토리 우선, 가득 차면 핫바)
    /// </summary>
    private void AddResultItem(string itemType, int amount) {
        if (InventoryController.instance == null) return;
        
        // 플레이어 인벤토리에 먼저 추가 시도
        if (!InventoryController.instance.InventoryFull(playerInventoryName, itemType)) {
            InventoryController.instance.AddItem(playerInventoryName, itemType, amount);
            return;
        }
        
        // 핫바에 추가
        InventoryController.instance.AddItem(hotbarInventoryName, itemType, amount);
    }
    
    /// <summary>
    /// 메시지 표시 (외부에서도 호출 가능)
    /// </summary>
    public void ShowMessage(string message) {
        Debug.Log($"[CraftingManager] {message}");
        OnCraftingMessage?.Invoke(message);
    }
    
    /// <summary>
    /// 활성화된 레시피 목록 반환
    /// </summary>
    public List<CraftingRecipe> GetUnlockedRecipes() {
        return allRecipes.FindAll(r => r != null && r.isUnlocked);
    }
}

