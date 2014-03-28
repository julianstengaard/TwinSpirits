using UnityEngine;
using System.Collections;

public class CollectableDivineFavor : Collectable {
	private GameObject shield;
	private Hero hero;

	#region implemented abstract members of Collectable
	public override void Collected (Hero collector)	{
		collector.AddEffectToWeapons(new ShieldChance(0.1f));
	}
	#endregion
}
