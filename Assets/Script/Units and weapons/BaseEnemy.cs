using UnityEngine;
using System.Collections;
using RAIN.Core;

public class BaseEnemy : BaseUnit {
	public float BaseDamage;

	public GameObject HealthBar;
	public float HealthBarPosition = 1.5f; // Remember: Only default value
	private float HealthBarWidth = 0.14f;

	[SerializeField]
	private float OverallDropChance;
	[SerializeField]
	private Drop[] Drops;
	private bool isQuiting;

	protected AIRig ai;
	protected RandomSoundPlayer _randomSounds;

	protected new void Start() {
		base.Start();

		HealthBar = GameObject.CreatePrimitive(PrimitiveType.Plane);
		HealthBar.renderer.material.color = Color.red;
		HealthBar.transform.parent = transform;
		HealthBar.transform.position = transform.position + Vector3.up * HealthBarPosition;
		HealthBar.collider.enabled = false;
		var s = new Vector3(HealthBarWidth, 1f, 0.01f);
		HealthBar.transform.localScale = s;

		AddEffectToWeapons(new Damage(BaseDamage));

		ai = GetComponentInChildren<AIRig>();
		ai.AI.Motor.DefaultSpeed = MovementSpeed;
		
		_randomSounds = GetComponent<RandomSoundPlayer>();
		_randomSounds.PlayRandomSound("Warcry");

	}

	protected new void LateUpdate() {
		base.LateUpdate();
		HealthBar.transform.LookAt(Camera.main.transform);
		HealthBar.transform.Rotate(Vector3.left, -90f);
		var s = HealthBar.transform.localScale;
		s.x = HealthBarWidth * Health / FullHealth;
		HealthBar.transform.localScale = s;
	}

	private void OnApplicationQuit() {
		isQuiting = true;
	}

	private void OnDestroy() {
		if(!isQuiting)
			OnDeath();
	}

	protected override void Died() {
		var timeToDeath = _randomSounds.PlayRandomSound("Death") + 0.5f;
		dead = true;
		GameObject.Destroy(gameObject, timeToDeath);
	}

	private void OnDeath() {
		if(Random.Range(0f, 100f) > OverallDropChance) return;
		foreach(var drop in Drops) {
			var roll = Random.Range(0f, 1f);
			if(roll < drop.Chance) {
				var p = transform.position;
				var x = Random.Range(p.x-1, p.x+1);
				var z = Random.Range(p.z-1, p.z+1);

				GameObject.Instantiate(drop.Item, new Vector3(x, p.y, z), Quaternion.identity);
			}
		}
	}

	public override void SetMovementSpeedBuff (float speedBuff)	{
		movementSpeedBuff += speedBuff;
		ai.AI.Motor.DefaultSpeed = Mathf.Clamp(MovementSpeed + movementSpeedBuff, minMovementSpeed, maxMovementSpeed);
	}
}
