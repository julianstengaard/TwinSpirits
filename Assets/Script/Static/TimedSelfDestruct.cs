using UnityEngine;

public class TimedSelfDestruct : MonoBehaviour {
	public float Seconds = 2f;

	void Start () {
		GameObject.Destroy(gameObject, Seconds);
	}
}
