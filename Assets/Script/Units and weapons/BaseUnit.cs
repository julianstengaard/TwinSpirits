using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(CharacterController))]
public abstract class BaseUnit : MonoBehaviour {
	public float Health;
	public float FullHealth;
	
	[HideInInspector] 
	public bool stunned = false;
	[HideInInspector] 
	public float stunTimeout;

	protected Animator _anim;
	protected CharacterController _cc;
	
	protected Material unitMaterial;
	protected Color initColor;
	protected Color deadColor = new Color(1f, 0f, 0f);
	protected Color damageLockColor = new Color(1f, 0f, 0f);
	protected float damageLockTimer = 0f;


	public float MovementSpeed;
	protected float minMovementSpeed = 0.6f;
	protected float maxMovementSpeed = 10f;
	[HideInInspector] public float movementSpeedBuff = 0f;

	public bool dead = false;
	public bool immortal = false;
    public bool damageLocked = false;

	// METHODS

	protected void Start() {
		_anim = GetComponent<Animator>();
		_cc = GetComponent<CharacterController>();

		SkinnedMeshRenderer[] unitRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
		foreach (var renderer in unitRenderers) {
			if (renderer.gameObject.tag == "DamageBody") {
				unitMaterial = renderer.material;
			}
		}
		if (unitMaterial == null) {
			unitMaterial = unitRenderers[0].material;
		}
		
		initColor = unitMaterial.GetColor("_Color");

	    if (!(FullHealth > 0f))
            FullHealth = Health;

	}
	
	protected void Update() {
		if (damageLockTimer >= 0f) {
			damageLockTimer -= Time.deltaTime;
			unitMaterial.SetColor("_Color", Color.Lerp(initColor, damageLockColor, damageLockTimer));
		}
	}

	protected void LateUpdate() {
		if (stunned && stunTimeout < Time.time) {
			stunned = false;
		}
	}

	protected void FixedUpdate() {
		_cc.Move(Vector3.down * 0.5f);
	}

	public virtual void TakeDamage(float damage)
	{
		if (!immortal) {
			Health = Mathf.Max(0, Health - damage);
			damageLockTimer = 0.3f;
			_anim.SetTrigger("Damaged");
			if(Health <= 0 && !dead)
				Died ();
		}
	}

    public virtual void Heal(float healAmount)
    {
		if (!dead) {
			Health = Mathf.Min(FullHealth, Health + healAmount);
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

	public List<Effect> GetEffectsFromWeapons() {
		return GetComponentInChildren<Weapon>().AttackEffects;
	}

	public void RemoveEffectFromWeapon(Effect effect) {
		foreach(var weapon in GetComponentsInChildren<Weapon>())
			weapon.AttackEffects.Remove(effect);
	}

	public abstract void SetMovementSpeedBuff(float movementSpeedBuff);
}
