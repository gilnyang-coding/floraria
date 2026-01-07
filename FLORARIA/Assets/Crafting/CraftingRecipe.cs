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
/// - 결과물 아이템
/// - 필요한 재료 목록
/// </summary>
[CreateAssetMenu(fileName = "New Recipe", menuName = "Crafting/Recipe")]
public class CraftingRecipe : ScriptableObject {
    [Header("결과물")]
    [Tooltip("제작 결과 아이템 타입")]
    public string resultItemType;
    
    [Tooltip("결과물 개수")]
    public int resultAmount = 1;
    
    [Tooltip("결과물 표시 이름 (한글)")]
    public string resultDisplayName;
    
    [Tooltip("결과물 아이콘 (없으면 ItemInitializer에서 가져옴)")]
    public Sprite resultIcon;
    
    [Header("재료")]
    [Tooltip("제작에 필요한 재료 목록")]
    public CraftingIngredient[] ingredients;
    
    [Header("제작 조건")]
    [Tooltip("제작 가능 여부 (false면 레시피 목록에서 숨김)")]
    public bool isUnlocked = true;
    
    /// <summary>
    /// 재료 정보를 문자열로 반환 (툴팁용)
    /// </summary>
    public string GetIngredientsText() {
        if (ingredients == null || ingredients.Length == 0) return "재료 없음";
        
        string result = "";
        for (int i = 0; i < ingredients.Length; i++) {
            if (i > 0) result += ", ";
            string displayName = string.IsNullOrEmpty(ingredients[i].displayName) 
                ? ingredients[i].itemType 
                : ingredients[i].displayName;
            result += $"{displayName}×{ingredients[i].amount}";
        }
        return result;
    }
}

