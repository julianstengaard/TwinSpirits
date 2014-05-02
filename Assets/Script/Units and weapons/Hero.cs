using UnityEngine;
using System.Collections;
using InControl;
using RAIN.Entities;
using RAIN.Entities.Aspects;
using Holoville.HOTween;

public class Hero : BaseUnit {
	public enum Player {One, Two}
	public Player PlayerSlot;
	protected InputDevice _input;

	private float _damageRecievedModifier = 1f;

	public bool IsControlled {get; private set;}

	//public float TurnSpeed; // ONLY USED IF ROTATION IS SLERPED

	public float currentSpiritAmount;
	private float minSpiritAmount 	= 0f;
	private float maxSpiritAmount 	= 100f;

	[HideInInspector]
	public SpiritPower 	currentSpiritPower;
	[HideInInspector]
	public bool 		spiritActive 		= false; // Standard power press
	[HideInInspector]
	public bool 		spiritSyncActive 	= false; // Synces power press

	public Hero otherPlayer;
	private SpiritMeterUI ui;
	
	public float CollectRadius;

	private float spiritRegen = 0f;

	private RAINAspect aspect;

	private RandomSoundPlayer SoundController;

	[HideInInspector]
	public bool ExchangingSpiritPower = false;

	private float lastLookTimestamp = 0;

    [HideInInspector]
    public bool SpiritShieldActive = false;

	//Reviving
	private Camera _mainCamera;
	private bool _revivingOther = false;
	private float _reviveTime = 6f;
    private float _reviveTimeNoEnemies = 1f;
    private float _reviveTimeCurrent = 6f;
    private float _reviveTimer = 0f;
	private GameObject _reviveHeartPrefab;
	private GameObject _currentReviveHeart;
	private GameObject _currentReviveHeartOverlay;

	[HideInInspector]
	public Vector3 CurrentMoveVector = Vector3.zero;

	public bool DashEnabled = false;
	private bool _dashing = false;
	private float _dashDistance = 2.5f;
	private int _dashOverFrames = 10;
	private float _dashCooldown = 1f;
	private float _dashTimer = 0f;
	private TrailRenderer _dashTrail;

	// METHODS -----

	new void Start() {
		base.Start();

		CollectRadius += 1;

		// TESTING
		AddEffectToWeapons(new Damage(25));
		
		ui = GameObject.Find("UI").GetComponent<SpiritMeterUI>();
		if (DashEnabled) {
			_dashTrail = GetComponent<TrailRenderer> ();
		}
		_mainCamera = GameObject.FindGameObjectWithTag("MainCamera").camera;
		_reviveHeartPrefab = (GameObject) Resources.Load("ReviveHeart");

		//currentSpiritPower = gameObject.AddComponent<SpiritBungie>();
		currentSpiritPower = gameObject.AddComponent<SpiritBungie>();

		aspect = GetComponentInChildren<EntityRig>().Entity.GetAspect("twinhero");

		//Search for menu settings
		GameObject levelInfo = GameObject.Find("LevelCreationInfo");
		if (levelInfo != null) {
			spiritRegen = levelInfo.GetComponent<LevelCreationInfo>().spiritRegen;
			_damageRecievedModifier = levelInfo.GetComponent<LevelCreationInfo>().DamageRecievedModifier;
		}

		SoundController = GetComponent<RandomSoundPlayer>();
	}

