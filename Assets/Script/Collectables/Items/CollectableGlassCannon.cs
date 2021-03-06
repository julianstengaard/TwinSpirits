﻿using UnityEngine;

public class CollectableGlassCannon : Collectable {
	public override void Collected (Hero collector)	{
		collector.AddEffectToWeapons(new Damage(10));
        collector.FullHealth = Mathf.Max(2, collector.FullHealth - 2);
        base.CreatePopUpText("Damage ++ HP -", collector);
		//GameObject.Destroy(gameObject);
	}
}
