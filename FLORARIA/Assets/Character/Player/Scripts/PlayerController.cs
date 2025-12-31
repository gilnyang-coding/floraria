using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour {
    private PlayerMove moveAction;
    private List<SkillBase> skills = new List<SkillBase>();

    void Start() {
        moveAction = GetComponent<PlayerMove>();
        // 현재 오브젝트에 붙어있는 모든 SkillBase 타입의 컴포넌트를 가져옵니다 (의존성 주입과 유사)
        skills.AddRange(GetComponents<SkillBase>());
    }

    void Update() {
        HandleInput();
    }

    private void HandleInput() {
        // Shift 키를 누르면 첫 번째 스킬(예: Dash)을 실행
        if (Keyboard.current.leftShiftKey.wasPressedThisFrame) {
            UseSkill(0); 
        }

        // Q 키를 누르면 두 번째 스킬을 실행 (확장성)
        if (Keyboard.current.qKey.wasPressedThisFrame) {
            UseSkill(1);
        }
    }

    private void UseSkill(int index) {
        if (index < skills.Count && skills[index].CanUseSkill() && moveAction.GetIsMoving()) {
            skills[index].Execute(transform.position + transform.forward); // 예시 타겟
        }
    }
}