using UnityEngine;
using System;
using System.Collections.Generic;

// This will transfor all Damage effects too!	
public class CollectableUltimateSacrifice : Collectable {
	public override void Start() {}

	#region implemented abstract members of Collectable
	public override void Collected (Hero collector)	{
		var list = collector.GetEffectsFromWeapons().ToArray();
		foreach(var effect in list) {
			collector.otherPlayer.AddEffectToWeapons(effect);
			collector.RemoveEffectFromWeapon(effect);
		}
		collector.AddEffectToWeapons(new Damage(15));
	}
	#endregion
}
