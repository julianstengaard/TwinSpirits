using UnityEngine;
using System.Collections;
using System.Linq;
using InControl;
using RAIN.Entities;
using RAIN.Entities.Aspects;

public class Hero : BaseUnit {
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

	// METHODS -----

	new void Start() {
		base.Start();

		// TESTING
		var weapons = GetComponentsInChildren<Weapon>();
		var eff =new Damage(15);
		weapons[0].AddEffect(eff);
		weapons[1].AddEffect(eff);
		
		ui = GameObject.Find("UI").GetComponent<SpiritMeterUI>();
		currentSpiritPower = gameObject.AddComponent<SpiritFire>();

		aspect = GetComponentInChildren<EntityRig>().Entity.GetAspect("twinhero");

		//Search for menu settings
		GameObject levelInfo = GameObject.Find("LevelCreationInfo");
		if (levelInfo != null) {
			spiritRegen = levelInfo.GetComponent<LevelCreationInfo>().spiritRegen;
		}

		SoundController = GetComponent<RandomSoundPlayer>();
	}

	// Update is called once per frame
	new void Update () {
		base.Update();

		if (dead)
			return;

		//Revive comrade if close!
		if (otherPlayer.dead && Health >= 2f) {
			if ((transform.position - otherPlayer.transform.position).magnitude < 2f)
			{
			    float transferedHealth = Mathf.Floor(Health/2f);
                otherPlayer.Revived(transferedHealth);
                Health -= transferedHealth;
			}
		}

		if(_input == null)
			return;

		// MOVEMENT
		var pos = new Vector3(_input.LeftStickX, 0, _input.LeftStickY);
		var dir = pos * GetMoveSpeed() * Time.deltaTime;

		_anim.SetBool("Attacking", _input.RightBumper);
		if(_input.RightBumper)
			MakeDangerous();
		else
			MakeInert();
		_anim.SetBool ("Moving", dir.magnitude > 0);

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
			lookPoint.x += _input.RightStickX;
			lookPoint.z += _input.RightStickY;

			transform.LookAt(lookPoint);
		}

		UpdateSpiritLink();
		UpdateCollection();

		//If auto spirit regen is on
		if (spiritRegen > 0f) {
			ChangeSpiritAmount(spiritRegen * Time.deltaTime);
		}
	}

    public override void TakeDamage(float damage)
    {
        if (!immortal && !damageLocked)
        {
            Health = Mathf.Max(0, Health - damage);
            _anim.SetTrigger("Damaged");

            damageLocked = true;
            StartCoroutine(DamageLockTimeout(1f));

            if (Health <= 0 && !dead)
                Died();
        }
    }

    private IEnumerator DamageLockTimeout(float t)
    {
        yield return new WaitForSeconds(t);
        damageLocked = false;
    }

	protected override void Died () {
		dead = true;
		GetComponentInChildren<SkinnedMeshRenderer>().material.SetColor("_Color", new Color(1f, 0, 0));
		aspect.IsActive = false;
	}

	public void Revived (float health) {
		dead = false;
		Health = Mathf.Min(health, FullHealth);
		GetComponentInChildren<SkinnedMeshRenderer>().material.SetColor("_Color", new Color(156/255f, 156/255f, 156/255f));
		aspect.IsActive = true;
	}



	private void UpdateSpiritLink() {
		// SPIRIT STUFF
		if (_input.Action1.WasPressed && !spiritActive && !spiritSyncActive)
		{
			ActivateSpiritPower();
		}
		else if (_input.Action1.IsPressed && spiritActive && !spiritSyncActive)
		{
			UpdateSpiritPower();
		}
		else if (_input.Action1.WasReleased)
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
		//DropSpiritPower();
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
