using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIHoverShowImage : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("호버 시 보이게/숨기게 할 이미지")]
    [SerializeField] private Image targetImage;

    private void Awake()
    {
        // targetImage를 지정 안 했으면, 자기 자신에서 찾기
        if (targetImage == null)
            targetImage = GetComponent<Image>();

        // 처음에는 안 보이게 시작하고 싶으면 주석 해제
        if (targetImage != null)
            targetImage.enabled = false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (targetImage != null)
            targetImage.enabled = true;   // 마우스가 올라왔을 때 보이기
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (targetImage != null)
            targetImage.enabled = false;  // 마우스가 나갔을 때 숨기기
    }
}