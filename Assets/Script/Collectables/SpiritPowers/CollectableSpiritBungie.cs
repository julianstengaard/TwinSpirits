using UnityEngine;
using System.Collections;

public class CollectableSpiritBungie : Collectable {
	public override void Collected (Hero collector)	{
		SpiritPower newPower = (SpiritPower) collector.gameObject.AddComponent<SpiritBungie>();
		collector.ChangeSpiritPower(newPower);
	}

}
