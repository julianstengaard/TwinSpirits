using UnityEngine;
using System.Collections;
using System.Linq;

public class CollectableHealthUp : Collectable {
	public override void Collected (Hero collector)	{
		Debug.Log("Collected fat fcuk!");
		collector.FullHealth += 1f;
        collector.Heal(1f);
	}
}
