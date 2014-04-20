using UnityEngine;

public class MeleeActivator : BaseUnit {
	[SerializeField]
	protected SpawnActivator activator;

    protected new void Start() {
        base.Start();
        usesGravity = false;
    }

	public override void SetMovementSpeedBuff (float movementSpeedBuff) { }
}
