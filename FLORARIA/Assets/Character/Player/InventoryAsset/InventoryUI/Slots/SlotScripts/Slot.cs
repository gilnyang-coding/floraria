using System.Drawing;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace InventorySystem
{
    //Author Jaxon Schauer
    /// <summary>
    /// This class creates a slot gameObject that displays an image of the item when notified by the assigned inventory
    /// </summary>
    public class Slot : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField]
        private int position;//The position if the inventories items list
        [SerializeField]
        private GameObject slotChildPrefab;//This holds the prefab for the image allowing it to be instantiated when the child object is dragged to a new location
        [SerializeField]
        private GameObject SlotItemHolder;//This is a child object that is used to display an image of the object

        private InventoryItem item;//This is the current item in the inventory, there is always an item however item.GetIsNull() determines if the object contains a real item
        private UnityEngine.Color color;//This is the color of the slot
        private Image slotImage;//This is the image of the slot
        private InventoryUIManager inventoryUIManager;
        private Vector3 initialChildScale;//holds the scale for the slot child to allow for it to be instantiated with the correct size
        private Vector3 initialSlotChildPosition;//This holds the position of the slot child so it can be instantiated with the correct location
        private float textSize;
        private Vector2 SlotItemHolderSize;
        private bool returnOnMiss = false;//checks whether or not item should return to inventory when the user misses



        /// <summary>
        /// Sets essential variables for the inventory slot
        /// </summary>
        private void Awake()
        {
            slotImage = GetComponent<Image>();
            color = slotImage.color;

            inventoryUIManager = transform.parent.GetComponent<InventoryUIManager>();

            initialChildScale = SlotItemHolder.transform.localScale;


        }
        /// <summary>
        /// Initializes slot child, calling <see cref="UpdateSlot"/>
        /// </summary>
        private void Start()
        {
            item = inventoryUIManager.GetInventoryItem(position);
            initialSlotChildPosition = SlotItemHolder.transform.position;
            
            // CraftTable, FirePot, AlchemyPot은 각각의 Manager에서 직접 아이콘을 관리하므로 초기에는 비활성화
            string inventoryName = inventoryUIManager.GetInventoryName();
            if (inventoryName == InventoryNames.CraftTable || inventoryName == InventoryNames.FirePot || inventoryName == InventoryNames.AlchemyPot)
            {
                SlotItemHolder.SetActive(false);
            }
            else
            {
                SlotItemHolder.SetActive(true);
            }

            UpdateSlot();

        }
        /// <summary>
        /// Updates the slot to display the item in the slots associated position
        /// </summary>
        public void UpdateSlot()
        {
            // CraftTable, FirePot, AlchemyPot은 각각의 Manager에서 직접 아이콘을 관리
            string inventoryName = inventoryUIManager.GetInventoryName();
            switch (inventoryName)
            {
                case InventoryNames.CraftTable:
                    // CraftTable인 경우: 각 슬롯을 검사해서 레시피가 있으면 아이콘 활성화, 없으면 비활성화
                    if (CraftingManager.Instance != null)
                    {
                        // CraftingManager에 이 슬롯에 대한 레시피가 있는지 확인
                        var recipe = CraftingManager.Instance.GetRecipeBySlot(position);
                        if (recipe != null)
                        {
                            // 레시피가 있으면 아이콘 설정 및 활성화
                            CraftingManager.Instance.RefreshSlotIcon(position, this);
                            return;
                        }
                    }
                    
                    // 레시피가 없으면 SlotItemHolder 비활성화
                    SlotItemHolder.SetActive(false);
                    return; // CraftTable은 UpdateSlot 무시
                    
                case InventoryNames.FirePot:
                    // FirePot인 경우: 각 슬롯을 검사해서 레시피가 있으면 아이콘 활성화, 없으면 비활성화
                    if (FirePotManager.Instance != null)
                    {
                        // FirePotManager에 이 슬롯에 대한 레시피가 있는지 확인
                        var recipe = FirePotManager.Instance.GetRecipeBySlot(position);
                        if (recipe != null)
                        {
                            // 레시피가 있으면 아이콘 설정 및 활성화
                            FirePotManager.Instance.RefreshSlotIcon(position, this);
                            return;
                        }
                    }
                    
                    // 레시피가 없으면 SlotItemHolder 비활성화
                    SlotItemHolder.SetActive(false);
                    return; // FirePot은 UpdateSlot 무시
                    
                case InventoryNames.AlchemyPot:
                    // AlchemyPot인 경우: 각 슬롯을 검사해서 레시피가 있으면 아이콘 활성화, 없으면 비활성화
                    if (AlchemyManager.Instance != null)
                    {
                        // AlchemyManager에 이 슬롯에 대한 레시피가 있는지 확인
                        var recipe = AlchemyManager.Instance.GetRecipeBySlot(position);
                        if (recipe != null)
                        {
                            // 레시피가 있으면 아이콘 설정 및 활성화
                            AlchemyManager.Instance.RefreshSlotIcon(position, this);
                            return;
                        }
                    }
                    
                    // 레시피가 없으면 SlotItemHolder 비활성화
                    SlotItemHolder.SetActive(false);
                    return; // AlchemyPot은 UpdateSlot 무시
            }
            
            item = inventoryUIManager.GetInventoryItem(position);
            if (item != null)
            {
                if (!item.GetIsNull())
                {
                    DragItem dragItem = SlotItemHolder.GetComponent<DragItem>();
                    dragItem.SetItem(item);
                    dragItem.SetText();
                    SlotItemHolder.GetComponent<Image>().sprite = item.GetItemImage();
                    SlotItemHolder.SetActive(true);
                    
                    // Furnace는 모든 슬롯에서 드래그 가능 (입력/출력 모두 빼낼 수 있음)
                    dragItem.enabled = true;
                }
                else
                {

                    SlotItemHolder.SetActive(false);
                }
            }
            else
            {
                Debug.LogError("Item is null");
            }

        }
        /// <summary>
        /// Adds a new slotchild when slot child is dragged away, and resets the slot to empty
        /// </summary>
        public void ResetSlot()
        {
            GameObject newInstance = Instantiate(slotChildPrefab, initialSlotChildPosition, Quaternion.identity);
            newInstance.transform.SetParent(transform);
            newInstance.transform.localScale = initialChildScale;
            Vector2 prevTextPos = SlotItemHolder.GetComponent<DragItem>().GetTextPosition();

            SlotItemHolder = newInstance;
            DragItem slotDragItem= SlotItemHolder.GetComponent<DragItem>();
            slotDragItem.Initiailize();
            slotDragItem.SetTextPosition(prevTextPos);
            inventoryUIManager.GetInventory().EraseItemInPosition(position);
            SetChildImageSize(SlotItemHolderSize);
            SetTextSize(textSize);
            slotDragItem.SetReturnOnMiss(returnOnMiss);
            SlotItemHolder.SetActive(false);
        }
        /// <summary>
        /// 슬롯 클릭 처리
        /// </summary>
        public void OnPointerClick(PointerEventData eventData)
        {
            // 우클릭은 무시
            if (eventData.button != PointerEventData.InputButton.Left) return;
            
            if (inventoryUIManager == null) return; // inventoryUIManager가 null이면 리턴
            
            // CraftTable, FirePot, AlchemyPot인 경우 제작 실행
            string inventoryName = inventoryUIManager.GetInventoryName();
            switch (inventoryName)
            {
                case InventoryNames.CraftTable:
                    if (CraftingManager.Instance != null)
                    {
                        CraftingManager.Instance.OnSlotClicked(position);
                        return; // CraftTable은 일반 슬롯 동작 안 함
                    }
                    break;
                    
                case InventoryNames.FirePot:
                    if (FirePotManager.Instance != null)
                    {
                        FirePotManager.Instance.OnSlotClicked(position);
                        return; // FirePot은 일반 슬롯 동작 안 함
                    }
                    break;
                    
                case InventoryNames.AlchemyPot:
                    if (AlchemyManager.Instance != null)
                    {
                        AlchemyManager.Instance.OnSlotClicked(position);
                        return; // AlchemyPot은 일반 슬롯 동작 안 함
                    }
                    break;
            }
            
            // 일반 인벤토리 동작
            inventoryUIManager.SetPressed(gameObject);
            inventoryUIManager.MoveOnPress(gameObject);
        }
        
        /// <summary>
        /// 슬롯을 누르는 순간 아이템 사용 시도 (1초간 꾹 누르면 사용)
        /// 선택된 슬롯이 아니더라도 길게 클릭으로 바로 사용 가능
        /// </summary>
        public void OnPointerDown(PointerEventData eventData)
        {
            // 좌클릭만 처리
            if (eventData.button != PointerEventData.InputButton.Left) return;
            
            // CraftTable, FirePot, AlchemyPot은 길게 누르기 동작 안 함
            string inventoryName = inventoryUIManager.GetInventoryName();
            if (inventoryName == InventoryNames.CraftTable || inventoryName == InventoryNames.FirePot || inventoryName == InventoryNames.AlchemyPot) return;
            
            // 아이템이 있는 경우에만 사용 시도
            if (item != null && !item.GetIsNull())
            {
                if (ItemUsageManager.Instance != null)
                {
                    ItemUsageManager.Instance.UseItemFromSlot(item, inventoryName);
                }
            }
        }
        
        /// <summary>
        /// 마우스 진입 시 - CraftTable이면 툴팁 표시
        /// </summary>
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (inventoryUIManager == null) return; // inventoryUIManager가 null이면 리턴
            
            string inventoryName = inventoryUIManager.GetInventoryName();
            switch (inventoryName)
            {
                case InventoryNames.CraftTable:
                    if (CraftingManager.Instance != null)
                    {
                        CraftingManager.Instance.ShowTooltip(position, transform.position);
                    }
                    break;
                    
                case InventoryNames.FirePot:
                    if (FirePotManager.Instance != null)
                    {
                        FirePotManager.Instance.ShowTooltip(position, transform.position);
                    }
                    break;
                    
                case InventoryNames.AlchemyPot:
                    if (AlchemyManager.Instance != null)
                    {
                        AlchemyManager.Instance.ShowTooltip(position, transform.position);
                    }
                    break;
            }
        }
        
        /// <summary>
        /// 마우스 나갈 때 - 툴팁 숨김
        /// </summary>
        public void OnPointerExit(PointerEventData eventData)
        {
            if (inventoryUIManager == null) return; // inventoryUIManager가 null이면 리턴
            
            string inventoryName = inventoryUIManager.GetInventoryName();
            switch (inventoryName)
            {
                case InventoryNames.CraftTable:
                    if (CraftingManager.Instance != null)
                    {
                        CraftingManager.Instance.HideTooltip();
                    }
                    break;
                    
                case InventoryNames.FirePot:
                    if (FirePotManager.Instance != null)
                    {
                        FirePotManager.Instance.HideTooltip();
                    }
                    break;
                    
                case InventoryNames.AlchemyPot:
                    if (AlchemyManager.Instance != null)
                    {
                        AlchemyManager.Instance.HideTooltip();
                    }
                    break;
            }
        }
        public void SetTextSize(float size)
        {
            textSize = size;
            if (SlotItemHolder != null)
            {
                SlotItemHolder.GetComponent<DragItem>().SetTextSize(size);

            }
            else
            {
                Debug.LogError("Slot Child Null");
            }
        }
        public void SetTextOffset(Vector3 offset)
        {
            if (SlotItemHolder != null)
            {
                SlotItemHolder.GetComponent<DragItem>().SetTextPositionOffset(offset);

            }
            else
            {
                Debug.LogError("Slot Child Null");

            }
        }
        public void SetImageOffSet(Vector3 offset)
        {
            if (SlotItemHolder != null)
            {
                SlotItemHolder.GetComponent<DragItem>().SetImagePositionOffset(offset);

            }
            else
            {
                Debug.LogError("Slot Child Null");

            }
        }
        public void SetChildImageSize(Vector2 size)
        {
            SlotItemHolder.GetComponent<DragItem>().SetImageSize(size);
            SlotItemHolderSize = size;
        }
        public float GetTextSize()
        {
            return SlotItemHolder.GetComponent<DragItem>().GetTextSize();
        }
        public Image GetSlotImage()
        {
            return slotImage;
        }
        public void SetSlotImage(Image newImage)
        {
            slotImage = newImage;
        }
        public GameObject GetItemHolder()
        {
            return SlotItemHolder;
        }
        public InventoryUIManager GetInventoryUI()
        {
            return inventoryUIManager;
        }
        public UnityEngine.Color GetColor()
        {
            return color;
        }
        public InventoryItem GetItem()
        {
            return item;
        }
        public void SetPosition(int position)
        {
            this.position = position;
        }
        public int GetPosition()
        {
            return position;
        }
        public void SetReturnOnMiss(bool destroyOnMiss)
        {
            if (destroyOnMiss)
            {
                returnOnMiss = false;
                SlotItemHolder.GetComponent<DragItem>().SetReturnOnMiss(returnOnMiss);
            }
            else
            {
                returnOnMiss = true;
                SlotItemHolder.GetComponent<DragItem>().SetReturnOnMiss(returnOnMiss);
            }
        }
    }
}
