using UnityEngine;
using UnityEngine.UI;

public class InventoryStateEffect : MonoBehaviour {
    [SerializeField] private GameObject dimPanel;

    // 인벤토리 UI 오브젝트가 활성화될 때 유니티가 자동으로 호출
    private void OnEnable() {
        SetInventoryState(true);
    }

    // 인벤토리 UI 오브젝트가 비활성화될 때 유니티가 자동으로 호출
    private void OnDisable() {
        SetInventoryState(false);
    }

    private void SetInventoryState(bool isOpen) {
        Time.timeScale = isOpen ? 0f : 1f;
        if (dimPanel != null) dimPanel.SetActive(isOpen);

        if (isOpen) {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        } 
        else {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None; 
        }
    }
}