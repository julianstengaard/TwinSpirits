using UnityEngine;
using System;
using System.Collections.Generic;

public class ItemProjector : MonoBehaviour {	
	void Update () {
		transform.rotation = Quaternion.LookRotation(Vector3.down);
	}
}
