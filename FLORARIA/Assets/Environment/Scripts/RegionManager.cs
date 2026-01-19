using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RegionManager : MonoBehaviour
{
    [SerializeField] private GraphicRaycaster graphicRaycaster;
    [SerializeField] private EventSystem eventSystem;

    // 현재 하이라이트 중인 챕터
    private ChapterRegion currentHoveredRegion;

    private void Awake()
    {
        // 자동으로 찾기
        if (graphicRaycaster == null)
            graphicRaycaster = GetComponent<GraphicRaycaster>();
        
        if (graphicRaycaster == null)
            graphicRaycaster = GetComponentInParent<GraphicRaycaster>();
        
        if (eventSystem == null)
            eventSystem = EventSystem.current;
        
        // 경고 메시지
        if (graphicRaycaster == null)
            Debug.LogError("[RegionManager] GraphicRaycaster를 찾을 수 없습니다! Canvas에 GraphicRaycaster 컴포넌트가 있는지 확인해주세요.");
        
        if (eventSystem == null)
            Debug.LogError("[RegionManager] EventSystem을 찾을 수 없습니다! 씬에 EventSystem이 있는지 확인해주세요.");
    }

    private void Update()
    {
        // null 체크
        if (graphicRaycaster == null || eventSystem == null)
            return;

        // 1. 마우스 포인터 데이터 생성
        PointerEventData pointerData = new PointerEventData(eventSystem);
        pointerData.position = Input.mousePosition;

        // 2. 레이캐스트 발사 (UI용)
        List<RaycastResult> results = new List<RaycastResult>();
        graphicRaycaster.Raycast(pointerData, results);

        ChapterRegion topPriorityRegion = null;

        // 3. 맞은 UI들을 순서대로 검사 (앞에 있는 것부터 나옴)
        foreach (RaycastResult result in results)
        {
            // ChapterRegion 컴포넌트가 있는지 확인
            ChapterRegion region = result.gameObject.GetComponent<ChapterRegion>();
            
            if (region != null)
            {
                // 픽셀이 투명하지 않은지(실제 그림 위에 있는지) 체크
                // eventCamera는 Overlay 모드면 null, Camera 모드면 worldCamera
                Canvas canvas = graphicRaycaster.GetComponent<Canvas>();
                Camera cam = canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay 
                    ? null : (canvas != null ? canvas.worldCamera : null);

                if (!region.IsTransparentAt(Input.mousePosition, cam))
                {
                    // 가장 먼저 발견된(가장 위에 있는) 유효한 리전 당첨
                    topPriorityRegion = region;
                    break; // 뒤에 있는 건 무시하고 반복문 종료
                }
            }
        }

        // 4. 상태 갱신
        if (currentHoveredRegion != topPriorityRegion)
        {
            // 기존 것 끄기
            if (currentHoveredRegion != null)
                currentHoveredRegion.SetHighlight(false);

            // 새 것 켜기
            if (topPriorityRegion != null)
                topPriorityRegion.SetHighlight(true);

            // 현재 상태 업데이트
            currentHoveredRegion = topPriorityRegion;
        }
    }
}