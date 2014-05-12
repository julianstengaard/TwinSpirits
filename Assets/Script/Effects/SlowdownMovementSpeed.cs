using UnityEngine;
using System.Collections;

public class SlowdownMovementSpeed : Effect 
{
	private float slowAmount = 2f;
	private float duration = 2f;
	private float hitChance;

	public SlowdownMovementSpeed(float slowAmount, float duration, float hitChance = 1) {
		this.slowAmount = slowAmount;
		this.hitChance = hitChance;
	}

	public override void DoEffect (BaseUnit target, GameObject source, Vector3 attackPosition, ref float damage)
	{
		//Debug.Log("Changing movement speed from: " + target.movementSpeedBuff + " to " +  (target.movementSpeedBuff + slowAmount));
	}

	public override IEnumerator DoEffectCoroutine (BaseUnit target, GameObject source, Vector3 attackPosition)
	{
		if(Random.Range(0f, 1f) < hitChance) {
			target.SetMovementSpeedBuff(-slowAmount);
			/* 
			float startTime = Time.time;
			Color slowColor = new Color(0f, 0f, 1f);
			while (startTime + duration > Time.time) {
				float pct = Mathf.PingPong((Time.time - startTime) * 10f, 1f);
				target.SetDamageBodyColor(slowColor, pct);
				yield return new WaitForEndOfFrame();
			}
			target.SetDamageBodyColor(slowColor, 0f);
			*/
			yield return new WaitForSeconds(duration);

			target.SetMovementSpeedBuff(slowAmount);
		}
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
