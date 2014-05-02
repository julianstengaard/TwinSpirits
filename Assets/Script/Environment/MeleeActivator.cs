using UnityEngine;

public class MeleeActivator : BaseUnit {
	[SerializeField]
	protected SpawnActivator activator;

    protected new void Start() {
        base.Start();
        usesGravity = true;
    }

	public override void SetMovementSpeedBuff (float movementSpeedBuff) { }
}
