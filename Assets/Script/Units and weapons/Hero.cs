using UnityEngine;
using System.Collections;
using System.Linq;
using InControl;
using RAIN.Entities;
using RAIN.Entities.Aspects;
using Holoville.HOTween;

public class Hero : BaseUnit {
	public enum Player {One, Two}
	public Player PlayerSlot;
	protected InputDevice _input;

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
	private float _reviveTimer = 0f;
	private GameObject _reviveHeartPrefab;
	private GameObject _currentReviveHeart;
	private GameObject _currentReviveHeartOverlay;

	[HideInInspector]
	public Vector3 CurrentMoveVector = Vector3.zero;

	// METHODS -----

	new void Start() {
		base.Start();

		// TESTING
		var weapons = GetComponentsInChildren<Weapon>();
		AddEffectToWeapons(new Damage(25));
		
		ui = GameObject.Find("UI").GetComponent<SpiritMeterUI>();

		_mainCamera = GameObject.FindGameObjectWithTag("MainCamera").camera;
		_reviveHeartPrefab = (GameObject) Resources.Load("ReviveHeart");

		currentSpiritPower = gameObject.AddComponent<SpiritImmortal>();

		aspect = GetComponentInChildren<EntityRig>().Entity.GetAspect("twinhero");

		//Search for menu settings
		GameObject levelInfo = GameObject.Find("LevelCreationInfo");
		if (levelInfo != null) {
			spiritRegen = levelInfo.GetComponent<LevelCreationInfo>().spiritRegen;
		}

		SoundController = GetComponent<RandomSoundPlayer>();

		print(currentSpiritPower);
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


		//Revive comrade if close! TODO 3 second revive
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

		_cc.Move(dir);

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
		}

		UpdateSpiritLink();
		UpdateCollection();

		//If auto spirit regen is on
		if (spiritRegen > 0f) {
			ChangeSpiritAmount(spiritRegen * Time.deltaTime);
		}
	}

	public override void TakeDamage(float damage, GameObject src)
	{
	    bool blocked = false;
        if (SpiritShieldActive) {
	        Vector3 blockAngle = gameObject.transform.TransformDirection(Vector3.forward);
	        Vector3 incomingAngle = src.transform.position - gameObject.transform.position;
            if (Vector3.Angle(blockAngle, incomingAngle) < 80f) {
                blocked = true;
            }
        }
        if (!damageLocked && !blocked)
		{
			Health = Mathf.Max(0, Health - damage);
			_anim.SetTrigger("Damaged");
			StartCoroutine(DamageLockTimeout(1f));
			
			if (Health <= 0 && !dead)
				Died();
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
		dead = true;
		if (spiritActive) {
			DeactivateSpiritPower();
		}
		unitMaterial.SetColor("_Color", deadColor);
		aspect.IsActive = false;
		_anim.SetBool("Dead", true);
		_anim.SetBool("Attacking", false);
		MakeInert();
	}

	private void RunRevive () {
		if (!_revivingOther) {
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
			float pct = Mathf.Lerp(0f, 1f, _reviveTimer/_reviveTime);
			Vector3 heartPosition = otherPlayer.transform.position + Vector3.up;
			_currentReviveHeart.transform.rotation = Quaternion.LookRotation(heartPosition - _mainCamera.transform.position);
			_currentReviveHeart.transform.position = heartPosition;
			_currentReviveHeartOverlay.transform.localPosition = Vector3.down * 0.5f + new Vector3(0f, pct/2f, 0f) + _currentReviveHeart.transform.TransformDirection(Vector3.back) * 0.03f;
			_currentReviveHeartOverlay.transform.localScale = new Vector3(1f, pct, 1f);
			_currentReviveHeartOverlay.renderer.material.mainTextureScale = new Vector2(1f, pct); 
			if (_reviveTimer >= _reviveTime) {
				DoRevive();
				_revivingOther = false;
				GameObject.Destroy(_currentReviveHeart);
			}
		}

	}

	private void DoRevive () {
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
				DeactivateSpiritPower();
			}
		}
	}

	private void UpdateCollection() {
		var hits = Physics.OverlapSphere(transform.position, CollectRadius, 1 << LayerMask.NameToLayer("Collectable"));

		foreach(var c in hits) {
			var corrected = transform.position;
			corrected.y += 0.5f;
			var dir = corrected - c.transform.position;
			//rigidbody.AddForce(dir.normalized * 1000);
			if(Vector3.Distance(corrected, c.transform.position) < 1f) {
				c.SendMessage("Collected", this);
				GameObject.Destroy(c.gameObject);
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
		if (currentSpiritAmount > currentSpiritPower.GetCostActivate() && !spiritSyncActive)
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
			DeactivateSpiritPower();
		}
	}
	
	void DeactivateSpiritPower()
	{
		currentSpiritPower.OnDeactivate(this, otherPlayer);
		spiritActive = false;
	}
	
	public void SwitchToSyncPower()
	{
		DeactivateSpiritPower();
        currentSpiritPower.OnActivateSync(this, otherPlayer, true);
        currentSpiritPower.syncActive = true;
		spiritSyncActive = true;
	}
	
	public void ChangeSpiritPower(SpiritPower newPower)
	{
		DeactivateSpiritPower();
		currentSpiritPower.OnDeactivateSync(this, otherPlayer);
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
}
