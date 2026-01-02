using UnityEngine;
using UnityEngine.InputSystem;

public class ItemPickup : MonoBehaviour
{
    public GameObject interactionIcon; // 자식으로 넣은 F키 스프라이트
    private bool isPlayerInRange = false;

    void Start()
    {
        if (interactionIcon != null)
            interactionIcon.SetActive(false); // 시작할 때 아이콘 숨기기
    }

    void Update()
    {
        if (isPlayerInRange)
        {
            // 1. 아이콘이 항상 카메라를 바라보게 함 (빌보드 효과)
            if (interactionIcon != null)
            {
                interactionIcon.transform.LookAt(Camera.main.transform);
                // 스프라이트가 뒤집혀 보인다면 아래 코드 주석을 해제하세요.
                // interactionIcon.transform.Rotate(0, 180, 0); 
            }

            // 2. F키를 누르면 획득
            if (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
            {
                PickUp();
            }
        }
    }

    void PickUp()
    {
        Debug.Log(gameObject.name + " 획득!");
        // 인벤토리 처리 로직 추가 가능
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other) {
        Debug.Log("무언가 들어옴: " + other.name);
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            if (interactionIcon != null) interactionIcon.SetActive(true); // 아이콘 표시
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            if (interactionIcon != null) interactionIcon.SetActive(false); // 아이콘 숨기기
        }
    }
}