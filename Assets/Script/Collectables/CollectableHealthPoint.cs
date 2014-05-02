using UnityEngine;
using System.Collections;

public class CollectableHealthPoint : Collectable {
    public AudioClip HealthPickupSound;
	private bool _collected = false;

	public new virtual void Start() {
		var x = Random.Range (-0.5f, 0.5f);
		var z = Random.Range (-0.5f, 0.5f);
		var y = Random.Range (1, 2);
		rigidbody.AddForce(new Vector3(x,y,z) * 3000);
		StartCoroutine(delayedLayerChange());
	}

	public override void Collected (Hero collector) {
		if (_collected == true) return;

		_collected = true;
	    gameObject.audio.clip = HealthPickupSound;
        gameObject.audio.Play();
		collector.Heal(2);
	    gameObject.renderer.enabled = false;
	    for (int i = 0; i < gameObject.transform.childCount; i++) {
	        gameObject.transform.GetChild(i).gameObject.SetActive(false);
        }
        Destroy(gameObject, HealthPickupSound.length);
	}

	public override bool IsCollectable (Hero collector)	{
		return collector.Health != collector.FullHealth;
	}
}
