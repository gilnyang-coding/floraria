using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 제작대 툴팁 UI
/// - 레시피 이름 표시
/// - 필요 재료 목록 표시 (보유량에 따라 색상 변경)
/// </summary>
public class CraftingTooltip : MonoBehaviour {
    [Header("UI 참조")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI ingredientsText;
    [SerializeField] private RectTransform tooltipRect;
    [SerializeField] private CanvasGroup canvasGroup;
    
    [Header("색상 설정")]
    [SerializeField] private Color availableColor = new Color(0.2f, 0.8f, 0.2f); // 녹색
    [SerializeField] private Color unavailableColor = new Color(0.9f, 0.2f, 0.2f); // 빨간색
    
    [Header("위치 설정")]
    [SerializeField] private Vector2 offset = new Vector2(20f, -20f);
    
    private Canvas parentCanvas;
    private RectTransform canvasRect;
    
    private void Awake() {
        // 캔버스 참조 가져오기
        parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas != null) {
            canvasRect = parentCanvas.GetComponent<RectTransform>();
        }
        
        if (tooltipRect == null) {
            tooltipRect = GetComponent<RectTransform>();
        }
        
        if (canvasGroup == null) {
            canvasGroup = GetComponent<CanvasGroup>();
        }
        
        Hide();
    }
    
    /// <summary>
    /// 툴팁 표시
    /// </summary>
    public void Show(CraftingRecipe recipe, bool canCraft, Vector3 position) {
        if (recipe == null) return;
        
        gameObject.SetActive(true);
        if (canvasGroup != null) {
            canvasGroup.alpha = 1f;
        }
        
        // 제목 설정
        string title = string.IsNullOrEmpty(recipe.resultDisplayName) 
            ? recipe.resultItemType 
            : recipe.resultDisplayName;
        
        if (titleText != null) {
            titleText.text = title;
        }
        
        // 재료 텍스트 생성
        if (ingredientsText != null && recipe.ingredients != null) {
            string ingredientStr = BuildIngredientsText(recipe, canCraft);
            ingredientsText.text = ingredientStr;
        }
        
        // 위치 설정
        UpdatePosition(position);
    }
    
    /// <summary>
    /// 재료 텍스트 생성 (색상 적용)
    /// </summary>
    private string BuildIngredientsText(CraftingRecipe recipe, bool canCraft) {
        if (recipe.ingredients == null || recipe.ingredients.Length == 0) {
            return "재료 없음";
        }
        
        var availability = CraftingManager.Instance?.GetIngredientAvailability(recipe) 
            ?? new Dictionary<string, bool>();
        
        string result = "";
        for (int i = 0; i < recipe.ingredients.Length; i++) {
            var ingredient = recipe.ingredients[i];
            
            string displayName = string.IsNullOrEmpty(ingredient.displayName) 
                ? ingredient.itemType 
                : ingredient.displayName;
            
            // 보유량 확인
            int playerCount = CraftingManager.Instance?.GetTotalItemCount(ingredient.itemType) ?? 0;
            bool hasEnough = availability.ContainsKey(ingredient.itemType) && availability[ingredient.itemType];
            
            // 색상 적용
            string colorHex = hasEnough 
                ? ColorUtility.ToHtmlStringRGB(availableColor) 
                : ColorUtility.ToHtmlStringRGB(unavailableColor);
            
            if (i > 0) result += "\n";
            result += $"<color=#{colorHex}>{displayName}×{ingredient.amount}</color> ({playerCount}/{ingredient.amount})";
        }
        
        return result;
    }
    
    /// <summary>
    /// 툴팁 위치 업데이트
    /// </summary>
    private void UpdatePosition(Vector3 targetPosition) {
        if (tooltipRect == null) return;
        
        // 월드 좌표를 스크린 좌표로 변환
        Vector3 screenPos = targetPosition;
        
        // 오프셋 적용
        screenPos.x += offset.x;
        screenPos.y += offset.y;
        
        // 화면 밖으로 나가지 않도록 조정
        if (canvasRect != null) {
            Vector2 tooltipSize = tooltipRect.sizeDelta;
            
            // 오른쪽 경계 체크
            if (screenPos.x + tooltipSize.x > Screen.width) {
                screenPos.x = targetPosition.x - tooltipSize.x - offset.x;
            }
            
            // 하단 경계 체크
            if (screenPos.y - tooltipSize.y < 0) {
                screenPos.y = targetPosition.y + tooltipSize.y - offset.y;
            }
        }
        
        tooltipRect.position = screenPos;
    }
    
    /// <summary>
    /// 툴팁 숨기기
    /// </summary>
    public void Hide() {
        if (canvasGroup != null) {
            canvasGroup.alpha = 0f;
        }
        gameObject.SetActive(false);
    }
    
    /// <summary>
    /// 색상 설정
    /// </summary>
    public void SetColors(Color available, Color unavailable) {
        availableColor = available;
        unavailableColor = unavailable;
    }
}

