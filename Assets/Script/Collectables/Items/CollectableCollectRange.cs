using UnityEngine;
using System.Collections;

public class CollectableCollectRange : Collectable {
	public override void Collected (Hero collector)	{
        collector.CollectRadius += 0.5f;
        base.CreatePopUpText("Collect radius up", collector);
        //GameObject.Destroy(gameObject);
	}
}
