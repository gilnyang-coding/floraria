using UnityEngine;
using System.Collections;

public abstract class SkillBase : MonoBehaviour {
    [Header("Skill Info")]
    public string skillName;
    public float cooldown = 1f;
    protected bool canUse = true;
    
    // 현재 스킬이 실행 중인지 확인하는 Getter (캡슐화)
    public bool IsActive { get; protected set; }

    public bool CanUseSkill() {
        return canUse;
    }

    public abstract void Execute(Vector3 targetPos);

    protected IEnumerator CooldownRoutine() {
        canUse = false;
        yield return new WaitForSeconds(cooldown);
        canUse = true;
    }
}