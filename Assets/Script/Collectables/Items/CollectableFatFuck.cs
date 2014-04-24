using UnityEngine;
using System.Collections;
using System.Linq;

public class CollectableFatFuck : Collectable {
	public override void Collected (Hero collector)	{
		collector.movementSpeedBuff -= 1f;
		collector.FullHealth += 2f;
        collector.Heal(2f);
        base.CreatePopUpText("A worthy tradeoff", collector);
        GameObject.Destroy(gameObject);
	}
}
