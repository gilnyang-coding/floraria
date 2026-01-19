using UnityEngine;
using UnityEngine.UI;

public class ChapterRegion : MonoBehaviour
{
    [Header("UI 연결")]
    [SerializeField] private Image targetImage;

    [Header("설정")]
    [SerializeField] private float hiddenAlpha = 0f;
    [Range(0f, 1f)]
    [SerializeField] private float pixelAlphaThreshold = 0.1f;

    private Color originalColor;
    private RectTransform rectTransform;

    private void Awake()
    {
        if (targetImage == null) targetImage = GetComponent<Image>();
        rectTransform = targetImage.GetComponent<RectTransform>();

        // 초기화
        originalColor = targetImage.color;
        SetHighlight(false); // 처음엔 꺼둠
    }

    // 외부(Manager)에서 호출하여 하이라이트 켜기/끄기
    public void SetHighlight(bool isActive)
    {
        if (targetImage == null) return;
        Color color = originalColor;
        // 호버 시에는 Image 컴포넌트의 원래 알파값 사용, 비호버 시에는 hiddenAlpha 사용
        color.a = isActive ? originalColor.a : hiddenAlpha;
        targetImage.color = color;
    }

    // 외부(Manager)에서 "이 지점이 투명한가?" 물어볼 때 사용
    public bool IsTransparentAt(Vector2 screenPosition, Camera eventCamera)
    {
        if (targetImage == null || targetImage.sprite == null || targetImage.sprite.texture == null)
            return true;

        if (rectTransform == null)
            return true;

        Vector2 localPoint;
        // 스크린 좌표를 로컬 좌표로 변환 실패하면 투명하다고 판단
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform, screenPosition, eventCamera, out localPoint))
        {
            return true;
        }

        // 1. Rect 범위 체크
        Rect rect = rectTransform.rect;
        if (!rect.Contains(localPoint))
            return true;

        // 2. 픽셀 알파값 체크
        try
        {
            float normalizedX = (localPoint.x - rect.x) / rect.width;
            float normalizedY = (localPoint.y - rect.y) / rect.height;

            // 정규화 좌표 범위 체크
            if (normalizedX < 0f || normalizedX > 1f || normalizedY < 0f || normalizedY > 1f)
                return true;

            Rect spriteRect = targetImage.sprite.textureRect;
            Texture2D texture = targetImage.sprite.texture;

            // UV 좌표 계산
            float u = spriteRect.x / texture.width + normalizedX * (spriteRect.width / texture.width);
            float v = spriteRect.y / texture.height + normalizedY * (spriteRect.height / texture.height);

            // 텍스처 픽셀 좌표로 변환
            int pixelX = Mathf.FloorToInt(u * texture.width);
            int pixelY = Mathf.FloorToInt(v * texture.height);

            // 텍스처 범위 체크
            if (pixelX < 0 || pixelX >= texture.width || pixelY < 0 || pixelY >= texture.height)
                return true;

            Color pixelColor = texture.GetPixel(pixelX, pixelY);
            
            // 알파값이 임계치보다 낮으면 "투명하다"고 판단
            return pixelColor.a < pixelAlphaThreshold;
        }
        catch (System.Exception e)
        {
            // 텍스처가 읽기 불가능한 경우 등
            Debug.LogWarning($"[ChapterRegion] 픽셀 체크 실패: {e.Message}. 스프라이트 '{targetImage.sprite.name}'의 Import 설정에서 'Read/Write Enabled'를 체크해주세요.");
            return true; // 에러나면 선택 안 되게
        }
    }
}