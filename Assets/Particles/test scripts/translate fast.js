﻿	function Update() {
		// Move the object forward along its z axis 1 unit/second.
		transform.Translate((-1.7) *Vector3.forward * Time.deltaTime);
		// Move the object upward in world space 1 unit/second.
		//transform.Translate(Vector3.up * Time.deltaTime, Space.World);
	}