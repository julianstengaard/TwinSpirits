using UnityEngine;
using System.Collections;

//[RequireComponent(typeof(Rigidbody))]
public abstract class Collectable : MonoBehaviour {
	protected IEnumerator delayedLayerChange() {
		var curLayer = gameObject.layer;
		gameObject.layer = LayerMask.NameToLayer("Default");
		yield return new WaitForSeconds(1);
		gameObject.layer = curLayer;
	}

	public abstract void Collected(Hero collector);
	public virtual bool IsCollectable(Hero collector) {
		return true;
	}
}
