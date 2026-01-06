using InventorySystem;
using UnityEngine;

// 인벤토리에서 좌클릭 시 소비되어 체력을 회복하는 포션 아이템
public class HealthPotionItem : BaseItem
{
    [SerializeField] private float healAmount = 30f;
    [SerializeField] private string playerTag = "Player";

    /// <summary>
    /// 인벤토리 슬롯을 좌클릭해 선택했을 때 호출하도록 UnityEvent에 연결
    /// </summary>
    public void Use(InventoryItem item) {
        if (item == null || item.GetIsNull()) return;

        GameObject playerObj = FindPlayer();
        if (playerObj == null) return;

        var health = playerObj.GetComponent<Health>();
        if (health == null) {
            Debug.LogWarning("[HealthPotionItem] Player에 Health 컴포넌트가 없습니다.");
            return;
        }

        if (InventoryController.instance != null) {
            // 사용한 포션 1개 소비
            InventoryController.instance.RemoveItem(item.GetInventory(), item, 1);
            health.Heal(healAmount);
        }
        else {
            Debug.LogWarning("[HealthPotionItem] InventoryController가 없어 아이템을 소비하지 못했습니다.");
        }
    }

    private GameObject FindPlayer() {
        GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObj == null) {
            Debug.LogWarning($"[HealthPotionItem] '{playerTag}' 태그를 가진 Player를 찾을 수 없습니다.");
        }
        return playerObj;
    }
}

