using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WeaponPlane : Weapon {
	private float _startTime;
	private float _speed = 20;

	private Vector3 _startPoint;
	private Vector3 _endPoint;

	public new void Start() {
		base.Start();
		_startTime = Time.time;
		_startPoint = transform.position;
		_endPoint = transform.position + Body.transform.forward;
	}

	public void Update() {
		var forward = transform.up * -0.25f;
		var deltaTime = (Time.time - _startTime) / (1/_speed);
		transform.position = Vector3.Lerp(_startPoint, _endPoint, deltaTime);

		if(deltaTime > 1)
			GameObject.Destroy(gameObject);
	}
}
