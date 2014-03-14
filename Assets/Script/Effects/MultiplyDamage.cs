using UnityEngine;
using System.Collections;

public class MultiplyDamage : Effect 
{
	private float multiplier = 2f;

	public MultiplyDamage(float multiplier) {
		this.multiplier = multiplier;
	}

	public override void DoEffect (BaseUnit target, GameObject source, Vector3 attackPosition, ref float damage)
	{
		Debug.Log("Multiplying damage");
		damage *= multiplier;
	}

	public override IEnumerator DoEffectCoroutine (BaseUnit target, GameObject source, Vector3 attackPosition)
	{
		yield return null;
	}

	public override int GetPriority() {
		return 100;
	}

	public override bool IsSplashable ()
	{
		return true;
	}
}
