using UnityEngine;

public class CollectableSpiritPoint : Collectable {
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

    public override void Collected (Hero collector)	{
		collector.ChangeSpiritAmount(2);	
        Destroy(gameObject);
	}

}
