using UnityEngine;
using System.Collections;

public class CollectableSpiritPoint : Collectable {
	public override void Collected (Hero collector)	{
		collector.ChangeSpiritAmount(2);	
	}

}
