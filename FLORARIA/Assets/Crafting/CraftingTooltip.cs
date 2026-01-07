using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 제작대 툴팁 UI
/// - 레시피 이름 표시
/// - 필요 재료 목록 (보유량에 따라 녹색/빨간색)
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
    [SerializeField] private Vector2 tooltipOffset = new Vector2(30f, -30f); // 툴팁 전체의 마우스로부터의 오프셋
    [SerializeField] private Vector2 ingredientsTextOffset = new Vector2(0f, -25f); // 재료 텍스트의 제목으로부터의 오프셋 (X: 좌우, Y: 위아래, 음수면 아래)
    
    private void Awake() {
        if (tooltipRect == null) tooltipRect = GetComponent<RectTransform>();
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        
        // 레이캐스트 차단 방지 (깜빡거림 방지)
        if (canvasGroup != null) {
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }
        
        // 모든 자식 요소의 Raycast Target 끄기
        DisableAllRaycastTargets();
        
        // 텍스트 설정 (줄바꿈 방지)
        if (titleText != null) {
            titleText.enableWordWrapping = false; // 제목은 줄바꿈 안 함
        }
        if (ingredientsText != null) {
            ingredientsText.enableWordWrapping = false; // 재료 목록도 줄바꿈 안 함 (한 줄에 하나씩)
            ingredientsText.overflowMode = TextOverflowModes.Overflow; // 넘치면 그냥 표시
        }
        
        Hide();
    }
    
    private void Update() {
        // 툴팁이 활성화되어 있으면 마우스 위치에 따라 업데이트
        if (gameObject.activeSelf && canvasGroup != null && canvasGroup.alpha > 0f) {
            UpdatePosition();
        }
    }
    
    /// <summary>
    /// 툴팁의 모든 Graphic 컴포넌트의 Raycast Target 비활성화
    /// </summary>
    private void DisableAllRaycastTargets() {
        // 자기 자신 포함 모든 Graphic 컴포넌트
        Graphic[] graphics = GetComponentsInChildren<Graphic>(true);
        foreach (var graphic in graphics) {
            graphic.raycastTarget = false;
        }
    }
    
    /// <summary>
    /// 툴팁 표시
    /// </summary>
    public void Show(CraftingRecipe recipe, bool canCraft, Vector3 position) {
        if (recipe == null) return;
        
        gameObject.SetActive(true);
        if (canvasGroup != null) canvasGroup.alpha = 1f;
        
        // 제목 설정 (제작 가능 여부에 따라 색상 적용)
        if (titleText != null) {
            string title = string.IsNullOrEmpty(recipe.resultDisplayName) 
                ? recipe.resultItemType : recipe.resultDisplayName;
            
            // 제작 가능 여부에 따라 색상 적용 (재료와 동일한 색상)
            string colorHex = canCraft 
                ? ColorUtility.ToHtmlStringRGB(availableColor) 
                : ColorUtility.ToHtmlStringRGB(unavailableColor);
            
            titleText.text = $"<color=#{colorHex}>{title}</color>";
        }
        
        // 재료 텍스트 생성 (색상 적용)
        if (ingredientsText != null && recipe.ingredients != null) {
            ingredientsText.enableWordWrapping = false; // 줄바꿈 비활성화
            ingredientsText.overflowMode = TextOverflowModes.Overflow; // 넘치면 그냥 표시
            ingredientsText.text = BuildIngredientsText(recipe);
        }
        
        // 재료 텍스트 위치를 제목 기준으로 설정
        UpdateIngredientsTextPosition();
        
        // 위치 설정 (마우스 위치 기준)
        UpdatePosition();
    }
    
    /// <summary>
    /// 재료 텍스트 생성 (보유량에 따라 색상 적용)
    /// </summary>
    private string BuildIngredientsText(CraftingRecipe recipe) {
        if (recipe.ingredients == null || recipe.ingredients.Length == 0)
            return "재료 없음";
        
        var availability = CraftingManager.Instance?.GetIngredientAvailability(recipe) 
            ?? new Dictionary<string, bool>();
        
        System.Text.StringBuilder result = new System.Text.StringBuilder();
        foreach (var ingredient in recipe.ingredients) {
            string displayName = string.IsNullOrEmpty(ingredient.displayName) 
                ? ingredient.itemType : ingredient.displayName;
            
            bool hasEnough = availability.ContainsKey(ingredient.itemType) && availability[ingredient.itemType];
            
            // 색상 적용 (녹색: 충분, 빨간색: 부족)
            string colorHex = hasEnough 
                ? ColorUtility.ToHtmlStringRGB(availableColor) 
                : ColorUtility.ToHtmlStringRGB(unavailableColor);
            
            // 각 재료를 한 줄에 표시: "나무: 2"
            if (result.Length > 0) result.Append("\n");
            result.Append($"<color=#{colorHex}>{displayName}: {ingredient.amount}</color>");
        }
        
        return result.ToString();
    }
    
    /// <summary>
    /// 재료 텍스트 위치를 제목 기준으로 설정
    /// </summary>
    private void UpdateIngredientsTextPosition() {
        if (titleText == null || ingredientsText == null) return;
        
        RectTransform titleRect = titleText.rectTransform;
        RectTransform ingredientsRect = ingredientsText.rectTransform;
        
        // 제목의 위치를 기준으로 재료 텍스트 배치
        Vector3 titlePos = titleRect.localPosition;
        ingredientsRect.localPosition = new Vector3(
            titlePos.x + ingredientsTextOffset.x,
            titlePos.y + ingredientsTextOffset.y,
            titlePos.z
        );
    }
    
    /// <summary>
    /// 툴팁 위치 업데이트 (마우스 위치 기준)
    /// </summary>
    private void UpdatePosition() {
        if (tooltipRect == null) return;
        
        // 마우스 위치를 화면 좌표로 변환
        Vector3 mousePos = Input.mousePosition;
        Vector2 size = tooltipRect.sizeDelta;
        
        // 기본 위치: 마우스 오른쪽 아래
        Vector3 pos = mousePos;
        pos.x += tooltipOffset.x;
        pos.y += tooltipOffset.y;
        
        // 화면 밖으로 나가지 않도록 조정
        if (pos.x + size.x > Screen.width) {
            // 오른쪽이 넘치면 왼쪽에 배치
            pos.x = mousePos.x - size.x - tooltipOffset.x;
        }
        if (pos.y - size.y < 0) {
            // 아래가 넘치면 위에 배치
            pos.y = mousePos.y + size.y - tooltipOffset.y;
        }
        
        // 최소 여백 보장
        pos.x = Mathf.Clamp(pos.x, 10f, Screen.width - size.x - 10f);
        pos.y = Mathf.Clamp(pos.y, size.y + 10f, Screen.height - 10f);
        
        tooltipRect.position = pos;
        
        // 재료 텍스트 위치도 다시 업데이트 (툴팁이 움직였으므로)
        UpdateIngredientsTextPosition();
    }
    
    /// <summary>
    /// 툴팁 숨기기
    /// </summary>
    public void Hide() {
        if (canvasGroup != null) canvasGroup.alpha = 0f;
        gameObject.SetActive(false);
    }
}

