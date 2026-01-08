using UnityEngine;

/// <summary>
/// 무기 아이템 인터페이스
/// 인벤토리 아이템의 RelatedGameObject에 연결된 무기가 구현
/// </summary>
public interface IWeapon {
    /// <summary>
    /// 무기 공격 실행
    /// </summary>
    void Attack(GameObject user);
}

