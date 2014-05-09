using UnityEngine;

public class MeleeActivator : BaseUnit {
	[SerializeField]
	protected SpawnActivator activator;
	public GameObject TakeDamageParticlePrefab;

    protected new void Start() {
        base.Start();
        usesGravity = true;
    }

	public override void TakeDamage(float damage, GameObject src, bool forceKill = false) {
		if (!immortal) {
			Health = Mathf.Max(0, Health - damage);
			damageLockTimer = 0.3f;
			
			//Fire particles
			if (TakeDamageParticlePrefab != null) {
				Vector3 particlePoint = transform.position + Vector3.up + (src.transform.position - transform.position).normalized * 1.0f;
				GameObject.Instantiate(TakeDamageParticlePrefab, particlePoint, Quaternion.identity);
			}

			if(Health <= 0 && !dead) {
				Died ();
			}
		}
	}

	public override void SetMovementSpeedBuff (float movementSpeedBuff) { }
}
