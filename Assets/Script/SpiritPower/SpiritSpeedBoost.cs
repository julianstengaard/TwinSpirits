using UnityEngine;
using System.Collections;

public class SpiritSpeedBoost : SpiritPower 
{
	private Color 		targetHeroColor;
	private BaseUnit[] 	enemiesToSlow;

	private float 		speedBoost			= 10f;
	private float 		spiritSyncSlow		=  5f;
	private float 		spiritSyncDuration 	=  5f;

	void Start() {
		costActivate 		=  10f;
		costPerSecond 		=  10f;
		costActivateSync 	= 100f;
	}

	/* BEGIN REGULAR POWER */
	public override IEnumerator OnActivate (Hero sourceHero, Hero otherHero)
	{
		base.OnActivate(sourceHero, otherHero);
		//Debug.Log("Activating" + this.GetType());

//		targetHeroColor = otherHero.renderer.material.GetColor("_Color");
//		otherHero.renderer.material.SetColor("_Color", new Color(0, 255, 255));

		otherHero.movementSpeedBuff += speedBoost;

		return null;
	}
	public override IEnumerator OnUpdate (Hero sourceHero, Hero otherHero)
	{
		return null;
	}
	public override IEnumerator OnDeactivate (Hero sourceHero, Hero otherHero)
	{
		//Debug.Log("Deactivating" + this.GetType());

//		otherHero.renderer.material.SetColor("_Color", targetHeroColor);

		if(IsActive)
			otherHero.movementSpeedBuff -= speedBoost;

		return base.OnDeactivate(sourceHero, otherHero);
	}
	/* END REGULAR POWER */



	/* BEGIN SYNC POWER */
	public override bool OnPotentialSync (Hero sourceHero, Hero otherHero)
	{
		//Debug.Log("Potential" + this.GetType() + " sync power!");
		potentialSyncTime = Time.time;

		//If other Hero has pressed already
		if (Mathf.Abs(potentialSyncTime - otherHero.currentSpiritPower.potentialSyncTime) < timeWindowForSync)
		{
			syncActive = true;
			OnActivateSync(sourceHero, otherHero);
			return true;
		}
		else
		{
			return false;
		}

	}
	public override IEnumerator OnActivateSync (Hero sourceHero, Hero otherHero, bool secondSync = true)
	{
		//Debug.Log("Activating" + this.GetType() + " SYNC POWER!");

        if (!secondSync)
        {
            //Pay for activation
            sourceHero.ChangeSpiritAmount(-costActivateSync);
            otherHero.ChangeSpiritAmount(-costActivateSync);

            //Stop other Heros effect
            otherHero.SwitchToSyncPower();
        }

		//Find enemies to do effect on
		GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
		enemiesToSlow = new BaseUnit[enemies.Length];

		for (int i = 0; i < enemies.Length; i++)
		{
			enemiesToSlow[i] = enemies[i].GetComponent<BaseUnit>();
			enemiesToSlow[i].SetMovementSpeedBuff(-spiritSyncSlow);
		}
		StartCoroutine(OnDeactivateSync(sourceHero, otherHero));

		return null;
	}

	public override IEnumerator OnUpdateSync (Hero sourceHero, Hero otherHero)
	{
		return null;
	}

	public override IEnumerator OnDeactivateSync (Hero sourceHero, Hero otherHero)
	{
		yield return new WaitForSeconds(spiritSyncDuration);

		//Debug.Log("Deactivating" + this.GetType() + " SYNC POWER!");
		foreach (var enemy in enemiesToSlow)
		{
			if (enemy != null)
			{
				enemy.SetMovementSpeedBuff(spiritSyncSlow);
			}
		}
		yield return null;
	}
	/* END SYNC POWER */


	public override float GetCostActivate ()
	{
		return costActivate;
	}
	public override float GetCostThisUpdate ()
	{
		return costPerSecond * Time.deltaTime;
	}
	public override float GetCostActivateSync ()
	{
		return costActivateSync;
	}

}
