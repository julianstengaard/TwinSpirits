using UnityEngine;
using System.Collections;

public class CollectableSpiritFire : CollectableSpiritPower {
	public override void Collected (Hero collector)	{
		SpiritPower newPower = (SpiritPower) collector.gameObject.AddComponent<SpiritFire>();
		collector.ChangeSpiritPower(newPower);
	}
	public override bool SpiritPowerEquals (SpiritPower power) {
		if (power.GetType() == typeof(SpiritFire)) {
			return true;
		}
		return false;
	}
}
