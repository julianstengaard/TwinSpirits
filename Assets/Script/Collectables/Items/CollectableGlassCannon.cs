using UnityEngine;
using System.Collections;
using System.Linq;

public class CollectableGlassCannon : Collectable {
	public override void Start() {}

	public override void Collected (Hero collector)	{
		collector.AddEffectToWeapons(new Damage(2));
		collector.FullHealth = Mathf.Max(2, collector.FullHealth - 2);
	}
}
