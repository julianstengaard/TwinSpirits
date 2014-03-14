using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {
	public bool singlePlayer;
	Hero Player1;
	Hero Player2;

	Vector3 target;
	Vector3 currentLook;
	float distance = 10.0f;
	float height = 8f;

	bool intro = true;

	// Use this for initialization
	void Start () {
		var ps = GameObject.FindGameObjectsWithTag("Player");
		if(ps.Length > 0) {
			Player1 = ps[0].GetComponent<Hero>();
			Player2 = ps[1].GetComponent<Hero>();
		}
	}

	void LateUpdate () {

		// Early out if we don't have a target
		if(Player2 == null) {
			Start ();
			return;
		}

		if (singlePlayer) {
			target = Player1.transform.position;
		} else if (!Player1.dead && !Player2.dead) {
			target = (Player1.transform.position + Player2.transform.position) * 0.5f;
		} else if (!Player1.dead && Player2.dead) {
			target = Player1.transform.position;
		} else if (Player1.dead && !Player2.dead) {
			target = Player2.transform.position;
		} else {
			//target = (Player1.transform.position + Player2.transform.position) * 0.5f + new Vector3(0f, 50f, 0f);
			target = (Player1.transform.position + Player2.transform.position) * 0.5f;
			distance = 3f;
			height = 50f;
		}

		Vector3 wantedPosition = target + Vector3.back * distance + Vector3.up * height;
		if ((wantedPosition - transform.position).magnitude > 0.1f)	{
			transform.position = Vector3.Lerp(transform.position, wantedPosition, Time.deltaTime * 2f);
		}
		else if (intro) {
			intro = false;
		}
		if ((currentLook - target).magnitude > 0.1f)	{
			currentLook = Vector3.Lerp(currentLook, target, Time.deltaTime * 4f);
		}

		if (intro) {
			transform.LookAt (target);
		} else {
			transform.LookAt (currentLook);
		}
	}
}
