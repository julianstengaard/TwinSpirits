using UnityEngine;
using System.Collections;

public class CollectableDivineFavor : Collectable {
	private GameObject shield;
	private Hero hero;
	public GameObject ShieldPrefab;

	#region implemented abstract members of Collectable
	public override void Collected (Hero collector)	{
		var sc = new ShieldChance(0.1f);
		sc.shieldPrefab = ShieldPrefab;

        collector.AddEffectToWeapons(sc);
        base.CreatePopUpText("Divine favor", collector);

		sc.ForceEffect(collector.gameObject); 
		//GameObject.Destroy(gameObject);
	}
	#endregion
}
