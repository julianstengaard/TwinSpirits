using UnityEngine;
using System.Collections;
using System.Linq;

public class CollectableHealthUp : Collectable {
	public override void Collected (Hero collector)	{

		collector.FullHealth += 2f;
        collector.Heal(2f);
		base.CreatePopUpText("Health up", collector);
		GameObject.Destroy(gameObject);
	}
}
