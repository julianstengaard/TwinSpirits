using UnityEngine;
using System.Collections;

public class CollectableSpiritSpeedBoost : Collectable {
	public override void Collected (Hero collector)	{
		SpiritPower newPower = (SpiritPower) collector.gameObject.AddComponent<SpiritSpeedBoost>();
		collector.ChangeSpiritPower(newPower);
	}

}
