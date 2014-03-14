using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public abstract class Collectable : MonoBehaviour {
	void Start() {
		var x = Random.Range (-0.5f, 0.5f);
		var z = Random.Range (-0.5f, 0.5f);
		var y = Random.Range (1, 2);
		rigidbody.AddForce(new Vector3(x,y,z) * 3000);
		StartCoroutine(delayedLayerChange());
	}

	private IEnumerator delayedLayerChange() {
		var curLayer = gameObject.layer;
		gameObject.layer = LayerMask.NameToLayer("Default");
		yield return new WaitForSeconds(1);
		gameObject.layer = curLayer;
	}

	public abstract void Collected(Hero collector);
}
