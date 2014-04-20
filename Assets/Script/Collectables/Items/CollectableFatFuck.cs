using UnityEngine;
using System.Collections;
using System.Linq;

public class CollectableFatFuck : Collectable {
	public override void Collected (Hero collector)	{
		Debug.Log("Collected fat fcuk!");
		collector.movementSpeedBuff -= 1f;
		collector.FullHealth += 2f;
        collector.Heal(2f);
	}
}
