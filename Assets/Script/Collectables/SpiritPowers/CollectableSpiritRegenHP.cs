using UnityEngine;
using System.Collections;

public class CollectableSpiritRegenHP : Collectable {
	public override void Collected (Hero collector)	{
		SpiritPower newPower = (SpiritPower) collector.gameObject.AddComponent<SpiritRegenHP>();
		collector.ChangeSpiritPower(newPower);
	}

}
