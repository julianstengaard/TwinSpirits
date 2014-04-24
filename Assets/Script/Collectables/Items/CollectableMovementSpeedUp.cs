using UnityEngine;
using System.Collections;
using System.Linq;

public class CollectableMovementSpeedUp : Collectable {
	public override void Collected (Hero collector)	{
		Debug.Log("Collected Movement Speed Up");
        collector.movementSpeedBuff += 2;
        base.CreatePopUpText("Speed boost", collector);
        GameObject.Destroy(gameObject);
	}
}
