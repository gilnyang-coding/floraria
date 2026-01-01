using UnityEngine;

public class RecoveryZone : ErosionAreaBase {
    protected override void ApplyEffect(PlayerErosion erosion) {
        // 수치를 감소(회복)시킵니다.
        erosion.AddErosion(-amountPerSecond * Time.deltaTime);
        erosion.SetInRecoveryArea(true);
    }

    protected override void ResetStatus(PlayerErosion erosion) {
        erosion.SetInRecoveryArea(false);
    }
}