using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Holoville.HOTween;

public class SpiritPingPong : SpiritPower 
{
	private GameObject _ball;
    private GameObject _pingPongPrefab;
    /*
	private Color _ballColor = new Color(1f, 1f, 1f, 0.8f);
	private Color _ballColorNoBounce = new Color(0.2f, 0.2f, 0.2f, 0.8f);

    private bool _ballReady = true;
    private bool _ballActive = false;
	private float _ballRadius = 0.2f;
	private float _syncBallRadius = 0.3f;
	private Vector3 _currentOriginPosition;
	private Vector3 _currentTargetPosition;
	private Hero _origin;
	private Hero _receiver;

	private List<BaseUnit> _enemiesHit = new List<BaseUnit>();
	private List<BaseUnit> _enemiesHitPrevious = new List<BaseUnit>();
	//private GameObject _particleEffectPrefab;

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
     */
	
	void Start() {
		costActivate 		=  10f;
		costPerSecond 		=  0f;
		costActivateSync 	= 50f;
        _pingPongPrefab = (GameObject)Resources.Load("SpiritPingPongBall", typeof(GameObject));
		//_particleEffectPrefab = (GameObject) Resources.Load("SpiritPingPongParticle", typeof(GameObject));
	}
	
	/* BEGIN REGULAR POWER */
	public override IEnumerator OnActivate (Hero sourceHero, Hero otherHero) {
        _ball = InstantiateBall(otherHero, sourceHero, false);
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

    public GameObject InstantiateBall(Hero source, Hero target, bool sync) {
        var ball = (GameObject)GameObject.Instantiate(_pingPongPrefab, source.transform.position + Vector3.up, Quaternion.identity);
        ball.GetComponent<BallForSpiritPingPong>().Activate(source, target, sync);
        return ball;
    }

	public override IEnumerator OnUpdate (Hero sourceHero, Hero otherHero) {
		return null;
	}
    
    public override IEnumerator OnDeactivate (Hero sourceHero, Hero otherHero, bool onDestroy)
	{
		//Debug.Log("Deactivating" + this.GetType());
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
		if (Mathf.Abs(potentialSyncTime - otherHero.currentSpiritPower.potentialSyncTime) < timeWindowForSync) {
			syncActive = true;
			OnActivateSync(sourceHero, otherHero);
			return true;
		}
		else {
			return false;
		}
		
	}
	public override IEnumerator OnActivateSync (Hero sourceHero, Hero otherHero, bool secondSync = false)
	{
        DestroyBall();

		if (!secondSync) {
			//Stop other Heros effect
			otherHero.SwitchToSyncPower();
			
			//Pay for activation
			sourceHero.ChangeSpiritAmount(-costActivateSync);
			otherHero.ChangeSpiritAmount(-costActivateSync);
		}
        InstantiateBall(otherHero, sourceHero, true);
		return null;
	}
	
	public override IEnumerator OnUpdateSync (Hero sourceHero, Hero otherHero) {
		return null;
	}

    public override IEnumerator OnDeactivateSync(Hero sourceHero, Hero otherHero, bool onDestroy = false) {
        //Debug.Log("Deactivating" + this.GetType() + " SYNC POWER!");
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
