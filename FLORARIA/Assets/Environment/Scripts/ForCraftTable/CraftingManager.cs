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
public class CraftingManager : BaseCraftingManager {
    public static CraftingManager Instance { get; private set; }
    
    [Header("레시피 설정")]
    [Tooltip("게임에서 사용할 모든 제작 레시피")]
    [SerializeField] private List<CraftingRecipe> allRecipes = new List<CraftingRecipe>();
    
    [Header("인벤토리 설정")]
    [Tooltip("제작대 인벤토리 이름 (InventoryController에 등록된 이름)")]
    [SerializeField] private string craftTableName = "CraftTable";
    
    private void Awake() {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject);
        }
    }
    
    protected override string GetInventoryName() {
        return craftTableName;
    }
    
    protected override List<CraftingRecipe> GetAllRecipes() {
        return allRecipes;
    }
    
    public List<CraftingRecipe> AllRecipes => allRecipes;
}
