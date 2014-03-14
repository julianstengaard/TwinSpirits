using UnityEngine;
using System;
using System.Collections.Generic;

public class DeathFloor : MonoBehaviour {
	void OnTriggerEnter(Collider other) {
		other.gameObject.SendMessage("TakeDamage", 10000f);
		GameObject.Destroy(other.gameObject);

		if(other.GetComponent<Hero>() != null)
			Application.LoadLevel(0);
	}
}
