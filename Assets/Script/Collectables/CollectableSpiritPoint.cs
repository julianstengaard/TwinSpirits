using UnityEngine;

public class CollectableSpiritPoint : Collectable {
	public AudioClip SpiritPickupSound;
	private bool _collected = false;

    private float _selfDestructAfter = 10f;
    private float _selfDestructAfterTimer = 0f;

	public virtual void Start() {
		var x = Random.Range (-0.5f, 0.5f);
		var z = Random.Range (-0.5f, 0.5f);
		var y = Random.Range (1, 2);
		rigidbody.AddForce(new Vector3(x,y,z) * 3000);
		StartCoroutine(delayedLayerChange());
	}

    void FixedUpdate() {
        if (_selfDestructAfterTimer > _selfDestructAfter) {
            Destroy(gameObject);
        }
        _selfDestructAfterTimer += Time.deltaTime;
    }

	public override void Collected (Hero collector) {
		if (_collected == true) return;
		
		_collected = true;
		gameObject.audio.clip = SpiritPickupSound;
		gameObject.audio.Play();
		collector.ChangeSpiritAmount(2);
		gameObject.renderer.enabled = false;
		for (int i = 0; i < gameObject.transform.childCount; i++) {
			gameObject.transform.GetChild(i).gameObject.SetActive(false);
		}
		Destroy(gameObject, SpiritPickupSound.length);
	}
}
