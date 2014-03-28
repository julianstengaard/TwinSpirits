using UnityEngine;
using System.Collections;

public class CollectableSpiritRegenHP : CollectableSpiritPower {
	public override void Collected (Hero collector)	{
		SpiritPower newPower = (SpiritPower) collector.gameObject.AddComponent<SpiritRegenHP>();
		collector.ChangeSpiritPower(newPower);
	}
	public override bool SpiritPowerEquals (SpiritPower power) {
		if (power.GetType() == typeof(SpiritRegenHP)) {
			return true;
		}
		return false;
	}
}
