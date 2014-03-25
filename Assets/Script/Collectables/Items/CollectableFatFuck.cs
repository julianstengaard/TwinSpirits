using UnityEngine;
using System.Collections;
using System.Linq;

public class CollectableFatFuck : Collectable {
	public override void Collected (Hero collector)	{
		Debug.Log("Collected fat fcuk!");
		collector.movementSpeedBuff -= 2;
		collector.FullHealth += 2;
	}
}
