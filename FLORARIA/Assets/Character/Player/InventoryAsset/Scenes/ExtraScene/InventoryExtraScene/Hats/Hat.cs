using UnityEngine;
using InventorySystem;
namespace InventorySampleScene
{
    public class Hat : MonoBehaviour
    {
        // Start is called before the first frame update
        [SerializeField]
        string hatName;
        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.name == "Player")
            {
                if (!InventoryController.instance.InventoryFull(InventoryNames.Hotbar, hatName))
                {
                    InventoryController.instance.AddItem(InventoryNames.Hotbar, hatName);
                    Destroy(gameObject);

                }
                else
                {
                    Debug.Log("Inventory Cannot Fit Item");
                }

            }
        }
    }
}
