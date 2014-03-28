using UnityEngine;
using System;
using System.Collections.Generic;

public class ItemProjector : MonoBehaviour {	
	public enum Direction {Up, Down}
	public Direction dir = Direction.Down;

	void Update () {
		transform.rotation = Quaternion.LookRotation(dir == Direction.Down ? Vector3.down : Vector3.up);
	}
}
