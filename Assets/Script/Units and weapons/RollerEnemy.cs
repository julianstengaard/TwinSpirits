using UnityEngine;
using System.Collections;
using RAIN.Core;

public class RollerEnemy : BaseEnemy {
	private LayerMask lastLayer;

	protected new void Start() {
		base.Start ();
		AddEffectToWeapons(new Knockback(2));
	}
	
	public new void MakeDangerous() {
		base.MakeDangerous();
		lastLayer = gameObject.layer;
		gameObject.layer = LayerMask.NameToLayer("RollingUnits");
	}

	public new void MakeInert() {
		base.MakeInert();
		gameObject.layer = lastLayer;
	}
}
