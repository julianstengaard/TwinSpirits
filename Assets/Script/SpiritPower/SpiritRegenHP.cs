using UnityEngine;
using System.Collections;

public class SpiritRegenHP : SpiritPower {
	private float regenPerSecond = 1f;
	private float regenPerSync	 = 100f;
		
	void Start() {
		costActivate 		=  10f;
		costPerSecond 		=  10f;
		costActivateSync 	= 100f;
	}
	
	/* BEGIN REGULAR POWER */
	public override IEnumerator OnActivate (Hero sourceHero, Hero otherHero)
	{
		//Debug.Log("Activating" + this.GetType());
		otherHero.Heal(regenPerSecond);
		
		return base.OnActivate(sourceHero, otherHero);
	}
	public override IEnumerator OnUpdate (Hero sourceHero, Hero otherHero)
	{
		otherHero.Heal(regenPerSecond * Time.deltaTime);
		return null;
	}
	public override IEnumerator OnDeactivate (Hero sourceHero, Hero otherHero, bool onDestroy)
	{
		//Debug.Log("Deactivating" + this.GetType());
//		if(IsActive && healthBarDefaultColor != null)
//			otherHero.HealthBar.renderer.material.SetColor("_Color", healthBarDefaultColor);


		return base.OnDeactivate(sourceHero, otherHero, onDestroy);
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
    public override IEnumerator OnActivateSync(Hero sourceHero, Hero otherHero, bool secondSync = false)
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

		//Heal both players
		otherHero.Heal(regenPerSync);
		sourceHero.Heal(regenPerSync);

		return null;
	}
	
	public override IEnumerator OnUpdateSync (Hero sourceHero, Hero otherHero)
	{
		return null;
	}
	
	public override IEnumerator OnDeactivateSync (Hero sourceHero, Hero otherHero)
	{
		//Debug.Log("Deactivating" + this.GetType() + " SYNC POWER!");
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
