using UnityEngine;
using System.Collections;

public class BaseBusk : MonoBehaviour {
	public float damageOnTouch = 1f;

	void OnTriggerEnter(Collider other) {
		Hero collisionHero = other.gameObject.GetComponent<Hero>();
		if (collisionHero != null) {
			collisionHero.TakeDamage(damageOnTouch);
		}
	}
}
