using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class Projectile : Weapon {

	private Vector3 startPosition;
	private Vector3 endPosition;
	private Vector3 aimPosition;
	public float journeyTime = 5.0f;
	public float angle = 15f;
	public float BaseDamage;
	private float startTime;
	private bool landed = false;


	void Start () {
		startTime = Time.time;
		//AddEffect(new Damage(BaseDamage));
	}

	public void SetPoints(Vector3 start, Vector3 aim, Vector3 end) {
		startPosition = start;
		endPosition = end;
		aimPosition = aim;
	}
	
	void Update () {
		if(startPosition == null)
			return;

		float fracComplete = (Time.time - startTime) / journeyTime;

		Vector3 a = Vector3.Lerp(startPosition, aimPosition, fracComplete);
		Vector3 b = Vector3.Lerp (aimPosition, endPosition, fracComplete);

		transform.position = Vector3.Slerp(a, b, fracComplete);
		if(fracComplete >= 1 && !landed) {
			landed = true;
			StartCoroutine(delay());
		}
	}

	IEnumerator delay() {
		yield return new WaitForSeconds(2);
		remove ();
	}

	void remove() {
		GameObject.Destroy(gameObject);
	}
}
