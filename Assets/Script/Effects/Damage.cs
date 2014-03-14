using UnityEngine;
using System.Collections;

public class Damage : Effect 
{
	private float damage = 10f;

	public Damage(float damage) {
		this.damage = damage;
	}

	public override void DoEffect (BaseUnit target, GameObject source, Vector3 attackPosition, ref float damage)
	{
		damage += this.damage;
	}

	public override IEnumerator DoEffectCoroutine (BaseUnit target, GameObject source, Vector3 attackPosition)
	{
		yield return null;
	}

	public override int GetPriority() {
		return 1;
	}

	public override bool IsSplashable ()
	{
		return true;
	}
}
