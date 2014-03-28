using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {
	public bool singlePlayer;
	public bool switchPlayer;

	Hero Player1;
	Hero Player2;

	Vector3 target;
	Vector3 currentLook;
	float distance = 10.0f;
	float height = 8f;

	bool intro = true;

	// Use this for initialization
	void Start () {
		var ps = GameObject.FindObjectsOfType<Hero>();
		if(ps.Length > 0) {
			foreach(var player in ps) {
				if (player.PlayerSlot == Hero.Player.One)
					Player1 = player;
				else if (player.PlayerSlot == Hero.Player.Two)
					Player2 = player;
			}
		}
	}

	void LateUpdate () {

		// Early out if we don't have a target
		if(Player2 == null) {
			Start ();
			return;
		}

		if (singlePlayer && !switchPlayer) {
			target = Player1.transform.position;
		} else if (singlePlayer && switchPlayer) {
			target = Player2.transform.position;
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
