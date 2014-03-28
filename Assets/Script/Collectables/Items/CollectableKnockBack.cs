using UnityEngine;
using System;
using System.Collections.Generic;

public class CollectableKnockBack : Collectable {
	public override void Start() {}

	#region implemented abstract members of Collectable

	public override void Collected (Hero collector)	{
		collector.AddEffectToWeapons(new Knockback(100));
	}

	#endregion


}
