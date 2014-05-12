using UnityEngine;

public class CollectableDamageUp : Collectable {
	public override void Collected (Hero collector)	{
		var weapons = collector.GetComponentsInChildren<Weapon>();
		foreach(var w in weapons)
			w.AddEffect(new Damage(5));
        base.CreatePopUpText("Damage +", collector);
		//GameObject.Destroy(gameObject);
	}
}
