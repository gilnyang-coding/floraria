using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

/// <summary>
/// 간단한 호버/클릭 효과가 있는 애니메이션 버튼
/// - Hover: 약간 커지기
/// - Click: 작아졌다가 원래 크기로 돌아가기
/// </summary>
public class AnimatedButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler {
    
    [Header("Base Settings")]
    [SerializeField] private RectTransform buttonRect;
    
    [Header("Hover Settings")]
    [Tooltip("호버 시 스케일 배수 (예: 1.1 = 10% 커짐)")]
    [SerializeField] private float hoverScale = 1.1f;
    [Tooltip("호버 스케일 전환 속도")]
    [SerializeField] private float hoverTransitionSpeed = 5f;
    
    [Header("Click Settings")]
    [Tooltip("클릭 시 작아지는 스케일 배수 (예: 0.9 = 10% 작아짐)")]
    [SerializeField] private float clickScale = 0.9f;
    [Tooltip("클릭 효과 지속 시간")]
    [SerializeField] private float clickEffectDuration = 0.15f;
    
    // 내부 변수
    private Vector3 baseScale;
    private Vector3 hoverScaleVector;
    private bool isHovering = false;
    private Coroutine clickEffectCoroutine;
    
    private void Awake() {
        // 컴포넌트 자동 찾기
        if (buttonRect == null) buttonRect = GetComponent<RectTransform>();
        
        // 기본 스케일 저장
        if (buttonRect != null) {
            baseScale = buttonRect.localScale;
            hoverScaleVector = baseScale * hoverScale;
        }
    }
    
    private void Update() {
        // Hover 시 스케일 전환 (클릭 효과 중이 아닐 때만)
        if (buttonRect != null && clickEffectCoroutine == null) {
            Vector3 targetScale = isHovering ? hoverScaleVector : baseScale;
            buttonRect.localScale = Vector3.Lerp(buttonRect.localScale, targetScale, Time.deltaTime * hoverTransitionSpeed);
        }
    }
    
    /// <summary>
    /// 마우스 진입 시
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData) {
        isHovering = true;
    }
    
    /// <summary>
    /// 마우스 나갈 때
    /// </summary>
    public void OnPointerExit(PointerEventData eventData) {
        isHovering = false;
    }
    
    /// <summary>
    /// 클릭 시
    /// </summary>
    public void OnPointerClick(PointerEventData eventData) {
        if (clickEffectCoroutine != null) {
            StopCoroutine(clickEffectCoroutine);
        }
        clickEffectCoroutine = StartCoroutine(ClickEffectCoroutine());
    }
    
    /// <summary>
    /// 클릭 효과 코루틴: 작아졌다가 원래 크기로 돌아가기
    /// </summary>
    private IEnumerator ClickEffectCoroutine() {
        if (buttonRect == null) yield break;
        
        Vector3 clickScaleVector = baseScale * clickScale;
        float elapsed = 0f;
        
        // 작아지는 단계 (절반 시간)
        while (elapsed < clickEffectDuration * 0.5f) {
            elapsed += Time.deltaTime;
            float t = elapsed / (clickEffectDuration * 0.5f);
            buttonRect.localScale = Vector3.Lerp(baseScale, clickScaleVector, t);
            yield return null;
        }
        
        // 원래 크기로 돌아가는 단계 (절반 시간)
        elapsed = 0f;
        while (elapsed < clickEffectDuration * 0.5f) {
            elapsed += Time.deltaTime;
            float t = elapsed / (clickEffectDuration * 0.5f);
            Vector3 targetScale = isHovering ? hoverScaleVector : baseScale;
            buttonRect.localScale = Vector3.Lerp(clickScaleVector, targetScale, t);
            yield return null;
        }
        
        // 최종 스케일 설정
        buttonRect.localScale = isHovering ? hoverScaleVector : baseScale;
        
        clickEffectCoroutine = null;
    }
    
    private void OnDestroy() {
        // 코루틴 정리
        if (clickEffectCoroutine != null) {
            StopCoroutine(clickEffectCoroutine);
        }
    }
}
