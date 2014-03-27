using UnityEngine;
using System.Collections;

public class CollectableSpiritLightning : CollectableSpiritPower {
	public override void Collected (Hero collector)	{
		SpiritPower newPower = (SpiritPower) collector.gameObject.AddComponent<SpiritLightning>();
		collector.ChangeSpiritPower(newPower);
	}
	public override bool SpiritPowerEquals (SpiritPower power) {
		if (power.GetType() == typeof(SpiritLightning)) {
			return true;
		}
		return false;
	}
}
