using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 제작대 슬롯 UI
/// - 마우스 호버 시 툴팁 표시
/// - 클릭 시 제작 시도
/// - 제작 가능/불가능 상태에 따른 시각적 표시
/// </summary>
public class CraftingSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler {
    [Header("UI 참조")]
    [SerializeField] private Image itemIcon;
    [SerializeField] private Image backgroundImage;
    
    [Header("상태 색상")]
    [SerializeField] private Color canCraftColor = Color.white;
    [SerializeField] private Color cannotCraftColor = new Color(1f, 1f, 1f, 0.7f);
    
    private CraftingRecipe recipe;
    private CraftingUI craftingUI;
    private bool canCraft = false;
    
    /// <summary>
    /// 슬롯 초기화
    /// </summary>
    public void Initialize(CraftingRecipe recipe, CraftingUI craftingUI) {
        this.recipe = recipe;
        this.craftingUI = craftingUI;
        
        if (recipe != null) {
            // 아이콘 설정
            if (recipe.resultIcon != null) {
                itemIcon.sprite = recipe.resultIcon;
            } else {
                // ItemInitializer에서 아이콘 가져오기 시도
                Sprite icon = GetItemIcon(recipe.resultItemType);
                if (icon != null) {
                    itemIcon.sprite = icon;
                }
            }
            itemIcon.enabled = itemIcon.sprite != null;
        }
        
        RefreshState();
    }
    
    /// <summary>
    /// ItemInitializer에서 아이템 아이콘 가져오기
    /// </summary>
    private Sprite GetItemIcon(string itemType) {
        if (InventorySystem.InventoryController.instance == null) return null;
        
        var items = InventorySystem.InventoryController.instance.GetItems();
        foreach (var item in items) {
            if (item.GetItemType() == itemType) {
                return item.GetItemImage();
            }
        }
        return null;
    }
    
    /// <summary>
    /// 제작 가능 상태 갱신
    /// </summary>
    public void RefreshState() {
        if (recipe == null || CraftingManager.Instance == null) {
            canCraft = false;
        } else {
            canCraft = CraftingManager.Instance.CanCraft(recipe);
        }
        
        UpdateVisuals();
    }
    
    /// <summary>
    /// 시각적 상태 업데이트
    /// </summary>
    private void UpdateVisuals() {
        if (itemIcon != null) {
            // 제작 가능: 원래 색상, 제작 불가: 알파값 0.7
            itemIcon.color = canCraft ? canCraftColor : cannotCraftColor;
        }
    }
    
    /// <summary>
    /// 마우스 진입 시 툴팁 표시
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData) {
        if (recipe != null && craftingUI != null) {
            craftingUI.ShowTooltip(recipe, canCraft, transform.position);
        }
    }
    
    /// <summary>
    /// 마우스 나갈 때 툴팁 숨김
    /// </summary>
    public void OnPointerExit(PointerEventData eventData) {
        if (craftingUI != null) {
            craftingUI.HideTooltip();
        }
    }
    
    /// <summary>
    /// 클릭 시 제작 시도
    /// </summary>
    public void OnPointerClick(PointerEventData eventData) {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        if (recipe == null || CraftingManager.Instance == null) return;
        
        if (canCraft) {
            // 제작 실행
            bool success = CraftingManager.Instance.TryCraft(recipe);
            if (success) {
                // 제작 성공 시 모든 슬롯 상태 갱신
                craftingUI?.RefreshAllSlots();
            }
} else {
            // 제작 불가능 - 메시지 표시
            CraftingManager.Instance.ShowMessage("재료가 부족합니다.");
        }
    }
    
    public CraftingRecipe GetRecipe() => recipe;
    public bool CanCraft() => canCraft;
}

