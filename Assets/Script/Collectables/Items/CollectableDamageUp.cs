using UnityEngine;

public class CollectableDamageUp : Collectable {
	public override void Collected (Hero collector)	{
		var weapons = collector.GetComponentsInChildren<Weapon>();
		foreach(var w in weapons)
			w.AddEffect(new Damage(1));
        base.CreatePopUpText("Damage up", collector);
        GameObject.Destroy(gameObject);
	}
}
