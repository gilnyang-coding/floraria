using System;
using UnityEngine;

/// <summary>
/// 제작에 필요한 재료 정보
/// </summary>
[Serializable]
public class CraftingIngredient {
    [Tooltip("재료 아이템 타입 (InventoryController의 items에 등록된 이름)")]
    public string itemType;
    
    [Tooltip("필요한 개수")]
    public int amount = 1;
    
    [Tooltip("재료 표시 이름 (한글)")]
    public string displayName;
}

/// <summary>
/// 제작 레시피 ScriptableObject
/// </summary>
[CreateAssetMenu(fileName = "New Recipe", menuName = "Crafting/Recipe")]
public class CraftingRecipe : ScriptableObject {
    [Header("결과물")]
    [Tooltip("제작 결과 아이템 타입 (InventoryController items에 등록된 이름)")]
    public string resultItemType;
    
    [Tooltip("결과물 개수")]
    public int resultAmount = 1;
    
    [Tooltip("결과물 표시 이름 (한글)")]
    public string resultDisplayName;
    
    [Tooltip("결과물 아이콘 (비워두면 InventoryController에서 자동으로 가져옴)")]
    public Sprite resultIcon;
    
    [Header("재료")]
    [Tooltip("제작에 필요한 재료 목록")]
    public CraftingIngredient[] ingredients;
    
    // 슬롯 위치는 CraftingManager의 allRecipes 리스트 순서대로 자동 배치됨
    // (왼쪽 위부터 오른쪽으로, 끝나면 다음 줄)
    
    /// <summary>
    /// 재료 정보를 문자열로 반환 (툴팁용)
    /// </summary>
    public string GetIngredientsText() {
        if (ingredients == null || ingredients.Length == 0) return "재료 없음";
        
        string result = "";
        for (int i = 0; i < ingredients.Length; i++) {
            if (i > 0) result += "\n";
            string displayName = string.IsNullOrEmpty(ingredients[i].displayName) 
                ? ingredients[i].itemType 
                : ingredients[i].displayName;
            result += $"{displayName}×{ingredients[i].amount}";
        }
        return result;
    }
}

