using UnityEngine;
using System;
using System.Collections.Generic;

public class AreaEnemy : BaseEnemy {
	public GameObject ParticlesParent;

	public Renderer[] BodyParts;

	public new void Start() {
		base.Start();

		AddEffectToWeapons(new Knockback(1));
	}

	public void Splash() {
		var weapons = GetComponentsInChildren<Weapon>();
		foreach(var weapon in weapons)
			weapon.GetComponent<Animator>().enabled = true;
	}

	
	public void Explode () {
		ParticlesParent.SetActive(true);
		foreach(var part in BodyParts) {
			part.enabled = false;
		}
	}
}
