using UnityEngine;
using System.Collections;

public class CollectableSpiritImmortal : Collectable {
	public override void Collected (Hero collector)	{
		SpiritPower newPower = (SpiritPower) collector.gameObject.AddComponent<SpiritImmortal>();
		collector.ChangeSpiritPower(newPower);
	}

}
