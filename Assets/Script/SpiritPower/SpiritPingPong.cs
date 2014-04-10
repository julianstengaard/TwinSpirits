using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Holoville.HOTween;

public class SpiritPingPong : SpiritPower 
{
	private GameObject _ball;
	private Color _ballColor = new Color(1f, 1f, 1f, 0.8f);
	private Color _ballColorNoBounce = new Color(0.2f, 0.2f, 0.2f, 0.8f);

    private bool _ballReady = true;
    private bool _ballActive = false;
	private float _ballRadius = 0.2f;
	private float _syncBallRadius = 0.3f;
	//private float _ballRadiusSqr;
	private float _syncBallRadiusSqr;
	private Vector3 _currentOriginPosition;
	private Vector3 _currentTargetPosition;
	private Hero _origin;
	private Hero _receiver;

	private List<BaseUnit> _enemiesHit = new List<BaseUnit>();
	private List<BaseUnit> _enemiesHitPrevious = new List<BaseUnit>();
	private GameObject _particleEffectPrefab;

	private float _speedCurrent;
	private float _speedInit = 5f;
	private float _accelerationPerBounce = 1.1f; //pct
	private float _catchRadiusSqr = 0.6f;
	private float _syncSuctionThresholdSqr = 9f;
	private float _syncSuctionAmount = 2f;
    private float _ballLifeTime = 10f;
    private float _ballLifeTimeCounter = 0f;
	private float _syncBallLifeTime = 10f;
	private float _syncBallLifeTimeCounter = 0f;

	private int _currentBounces;
	private int _maxBounces = 10;
	private int _maxBouncesSync = 20;

	private float _damagePerHitBase = 10f;
	private float _damagePerHitCurrent;
	private float _damageIncreasePerBounce = 1.1f; //pct
	private float _damagePerHitBaseSync = 20f;
	private float _damagePerHitCurrentSync;
	private float _damageIncreasePerBounceSync = 1.1f; //pct
	
	void Start() {
		costActivate 		=  10f;
		costPerSecond 		=  0f;
		costActivateSync 	= 100f;
		_particleEffectPrefab = (GameObject) Resources.Load("SpiritPingPongParticle", typeof(GameObject));
	}
	
	/* BEGIN REGULAR POWER */
	public override IEnumerator OnActivate (Hero sourceHero, Hero otherHero)
	{
        if (!_ballReady)
            return null;

		if (syncActive)
			syncActive = false;
        
        _ballActive = true;
        _ballReady = false;
        _ballLifeTimeCounter = 0f;

		DestroyBall();
		_ball = CreateBallSphere(_ballColor, otherHero.transform, _ballRadius);
		_currentBounces = 0;
		_damagePerHitCurrent = _damagePerHitBase;
		_damagePerHitCurrentSync = _damagePerHitBaseSync;

		_origin = otherHero;
		_receiver = sourceHero;

		_currentOriginPosition = _origin.transform.position + Vector3.up;
		_currentTargetPosition = _receiver.transform.position + Vector3.up;
		_speedCurrent = _speedInit;

		return null;
	}
	public GameObject CreateBallSphere(Color color, Transform trans, float radius) {
		var ballMesh = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		ballMesh.transform.position = trans.position + Vector3.up;
		ballMesh.transform.localScale = Vector3.one * radius * 2f;
		ballMesh.collider.enabled = false;
		ballMesh.renderer.material = new Material(Shader.Find("Transparent/Diffuse"));
		ballMesh.renderer.material.SetColor("_Color", color);
		return ballMesh;
	}
	public override IEnumerator OnUpdate (Hero sourceHero, Hero otherHero)
	{
		
		return null;
	}

	private void BounceBall() {
		//Switch origin and reciever
		Hero tempHero = _receiver;
		_receiver = _origin;
		_origin = tempHero;
		
		//Find the new positions
		_currentOriginPosition = _ball.transform.position;
		_currentTargetPosition = _receiver.transform.position + Vector3.up; 
	}

