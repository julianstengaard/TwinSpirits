using UnityEngine;
using System.Collections.Generic;

public class MeleeActivator : BaseUnit {
	[SerializeField]
	protected SpawnActivator activator;
	public GameObject TakeDamageParticlePrefab;

    protected new void Start() {
        base.Start();
        usesGravity = true;
    }

	public override void EvaluateAttacks(GameObject attacker, Vector3 origin, List<Effect> effects, string[] immuneTags) {
		print ("im the boss");
		if (CollisionTargetIsValid(immuneTags))
		{
			var damage = 0f;
			foreach(var e in effects) 
			{
				if (e.GetType() == typeof(Knockback)) {
					print ("no knockback");
					continue;
				}
				e.DoEffect(this, attacker, origin, ref damage);
				StartCoroutine(e.DoEffectCoroutine(this, attacker, origin));
			}
			TakeDamage(damage, attacker);
		}
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
