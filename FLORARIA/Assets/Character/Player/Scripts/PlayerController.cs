using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour {
    [Header("Raycast Settings")]
    [SerializeField] private float maxRaycastDistance = 100f;

    private PlayerMove moveAction;
    private List<SkillBase> skills = new List<SkillBase>();
    private Vector3 targetPosition;
    private Camera mainCamera;

    void Start() {
        mainCamera = Camera.main;
        moveAction = GetComponent<PlayerMove>();
        targetPosition = transform.position;
        skills.AddRange(GetComponents<SkillBase>());
    }

    void Update() {
        HandleInput();
    }

    void FixedUpdate() {
        if (IsAnySkillActive()) {
            return;
        }

        if (moveAction != null) {
            // 스킬 직후 거리 오차를 방지하기 위해 정지 거리보다 멀 때만 명령
            moveAction.MoveTo(targetPosition);
        }
    }

    // 모든 스킬 리스트를 순회하며 하나라도 작동 중인지 체크하는 헬퍼 함수
    private bool IsAnySkillActive() {
        foreach (var skill in skills) {
            if (skill != null && skill.IsActive) {
                return true;
            }
        }
        return false;
    }

    private void HandleInput() {
        if (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame) {
            SetTargetPosition();
        }

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) {
            UseSkill(1); 
        }

        if (Keyboard.current != null) { //여러 스킬이 들어올 예정이라 if를 두 개로 나눔.
            if (Keyboard.current.leftShiftKey.wasPressedThisFrame) {
                if (moveAction != null && moveAction.GetIsMoving()) { //가만히 있을 떄는 못 씀
                    UseSkill(0); 
                }
            }
        }
    }

    private void SetTargetPosition() {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = mainCamera.ScreenPointToRay(mousePos);
        
        // Ground 레이어만 인식
        LayerMask groundLayerMask = LayerMask.GetMask("Ground");
        
        if (Physics.Raycast(ray, out RaycastHit hit, maxRaycastDistance, groundLayerMask)) {
            targetPosition = hit.point;
        }
    }

    private void UseSkill(int index) {
        if (index < skills.Count && skills[index].CanUseSkill()) {
            // 1. 공통사항: 현재 진행 중인 모든 걷기 물리/애니메이션 중단
            moveAction.Stop();

            Vector3 finalSkillTarget;

            if (index == 0) { 
                // [대쉬 전용] 사용자님의 통찰대로 목적지를 미래의 지점으로 멀리 보냅니다.
                finalSkillTarget = transform.position + transform.forward * 5f;
                targetPosition = finalSkillTarget; // 대쉬 종료 후 뒤돌지 않도록 목적지 선행 이동
            } else {
                // [공격 및 기타 스킬] 현재 내 발밑을 목적지로 설정하여 제자리에 멈추게 합니다.
                finalSkillTarget = transform.position; 
                targetPosition = transform.position; 
            }

            // 2. 결정된 목적지를 스킬에 전달하며 실행
            skills[index].Execute(finalSkillTarget); 
        }
    }
}