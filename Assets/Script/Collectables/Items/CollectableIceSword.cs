using UnityEngine;
using System;
using System.Collections.Generic;

public class CollectableIceSword : Collectable {
	#region implemented abstract members of Collectable
	public override void Start() {}

	public override void Collected (Hero collector)	{
		collector.AddEffectToWeapons(new SlowdownMovementSpeed(1f, 3f, 0.25f));
	}
	#endregion
}
