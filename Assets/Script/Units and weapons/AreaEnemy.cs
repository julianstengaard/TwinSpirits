using UnityEngine;
using System;
using System.Collections.Generic;

public class AreaEnemy : BaseEnemy {
	public new void Start() {
		base.Start();

		AddEffectToWeapons(new Knockback(3));
	}

	public void Splash() {
		var weapons = GetComponentsInChildren<Weapon>();
		foreach(var weapon in weapons)
			weapon.GetComponent<Animator>().enabled = true;
	}
}
