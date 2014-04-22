using UnityEngine;
using System.Collections;

public class CollectableHealthPoint : Collectable {
	public virtual void Start() {
		var x = Random.Range (-0.5f, 0.5f);
		var z = Random.Range (-0.5f, 0.5f);
		var y = Random.Range (1, 2);
		rigidbody.AddForce(new Vector3(x,y,z) * 3000);
		StartCoroutine(delayedLayerChange());
	}

	public override void Collected (Hero collector)	{
		collector.Heal(1);	
	}

	public override bool IsCollectable (Hero collector)	{
		return collector.Health != collector.FullHealth;
	}
}
