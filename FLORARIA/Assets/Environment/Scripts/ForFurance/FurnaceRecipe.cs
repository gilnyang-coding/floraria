using UnityEngine;

/// <summary>
/// 용광로 레시피 ScriptableObject
/// 입력 아이템(광석)을 출력 아이템(주괴)로 변환
/// </summary>
[CreateAssetMenu(fileName = "New Furnace Recipe", menuName = "Crafting/Furnace Recipe")]
public class FurnaceRecipe : ScriptableObject {
    [Header("입력 아이템")]
    [Tooltip("녹일 수 있는 아이템 타입 (InventoryController의 items에 등록된 이름)")]
    public string inputItemType;
    
    [Tooltip("입력 아이템 표시 이름 (한글)")]
    public string inputDisplayName;
    
    [Header("출력 아이템")]
    [Tooltip("녹인 후 나올 아이템 타입 (InventoryController의 items에 등록된 이름)")]
    public string outputItemType;
    
    [Tooltip("출력 아이템 개수")]
    public int outputAmount = 1;
    
    [Tooltip("출력 아이템 표시 이름 (한글)")]
    public string outputDisplayName;
    
    [Tooltip("출력 아이템 아이콘 (비워두면 InventoryController에서 자동으로 가져옴)")]
    public Sprite outputIcon;
    
    /// <summary>
    /// 레시피 정보를 문자열로 반환
    /// </summary>
    public string GetRecipeText() {
        string input = string.IsNullOrEmpty(inputDisplayName) ? inputItemType : inputDisplayName;
        string output = string.IsNullOrEmpty(outputDisplayName) ? outputItemType : outputDisplayName;
        return $"{input} → {output}×{outputAmount}";
    }
}
