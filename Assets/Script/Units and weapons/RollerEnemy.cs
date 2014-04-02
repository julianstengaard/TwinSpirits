using UnityEngine;
using System.Collections;
using RAIN.Core;

public class RollerEnemy : BaseEnemy {

	protected new void Start() {
		base.Start ();
		AddEffectToWeapons(new Knockback(40));
	}
}
