using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Stun : Effect 
{
	private float 	duration = 1f;
	public 	float 	timeout;

	public Stun(float duration) 
	{
		this.duration = duration;
	}

	public override void DoEffect (BaseUnit target, GameObject source, Vector3 attackPosition, ref float damage)
	{
		Debug.Log("Stunning");
		timeout = Time.time + duration;

		if (target.stunTimeout < timeout)
		{
			target.stunned = true;
			target.stunTimeout = timeout;
		}
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
		return true;
	}
}
