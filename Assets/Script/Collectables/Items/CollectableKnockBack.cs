using UnityEngine;
using System;
using System.Collections.Generic;

public class CollectableKnockBack : Collectable {
	#region implemented abstract members of Collectable

	public override void Collected (Hero collector)	{
        collector.AddEffectToWeapons(new Knockback(2));
        base.CreatePopUpText("Knockback", collector);
		//GameObject.Destroy(gameObject);
	}

	#endregion


}
