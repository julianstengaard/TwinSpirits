using UnityEngine;
using System.Collections;

public class CollectableSpiritLightning : Collectable {
	public override void Collected (Hero collector)	{
		SpiritPower newPower = (SpiritPower) collector.gameObject.AddComponent<SpiritLightning>();
		collector.ChangeSpiritPower(newPower);
	}

}
