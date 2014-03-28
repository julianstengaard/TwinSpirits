using UnityEngine;
using System.Collections;
using RAIN.Core;

public class BaseEnemy : BaseUnit {
	public float BaseDamage;

	[SerializeField]
	private float OverallDropChance;
	[SerializeField]
	private Drop[] Drops;
	private bool isQuiting;

	protected AIRig ai;
	protected RandomSoundPlayer _randomSounds;

	private new void Start() {
		base.Start();
		AddEffectToWeapons(new Damage(BaseDamage));

		ai = GetComponentInChildren<AIRig>();
		ai.AI.Motor.DefaultSpeed = MovementSpeed;
		
		_randomSounds = GetComponent<RandomSoundPlayer>();
		_randomSounds.PlayRandomSound("Warcry");

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
