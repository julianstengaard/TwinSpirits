using UnityEngine;
using System.Collections;

public class CollectableSpiritBungie : CollectableSpiritPower {
	public override void Collected (Hero collector)	{
		SpiritPower newPower = (SpiritPower) collector.gameObject.AddComponent<SpiritBungie>();
		collector.ChangeSpiritPower(newPower);
	}
	public override bool SpiritPowerEquals (SpiritPower power) {
		if (power.GetType() == typeof(SpiritBungie)) {
			return true;
		}
		return false;
	}
}
