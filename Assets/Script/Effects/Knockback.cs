using UnityEngine;
using System.Collections;

public class Knockback : Effect 
{
	private float knockbackPower = 1000f;
	private CharacterController _cc;
	private int _framesToKnockbackOver = 10;

	public Knockback(int power) {
		knockbackPower = power;
	}

	public override void DoEffect (BaseUnit target, GameObject source, Vector3 attackPosition, ref float damage)
	{
		Debug.Log("Knockback");
	}

	public override IEnumerator DoEffectCoroutine (BaseUnit target, GameObject source, Vector3 attackPosition)
	{
		var cc = target.GetComponent<CharacterController>();
		var direction = (target.transform.position - attackPosition).normalized;
		for (int i = 0; i < _framesToKnockbackOver; i++) {
			yield return new WaitForFixedUpdate();
			if (target.dead)
				break;

			float move = EasedMove(i/(float)_framesToKnockbackOver);
			cc.Move(direction * (move * (knockbackPower/_framesToKnockbackOver)));
		}
		yield return null;
	}

	private float EasedMove(float pct)
	{
		if (pct < 0.5f)
			return (1f - Mathf.Pow(1f - pct, 3f));
		else 
			return -1f * (Mathf.Cos(Mathf.PI*pct) - 1f);
	}

	public override int GetPriority() {
		return 10;
	}

	public override bool IsSplashable ()
	{
		return false;
	}
}
