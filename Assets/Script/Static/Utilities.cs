using UnityEngine;
using System.Collections;

public static class Utilities {
	public static Vector3 SetZ(this Vector3 orig, float value) {
		var tempVector = orig;
		tempVector.z = value;
		return tempVector;
	}
}
