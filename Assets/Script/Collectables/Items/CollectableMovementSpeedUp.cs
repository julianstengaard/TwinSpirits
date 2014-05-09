using UnityEngine;
using System.Collections;
using System.Linq;

public class CollectableMovementSpeedUp : Collectable {
	public override void Collected (Hero collector)	{
		Debug.Log("Collected Movement Speed Up");
        collector.movementSpeedBuff += 1.3f;
        base.CreatePopUpText("Speed boost", collector);
		//GameObject.Destroy(gameObject);
	}
}
