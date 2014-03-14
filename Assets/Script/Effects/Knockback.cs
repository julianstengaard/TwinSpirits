using UnityEngine;
using System.Collections;

public class Knockback : Effect 
{
	private float knockbackPower = 1000f;

	public Knockback(int power) {
		knockbackPower = power;
	}

	public override void DoEffect (BaseUnit target, GameObject source, Vector3 attackPosition, ref float damage)
	{
		Debug.Log("Knockback");
		//target.gameObject.rigidbody.AddRelativeForce((target.transform.position - attackPosition).normalized * knockbackPower);
		target.GetComponent<CharacterController>().SimpleMove((target.transform.position - attackPosition).normalized * knockbackPower);

	}

	public override IEnumerator DoEffectCoroutine (BaseUnit target, GameObject source, Vector3 attackPosition)
	{
		yield return null;
	}

	public override int GetPriority() {
		return 10;
	}

	public override bool IsSplashable ()
	{
		return false;
	}
}
