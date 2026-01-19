using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 제작 툴팁의 베이스 클래스
/// CraftingTooltip과 FirePotTooltip의 공통 로직을 추출
/// </summary>
public abstract class BaseCraftingTooltip : MonoBehaviour {
    [Header("UI 참조")]
    [SerializeField] protected TextMeshProUGUI titleText;
    [SerializeField] protected TextMeshProUGUI ingredientsText;
    [SerializeField] protected RectTransform tooltipRect;
    [SerializeField] protected CanvasGroup canvasGroup;
    
    [Header("색상 설정")]
    [SerializeField] protected Color availableColor = new Color(0.2f, 0.8f, 0.2f); // 녹색
    [SerializeField] protected Color unavailableColor = new Color(0.9f, 0.2f, 0.2f); // 빨간색
    
    [Header("위치 설정")]
    [SerializeField] protected Vector2 ingredientsTextOffset = new Vector2(0f, -25f); // 재료 텍스트의 제목으로부터의 오프셋 (X: 좌우, Y: 위아래, 음수면 아래)
    [SerializeField] protected float margin = 50f; // 툴팁 크기의 여유 공간 (픽셀)
    [SerializeField] protected float ingredientOffsetPerItem = -300f; // 재료 하나당 Y 오프셋 (재료 개수에 따라 동적으로 적용)
    [SerializeField] protected float titleOffsetPerItem = 150f; // 재료 하나 추가될 때마다 타이틀 텍스트 Y 오프셋 증가량 (재료 1개: 0, 2개: 150, 3개: 300...)
    
    // 타이틀의 초기 위치 (margin 적용 전)
    protected Vector3? titleInitialPosition = null;
    
