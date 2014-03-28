using UnityEngine;
using System.Collections;

public abstract class CollectableSpiritPower : MonoBehaviour {
	public abstract void Collected(Hero collector);

	public abstract bool SpiritPowerEquals(SpiritPower power);
}
