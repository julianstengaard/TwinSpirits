using UnityEngine;
using System.Collections;

public class SpiritBungie : SpiritPower 
{
	private Color 		targetHeroColor;
	private BaseUnit[] 	enemiesToKill;

	private float 		currentBungieTime	= 	 0f;
	private float 		durationToBungie	= 	 0.5f;
	private float 		closestDistance		= 	 2f;
	private Vector3 	initialPosition;

	private float 		damageOnSync		=  100f;

	private CharacterController heroCC;

	private GameObject link;

	void Start() {
		costActivate 		=  10f;
		costPerSecond 		=  10f;
		costActivateSync 	= 100f;
	}
	
	/* BEGIN REGULAR POWER */
	public override IEnumerator OnActivate (Hero sourceHero, Hero otherHero)
	{
		CreateBungieLink(otherHero, sourceHero, "SpiritLinkChain");

		//Debug.Log("Activating" + this.GetType());

		heroCC = otherHero.GetComponent<CharacterController>();

		currentBungieTime = 0;
		initialPosition = otherHero.transform.position;
		Vector3 directionToBungie = GetBungieMovement(currentBungieTime, initialPosition, sourceHero.transform.position - initialPosition, durationToBungie);
		heroCC.Move(directionToBungie - otherHero.transform.position);

		return null;
	}
	public override IEnumerator OnUpdate (Hero sourceHero, Hero otherHero)
	{
		if ((otherHero.transform.position - sourceHero.transform.position).magnitude > closestDistance)
		{
			UpdateBungieLink(otherHero, sourceHero);
			currentBungieTime = Mathf.Clamp(currentBungieTime + Time.deltaTime, 0, durationToBungie);
			Vector3 directionToBungie = GetBungieMovement(currentBungieTime, initialPosition, sourceHero.transform.position - initialPosition, durationToBungie);
			heroCC.Move(directionToBungie - otherHero.transform.position);
		}
		else
		{
			link.renderer.enabled = false;
			currentBungieTime = 0;
			initialPosition = otherHero.transform.position;
		}

		return null;
	}
	public override IEnumerator OnDeactivate (Hero sourceHero, Hero otherHero)
	{
		//Debug.Log("Deactivating" + this.GetType());
		DestroyBungieLink (0f);
		return null;
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
	public override IEnumerator OnActivateSync (Hero sourceHero, Hero otherHero, bool secondSync = false)
	{
		//Debug.Log("Activating" + this.GetType() + " SYNC POWER!");
		CreateBungieLink(sourceHero, otherHero, "SpiritLinkRay");

        if (!secondSync)
        {
            //Pay for activation
            sourceHero.ChangeSpiritAmount(-costActivateSync);
            otherHero.ChangeSpiritAmount(-costActivateSync);

            //Stop other Heros effect
            otherHero.SwitchToSyncPower();
        }

		StartCoroutine(DeathRay(sourceHero, otherHero));

		DestroyBungieLink (0.5f);
		return null;
	}

	IEnumerator DeathRay(Hero sourceHero, Hero otherHero)
	{
		yield return new WaitForFixedUpdate();

		//Create the ray
		Vector3 raycastDirection = (otherHero.transform.position - sourceHero.transform.position).normalized;
		Vector3 raycastFrom 	 = sourceHero.transform.position - raycastDirection;
		Vector3 raycastTo	 	 = otherHero.transform.position  + raycastDirection;
		float	raycastDistance	 = (raycastFrom - raycastTo).magnitude;

		RaycastHit[] hits;
		hits = Physics.SphereCastAll(raycastFrom, 2f, raycastDirection, raycastDistance, 1 << 8);

		//Find enemies to do effect on
		foreach (RaycastHit hit in hits) {
			if (hit.collider.gameObject.tag == "Enemy") {
				hit.collider.gameObject.GetComponent<BaseEnemy>().TakeDamage(damageOnSync);
			}
		}
	}

	public override IEnumerator OnUpdateSync (Hero sourceHero, Hero otherHero)
	{
		UpdateBungieLink(sourceHero, otherHero);
		return null;
	}
	
	public override IEnumerator OnDeactivateSync (Hero sourceHero, Hero otherHero)
	{
		return null;
	}
	/* END SYNC POWER */
	

	private Vector3 GetBungieMovement (float time, Vector3 startValue, Vector3 changeValue, float duration)
	{
		//float t = time / (duration/2f);
		return -changeValue/2 * (Mathf.Cos(Mathf.PI*time/duration) - 1) + startValue;
	}

	private void CreateBungieLink (Hero sourceHero, Hero otherHero, string prefab)
	{
		link = (GameObject) Instantiate(Resources.Load(prefab), Vector3.zero, Quaternion.identity); 
		UpdateBungieLink (sourceHero, otherHero);

	}

	private void UpdateBungieLink (Hero sourceHero, Hero otherHero)
	{
		link.renderer.enabled = true;

		Vector3 midPoint = (sourceHero.transform.position + otherHero.transform.position) * 0.5f + Vector3.up;
		link.transform.position = midPoint;

		//Calculate scale/rotation
		float spanningScale = Vector3.Distance(sourceHero.transform.position, otherHero.transform.position)/10f;
		Quaternion rotation = Quaternion.LookRotation(sourceHero.transform.position - otherHero.transform.position, Vector3.up);
		rotation *= Quaternion.Euler(0, 90, 0);

		//Scale and rotate it
		var s = link.transform.localScale;
		s.x = spanningScale;
		link.transform.localScale = s;
		link.transform.localRotation = rotation;
	}

	private void DestroyBungieLink (float time)
	{
		GameObject.Destroy(link, time);
	}

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
