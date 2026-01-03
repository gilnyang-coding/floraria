using UnityEngine;

public abstract class BaseItem : BaseInteractable {
    public ItemData data;

    public override void OnInteract(GameObject player) {
        Inventory inv = player.GetComponent<Inventory>();
        if (inv != null) {
            inv.AddItem(data, 1);
            Destroy(gameObject);
        }
    }
}