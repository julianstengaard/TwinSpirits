using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(CharacterController))]
public abstract class BaseUnit : MonoBehaviour {
	public float Health;
	private float fullHealth;
	
	[HideInInspector] 
	public bool stunned = false;
	[HideInInspector] 
	public float stunTimeout;

	protected Animator _anim;
	protected CharacterController _cc;


	public float MovementSpeed;
	protected float minMovementSpeed = 0.6f;
	protected float maxMovementSpeed = 10f;
	[HideInInspector] public float movementSpeedBuff = 0f;

	public float HealthBarPosition = 1.5f; // Remember: Only default value
	public GameObject HealthBar;
	private float HealthBarWidth = 0.14f;

	public bool dead = false;
	public bool immortal = false;

	// METHODS

	protected void Start() {
		_anim = GetComponent<Animator>();
		_cc = GetComponent<CharacterController>();
		HealthBar = GameObject.CreatePrimitive(PrimitiveType.Plane);
		HealthBar.renderer.material.color = Color.red;
		HealthBar.transform.parent = transform;
		HealthBar.transform.position = transform.position + Vector3.up * HealthBarPosition;
		HealthBar.collider.enabled = false;
		var s = new Vector3(HealthBarWidth, 1f, 0.01f);
		HealthBar.transform.localScale = s;

		fullHealth = Health;

	}

	protected void Update() {}

	protected void LateUpdate() {
		if (stunned && stunTimeout < Time.time) {
			stunned = false;
		}
		HealthBar.transform.LookAt(Camera.main.transform);
		HealthBar.transform.Rotate(Vector3.left, -90f);
		var s = HealthBar.transform.localScale;
		s.x = HealthBarWidth * Health / fullHealth;
		HealthBar.transform.localScale = s;
	}

	protected void FixedUpdate() {
		_cc.Move(Vector3.down * 0.5f);
	}

	public void TakeDamage(float damage) {
		if (!immortal) {
			Health = Mathf.Max(0, Health - damage);
			_anim.SetTrigger("Damaged");
			if(Health <= 0 && !dead)
				Died ();
		}
	}

	public void Heal(float healAmount) {
		if (!dead) {
			Health = Mathf.Min(fullHealth, Health + healAmount);
		}
	}

	protected virtual void Died()
	{
		dead = true;
		GameObject.Destroy(gameObject);
	}

	public void EvaluateAttacks(GameObject attacker, Vector3 origin, List<Effect> effects, string[] immuneTags) {
		if (CollisionTargetIsValid(immuneTags))
		{
			var damage = 0f;
			foreach(var e in effects) 
			{
				e.DoEffect(this, attacker, origin, ref damage);
				StartCoroutine(e.DoEffectCoroutine(this, attacker, origin));
			}
			TakeDamage(damage);
		}
	}

	private bool CollisionTargetIsValid(string[] immuneTags) {
		foreach (string tag in immuneTags)
		{
			if (tag == this.tag)
			{
				return false;
			}
		}
		return true;
	}

	public void MakeDangerous() {
		var weapons = GetComponentsInChildren<Weapon>();
		if(weapons == null) return;
		foreach(var w in weapons)
			w.MakeDangerous();
	}

	public void MakeInert() {
		var weapons = GetComponentsInChildren<Weapon>();
		if(weapons == null) return;
		foreach(var w in weapons)
			w.MakeInert();
	}

	public void AddEffectToWeapons(Effect effect) {
		var weapons = GetComponentsInChildren<Weapon>();
		if(weapons == null) return;
		foreach(var w in weapons)
			w.AddEffect(effect);
	}

	public abstract void SetMovementSpeedBuff(float movementSpeedBuff);
}
