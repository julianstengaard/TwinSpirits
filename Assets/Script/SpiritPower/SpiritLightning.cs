using UnityEngine;
using System.Collections;

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
	private float _lightningSphereColliderRadius = 2f;
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
		//Vector3 moveDirection = otherHero.CurrentMoveVector;
		Vector3 moveDirection = Vector3.zero;
		ball.ActivateBall(otherHero.transform.position + throwDirection + moveDirection, _ballExplosionRadius, _damagePerBall, _ballTravelTime);

		return null;
	}

	public override IEnumerator OnUpdate (Hero sourceHero, Hero otherHero)
	{
		return null;
	}

	public override IEnumerator OnDeactivate (Hero sourceHero, Hero otherHero, bool onDestroy)
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
		if (_currentLightning != null) {
	        GameObject.Destroy(_currentLightning);
	    }
        
		if (!secondSync) {
			//Stop other Heros effect
			otherHero.SwitchToSyncPower();
			
			//Pay for activation
			sourceHero.ChangeSpiritAmount(-costActivateSync);
			otherHero.ChangeSpiritAmount(-costActivateSync);
		}
		LightningSync(sourceHero, otherHero);

		return null;
	}

	private void LightningSync(Hero sourceHero, Hero otherHero) {
        Vector3 center = (sourceHero.transform.position + otherHero.transform.position) * 0.5f;
        _currentLightning = (GameObject)GameObject.Instantiate(_lightningPrefab, center, Quaternion.identity);

	    _currentLightning.GetComponent<LightningForSpiritLightning>()
	        .Activate(sourceHero, otherHero, _lightningDuration, _lightningDamageInterval, _mainCamera,
	            _lightningSphereColliderRadius, _damagePerLightningInterval, _particleEffectPrefab);
	}

	public override IEnumerator OnUpdateSync (Hero sourceHero, Hero otherHero) {
		return null;
	}

    public override IEnumerator OnDeactivateSync(Hero sourceHero, Hero otherHero, bool onDestroy = false)
	{
		//Debug.Log("Deactivating" + this.GetType() + " SYNC POWER!");
		syncActive = false;
		if (_currentLightning != null) {
			GameObject.Destroy(_currentLightning);
		}
        return null;
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
