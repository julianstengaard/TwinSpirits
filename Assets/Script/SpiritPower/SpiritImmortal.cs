using UnityEngine;
using System.Collections;

public class SpiritImmortal : SpiritPower 
{
	private GameObject immortalitySphere;
	private GameObject pullSphere;

	private CharacterController[] enemies;
	private Vector3 center; 
	private float maxPullDistance = 10f; 
	private float maxPullDistanceSqr; 
	
	void Start() {
		costActivate 		=  10f;
		costPerSecond 		=  10f;
		costActivateSync 	= 100f;
		maxPullDistanceSqr  = maxPullDistance * maxPullDistance; 
	}
	
	/* BEGIN REGULAR POWER */
	public override IEnumerator OnActivate (Hero sourceHero, Hero otherHero)
	{
		//Debug.Log("Activating" + this.GetType());
		immortalitySphere = CreateShieldMesh(new Color(0f, 1f, 1f, 0.5f), otherHero.transform);

		otherHero.immortal = true;
		
		return null;
	}
	public static GameObject CreateShieldMesh(Color color, Transform trans) {
		var shieldMesh = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		shieldMesh.transform.position = trans.position + Vector3.up;
		shieldMesh.transform.localScale *= 2f;
		shieldMesh.collider.enabled = false;
		shieldMesh.renderer.material = new Material(Shader.Find("Transparent/Diffuse"));
		shieldMesh.renderer.material.SetColor("_Color", color);

		return shieldMesh;
	}
	public override IEnumerator OnUpdate (Hero sourceHero, Hero otherHero)
	{
		immortalitySphere.transform.position = otherHero.transform.position + Vector3.up;
		return null;
	}
	public override IEnumerator OnDeactivate (Hero sourceHero, Hero otherHero)
	{
		//Debug.Log("Deactivating" + this.GetType());
		GameObject.Destroy(immortalitySphere);
		otherHero.immortal = false;

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

	    if (!secondSync)
	    {
	        //Pay for activation
	        sourceHero.ChangeSpiritAmount(-costActivateSync);
	        otherHero.ChangeSpiritAmount(-costActivateSync);

	        //Stop other Heros effect
	        otherHero.SwitchToSyncPower();
	    }

	    //Find enemies to do effect on
		GameObject[] enemiesObjects = GameObject.FindGameObjectsWithTag("Enemy");

		enemies = new CharacterController[enemiesObjects.Length];
		int i = 0;
		foreach (GameObject enemy in enemiesObjects) {
			enemies[i] = enemy.GetComponent<CharacterController>();
			i++;
		}

		center = (sourceHero.transform.position + otherHero.transform.position) * 0.5f;

		StartCoroutine(PullEnemies());
		return null;
	}
	
	public override IEnumerator OnUpdateSync (Hero sourceHero, Hero otherHero)
	{
		return null;
	}

	IEnumerator PullEnemies() {
		//Effect
		pullSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		pullSphere.transform.position = center;
		pullSphere.transform.localScale = Vector3.one * (maxPullDistance * 2f);
		pullSphere.collider.enabled = false;
		pullSphere.renderer.material = new Material(Shader.Find("Transparent/Diffuse"));
		pullSphere.renderer.material.SetColor("_Color", new Color(0f, 1f, 1f, 0.5f));

		yield return new WaitForFixedUpdate();

		//Try to pull enemies over n ticks
		int ticks = 10;
		for (int i = 0; i < ticks; i++) {
			pullSphere.transform.localScale = Vector3.Lerp(Vector3.one * (maxPullDistance * 2f), Vector3.one, i/(float) ticks);
			foreach (CharacterController enemy in enemies) {
				var offset = center - enemy.transform.position;
				var sqrMagnitude = offset.sqrMagnitude;
				if (sqrMagnitude > maxPullDistanceSqr) {
					continue;
				}
				else if(sqrMagnitude > 1f) {
					offset = offset.normalized;
					enemy.Move(offset);
				}
			}
			yield return new WaitForFixedUpdate();
		}
		GameObject.Destroy(pullSphere);
		yield return null;
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