	private void UpdateBall(int maxBounces, bool sync) {
		bool bounced = false;
		Vector3 previousPosition = _ball.transform.position;

		if (sync && ! (_currentBounces >= _maxBouncesSync)) {
			//Is the ball close enough to the target for turning the ball?
			if (_syncSuctionThresholdSqr > (previousPosition - (_receiver.transform.position + Vector3.up)).sqrMagnitude) {
				//Turn the target from the the origin to make its path overlap the player
				Vector3 targetDirection = _currentTargetPosition - _currentOriginPosition;
				Vector3 wantedTargetDirection = ((_receiver.transform.position + Vector3.up) - _currentOriginPosition).normalized * targetDirection.magnitude;
				_currentTargetPosition = _currentOriginPosition + Vector3.RotateTowards(targetDirection, wantedTargetDirection, _syncSuctionAmount * Time.deltaTime, 0.0F);

				Vector3 wantedOriginDirection = (previousPosition - _currentTargetPosition).normalized * targetDirection.magnitude;
				_currentOriginPosition = _currentTargetPosition + Vector3.RotateTowards(-targetDirection, wantedOriginDirection, _syncSuctionAmount * Time.deltaTime, 0.0F);
			}
		}
		_ball.transform.position = GetBallMovement(_currentOriginPosition, _ball.transform.position, _currentTargetPosition, sync);
		CheckForBallCollision(previousPosition, _ball.transform.position, sync);

		//No more bounce for you
		if ((sync && _currentBounces >= _maxBouncesSync) || (!sync && _currentBounces >= _maxBounces)) {
			_ball.renderer.material.SetColor("_Color", _ballColorNoBounce);
			return;
		}
		
		if ((_ball.transform.position - (_receiver.transform.position + Vector3.up)).sqrMagnitude < _catchRadiusSqr) {
			BounceBall();
			bounced = true;
		}
		
		if (bounced) {
			_ballLifeTimeCounter = 0f;
			_syncBallLifeTimeCounter = 0f;
			_currentBounces++;
			_speedCurrent *= _accelerationPerBounce;
			_damagePerHitCurrent *= _damageIncreasePerBounce;
			_damagePerHitCurrentSync *= _damageIncreasePerBounceSync;
		}
	}

	public Vector3 GetBallMovement(Vector3 origin, Vector3 current, Vector3 target, bool sync) {
		float dPath = Vector3.Distance (origin, target);
		float dCurrent = Vector3.Distance (origin, current);
		float pct = dCurrent/dPath;
		Vector3 baseMoveSpeed = (target-origin).normalized * _speedCurrent * Time.deltaTime;
		Vector3 move = Vector3.Lerp(baseMoveSpeed*2f, baseMoveSpeed, pct);
		return current + move;
	}

	private void CheckForBallCollision (Vector3 from, Vector3 to, bool sync) {
		//Copy the previous list
		_enemiesHitPrevious = new List<BaseUnit>(_enemiesHit);
		//then clear it
		_enemiesHit.Clear();
		RaycastHit[] hits  = Physics.SphereCastAll(from, _ballRadius, to-from, (to-from).magnitude, 1 << 8);
		foreach(RaycastHit hit in hits) {
			var enemy = hit.collider.gameObject.GetComponent<BaseUnit>();
			if (!_enemiesHitPrevious.Contains(enemy)) {
				if (sync) {
					enemy.TakeDamage(_damagePerHitCurrentSync);
				} else {
					enemy.TakeDamage(_damagePerHitCurrent);
				}
				StartCoroutine(CreateBallDamageParticle(enemy.gameObject));
				_enemiesHit.Add(enemy);
			}
		}
	}

	private IEnumerator CreateBallDamageParticle(GameObject target) {
		GameObject _particleEffect = (GameObject) Instantiate(_particleEffectPrefab, target.transform.position, Quaternion.identity);
		GameObject.Destroy(_particleEffect, 1f);
		yield return null;
	}
	


	public override IEnumerator OnDeactivate (Hero sourceHero, Hero otherHero)
	{
		//Debug.Log("Deactivating" + this.GetType());
        _ballReady = true;
		
		return null;
	}
	/* END REGULAR POWER */

	public void DestroyBall() {
		if (_ball != null)
			GameObject.Destroy(_ball);
	}
	
	
	
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
		DestroyBall();
		_syncBallLifeTimeCounter = 0f;
		if (!secondSync)
		{
			//Pay for activation
			sourceHero.ChangeSpiritAmount(-costActivateSync);
			otherHero.ChangeSpiritAmount(-costActivateSync);
			
			//Stop other Heros effect
			otherHero.SwitchToSyncPower();
		}

		_ball = CreateBallSphere(_ballColor, otherHero.transform, _syncBallRadius);
		_currentBounces = 0;
		
		_origin = otherHero;
		_receiver = sourceHero;
		
		_currentOriginPosition = _origin.transform.position + Vector3.up;
		_currentTargetPosition = _receiver.transform.position + Vector3.up;
		
		_speedCurrent = _speedInit;
		
		return null;
	}

	public void Update() {
		if (syncActive) {
			if (_syncBallLifeTime < _syncBallLifeTimeCounter) {
				syncActive = false;
				DestroyBall();
			} else {
				UpdateBall(_maxBouncesSync, true);
				_syncBallLifeTimeCounter += Time.deltaTime;
			}
		} else if (_ballActive) {
            if (_ballLifeTime < _ballLifeTimeCounter) {
                _ballActive = false;
                DestroyBall();
            }
            else {
                UpdateBall(_maxBounces, false);
                _ballLifeTimeCounter += Time.deltaTime;
            }
            
		}
	}
	
	private IEnumerator CreateSyncExplosion(Vector3 center) {
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
