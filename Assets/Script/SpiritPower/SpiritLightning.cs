using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class SpiritLightning : SpiritPower 
{
	private GameObject _lightningBallPrefab;
	private GameObject _lightningPrefab;
	private GameObject _currentLightning;
	private GameObject[] _enemiesGameObjects;
	private BaseUnit[] _enemies;

	private float _damagePerBall = 50f;
	private float _ballBaseRange = 5f;
	private float _ballTravelTime = 1f;
	private float _ballExplosionRadius = 2f;

	private float _damagePerLightningInterval = 20f;
	private float _lightningDuration = 10f;
	private float _lightningSphereColliderRadius = 1f;
	private float _lightningTimer = 0f;
	private float _lightningDamageIntervalTimer = 0f;
	private float _lightningDamageInterval = 0.5f;
	private GameObject _particleEffectPrefab;

	private Camera _mainCamera;

	void Start() {
		costActivate 		=  10f;
		costPerSecond 		=  0f;
		costActivateSync 	=  50f;
		_lightningBallPrefab = (GameObject) Resources.Load("SpiritLightningBall");
		_lightningPrefab = (GameObject) Resources.Load("Lightning");
		_mainCamera = GameObject.FindGameObjectWithTag("MainCamera").camera;
		_particleEffectPrefab = (GameObject) Resources.Load("SpiritLightningParticle", typeof(GameObject));
	}
	
	/* BEGIN REGULAR POWER */
	public override IEnumerator OnActivate (Hero sourceHero, Hero otherHero)
	{
		//Create a ball
		GameObject ballGO = (GameObject) GameObject.Instantiate(_lightningBallPrefab, otherHero.transform.position, Quaternion.identity);
		BallForSpiritLightning ball = ballGO.GetComponent<BallForSpiritLightning>();
		Vector3 throwDirection = otherHero.transform.TransformDirection(Vector3.forward) * _ballBaseRange;
		Vector3 moveDirection = otherHero.CurrentMoveVector;
		ball.ActivateBall(otherHero.transform.position + throwDirection + moveDirection, _ballExplosionRadius, _damagePerBall, _ballTravelTime);

		return null;
	}

	public override IEnumerator OnUpdate (Hero sourceHero, Hero otherHero)
	{
		return null;
	}

	public override IEnumerator OnDeactivate (Hero sourceHero, Hero otherHero)
	{
		//Debug.Log("Deactivating" + this.GetType());
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
		StartCoroutine(LightningSync(sourceHero, otherHero));

		return null;
	}

	private IEnumerator LightningSync(Hero sourceHero, Hero otherHero) {
		_lightningTimer = _lightningDuration + Time.time;
		_lightningDamageIntervalTimer = _lightningDamageInterval;

		Vector3 center = (sourceHero.transform.position + otherHero.transform.position) * 0.5f;
		_currentLightning = (GameObject) GameObject.Instantiate(_lightningPrefab, center, Quaternion.identity);

		while (true) {
			if (!syncActive) {
				break;
			}
			yield return new WaitForEndOfFrame();
			_lightningDamageIntervalTimer += Time.deltaTime;
			if (Time.time < _lightningTimer) {
				center = (sourceHero.transform.position + otherHero.transform.position) * 0.5f;
				_currentLightning.transform.position = center + Vector3.up * _currentLightning.transform.localScale.y * 0.45f;
				Vector3 rotationTarget = _mainCamera.transform.position - new Vector3(1f, _mainCamera.transform.position.y, 1f) + Vector3.up * _currentLightning.transform.position.y;
				_currentLightning.transform.rotation = Quaternion.LookRotation(_currentLightning.transform.position - rotationTarget);
				if (_lightningDamageIntervalTimer >= _lightningDamageInterval) {
					_lightningDamageIntervalTimer = 0f;
					DoLightningDamage(center);
				}
			} else {
				break;
			}
		}
		GameObject.Destroy(_currentLightning);
		yield return null;
	}

	private void DoLightningDamage(Vector3 position) {
		Collider[] hits = Physics.OverlapSphere(position, _lightningSphereColliderRadius, 1 << 8);
		foreach (var other in hits) {
			if (other.tag == "Enemy") {
				other.gameObject.GetComponent<BaseUnit>().TakeDamage(_damagePerLightningInterval, gameObject);
				CreateLightningDamageParticle(other.gameObject);
			}
		}
	}

	private IEnumerator CreateLightningDamageParticle(GameObject target) {
		GameObject _particleEffect = (GameObject) Instantiate(_particleEffectPrefab, target.transform.position, Quaternion.identity);
		GameObject.Destroy(_particleEffect, 1f);
		yield return null;
	}

	public override IEnumerator OnUpdateSync (Hero sourceHero, Hero otherHero)
	{
		return null;
	}
	
	public override IEnumerator OnDeactivateSync (Hero sourceHero, Hero otherHero)
	{
		//Debug.Log("Deactivating" + this.GetType() + " SYNC POWER!");
		syncActive = false;
		if (_currentLightning != null) {
			GameObject.Destroy(_currentLightning);
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