    protected virtual void Awake() {
        if (tooltipRect == null) tooltipRect = GetComponent<RectTransform>();
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        
        // RectTransform 설정: anchor와 pivot을 top-left로 설정하여 sizeDelta가 정확히 작동하도록
        SetupRectTransform();
        
        // Layout 컴포넌트 비활성화 (크기 자동 조정 방해 방지)
        DisableLayoutComponents();
        
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
    
    /// <summary>
    /// RectTransform을 크기 조정에 적합하도록 설정
    /// </summary>
    protected virtual void SetupRectTransform() {
        if (tooltipRect == null) return;
        
        // Anchor를 top-left로 설정 (anchorMin과 anchorMax가 같으면 sizeDelta가 실제 크기가 됨)
        // 이렇게 하면 sizeDelta가 절대 크기로 작동합니다
        tooltipRect.anchorMin = new Vector2(0f, 1f);
        tooltipRect.anchorMax = new Vector2(0f, 1f);
        tooltipRect.pivot = new Vector2(0f, 1f); // pivot도 top-left로 설정
        
        // anchoredPosition 초기화 (anchor가 변경되면 위치가 바뀔 수 있으므로)
        // 크기는 UpdateTooltipSize()에서 설정하므로 여기서는 초기화만
        if (tooltipRect.sizeDelta.x <= 0 || tooltipRect.sizeDelta.y <= 0) {
            tooltipRect.sizeDelta = new Vector2(200f, 100f); // 임시 크기
        }
    }
    
    /// <summary>
    /// Layout 컴포넌트 비활성화 (크기 자동 조정 방해 방지)
    /// </summary>
    protected virtual void DisableLayoutComponents() {
        if (tooltipRect == null) return;
        
        // ContentSizeFitter 비활성화
        ContentSizeFitter fitter = tooltipRect.GetComponent<ContentSizeFitter>();
        if (fitter != null) {
            fitter.enabled = false;
        }
        
        // LayoutElement의 preferred size 설정 무시
        LayoutElement layoutElement = tooltipRect.GetComponent<LayoutElement>();
        if (layoutElement != null) {
            layoutElement.ignoreLayout = true;
        }
    }
    
    protected virtual void Update() {
        // 마우스 위치 추적 로직 제거됨
    }
    
    /// <summary>
    /// 툴팁의 모든 Graphic 컴포넌트의 Raycast Target 비활성화
    /// </summary>
    protected virtual void DisableAllRaycastTargets() {
        // 자기 자신 포함 모든 Graphic 컴포넌트
        Graphic[] graphics = GetComponentsInChildren<Graphic>(true);
        foreach (var graphic in graphics) {
            graphic.raycastTarget = false;
        }
    }
    
    /// <summary>
    /// 툴팁 표시
    /// </summary>
    public virtual void Show(CraftingRecipe recipe, bool canCraft, Vector3 position, Dictionary<string, bool> ingredientAvailability) {
        if (recipe == null) {
            Debug.LogWarning($"[{GetType().Name}] 레시피가 null입니다.");
            return;
        }
        
        gameObject.SetActive(true);
        if (canvasGroup != null) canvasGroup.alpha = 1f;
        
        // RectTransform 설정을 확실히 적용 (Inspector 설정이 덮어쓸 수 있으므로)
        SetupRectTransform();
        
        // 제목 설정 (제작 가능 여부에 따라 색상 적용)
        if (titleText != null) {
            string title = string.IsNullOrEmpty(recipe.resultDisplayName) 
                ? recipe.resultItemType : recipe.resultDisplayName;
            
            // 제작 가능 여부에 따라 색상 적용 (재료와 동일한 색상)
            string colorHex = canCraft 
                ? ColorUtility.ToHtmlStringRGB(availableColor) 
                : ColorUtility.ToHtmlStringRGB(unavailableColor);
            
            titleText.text = $"<color=#{colorHex}>{title}</color>";
        } else {
            Debug.LogWarning($"[{GetType().Name}] titleText가 할당되지 않았습니다.");
        }
        
        // 재료 텍스트 생성 (색상 적용)
        if (ingredientsText != null && recipe.ingredients != null) {
            ingredientsText.enableWordWrapping = false; // 줄바꿈 비활성화
            ingredientsText.overflowMode = TextOverflowModes.Overflow; // 넘치면 그냥 표시
            ingredientsText.text = BuildIngredientsText(recipe, ingredientAvailability);
        } else {
            if (ingredientsText == null) {
                Debug.LogWarning($"[{GetType().Name}] ingredientsText가 할당되지 않았습니다.");
            }
            if (recipe.ingredients == null) {
                Debug.LogWarning($"[{GetType().Name}] 레시피의 ingredients가 null입니다.");
            }
        }
        
        // 재료 개수에 따라 Y 오프셋 계산
        int ingredientCount = recipe.ingredients != null ? recipe.ingredients.Length : 0;
        float dynamicYOffset = ingredientCount * ingredientOffsetPerItem; // 재료 텍스트용
        float titleYOffset = (ingredientCount - 1) * titleOffsetPerItem; // 타이틀 텍스트용 (재료 1개: 0, 2개: 150, 3개: 300...)
        
        // 타이틀 텍스트 위치 조정
        if (titleText != null) {
            RectTransform titleRect = titleText.rectTransform;
            Vector2 currentPos = titleRect.anchoredPosition;
            titleRect.anchoredPosition = new Vector2(currentPos.x, titleYOffset);
        }
        
        // 재료 텍스트 위치를 제목 기준으로 먼저 설정 (크기 계산을 위해)
        UpdateIngredientsTextPosition(dynamicYOffset);
        
        // 텍스트 크기 계산 후 툴팁 크기 조정
        UpdateTooltipSize(dynamicYOffset);
        
        // 크기 조정 후 타이틀 위치 다시 설정 (크기 변경으로 인한 위치 이동 방지)
        if (titleText != null) {
            RectTransform titleRect = titleText.rectTransform;
            Vector2 currentPos = titleRect.anchoredPosition;
            titleRect.anchoredPosition = new Vector2(currentPos.x, titleYOffset);
        }
        
        // 크기 조정 후 재료 텍스트 위치 다시 설정 (크기가 바뀌었을 수 있으므로)
        UpdateIngredientsTextPosition(dynamicYOffset);
        
        // 전달받은 position 파라미터로 위치 설정
        if (tooltipRect != null) {
            tooltipRect.position = position;
        }
    }
    
    /// <summary>
    /// 재료 텍스트 생성 (보유량에 따라 색상 적용)
    /// </summary>
    protected virtual string BuildIngredientsText(CraftingRecipe recipe, Dictionary<string, bool> ingredientAvailability) {
        if (recipe.ingredients == null || recipe.ingredients.Length == 0)
            return "재료 없음";
        
        System.Text.StringBuilder result = new System.Text.StringBuilder();
        foreach (var ingredient in recipe.ingredients) {
            string displayName = string.IsNullOrEmpty(ingredient.displayName) 
                ? ingredient.itemType : ingredient.displayName;
            
            bool hasEnough = ingredientAvailability != null && 
                ingredientAvailability.ContainsKey(ingredient.itemType) && 
                ingredientAvailability[ingredient.itemType];
            
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
    /// 툴팁 크기를 타이틀과 재료 텍스트에 맞춰 자동 조정
    /// </summary>
    protected virtual void UpdateTooltipSize(float dynamicYOffset = 0f) {
        if (tooltipRect == null || titleText == null || ingredientsText == null) return;
        
        // RectTransform 설정이 올바른지 확인 (매번 확인하여 Inspector 설정이 덮어쓰는 것을 방지)
        if (tooltipRect.anchorMin != new Vector2(0f, 1f) || tooltipRect.anchorMax != new Vector2(0f, 1f)) {
            SetupRectTransform();
        }
        
        // 텍스트 렌더링 강제 업데이트 (크기 계산을 위해)
        Canvas.ForceUpdateCanvases();
        
        // 타이틀 텍스트의 실제 크기
        float titleWidth = titleText.preferredWidth;
        float titleHeight = titleText.preferredHeight;
        
        // 재료 텍스트의 실제 크기
        float ingredientsWidth = ingredientsText.preferredWidth;
        float ingredientsHeight = ingredientsText.preferredHeight;
        
        // 두 텍스트의 위치를 고려하여 전체 크기 계산
        RectTransform titleRect = titleText.rectTransform;
        RectTransform ingredientsRect = ingredientsText.rectTransform;
        
        // 타이틀의 위치 (로컬 좌표 기준, pivot 고려)
        Vector2 titlePos = titleRect.anchoredPosition;
        Vector2 titlePivot = titleRect.pivot;
        
        // 재료 텍스트의 위치 (로컬 좌표 기준, 동적 Y 오프셋 적용)
        Vector2 ingredientsPos = new Vector2(
            titlePos.x + ingredientsTextOffset.x,
            titlePos.y + ingredientsTextOffset.y + dynamicYOffset
        );
        Vector2 ingredientsPivot = ingredientsRect.pivot;
        
        // 텍스트의 실제 bounds 계산 (pivot 고려)
        // 타이틀 bounds
        float titleLeft = titlePos.x - (titleWidth * titlePivot.x);
        float titleRight = titlePos.x + (titleWidth * (1f - titlePivot.x));
        float titleTop = titlePos.y + (titleHeight * (1f - titlePivot.y));
        float titleBottom = titlePos.y - (titleHeight * titlePivot.y);
        
        // 재료 텍스트 bounds
        float ingredientsLeft = ingredientsPos.x - (ingredientsWidth * ingredientsPivot.x);
        float ingredientsRight = ingredientsPos.x + (ingredientsWidth * (1f - ingredientsPivot.x));
        float ingredientsTop = ingredientsPos.y + (ingredientsHeight * (1f - ingredientsPivot.y));
        float ingredientsBottom = ingredientsPos.y - (ingredientsHeight * ingredientsPivot.y);
        
        // 전체 bounds 계산
        float minX = Mathf.Min(titleLeft, ingredientsLeft);
        float maxX = Mathf.Max(titleRight, ingredientsRight);
        float minY = Mathf.Min(titleBottom, ingredientsBottom);
        float maxY = Mathf.Max(titleTop, ingredientsTop);
        
        // 전체 크기 계산 (위아래 30px, 좌우 20px 여유 공간 추가)
        float totalWidth = (maxX - minX) + 200f; // 좌우 각각 20px = 40px
        float totalHeight = (maxY - minY) + 140f; // 위아래 각각 30px = 60px
        
        // 최소 크기 보장
        totalWidth = Mathf.Max(totalWidth, 100f);
        totalHeight = Mathf.Max(totalHeight, 50f);
        
        // 툴팁 크기 업데이트 (SetSizeWithCurrentAnchors 사용)
        tooltipRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, totalWidth);
        tooltipRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, totalHeight);
    }
    
    /// <summary>
    /// 재료 텍스트 위치를 제목 기준으로 설정
    /// </summary>
    protected virtual void UpdateIngredientsTextPosition(float dynamicYOffset = 0f) {
        if (titleText == null || ingredientsText == null) return;
        
        RectTransform titleRect = titleText.rectTransform;
        RectTransform ingredientsRect = ingredientsText.rectTransform;
        
        // 제목의 위치를 기준으로 재료 텍스트 배치 (동적 Y 오프셋 적용)
        Vector3 titlePos = titleRect.localPosition;
        ingredientsRect.localPosition = new Vector3(
            titlePos.x + ingredientsTextOffset.x,
            titlePos.y + ingredientsTextOffset.y + dynamicYOffset,
            titlePos.z
        );
    }
    
    
    /// <summary>
    /// 툴팁 숨기기
    /// </summary>
    public virtual void Hide() {
        if (canvasGroup != null) canvasGroup.alpha = 0f;
        gameObject.SetActive(false);
    }
}
