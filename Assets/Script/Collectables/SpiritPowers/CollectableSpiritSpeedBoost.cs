using UnityEngine;
using System.Collections;

public class CollectableSpiritSpeedBoost : CollectableSpiritPower {
	public override void Collected (Hero collector)	{
		SpiritPower newPower = (SpiritPower) collector.gameObject.AddComponent<SpiritSpeedBoost>();
		collector.ChangeSpiritPower(newPower);
	}
	public override bool SpiritPowerEquals (SpiritPower power) {
		if (power.GetType() == typeof(SpiritSpeedBoost)) {
			return true;
		}
		return false;
	}
}
