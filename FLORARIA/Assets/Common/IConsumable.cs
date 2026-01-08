using UnityEngine;

/// <summary>
/// 소비 가능한 아이템이 구현해야 하는 인터페이스
/// </summary>
public interface IConsumable {
    /// <summary>
    /// 아이템을 소비합니다.
    /// </summary>
    /// <param name="user">아이템을 사용하는 주체 (보통 플레이어)</param>
    void Consume(GameObject user);
}

