using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 연금대 툴팁 UI
/// - 레시피 이름 표시
/// - 필요 재료 목록 (보유량에 따라 녹색/빨간색)
/// </summary>
public class AlchemyTooltip : BaseCraftingTooltip {
    /// <summary>
    /// 툴팁 표시 (AlchemyManager용 호환성 메서드)
    /// </summary>
    public void Show(CraftingRecipe recipe, bool canCraft, Vector3 position) {
        var availability = AlchemyManager.Instance?.GetIngredientAvailability(recipe) 
            ?? new Dictionary<string, bool>();
        base.Show(recipe, canCraft, position, availability);
    }
}
