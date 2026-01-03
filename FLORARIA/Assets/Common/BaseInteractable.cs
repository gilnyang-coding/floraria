using UnityEngine;
using UnityEngine.InputSystem;

public abstract class BaseInteractable : MonoBehaviour {
    public GameObject interactionIcon;
    protected bool isPlayerInRange = false;
    protected GameObject playerRef;

    // "어떻게 상호작용할 것인가"는 자식이 결정합니다.
    public abstract void OnInteract(GameObject player);

    protected virtual void Start() {
        if (interactionIcon != null) interactionIcon.SetActive(false);
    }

    protected virtual void Update() {
        if (isPlayerInRange && Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame) {
            OnInteract(playerRef);
        }
        if (isPlayerInRange && interactionIcon != null) {
            interactionIcon.transform.LookAt(Camera.main.transform);
        }
    }

    protected virtual void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {
            isPlayerInRange = true;
            playerRef = other.gameObject;
            if (interactionIcon != null) interactionIcon.SetActive(true);
        }
    }

    protected virtual void OnTriggerExit(Collider other) {
        if (other.CompareTag("Player")) {
            isPlayerInRange = false;
            if (interactionIcon != null) interactionIcon.SetActive(false);
        }
    }
}