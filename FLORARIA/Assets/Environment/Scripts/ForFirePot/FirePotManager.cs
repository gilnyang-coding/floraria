using InventorySystem;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 화로 시스템 매니저
/// - InventoryController의 FirePot 인벤토리를 사용
/// - CraftTable과 같은 로직이지만 먹는 아이템만 제작
/// </summary>
public class FirePotManager : BaseCraftingManager {
    public static FirePotManager Instance { get; private set; }
    
    [Header("레시피 설정")]
    [Tooltip("게임에서 사용할 모든 제작 레시피 (FirePotManager는 이 중에서 먹는 아이템만 필터링)")]
    [SerializeField] private List<CraftingRecipe> allRecipes = new List<CraftingRecipe>();
    
    [Header("인벤토리 설정")]
    [Tooltip("화로 인벤토리 이름 (InventoryController에 등록된 이름)")]
    [SerializeField] private string firePotName = "FirePot";
    
    private void Awake() {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject);
        }
    }
    
    protected override string GetInventoryName() {
        return firePotName;
    }
    
    protected override List<CraftingRecipe> GetAllRecipes() {
        return allRecipes;
    }
}
