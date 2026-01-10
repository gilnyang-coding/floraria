using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 화로 툴팁 UI
/// - 레시피 이름 표시
/// - 필요 재료 목록 (보유량에 따라 녹색/빨간색)
/// </summary>
public class FirePotTooltip : BaseCraftingTooltip {
    /// <summary>
    /// 툴팁 표시 (FirePotManager용 호환성 메서드)
    /// </summary>
    public void Show(CraftingRecipe recipe, bool canCraft, Vector3 position) {
        var availability = FirePotManager.Instance?.GetIngredientAvailability(recipe) 
            ?? new Dictionary<string, bool>();
        base.Show(recipe, canCraft, position, availability);
    }
}
