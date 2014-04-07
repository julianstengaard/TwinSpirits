using UnityEngine;
using System.Collections;

public class CollectableSpiritPingPong : CollectableSpiritPower {
	public override void Collected (Hero collector)	{
		SpiritPower newPower = (SpiritPower) collector.gameObject.AddComponent<SpiritPingPong>();
		collector.ChangeSpiritPower(newPower);
	}
	public override bool SpiritPowerEquals (SpiritPower power) {
		if (power.GetType() == typeof(SpiritPingPong)) {
			return true;
		}
		return false;
	}
}