	// Update is called once per frame
	new void Update () {
		base.Update();

		if (dead) {
			unitMaterial.SetColor("_Color", deadColor);
			return;
		}
		
		if (damageLocked) {
			damageLockTimer = (damageLockTimer + Time.deltaTime * 5f) % 1f;
			unitMaterial.SetColor("_Color", Color.Lerp(initColor, damageLockColor, damageLockTimer));
		}


		//Revive comrade if close!
		if (otherPlayer.dead) {
			if ((transform.position - otherPlayer.transform.position).magnitude < 2f)
			{
				//Try to revive
				RunRevive();
			} else {
				if (_revivingOther) {
					_revivingOther = false;
					GameObject.Destroy(_currentReviveHeart);
				}
			}
		}

		if(_input == null)
			return;

		// MOVEMENT
		var tPos = new Vector3(_input.LeftStickX, 0, _input.LeftStickY);
		var pos = tPos.sqrMagnitude > 1 ? tPos.normalized : tPos;
		CurrentMoveVector = pos * GetMoveSpeed();
		var dir = CurrentMoveVector * Time.deltaTime;

		_anim.SetBool("Attacking", _input.RightBumper);
		if(_input.RightBumper)
			MakeDangerous();
		else
			MakeInert();

		_anim.SetBool ("Moving", dir.sqrMagnitude > 0);
		_anim.SetFloat("MovementSpeed", pos.magnitude);

		Vector3 lookDirection = Vector3.zero; 

		// LOOK DIRECTION
		if (_input.Name == FPSProfile.ProfileName) {
			var playerPlane = new Plane(Vector3.up, transform.position);
			var ray = Camera.main.ScreenPointToRay (Input.mousePosition);
			var hitdist = 0.0f;
			// If the ray is parallel to the plane, Raycast will return false.
			if (playerPlane.Raycast(ray, out hitdist)) {
				// Get the point along the ray that hits the calculated distance.
				var targetPoint = ray.GetPoint(hitdist);
				
				// Determine the target rotation.  This is the rotation if the transform looks at the target point.
				var targetRotation = Quaternion.LookRotation(targetPoint - transform.position);
				lookDirection = targetPoint - transform.position;
				
				// Smoothly rotate towards the target point.
				//transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, TurnSpeed * Time.deltaTime); // WITH SPEED
				transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 1); // WITHOUT SPEED!!!
			}
		} else {
			var lookPoint = transform.position;
			if(_input.RightStickX != 0 ||  _input.RightStickY != 0) {
				lookPoint.x += _input.RightStickX;
				lookPoint.z += _input.RightStickY;
				lastLookTimestamp = Time.time;
			} else if(Time.time - lastLookTimestamp > 0.5f) {
				lookPoint.x += _input.LeftStickX;
				lookPoint.z += _input.LeftStickY;
			}
			transform.LookAt(lookPoint);
			lookDirection = transform.forward;

		}

		if (_dashTimer >= 0f)
			_dashTimer -= Time.deltaTime;

		if (!_dashing) {
			if (DashEnabled && _input.Action1.WasPressed && _dashTimer <= 0f) {
				if (lookDirection != Vector3.zero) {
					StartCoroutine (Dash (lookDirection, _dashDistance, _dashOverFrames));
				}
			} else {
				_cc.Move (dir);
			}
		}

		UpdateSpiritLink();
		UpdateCollection();

