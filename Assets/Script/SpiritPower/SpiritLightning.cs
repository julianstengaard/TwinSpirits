using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class SpiritLightning : SpiritPower 
{
	private GameObject 		lightningPrefab;
	private GameObject[] 	enemiesGameObjects;
	private BaseUnit[] 		enemies;
	private BaseUnit		currentTarget;

	private GameObject		 linkSingle;
	private List<GameObject> linkChain;

	private List<BaseUnit> chainedEnemies;
	private List<BaseUnit> nonChainedEnemies;

	private float damagePerSecond 	= 20f;
	private float damageOnSync 		= 100f;

	private float rangeSingle		= 20f;
	private float rangeChains		= 20f;
	private int	  numberOfChains	= 2;

	void Start() {
		costActivate 		=  10f;
		costPerSecond 		=  10f;
		costActivateSync 	= 100f;
	}
	
	/* BEGIN REGULAR POWER */
	public override IEnumerator OnActivate (Hero sourceHero, Hero otherHero)
	{
		currentTarget = FindNearestEnemy(otherHero.transform.position, false);

		//Create lightning to it
		if (currentTarget != null) {
			CreateLightningLink(otherHero, currentTarget, "SpiritLinkLightning");
		}

		Debug.Log("Activating" + this.GetType());

		return null;
	}

	private BaseUnit FindNearestEnemy(Vector3 source, bool sync)
	{
		//Find enemies
		enemiesGameObjects = GameObject.FindGameObjectsWithTag("Enemy");
		enemies = new BaseUnit[enemiesGameObjects.Length];
		
		for (int i = 0; i < enemiesGameObjects.Length; i++) {
			enemies[i] = enemiesGameObjects[i].GetComponent<BaseUnit>();
		}
		
		//Find nearest enemy
		float minDistanceSqr = 999666999;
		int nearestEnemy = -1;
		for (int i = 0; i < enemies.Length; i++) {
			float distanceSqr = (source - enemies[i].transform.position).sqrMagnitude;
			if (distanceSqr < minDistanceSqr) {
				nearestEnemy = i;
				minDistanceSqr = distanceSqr;
			}
		}

		float maxRange = sync ? rangeChains : rangeSingle;

		if (nearestEnemy == -1 || Mathf.Sqrt(minDistanceSqr) > maxRange)
			return null;
		else 
			return enemies[nearestEnemy];
	}

	private BaseUnit FindNearestEnemyChain(Vector3 source, List<BaseUnit> targets)
	{
		//Find nearest to chain to
		float minDistanceSqr = 999666999;
		BaseUnit nearestEnemy = null;
		foreach (BaseUnit target in targets) {
			float distanceSqr = (source - target.transform.position).sqrMagnitude;
			if (distanceSqr < minDistanceSqr) {
				nearestEnemy = target;
				minDistanceSqr = distanceSqr;
			}
		}
		if (nearestEnemy == null || Mathf.Sqrt(minDistanceSqr) > rangeChains)
			return null;
		else 
			return nearestEnemy;
	}

	private void CreateChainLightning(Vector3 source)
	{
		nonChainedEnemies 	= new List<BaseUnit>();
		chainedEnemies 		= new List<BaseUnit>();
		linkChain 			= new List<GameObject>();

		//Find enemies
		enemiesGameObjects = GameObject.FindGameObjectsWithTag("Enemy");
		enemies = new BaseUnit[enemiesGameObjects.Length];
		
		for (int i = 0; i < enemiesGameObjects.Length; i++) {
			enemies[i] = enemiesGameObjects[i].GetComponent<BaseUnit>();
			nonChainedEnemies.Add(enemies[i]);
		}

		//Find nearest enemy
		BaseUnit chainFrom = FindNearestEnemy(source, true);

		//Create the chain (if within range)
		if (chainFrom != null) {
			//Add the first enemy to it
			chainedEnemies.Add(chainFrom);
			//And remove it from the potential targets
			nonChainedEnemies.Remove(chainFrom);

			//Then try chaining further
			for (int i = 0; i < numberOfChains; i++) {
				BaseUnit currentEnemy = FindNearestEnemyChain(chainedEnemies.Last().transform.position, nonChainedEnemies);
				if (currentEnemy == null) {
					break;
				}
				chainedEnemies.Add(currentEnemy);
				nonChainedEnemies.Remove(currentEnemy);
			}
		}

		//Create effects and deal damage for chain
		BaseUnit previous = null;
		foreach (BaseUnit target in chainedEnemies) {
			target.TakeDamage(damageOnSync);
			if (target == chainFrom) {
				previous = target;
			}
			else {
				linkChain.Add(CreateChainLink(previous.transform.position, target.transform.position, "SpiritLinkLightning"));
				previous = target;
			}
		}

		//Remove the effects again
		foreach (GameObject chain in linkChain) {
			GameObject.Destroy(chain, 0.5f);
		}
	}



	public override IEnumerator OnUpdate (Hero sourceHero, Hero otherHero)
	{
		if (currentTarget != null && !currentTarget.dead) {
			UpdateLightningLink(otherHero, currentTarget);
			currentTarget.TakeDamage(damagePerSecond * Time.deltaTime);
		}
		else {
			currentTarget = FindNearestEnemy(otherHero.transform.position, false);
		}

		if (currentTarget == null) {
			DestroyLightningLink(0f);
		}

		return null;
	}
	public override IEnumerator OnDeactivate (Hero sourceHero, Hero otherHero)
	{
		Debug.Log("Deactivating" + this.GetType());
		DestroyLightningLink (0f);
		return null;
	}
	/* END REGULAR POWER */
	
	
	
	/* BEGIN SYNC POWER */
	public override bool OnPotentialSync (Hero sourceHero, Hero otherHero)
	{
		Debug.Log("Potential" + this.GetType() + " sync power!");
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
	public override IEnumerator OnActivateSync (Hero sourceHero, Hero otherHero)
	{
		Debug.Log("Activating" + this.GetType() + " SYNC POWER!");
		//CreateLightningLink(sourceHero, otherHero, "SpiritLinkRay");

		//Pay for activation
		sourceHero.ChangeSpiritAmount(-costActivateSync);
		otherHero.ChangeSpiritAmount(-costActivateSync);
		
		//Stop other Heros effect
		otherHero.SwitchToSyncPower();

		Vector3 midpoint = (sourceHero.transform.position + otherHero.transform.position) * 0.5f;

		CreateChainLightning(midpoint);

		return null;
	}

	public override IEnumerator OnUpdateSync (Hero sourceHero, Hero otherHero)
	{
		//UpdateBungieLink(sourceHero, otherHero);
		return null;
	}
	
	public override IEnumerator OnDeactivateSync (Hero sourceHero, Hero otherHero)
	{
		return null;
	}
	/* END SYNC POWER */


	private void CreateLightningLink (Hero otherHero, BaseUnit target, string prefab)
	{
		linkSingle = (GameObject) Instantiate(Resources.Load(prefab), Vector3.zero, Quaternion.identity); 
		UpdateLightningLink (otherHero, target);
	}

	private void UpdateLightningLink (Hero otherHero, BaseUnit target)
	{
		linkSingle.renderer.enabled = true;

		Vector3 midPoint = (otherHero.transform.position + target.transform.position) * 0.5f + Vector3.up;
		linkSingle.transform.position = midPoint;

		//Calculate scale/rotation
		float spanningScale = Vector3.Distance(otherHero.transform.position, target.transform.position)/10f;
		Quaternion rotation = Quaternion.LookRotation(otherHero.transform.position - target.transform.position, Vector3.up);
		rotation *= Quaternion.Euler(0, 90, 0);

		//Scale and rotate it
		var s = linkSingle.transform.localScale;
		s.x = spanningScale;
		linkSingle.transform.localScale = s;
		linkSingle.transform.localRotation = rotation;
	}

	private void DestroyLightningLink (float time)
	{
		GameObject.Destroy(linkSingle, time);
	}

	private GameObject CreateChainLink(Vector3 source, Vector3 target, string prefab) 
	{
		GameObject chainLink = (GameObject) Instantiate(Resources.Load(prefab), Vector3.zero, Quaternion.identity); 
		
		chainLink.renderer.enabled = true;
		
		Vector3 midPoint = (source + target) * 0.5f + Vector3.up;
		chainLink.transform.position = midPoint;
		
		//Calculate scale/rotation
		float spanningScale = Vector3.Distance(source, target)/10f;
		Quaternion rotation = Quaternion.LookRotation(source - target, Vector3.up);
		rotation *= Quaternion.Euler(0, 90, 0);
		
		//Scale and rotate it
		var s = chainLink.transform.localScale;
		s.x = spanningScale;
		chainLink.transform.localScale = s;
		chainLink.transform.localRotation = rotation;
		
		return chainLink;
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
