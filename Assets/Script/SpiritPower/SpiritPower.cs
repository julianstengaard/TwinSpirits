using UnityEngine;
using System.Collections;

public abstract class SpiritPower : MonoBehaviour
{
	protected float costActivate 		=  10f;
	protected float costPerSecond 		=  10f;
	protected float costActivateSync 	= 100f;
	
	public float 	potentialSyncTime	=   0f;
	public float 	timeWindowForSync	=   0.5f;
	public bool 	syncActive			= false;

	public bool		IsActive {get; private set;}

	public virtual IEnumerator OnActivate		(Hero sourceHero, Hero otherHero) {
		IsActive = true;
		return null;
	}
	public abstract IEnumerator OnUpdate		(Hero sourceHero, Hero otherHero); 
	public virtual IEnumerator OnDeactivate	(Hero sourceHero, Hero otherHero, bool onDestroy = false) {
		IsActive = false;
		return null;
	}

	public abstract bool 		OnPotentialSync	(Hero sourceHero, Hero otherHero);

    public abstract IEnumerator OnActivateSync  (Hero sourceHero, Hero otherHero, bool secondSync = false);
	public abstract IEnumerator OnUpdateSync	(Hero sourceHero, Hero otherHero);
    public abstract IEnumerator OnDeactivateSync(Hero sourceHero, Hero otherHero, bool onDestroy = false);

	public abstract float 	GetCostActivate 	();
	public abstract float 	GetCostThisUpdate 	();
	
	public abstract float 	GetCostActivateSync	();
}