		//If auto spirit regen is on
		if (spiritRegen > 0f) {
			ChangeSpiritAmount(spiritRegen * Time.deltaTime);
		}
	}

	public IEnumerator Dash (Vector3 direction, float distance, float dashFrames) {
		_dashing = true;
		_dashTrail.enabled = true;
		_dashTrail.time = -0.01f;
		yield return new WaitForEndOfFrame();
		_dashTrail.time = 2f;
		for (int i = 0; i < 10; i++) {
			yield return new WaitForFixedUpdate();
			float move = EasedMove(i/(float)dashFrames);
			_cc.Move(direction.normalized * (move * (distance/10)));
		}
		_dashing = false;
		_dashTimer = _dashCooldown;
		_dashTrail.enabled = false;
		yield return null;
	}
	
	private float EasedMove(float pct) {
		if (pct < 0.5f)
			return (1f - Mathf.Pow(1f - pct, 3f));
		else 
			return -1f * (Mathf.Cos(Mathf.PI*pct) - 1f);
	}

	public override void TakeDamage(float damage, GameObject src, bool forceKill = false)
	{
	    bool blocked = false;
        if (SpiritShieldActive) {
	        Vector3 blockAngle = gameObject.transform.TransformDirection(Vector3.forward);
	        Vector3 incomingAngle = src.transform.position - gameObject.transform.position;
            if (Vector3.Angle(blockAngle, incomingAngle) < 80f) {
                SoundController.PlayRandomSound("SpiritShieldBlock");
                blocked = true;
            }
        } 
        
        if (immortal && !blocked) {
            SoundController.PlayRandomSound("ImmortalShieldBlock");
        }

	    if (!damageLocked && !blocked && !immortal && !dead)
		{
			Health = Mathf.Max(0, Health - (damage * _damageRecievedModifier));
            _anim.SetTrigger("Damaged");
			StartCoroutine(DamageLockTimeout(1f));
			
			if (Health <= 0f && !dead) {
				Died();
			}
			else {
				SoundController.PlayRandomSound("TakeDamage");
			}
		}

		if (forceKill) {
			Health = 0f;
			Died ();
		}
	}

    public override void Heal(float healAmount) {
        if (!dead) {
            Health = Mathf.Min(FullHealth, Health + healAmount);
            SoundController.PlayRandomSound("Heal");
        }
    }

	private IEnumerator DamageLockTimeout(float t)
	{
		damageLockTimer = 0f;
		damageLocked = true;
		yield return new WaitForSeconds(t);
		damageLocked = false;
		unitMaterial.SetColor("_Color", initColor);
	}
	
	protected override void Died () {
        if (dead) return;

		dead = true;
        SoundController.PlayRandomSound("Death");
		if (spiritActive) {
			DeactivateSpiritPower(false);
		}
		unitMaterial.SetColor("_Color", deadColor);
		aspect.IsActive = false;
		_anim.SetBool("Dead", true);
		_anim.SetBool("Attacking", false);
		MakeInert();
	}

	private void RunRevive () {
		if (!_revivingOther) {
            var enemies = GameObject.FindGameObjectsWithTag("Enemy");
		    _reviveTimeCurrent = (enemies.Length == 0) ? _reviveTimeNoEnemies : _reviveTime;

		    //Start the revive
			_reviveTimer = 0f;
			_revivingOther = true;
			Vector3 heartPosition = otherPlayer.transform.position + Vector3.up;
			_currentReviveHeart = (GameObject) GameObject.Instantiate(_reviveHeartPrefab, heartPosition, Quaternion.LookRotation(heartPosition - _mainCamera.transform.position));
			Transform[] _currentReviveHeartOverlays = _currentReviveHeart.GetComponentsInChildren<Transform>();
			foreach(var go in _currentReviveHeartOverlays) {
				if (!go.gameObject.Equals(_currentReviveHeart)) {
					_currentReviveHeartOverlay = go.gameObject;
					break;
				}
			}
			_currentReviveHeartOverlay.transform.localScale = new Vector3(1f, 0f, 1f);
			//Tween in the heart
			_currentReviveHeart.transform.localScale *= 0f;
			TweenParms shrineParms = new TweenParms().Prop(
				"localScale", new Vector3(1f, 1f, 1f)).Ease(
				EaseType.EaseOutBounce).Delay(0f);
			HOTween.To(_currentReviveHeart.transform, 0.7f, shrineParms);
		} else {
			_reviveTimer += Time.deltaTime;
			//Scale heart
            float pct = Mathf.Lerp(0f, 1f, _reviveTimer / _reviveTimeCurrent);
			Vector3 heartPosition = otherPlayer.transform.position + Vector3.up;
			_currentReviveHeart.transform.rotation = Quaternion.LookRotation(heartPosition - _mainCamera.transform.position);
			_currentReviveHeart.transform.position = heartPosition;
			_currentReviveHeartOverlay.transform.localPosition = Vector3.down * 0.5f + new Vector3(0f, pct/2f, 0f) + _currentReviveHeart.transform.TransformDirection(Vector3.back) * 0.03f;
			_currentReviveHeartOverlay.transform.localScale = new Vector3(1f, pct, 1f);
			_currentReviveHeartOverlay.renderer.material.mainTextureScale = new Vector2(1f, pct);
            if (_reviveTimer >= _reviveTimeCurrent) {
				DoRevive();
				_revivingOther = false;
				GameObject.Destroy(_currentReviveHeart);
			}
		}

	}

	private void DoRevive () {
        SoundController.PlayRandomSound("Revive");
		float transferedHealth = Mathf.Floor(Health/2f);
		otherPlayer.Revived(Mathf.Max(1f, transferedHealth));
		Health = Mathf.Max(1f, Health - transferedHealth);
	}

	public void Revived (float health) {
		dead = false;
		Health = Mathf.Min(health, FullHealth);
		unitMaterial.SetColor("_Color", initColor);
		aspect.IsActive = true;
		_anim.SetBool("Dead", false);
	}



	private void UpdateSpiritLink() {
		// SPIRIT STUFF
		if (_input.LeftBumper.WasPressed && !spiritActive && !spiritSyncActive)
		{
			ActivateSpiritPower();
		}
		else if (_input.LeftBumper.IsPressed && spiritActive && !spiritSyncActive)
		{
			UpdateSpiritPower();
		}
		else if (_input.LeftBumper.WasReleased)
		{
			if (spiritSyncActive) {
				spiritSyncActive 	= false;
				spiritActive 		= false;
			}
			if (spiritActive) {
				DeactivateSpiritPower(false);
			}
		}
	}

	private void UpdateCollection() {
		var hits = Physics.OverlapSphere(transform.position, CollectRadius, 1 << LayerMask.NameToLayer("Collectable"));

		foreach(var c in hits) {
			var coll = c.GetComponent<Collectable>();
			if(coll == null || !coll.IsCollectable(this))
				return;

			var corrected = transform.position;
			corrected.y += 0.5f;
			//var dir = corrected - c.transform.position;
			//rigidbody.AddForce(dir.normalized * 1000);
			if(Vector3.Distance(corrected, c.transform.position) < 1f) {
				c.SendMessage("Collected", this);
			} else {
				c.gameObject.transform.position = Vector3.Lerp(c.transform.position, corrected, 0.1f);
			}
		}
	}

	void ActivateSpiritPower()
	{
		spiritSyncActive = false;

		if(currentSpiritPower == null)
			return;
		
		//Sync spirit power
		if (currentSpiritAmount >= currentSpiritPower.GetCostActivateSync())
		{
			spiritSyncActive = currentSpiritPower.OnPotentialSync(this, otherPlayer);
		}
		
		//Regular spirit power
		if (currentSpiritAmount >= currentSpiritPower.GetCostActivate() && !spiritSyncActive)
		{
			currentSpiritPower.OnActivate(this, otherPlayer);
			spiritActive = true;
			ChangeSpiritAmount(-currentSpiritPower.GetCostActivate());
		}
	}
	
	void UpdateSpiritPower()
	{
		if (currentSpiritAmount > currentSpiritPower.GetCostThisUpdate())
		{
			currentSpiritPower.OnUpdate(this, otherPlayer);
			ChangeSpiritAmount(-currentSpiritPower.GetCostThisUpdate());
		}
		else
		{
			DeactivateSpiritPower(false);
		}
	}
	
	void DeactivateSpiritPower(bool onDestroy)
	{
		currentSpiritPower.OnDeactivate(this, otherPlayer, onDestroy);
		spiritActive = false;
	}
	
	public void SwitchToSyncPower()
	{
		DeactivateSpiritPower(true);
		currentSpiritAmount += currentSpiritPower.GetCostActivate();
        currentSpiritPower.OnActivateSync(this, otherPlayer, true);
        currentSpiritPower.syncActive = true;
		spiritSyncActive = true;
	}
	
	public void ChangeSpiritPower(SpiritPower newPower)
	{
		DeactivateSpiritPower(true);
		currentSpiritPower.OnDeactivateSync(this, otherPlayer, true);
		Destroy(currentSpiritPower);
		currentSpiritPower = newPower;
		ui.UpdateSpiritPowerIcons();
	}
	
	private void DropSpiritPower()
	{
		var res = Resources.Load("Collect"+currentSpiritPower.GetType().ToString());
		if(res == null) {
			Debug.Log ("Unable to load " + currentSpiritPower.GetType());
			return;
		}

		GameObject.Instantiate(res, transform.position + new Vector3(0, 2, 0), Quaternion.identity);

//		if (currentSpiritPower.GetType() == typeof(SpiritBungie)) {
//			GameObject.Instantiate(Resources.Load("CollectSpiritBungie"), transform.position + new Vector3(0, 2, 0), Quaternion.identity);
//		}
//		if (currentSpiritPower.GetType() == typeof(SpiritImmortal)) {
//			GameObject.Instantiate(Resources.Load("CollectSpiritImmortal"), transform.position + new Vector3(0, 2, 0), Quaternion.identity);
//		}
//		if (currentSpiritPower.GetType() == typeof(SpiritRegenHP)) {
//			GameObject.Instantiate(Resources.Load("CollectSpiritRegenHP"), transform.position + new Vector3(0, 2, 0), Quaternion.identity);
//		}
//		if (currentSpiritPower.GetType() == typeof(SpiritSpeedBoost)) {
//			GameObject.Instantiate(Resources.Load("CollectSpiritSpeedBoost"), transform.position + new Vector3(0, 2, 0), Quaternion.identity);
//		}
//		if (currentSpiritPower.GetType() == typeof(SpiritLightning)) {
//			GameObject.Instantiate(Resources.Load("CollectSpiritLightning"), transform.position + new Vector3(0, 2, 0), Quaternion.identity);
//		}
	}
	
	public void ChangeSpiritAmount(float spiritAmount)
	{
		this.currentSpiritAmount = Mathf.Clamp(this.currentSpiritAmount + spiritAmount, minSpiritAmount, maxSpiritAmount);
	}

	public float GetMoveSpeed()	{
		if (!stunned)
			return Mathf.Clamp(MovementSpeed + movementSpeedBuff, minMovementSpeed, maxMovementSpeed);
		else
			return 0f;
	}
	
	public void AttachInputDevice(InputDevice device) {
		_input = device;
		IsControlled = true;
	}

	public override void SetMovementSpeedBuff (float movementSpeedBuff)	{
		throw new System.NotImplementedException ();
	}

	public void StepSound() {
		SoundController.PlayRandomSound("Footstep");
	}
	public void SwordSound() {
		SoundController.PlayRandomSound("SwordSwing");
	}

    public void UseGravity(bool b) {
        usesGravity = b;
    }

	public void SetDamageRecievedModifier(float modifier) {
		_damageRecievedModifier = modifier;
	}
}
