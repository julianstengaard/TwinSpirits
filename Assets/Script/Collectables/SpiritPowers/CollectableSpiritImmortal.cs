using UnityEngine;
using System.Collections;

public class CollectableSpiritImmortal : CollectableSpiritPower {
	public override void Collected (Hero collector)	{
		SpiritPower newPower = (SpiritPower) collector.gameObject.AddComponent<SpiritImmortal>();
		collector.ChangeSpiritPower(newPower);
	}
	public override bool SpiritPowerEquals (SpiritPower power) {
		if (power.GetType() == typeof(SpiritImmortal)) {
			return true;
		}
		return false;
	}
}
