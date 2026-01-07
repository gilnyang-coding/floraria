using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 제작대 전체 UI 관리
/// - 10행 5열 그리드 레이아웃
/// - 마우스 휠 스크롤 지원
/// - 레시피 슬롯 생성 및 관리
/// </summary>
public class CraftingUI : MonoBehaviour {
    [Header("UI 구조")]
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform content;
    [SerializeField] private GridLayoutGroup gridLayout;
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private CraftingTooltip tooltip;
    
    [Header("그리드 설정")]
    [SerializeField] private int columns = 5;
    [SerializeField] private int visibleRows = 10;
    [SerializeField] private Vector2 cellSize = new Vector2(80f, 80f);
    [SerializeField] private Vector2 spacing = new Vector2(10f, 10f);
    
    [Header("메시지 UI")]
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private float messageDuration = 2f;
    
    [Header("토글 설정")]
    [SerializeField] private KeyCode toggleKey = KeyCode.C;
    
    private List<CraftingSlot> slots = new List<CraftingSlot>();
    private float messageTimer = 0f;
    
    private void Awake() {
        // 그리드 레이아웃 설정
        if (gridLayout != null) {
            gridLayout.cellSize = cellSize;
            gridLayout.spacing = spacing;
            gridLayout.constraintCount = columns;
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        }
        
        // 메시지 텍스트 숨김
        if (messageText != null) {
            messageText.gameObject.SetActive(false);
        }
        
        // 툴팁 숨김
        if (tooltip != null) {
            tooltip.Hide();
        }
    }
    
    private void Start() {
        // CraftingManager 이벤트 구독
        if (CraftingManager.Instance != null) {
            CraftingManager.Instance.OnCraftingMessage += ShowMessage;
            CraftingManager.Instance.OnInventoryChanged += RefreshAllSlots;
        }
        
        // 슬롯 생성
        CreateSlots();
        
        // 처음에는 비활성화
        gameObject.SetActive(false);
    }
    
    private void OnDestroy() {
        // 이벤트 구독 해제
        if (CraftingManager.Instance != null) {
            CraftingManager.Instance.OnCraftingMessage -= ShowMessage;
            CraftingManager.Instance.OnInventoryChanged -= RefreshAllSlots;
        }
    }
    
    private void Update() {
        // 토글 키 입력
        if (Input.GetKeyDown(toggleKey)) {
            Toggle();
        }
        
        // 메시지 타이머
        if (messageTimer > 0f) {
            messageTimer -= Time.unscaledDeltaTime;
            if (messageTimer <= 0f && messageText != null) {
                messageText.gameObject.SetActive(false);
            }
        }
    }
    
    /// <summary>
    /// 제작대 UI 토글
    /// </summary>
    public void Toggle() {
        bool newState = !gameObject.activeSelf;
        gameObject.SetActive(newState);
        
        if (newState) {
            RefreshAllSlots();
        } else {
            HideTooltip();
        }
    }
    
    /// <summary>
    /// 슬롯 생성
    /// </summary>
    private void CreateSlots() {
        if (CraftingManager.Instance == null || slotPrefab == null || content == null) {
            Debug.LogWarning("[CraftingUI] 필수 참조가 없습니다.");
            return;
        }
        
        // 기존 슬롯 제거
        foreach (var slot in slots) {
            if (slot != null) {
                Destroy(slot.gameObject);
            }
        }
        slots.Clear();
        
        // 레시피 가져오기
        var recipes = CraftingManager.Instance.GetUnlockedRecipes();
        
        // 각 레시피에 대해 슬롯 생성
        foreach (var recipe in recipes) {
            GameObject slotObj = Instantiate(slotPrefab, content);
            CraftingSlot slot = slotObj.GetComponent<CraftingSlot>();
            
            if (slot != null) {
                slot.Initialize(recipe, this);
                slots.Add(slot);
            }
        }
        
        // 콘텐츠 크기 조정
        UpdateContentSize();
    }
    
    /// <summary>
    /// 콘텐츠 크기 업데이트
    /// </summary>
    private void UpdateContentSize() {
        if (content == null || gridLayout == null) return;
        
        int totalSlots = slots.Count;
        int totalRows = Mathf.CeilToInt((float)totalSlots / columns);
        
        float contentHeight = totalRows * (cellSize.y + spacing.y) - spacing.y;
        contentHeight = Mathf.Max(contentHeight, visibleRows * (cellSize.y + spacing.y));
        
        content.sizeDelta = new Vector2(content.sizeDelta.x, contentHeight);
    }
    
    /// <summary>
    /// 모든 슬롯 상태 갱신
    /// </summary>
    public void RefreshAllSlots() {
        foreach (var slot in slots) {
            if (slot != null) {
                slot.RefreshState();
            }
        }
    }
    
    /// <summary>
    /// 툴팁 표시
    /// </summary>
    public void ShowTooltip(CraftingRecipe recipe, bool canCraft, Vector3 position) {
        if (tooltip != null) {
            tooltip.Show(recipe, canCraft, position);
        }
    }
    
    /// <summary>
    /// 툴팁 숨김
    /// </summary>
    public void HideTooltip() {
        if (tooltip != null) {
            tooltip.Hide();
        }
    }
    
    /// <summary>
    /// 메시지 표시
    /// </summary>
    public void ShowMessage(string message) {
        if (messageText != null) {
            messageText.text = message;
            messageText.gameObject.SetActive(true);
            messageTimer = messageDuration;
        }
    }
    
    /// <summary>
    /// 레시피 추가 시 슬롯 재생성
    /// </summary>
    public void RebuildSlots() {
        CreateSlots();
    }
}

