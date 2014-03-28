using UnityEngine;
using System.Collections;
using System.Linq;

public class CollectableDamageUp : Collectable {
	public override void Collected (Hero collector)	{
		var weapons = collector.GetComponentsInChildren<Weapon>();
		foreach(var w in weapons)
			w.AddEffect(new Damage(1));
	}
}
