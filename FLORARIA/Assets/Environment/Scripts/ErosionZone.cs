using UnityEngine;

public class ErosionZone : ErosionAreaBase {
    protected override void ApplyEffect(PlayerErosion erosion) {
        // 수치를 증가시킵니다.
        erosion.AddErosion(amountPerSecond * Time.deltaTime);
        erosion.SetInContaminatedArea(true);
    }

    protected override void ResetStatus(PlayerErosion erosion) {
        erosion.SetInContaminatedArea(false);
    }
}