using UnityEngine;

/// <summary>
/// 세계수 수액(WorldTreeSap) 아이템
/// - 필드에 떨어져 있는 월드 오브젝트
/// - 플레이어가 상호작용하면 인벤토리로 들어감 (BaseItem 로직 사용)
/// - CraftTableInteractable.requiredItemName 이 "WorldTreeSap" 이므로
///   인벤토리/핫바에 이 아이템을 들고 있으면 제작대를 사용할 수 있음.
/// </summary>
public class WorldTreeSapItem : BaseItem
{
    // 현재는 BaseItem 의 기본 동작(떨어지기, 줍기)만 사용하면 충분해서
    // 별도의 추가 로직이 필요 없습니다.
    //
    // 나중에 세계수 수액을 사용(소비)했을 때 특수 효과를 주고 싶다면
    // ConsumableItemBase 를 상속받도록 바꾸고 OnConsume 을 구현하면 됩니다.
}

