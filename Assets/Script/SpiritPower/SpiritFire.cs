using UnityEngine;
using System.Collections;
using Holoville.HOTween;

public class SpiritFire : SpiritPower 
{
	private GameObject _burnSphere;
	private float _burnSphereRadius = 5f;
	private float _burnSphereRadiusSqr;
	private GameObject _syncSphere;
	private float _syncSphereRadius = 20f;
	private float _syncSphereRadiusSqr;

	private GameObject[] _enemiesGO;
	private BaseEnemy[] _enemies;

	private float _damagePerSecond = 20f;

	private float _syncDuration = 5f;
	private float _syncDamageInterval = 1f;
	private float _syncDamagePerInterval = 15f;
	
	void Start() {
		costActivate 		=  10f;
		costPerSecond 		=  10f;
		costActivateSync 	= 50f;
		_burnSphereRadiusSqr = _burnSphereRadius * _burnSphereRadius;
		_syncSphereRadiusSqr = _syncSphereRadius * _syncSphereRadius;
	}
	
	/* BEGIN REGULAR POWER */
	public override IEnumerator OnActivate (Hero sourceHero, Hero otherHero)
	{
		//Debug.Log("Activating" + this.GetType());
		_burnSphere = CreateBurnSphere(new Color(1f, 0f, 0f, 0.5f), otherHero.transform);

		_enemiesGO = GameObject.FindGameObjectsWithTag("Enemy");
		_enemies = new BaseEnemy[_enemiesGO.Length];
		for (int i = 0; i < _enemies.Length; i++) {
			_enemies[i] = _enemiesGO[i].GetComponent<BaseEnemy>();
		}

		return null;
	}
	public GameObject CreateBurnSphere(Color color, Transform trans) {
		var burnMesh = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		burnMesh.transform.position = trans.position + Vector3.up;
		burnMesh.transform.localScale = Vector3.one * _burnSphereRadius * 2f;
		burnMesh.collider.enabled = false;
		burnMesh.renderer.material = new Material(Shader.Find("Transparent/Diffuse"));
		burnMesh.renderer.material.SetColor("_Color", color);
		return burnMesh;
	}
	public override IEnumerator OnUpdate (Hero sourceHero, Hero otherHero)
	{
		_burnSphere.transform.position = otherHero.transform.position + Vector3.up;

		//Damage enemies within burn radius
		foreach (var enemy in _enemies) {
			var distanceSqr = Vector3.SqrMagnitude(enemy.transform.position - otherHero.transform.position);
			if (distanceSqr < _burnSphereRadiusSqr) {
				enemy.TakeDamage(_damagePerSecond * Time.deltaTime, gameObject);
			}
		}
		return null;
	}
	public override IEnumerator OnDeactivate (Hero sourceHero, Hero otherHero, bool onDestroy)
	{
		//Debug.Log("Deactivating" + this.GetType());
		if (_burnSphere != null) {
			GameObject.Destroy(_burnSphere);
		}
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
		
		if (!secondSync) {
			//Stop other Heros effect
			otherHero.SwitchToSyncPower();
			
			//Pay for activation
			sourceHero.ChangeSpiritAmount(-costActivateSync);
			otherHero.ChangeSpiritAmount(-costActivateSync);
		}
		
		//Find enemies to do effect on
		_enemiesGO = GameObject.FindGameObjectsWithTag("Enemy");
		var center = (sourceHero.transform.position + otherHero.transform.position) * 0.5f;

		StartCoroutine(CreateSyncExplosion(center));

		return null;
	}

	private IEnumerator CreateSyncExplosion(Vector3 center) {
		if (_syncSphere != null) {
			GameObject.Destroy(_syncSphere);
		}
		//Create "explosion"
		_syncSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		_syncSphere.transform.position = center + Vector3.up;
		_syncSphere.collider.enabled = false;
		_syncSphere.renderer.material = new Material(Shader.Find("Transparent/Diffuse"));
		_syncSphere.renderer.material.SetColor("_Color", new Color(1f, 0f, 0f, 0.5f));

		//Expand it over time
		TweenParms explosionParms = new TweenParms().Prop(
			"localScale", Vector3.one * _syncSphereRadius * 2f).Ease(
			EaseType.EaseInExpo).Delay(0f);
		HOTween.To(_syncSphere.transform, 1f, explosionParms);

		//Wait for animation
		yield return new WaitForSeconds(1f);
		//Attach DOT to all enemies
		foreach (var enemy in _enemiesGO) {
			var distanceSqr = Vector3.SqrMagnitude(enemy.transform.position - center);
			if (distanceSqr < _syncSphereRadiusSqr) {
				FireDOT fireDOT = enemy.AddComponent<FireDOT>();
				fireDOT.InitDOT(_syncDuration, _syncDamageInterval, _syncDamagePerInterval);
			}
		}
		GameObject.Destroy(_syncSphere);
	}
	
	public override IEnumerator OnUpdateSync (Hero sourceHero, Hero otherHero)
	{
		return null;
	}

	public override IEnumerator OnDeactivateSync (Hero sourceHero, Hero otherHero)
	{
		//Debug.Log("Deactivating" + this.GetType() + " SYNC POWER!");
		if (_syncSphere != null) {
			GameObject.Destroy(_syncSphere);
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
