function Update() {
		// Slowly rotate the object around its X axis at 1 degree/second.
		//transform.Rotate(Vector3.right * 3);
		// ... at the same time as spinning relative to the global 
		// Y axis at the same speed.
		transform.Rotate(Vector3.up * (Time.deltaTime * 100), Space.World);
		
	}